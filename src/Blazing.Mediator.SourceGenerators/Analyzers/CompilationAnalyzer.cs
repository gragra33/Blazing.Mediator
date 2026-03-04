using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Blazing.Mediator.SourceGenerators.Models;

namespace Blazing.Mediator.SourceGenerators.Analyzers;

/// <summary>
/// Analyses the consuming assembly's compilation and produces a <see cref="CompilationModel"/>
/// that contains every request handler, notification handler, and middleware discovered.
/// This is the single source-of-truth discovery step fed into <c>MediatorCodeWriter</c>.
/// </summary>
internal sealed class CompilationAnalyzer
{
    // ── Interface name prefixes used for discovery ─────────────────────────
    private const string IRequestHandlerPrefix       = "Blazing.Mediator.IRequestHandler";
    private const string IStreamRequestHandlerPrefix  = "Blazing.Mediator.IStreamRequestHandler";
    private const string INotificationHandlerPrefix   = "Blazing.Mediator.INotificationHandler";
    private const string IRequestMiddlewarePrefix     = "Blazing.Mediator.IRequestMiddleware";
    private const string IStreamMiddlewarePrefix      = "Blazing.Mediator.IStreamRequestMiddleware";
    private const string INotificationMiddlewarePrefix = "Blazing.Mediator.INotificationMiddleware";
    private const string IConditionalMiddlewarePrefix  = "Blazing.Mediator.IConditionalMiddleware";
    private const string IQueryPrefix                 = "Blazing.Mediator.IQuery";
    private const string OrderAttributeName           = "Blazing.Mediator.OrderAttribute";
    private const string UnitTypeName                 = "global::Blazing.Mediator.Unit";

    private readonly Compilation _compilation;
    private readonly MiddlewareConstraintAnalyzer _constraintAnalyzer;

    // Shorthand: fully-qualified format includes "global::" prefix and expands keyword
    // aliases (e.g. "int" → "global::System.Int32"), producing valid C# identifiers.
    private static string Fq(ITypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public CompilationAnalyzer(Compilation compilation)
    {
        _compilation = compilation;
        _constraintAnalyzer = new MiddlewareConstraintAnalyzer(compilation);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public entry point
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs discovery over the collected class declarations and the compilation,
    /// then returns a fully populated <see cref="CompilationModel"/>.
    /// </summary>
    public CompilationModel Analyze(
        ImmutableArray<ClassDeclarationSyntax> classes,
        string rootNamespace)
    {
        // First pass: discover all handlers and raw middleware descriptors.
        var rawRequests      = new List<RawRequest>();
        var rawNotifications = new Dictionary<string, RawNotification>(System.StringComparer.Ordinal);
        var rawMiddleware    = new List<RawMiddleware>();

        foreach (var classDecl in classes.IsDefault ? ImmutableArray<ClassDeclarationSyntax>.Empty : classes)
        {
            var semanticModel = _compilation.GetSemanticModel(classDecl.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol typeSymbol)
                continue;

            if (typeSymbol.IsAbstract)
                continue;

            // Skip classes decorated with [ExcludeFromAutoDiscovery].
            if (HasExcludeFromAutoDiscoveryAttribute(typeSymbol))
                continue;

            // Open-generic types cannot be closed for handler DI registrations,
            // but open-generic middleware IS supported (applied to all matching request types).
            if (!typeSymbol.IsGenericType)
            {
                TryCollectRequestHandler(typeSymbol, rawRequests);
                TryCollectNotificationHandler(typeSymbol, rawNotifications);
            }
            TryCollectMiddleware(typeSymbol, rawMiddleware);
        }

        // Cross-assembly discovery: walk referenced assemblies that depend on Blazing.Mediator
        // (covers multi-project solutions such as AnalyzerExample with 5 assemblies).
        DiscoverFromReferencedAssemblies(rawRequests, rawNotifications, rawMiddleware);

        // Fan-out covariant notification handlers: merge interface/base-type handler
        // registrations into every concrete notification that is a subtype.
        // E.g. INotificationHandler<INotification> is also invoked for Pinged, Ponged, etc.
        MergeCovariantNotificationHandlers(rawNotifications);

        // Second pass: for every discovered request, resolve which middleware applies.
        var requests = BuildRequestModels(rawRequests, rawMiddleware);

        // Notification models: sort with derived types first so that switch arms for derived types
        // appear before base type arms, preventing CS8510 "unreachable pattern" errors.
        var notifications = rawNotifications.Values
            .OrderBy(n => n.NotificationType) // stable alphabetical baseline
            .ToList();

        // Topological sort: a derived type must precede all its base/interface types in the switch.
        notifications.Sort((a, b) =>
        {
            if (a.NotificationSymbol != null && b.NotificationSymbol != null)
            {
                if (IsNotificationSubtypeOf(a.NotificationSymbol, b.NotificationSymbol)) return -1; // a derived from b → a first
                if (IsNotificationSubtypeOf(b.NotificationSymbol, a.NotificationSymbol)) return  1; // b derived from a → b first
            }
            return string.Compare(a.NotificationType, b.NotificationType, System.StringComparison.Ordinal);
        });

        var notificationModels = notifications
            .Select(n =>
            {
                var applicable = BuildApplicableNotificationMiddleware(n, rawMiddleware);
                bool isInterface = n.NotificationSymbol?.TypeKind == Microsoft.CodeAnalysis.TypeKind.Interface;
                return new NotificationHandlerModel(
                    safeNotificationName: MakeSafeName(n.NotificationType),
                    notificationType:     n.NotificationType,
                    handlerTypes:         n.HandlerTypes,
                    isInterface:          isInterface,
                    applicableNotificationMiddleware: applicable);
            })
            .ToList();

        return new CompilationModel(rootNamespace, requests, notificationModels, rawMiddleware.Select(ToMiddlewareModel).ToList());
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Request handler discovery
    // ─────────────────────────────────────────────────────────────────────────

    private static void TryCollectRequestHandler(INamedTypeSymbol typeSymbol, List<RawRequest> sink)
    {
        // A handler class may implement multiple handler interfaces (e.g. IRequestHandler<A>
        // + IStreamRequestHandler<B,C>).  Do NOT return early — iterate all interfaces.
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            // IRequestHandler<TRequest>  — void command (single type arg)
            if (HasInterfacePrefix(iface, IRequestHandlerPrefix, typeArgCount: 1))
            {
                var requestType  = iface.TypeArguments[0];
                sink.Add(new RawRequest(
                    handlerSymbol:  typeSymbol,
                    requestSymbol:  requestType,
                    responseSymbol: null,
                    isStream:       false,
                    isVoid:         true,
                    isQuery:        false));  // void handlers are always commands
                continue;
            }

            // IRequestHandler<TRequest, TResponse>  — request with response
            if (HasInterfacePrefix(iface, IRequestHandlerPrefix, typeArgCount: 2))
            {
                var requestType  = iface.TypeArguments[0];
                var responseType = iface.TypeArguments[1];
                // Check if the request type implements IQuery<T> to classify as Query vs Command
                bool isQuery = requestType is INamedTypeSymbol reqSym &&
                               reqSym.AllInterfaces.Any(i => HasInterfacePrefix(i, IQueryPrefix, typeArgCount: 1));
                sink.Add(new RawRequest(
                    handlerSymbol:  typeSymbol,
                    requestSymbol:  requestType,
                    responseSymbol: responseType,
                    isStream:       false,
                    isVoid:         false,
                    isQuery:        isQuery));
                continue;
            }

            // IStreamRequestHandler<TRequest, TResponse>
            if (HasInterfacePrefix(iface, IStreamRequestHandlerPrefix, typeArgCount: 2))
            {
                var requestType  = iface.TypeArguments[0];
                var responseType = iface.TypeArguments[1];
                sink.Add(new RawRequest(
                    handlerSymbol:  typeSymbol,
                    requestSymbol:  requestType,
                    responseSymbol: responseType,
                    isStream:       true,
                    isVoid:         false,
                    isQuery:        false));  // stream requests are categorised as Stream, not Query
                continue;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Notification handler discovery
    // ─────────────────────────────────────────────────────────────────────────

    private static void TryCollectNotificationHandler(
        INamedTypeSymbol typeSymbol,
        Dictionary<string, RawNotification> notificationMap)
    {
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            if (!HasInterfacePrefix(iface, INotificationHandlerPrefix, typeArgCount: 1))
                continue;

            var notificationSymbol = iface.TypeArguments[0];
            var notificationType   = Fq(notificationSymbol);
            if (!notificationMap.TryGetValue(notificationType, out var raw))
            {
                raw = new RawNotification(notificationType, notificationSymbol);
                notificationMap[notificationType] = raw;
            }
            else if (raw.NotificationSymbol == null)
            {
                raw.NotificationSymbol = notificationSymbol;
            }
            raw.HandlerTypes.Add(Fq(typeSymbol));
            // NOTE: `continue`, not `return` — a handler class may implement multiple
            // INotificationHandler<T> interfaces (e.g. handles OrderCreated AND CustomerRegistered).
            // Using `return` would silently discard all but the first notification type.
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Middleware discovery
    // ─────────────────────────────────────────────────────────────────────────

    private static void TryCollectMiddleware(INamedTypeSymbol typeSymbol, List<RawMiddleware> sink)
    {
        bool isConditional = HasAnyInterfacePrefix(typeSymbol, IConditionalMiddlewarePrefix);

        // ── Notification middleware (non-generic INotificationMiddleware) ──
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            var display = iface.ToDisplayString();
            if (display == INotificationMiddlewarePrefix ||
                display.StartsWith(INotificationMiddlewarePrefix + "<"))
            {
                // Capture the type constraint for INotificationMiddleware<T> so that
                // MediatorCodeWriter can filter out constrained middleware for notification
                // types that don't satisfy the constraint.
                ITypeSymbol? constraintType = null;
                var ifaceOrig = iface.OriginalDefinition.ToDisplayString();
                if (ifaceOrig.StartsWith(INotificationMiddlewarePrefix + "<") &&
                    iface.TypeArguments.Length == 1)
                {
                    constraintType = iface.TypeArguments[0];
                }

                sink.Add(new RawMiddleware(
                    symbol:           typeSymbol,
                    kind:             MiddlewareKind.Notification,
                    isConditional:    isConditional,
                    closedRequest:    null,
                    closedResponse:   null,
                    constraintType:   constraintType));
                return;
            }
        }

        // ── Stream middleware ──
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            if (HasInterfacePrefix(iface, IStreamMiddlewarePrefix, typeArgCount: 2))
            {
                bool isOpenGeneric = typeSymbol.IsGenericType;
                ITypeSymbol? closedRequest  = isOpenGeneric ? null : iface.TypeArguments[0];
                ITypeSymbol? closedResponse = isOpenGeneric ? null : iface.TypeArguments[1];

                sink.Add(new RawMiddleware(
                    symbol:           typeSymbol,
                    kind:             MiddlewareKind.Stream,
                    isConditional:    isConditional,
                    closedRequest:    closedRequest,
                    closedResponse:   closedResponse));
                return;
            }
        }

        // ── Request middleware (void command: single type arg) ──
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            if (HasInterfacePrefix(iface, IRequestMiddlewarePrefix, typeArgCount: 1))
            {
                bool isOpenGeneric = typeSymbol.IsGenericType;
                ITypeSymbol? closedRequest = isOpenGeneric ? null : iface.TypeArguments[0];

                sink.Add(new RawMiddleware(
                    symbol:           typeSymbol,
                    kind:             MiddlewareKind.VoidCommand,
                    isConditional:    isConditional,
                    closedRequest:    closedRequest,
                    closedResponse:   null));
                return;
            }
        }

        // ── Request middleware (with response: two type args) ──
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            if (HasInterfacePrefix(iface, IRequestMiddlewarePrefix, typeArgCount: 2))
            {
                bool isOpenGeneric = typeSymbol.IsGenericType;
                ITypeSymbol? closedRequest  = isOpenGeneric ? null : iface.TypeArguments[0];
                ITypeSymbol? closedResponse = isOpenGeneric ? null : iface.TypeArguments[1];

                sink.Add(new RawMiddleware(
                    symbol:           typeSymbol,
                    kind:             MiddlewareKind.Request,
                    isConditional:    isConditional,
                    closedRequest:    closedRequest,
                    closedResponse:   closedResponse));
                return;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Pipeline matching: assign middleware to each request
    // ─────────────────────────────────────────────────────────────────────────

    private List<RequestHandlerModel> BuildRequestModels(
        List<RawRequest>    rawRequests,
        List<RawMiddleware> allMiddleware)
    {
        var models = new List<RequestHandlerModel>(rawRequests.Count);

        // Group raw requests by their request type so we can detect multiple handlers
        // for the same request type (which would produce duplicate identifiers / switch cases).
        var grouped = new System.Collections.Generic.Dictionary<string, List<RawRequest>>(System.StringComparer.Ordinal);
        foreach (var req in rawRequests)
        {
            var key = Fq(req.RequestSymbol);
            if (!grouped.TryGetValue(key, out var list))
            {
                list = new List<RawRequest>();
                grouped[key] = list;
            }
            list.Add(req);
        }

        foreach (var kvp in grouped)
        {
            var group = kvp.Value;
            var first = group[0];

            if (group.Count > 1)
            {
                // Multiple handlers found for this request type — mark as ambiguous.
                // The dispatcher will emit a ThrowHelper.ThrowMultipleHandlers*() call.
                // All handler types are still registered in DI so the error message can list them.
                var allHandlerTypes = new List<string>(group.Count);
                foreach (var r in group)
                    allHandlerTypes.Add(Fq(r.HandlerSymbol));

                var responseType2 = first.IsVoid ? UnitTypeName : Fq(first.ResponseSymbol!);

                models.Add(new RequestHandlerModel(
                    safeName:             MakeSafeName(Fq(first.RequestSymbol)),
                    requestType:          Fq(first.RequestSymbol),
                    responseType:         responseType2,
                    handlerType:          allHandlerTypes[0],
                    isStream:             first.IsStream,
                    isVoid:               first.IsVoid,
                    isQuery:              first.IsQuery,
                    applicableMiddleware: new List<MiddlewareModel>(),
                    isAmbiguous:          true,
                    allHandlerTypes:      allHandlerTypes));
                continue;
            }

            var applicable = new List<MiddlewareModel>();

            foreach (var mw in allMiddleware)
            {
                if (!MiddlewareAppliesTo(mw, first))
                    continue;

                applicable.Add(ToAppliedMiddlewareModel(mw, first));
            }

            // Sort by declared Order, then by type name for determinism.
            applicable.Sort((a, b) =>
            {
                int cmp = a.Order.CompareTo(b.Order);
                return cmp != 0 ? cmp : string.Compare(a.MiddlewareType, b.MiddlewareType, System.StringComparison.Ordinal);
            });

            var responseType = first.IsVoid
                ? UnitTypeName
                : Fq(first.ResponseSymbol!);

            models.Add(new RequestHandlerModel(
                safeName:             MakeSafeName(Fq(first.RequestSymbol)),
                requestType:          Fq(first.RequestSymbol),
                responseType:         responseType,
                handlerType:          Fq(first.HandlerSymbol),
                isStream:             first.IsStream,
                isVoid:               first.IsVoid,
                isQuery:              first.IsQuery,
                applicableMiddleware: applicable));
        }

        // Stable sort for deterministic output.
        models.Sort((a, b) => string.Compare(a.RequestType, b.RequestType, System.StringComparison.Ordinal));
        return models;
    }

    /// <summary>
    /// Builds the list of notification middleware that applies to a specific notification type.
    /// Non-constrained middleware (<c>INotificationMiddleware</c>) applies to all notifications.
    /// Constrained middleware (<c>INotificationMiddleware&lt;T&gt;</c>) only applies when the
    /// notification type implements or derives from the constraint type T.
    /// </summary>
    private static List<MiddlewareModel> BuildApplicableNotificationMiddleware(
        RawNotification notification,
        List<RawMiddleware> allMiddleware)
    {
        var applicable = new List<MiddlewareModel>();

        foreach (var mw in allMiddleware)
        {
            if (mw.Kind != MiddlewareKind.Notification)
                continue;

            // Non-constrained notification middleware applies to every notification.
            if (mw.ConstraintType == null)
            {
                applicable.Add(ToMiddlewareModel(mw));
                continue;
            }

            // Constrained middleware: only apply when the notification type satisfies the constraint.
            if (notification.NotificationSymbol != null &&
                NotificationSatisfiesConstraint(notification.NotificationSymbol, mw.ConstraintType))
            {
                applicable.Add(ToMiddlewareModel(mw));
            }
        }

        // Sort by declared order, then type name for determinism.
        applicable.Sort((a, b) =>
        {
            int cmp = a.Order.CompareTo(b.Order);
            return cmp != 0 ? cmp : string.Compare(a.MiddlewareType, b.MiddlewareType, System.StringComparison.Ordinal);
        });

        return applicable;
    }

    /// <summary>
    /// Returns true when <paramref name="notificationSymbol"/> implements or derives from <paramref name="constraintType"/>.
    /// </summary>
    private static bool NotificationSatisfiesConstraint(ITypeSymbol notificationSymbol, ITypeSymbol constraintType)
    {
        // Direct implementation: check all interfaces of the notification type.
        foreach (var iface in notificationSymbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition,
                constraintType is INamedTypeSymbol nc ? nc.OriginalDefinition : constraintType))
                return true;
            if (SymbolEqualityComparer.Default.Equals(iface, constraintType))
                return true;
        }

        // Check inheritance chain.
        var current = notificationSymbol;
        while (current != null)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition,
                constraintType is INamedTypeSymbol nc2 ? nc2.OriginalDefinition : constraintType))
                return true;
            if (SymbolEqualityComparer.Default.Equals(current, constraintType))
                return true;
            current = (current as INamedTypeSymbol)?.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Determines whether a raw middleware entry applies to a given request.
    /// Handles:
    ///   1. Notification middleware   — never applies to requests.
    ///   2. Stream vs non-stream       — must match.
    ///   3. VoidCommand vs response    — must match.
    ///   4. Closed-generic             — request type must match exactly.
    ///   5. Open-generic               — <see cref="MiddlewareConstraintAnalyzer"/> checks
    ///                                   <c>where TReq : IDomainInterface</c> constraints.
    /// </summary>
    private bool MiddlewareAppliesTo(RawMiddleware mw, RawRequest req)
    {
        // Notification middleware never goes into request pipelines.
        if (mw.Kind == MiddlewareKind.Notification)
            return false;

        // Stream/non-stream must match.
        bool mwIsStream = mw.Kind == MiddlewareKind.Stream;
        if (mwIsStream != req.IsStream)
            return false;

        // Void-command matching.
        bool mwIsVoid = mw.Kind == MiddlewareKind.VoidCommand;
        if (mwIsVoid != req.IsVoid)
            return false;

        bool isOpenGeneric = mw.Symbol.IsGenericType;

        if (!isOpenGeneric)
        {
            // Closed-generic: request type must match exactly.
            return mw.ClosedRequest != null &&
                   SymbolEqualityComparer.Default.Equals(mw.ClosedRequest, req.RequestSymbol);
        }

        // Open-generic: use MiddlewareConstraintAnalyzer to check where-constraints.
        // This correctly handles both unconstrained <TReq,TRes> (applies to all) and
        // domain-constrained <TReq,TRes> where TReq : IProductRequest<TRes>.
        return _constraintAnalyzer.CanApplyMiddleware(
            mw.Symbol,
            req.RequestSymbol,
            req.ResponseSymbol);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Model projection helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static MiddlewareModel ToAppliedMiddlewareModel(RawMiddleware mw, RawRequest req)
    {
        bool isOpenGeneric = mw.Symbol.IsGenericType;

        string? closedRequest  = isOpenGeneric ? Fq(req.RequestSymbol)          : mw.ClosedRequest  is null ? null : Fq(mw.ClosedRequest);
        string? closedResponse = isOpenGeneric ? req.ResponseSymbol is null ? null : Fq(req.ResponseSymbol) : mw.ClosedResponse is null ? null : Fq(mw.ClosedResponse);
        string? openBaseType   = isOpenGeneric ? GetOpenGenericBaseType(mw.Symbol) : null;

        return new MiddlewareModel(
            middlewareType:    Fq(mw.Symbol),
            order:             GetOrder(mw.Symbol),
            isOpenGeneric:     isOpenGeneric,
            isConditional:     mw.IsConditional,
            isStream:          mw.Kind == MiddlewareKind.Stream,
            isNotification:    mw.Kind == MiddlewareKind.Notification,
            isVoidCommand:     mw.Kind == MiddlewareKind.VoidCommand,
            closedRequestType: closedRequest,
            closedResponseType:closedResponse,
            openGenericBaseType: openBaseType);
    }

    /// <summary>Projects a <see cref="RawMiddleware"/> to the global <see cref="MiddlewareModel"/> list.</summary>
    private static MiddlewareModel ToMiddlewareModel(RawMiddleware mw)
    {
        return new MiddlewareModel(
            middlewareType:    Fq(mw.Symbol),
            order:             GetOrder(mw.Symbol),
            isOpenGeneric:     mw.Symbol.IsGenericType,
            isConditional:     mw.IsConditional,
            isStream:          mw.Kind == MiddlewareKind.Stream,
            isNotification:    mw.Kind == MiddlewareKind.Notification,
            isVoidCommand:     mw.Kind == MiddlewareKind.VoidCommand,
            closedRequestType: mw.ClosedRequest  is null ? null : Fq(mw.ClosedRequest),
            closedResponseType:mw.ClosedResponse is null ? null : Fq(mw.ClosedResponse),
            notificationConstraintType: mw.ConstraintType is null ? null : Fq(mw.ConstraintType),
            openGenericBaseType: mw.Symbol.IsGenericType ? GetOpenGenericBaseType(mw.Symbol) : null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Utility helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the interface matches the given prefix and has the expected number of type arguments.
    /// Uses the original definition to strip away concrete type arguments.
    /// </summary>
    /// <summary>
    /// Returns true if <paramref name="a"/> is a subtype of (derives from or implements) <paramref name="b"/>.
    /// Used to order notification types in the generated switch so derived types appear before base types.
    /// </summary>
    private static bool IsNotificationSubtypeOf(ITypeSymbol a, ITypeSymbol b)
    {
        // Use fully-qualified name comparison instead of SymbolEqualityComparer.Default.
        // When the same type is referenced via multiple assembly reference paths in a
        // multi-project compilation, SymbolEqualityComparer returns false for symbols
        // that are logically the same type.  String FQ comparison is unambiguous.
        var bFq = Fq(b);

        foreach (var iface in a.AllInterfaces)
        {
            if (Fq(iface) == bFq)
                return true;
        }
        var current = a.BaseType;
        while (current != null)
        {
            if (Fq(current) == bFq)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Merges covariant notification handler registrations into concrete notification entries.
    /// A handler implementing <c>INotificationHandler&lt;INotification&gt;</c> (or any interface/
    /// base-type notification) must also be invoked when a concrete subtype is published.
    /// This method merges such handlers into each concrete notification wrapper at compile time.
    /// </summary>
    private static void MergeCovariantNotificationHandlers(Dictionary<string, RawNotification> rawNotifications)
    {
        // Collect entries whose notification type is an interface or abstract class.
        var covariantEntries = rawNotifications.Values
            .Where(n => n.NotificationSymbol?.TypeKind == Microsoft.CodeAnalysis.TypeKind.Interface
                     || n.NotificationSymbol?.IsAbstract == true)
            .ToList();

        if (covariantEntries.Count == 0)
            return;

        // For every concrete (non-interface, non-abstract) notification entry,
        // check whether any covariant entry is a supertype, and if so merge its handlers.
        var concreteEntries = rawNotifications.Values
            .Where(n => n.NotificationSymbol?.TypeKind != Microsoft.CodeAnalysis.TypeKind.Interface
                     && n.NotificationSymbol?.IsAbstract != true
                     && n.NotificationSymbol != null)
            .ToList();

        foreach (var covariantEntry in covariantEntries)
        {
            if (covariantEntry.NotificationSymbol is null) continue;

            foreach (var concreteEntry in concreteEntries)
            {
                if (concreteEntry.NotificationSymbol is null) continue;

                if (IsNotificationSubtypeOf(concreteEntry.NotificationSymbol, covariantEntry.NotificationSymbol))
                {
                    foreach (var handlerType in covariantEntry.HandlerTypes)
                    {
                        if (!concreteEntry.HandlerTypes.Contains(handlerType, System.StringComparer.Ordinal))
                            concreteEntry.HandlerTypes.Add(handlerType);
                    }
                }
            }
        }
    }

    private static bool HasInterfacePrefix(INamedTypeSymbol iface, string prefix, int typeArgCount)
    {
        if (iface.TypeArguments.Length != typeArgCount)
            return false;

        var fqn = iface.OriginalDefinition.ToDisplayString();
        // The original definition display for e.g. IRequestHandler<TRequest, TResponse>
        // is "Blazing.Mediator.IRequestHandler<TRequest, TResponse>", so StartsWith works.
        return fqn.StartsWith(prefix);
    }

    private static bool HasAnyInterfacePrefix(INamedTypeSymbol typeSymbol, string prefix)
    {
        foreach (var iface in typeSymbol.AllInterfaces)
        {
            var fqn = iface.OriginalDefinition.ToDisplayString();
            if (fqn.StartsWith(prefix))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the execution order for a middleware type.
    /// Checks the <c>[Order(n)]</c> attribute first; if absent, falls back to reading
    /// a simple arrow-expression <c>public int Order =&gt; N;</c> property from source syntax.
    /// This supports both the attribute-based and interface-property-based ordering contracts.
    /// </summary>
    private static int GetOrder(INamedTypeSymbol symbol)
    {
        // Try [Order(n)] attribute first.
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass is null) continue;

            if (attrClass.ToDisplayString().StartsWith(OrderAttributeName))
            {
                if (attr.ConstructorArguments.Length > 0 &&
                    attr.ConstructorArguments[0].Value is int order)
                {
                    return order;
                }
            }
        }

        // Fallback: read 'public int Order => N;' arrow-bodied property from syntax.
        // Supports middleware that declares order via the IRequestMiddleware<,> interface
        // property rather than the [Order(n)] attribute.
        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not ClassDeclarationSyntax classDecl)
                continue;

            foreach (var member in classDecl.Members)
            {
                if (member is not PropertyDeclarationSyntax prop)
                    continue;

                if (prop.Identifier.Text != "Order")
                    continue;

                // Arrow-expression body: public int Order => 10;
                if (prop.ExpressionBody?.Expression is LiteralExpressionSyntax lit &&
                    lit.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression) &&
                    lit.Token.Value is int orderVal)
                {
                    return orderVal;
                }

                // Member-access expression: public int Order => int.MinValue; / int.MaxValue;
                if (prop.ExpressionBody?.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is PredefinedTypeSyntax { Keyword.Text: "int" })
                {
                    return memberAccess.Name.Identifier.Text switch
                    {
                        "MinValue" => int.MinValue,
                        "MaxValue" => int.MaxValue,
                        _          => 0,
                    };
                }
            }
        }

        // Walk base class chain for inherited Order property
        // (e.g. ValidationMiddleware<TRequest> inherits Order => 200 from ValidationMiddlewareBase<TRequest>).
        if (symbol.BaseType is { } baseType &&
            baseType.SpecialType != Microsoft.CodeAnalysis.SpecialType.System_Object)
        {
            return GetOrder(baseType);
        }

        return 0;
    }

    /// <summary>
    /// Returns true when the type is decorated with <c>[ExcludeFromAutoDiscovery]</c>,
    /// indicating it should be skipped by the source-generator auto-discovery pipeline.
    /// </summary>
    private static bool HasExcludeFromAutoDiscoveryAttribute(INamedTypeSymbol symbol)
    {
        foreach (var attr in symbol.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass is null) continue;
            var fqn = attrClass.ToDisplayString();
            if (fqn == "Blazing.Mediator.ExcludeFromAutoDiscoveryAttribute" ||
                fqn == "Blazing.Mediator.ExcludeFromAutoDiscovery")
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="typeSymbol"/> is accessible from the current
    /// compilation assembly.  Types defined in the current assembly are always included.
    /// Types from referenced assemblies are only included when they are <c>public</c> (or
    /// <c>internal</c> and the referenced assembly grants <c>InternalsVisibleTo</c> to the
    /// current assembly).  This prevents the source generator from emitting code that
    /// references <c>internal</c> types it cannot actually access, which would produce
    /// CS0122 "inaccessible due to its protection level" errors.
    /// </summary>
    private bool IsAccessibleFromCurrentAssembly(INamedTypeSymbol typeSymbol)
    {
        // Own-assembly types are always accessible.
        if (SymbolEqualityComparer.Default.Equals(typeSymbol.ContainingAssembly, _compilation.Assembly))
            return true;

        // Public types in referenced assemblies are always accessible.
        if (typeSymbol.DeclaredAccessibility == Accessibility.Public)
            return true;

        // Internal types are accessible only when the referenced assembly grants
        // InternalsVisibleTo access to the current assembly.
        if (typeSymbol.DeclaredAccessibility == Accessibility.Internal)
        {
            var currentAssemblyName = _compilation.Assembly.Name;
            foreach (var attr in typeSymbol.ContainingAssembly.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() != "System.Runtime.CompilerServices.InternalsVisibleToAttribute")
                    continue;
                if (attr.ConstructorArguments.Length > 0 &&
                    attr.ConstructorArguments[0].Value is string grantedTo)
                {
                    // Strip optional public-key suffix (e.g. "AssemblyName, PublicKey=…").
                    var grantedName = grantedTo.Contains(',')
                        ? grantedTo.Substring(0, grantedTo.IndexOf(',')).Trim()
                        : grantedTo.Trim();
                    if (string.Equals(grantedName, currentAssemblyName, System.StringComparison.Ordinal))
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the <c>global::</c>-prefixed class name of an open-generic type,
    /// WITHOUT type parameters. E.g. <c>global::MyApp.LoggingMiddleware</c> for
    /// <c>MyApp.LoggingMiddleware&lt;TReq, TRes&gt;</c>.
    /// Used by <c>MediatorCodeWriter</c> to construct closed-generic DI references.
    /// </summary>
    private static string GetOpenGenericBaseType(INamedTypeSymbol symbol)
    {
        // Walk up the ContainingType chain so nested types are fully qualified.
        // E.g. Outer.Inner<T,R> → "global::Ns.Outer.Inner", not "global::Ns.Inner".
        var parts = new System.Collections.Generic.Stack<string>();
        parts.Push(symbol.Name);

        var containingType = symbol.ContainingType;
        while (containingType != null)
        {
            parts.Push(containingType.Name);
            containingType = containingType.ContainingType;
        }

        var ns = symbol.ContainingNamespace;
        string nsPrefix = (ns == null || ns.IsGlobalNamespace)
            ? "global::"
            : $"global::{ns.ToDisplayString()}.";

        return nsPrefix + string.Join(".", parts);
    }

    /// <summary>
    /// Converts a fully qualified type name to an identifier-safe string by replacing
    /// dots, angle brackets, commas and spaces with underscores and stripping empty parts.
    /// E.g. "MyApp.Foo.PingRequest" → "MyApp_Foo_PingRequest".
    /// </summary>
    internal static string MakeSafeName(string fullyQualifiedName)
    {
        // Strip leading "global::" prefix so it doesn't appear in generated identifiers.
        var name = fullyQualifiedName.StartsWith("global::")
            ? fullyQualifiedName.Substring(8)
            : fullyQualifiedName;

        return name
            .Replace("::", "_")  // strip any remaining global:: inside generic type args
            .Replace('.', '_')
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace(',', '_')
            .Replace(' ', '_')
            .Trim('_');
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal scratch types (only live during a single Analyze() call)
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class RawRequest
    {
        public RawRequest(
            INamedTypeSymbol handlerSymbol,
            ITypeSymbol requestSymbol,
            ITypeSymbol? responseSymbol,
            bool isStream,
            bool isVoid,
            bool isQuery)
        {
            HandlerSymbol  = handlerSymbol;
            RequestSymbol  = requestSymbol;
            ResponseSymbol = responseSymbol;
            IsStream       = isStream;
            IsVoid         = isVoid;
            IsQuery        = isQuery;
        }

        public INamedTypeSymbol HandlerSymbol  { get; }
        public ITypeSymbol      RequestSymbol  { get; }
        public ITypeSymbol?     ResponseSymbol { get; }
        public bool             IsStream       { get; }
        public bool             IsVoid         { get; }
        public bool             IsQuery        { get; }
    }

    private sealed class RawNotification
    {
        public RawNotification(string notificationType, ITypeSymbol? notificationSymbol = null)
        {
            NotificationType   = notificationType;
            NotificationSymbol = notificationSymbol;
            HandlerTypes       = new List<string>();
        }

        public string        NotificationType   { get; }
        public ITypeSymbol?  NotificationSymbol { get; set; }
        public List<string>  HandlerTypes       { get; }
    }

    private enum MiddlewareKind { Request, VoidCommand, Stream, Notification }

    // ─────────────────────────────────────────────────────────────────────────
    // Cross-assembly discovery (multi-project solutions)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Walks all referenced assemblies that themselves reference <c>Blazing.Mediator</c>
    /// and discovers any handler/notification/middleware types they expose.
    /// Enables source-gen to work across multi-project solutions without any runtime reflection.
    /// </summary>
    private void DiscoverFromReferencedAssemblies(
        List<RawRequest> rawRequests,
        Dictionary<string, RawNotification> rawNotifications,
        List<RawMiddleware> rawMiddleware)
    {
        const string blazingMediatorLib = "Blazing.Mediator";

        // Track processed assemblies to avoid duplicate discovery when the same DLL
        // is both a direct reference and a transitive dependency of another reference.
        var processedAssemblies = new System.Collections.Generic.HashSet<string>(
            System.StringComparer.OrdinalIgnoreCase);

        foreach (var reference in _compilation.References)
        {
            if (_compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                continue;

            // Skip Blazing.Mediator library itself and the source generator assembly.
            if (assemblySymbol.Name == blazingMediatorLib ||
                assemblySymbol.Name.StartsWith("Blazing.Mediator.SourceGenerators", System.StringComparison.Ordinal))
                continue;

            // Deduplicate: skip if this assembly was already processed.
            if (!processedAssemblies.Add(assemblySymbol.Identity.GetDisplayName()))
                continue;

            // Only process assemblies that reference Blazing.Mediator (i.e. user assemblies).
            bool referencesBlazing = false;
            foreach (var module in assemblySymbol.Modules)
            {
                foreach (var referencedAssembly in module.ReferencedAssemblies)
                {
                    if (referencedAssembly.Name == blazingMediatorLib)
                    {
                        referencesBlazing = true;
                        break;
                    }
                }
                if (referencesBlazing) break;
            }

            if (!referencesBlazing) continue;

            WalkNamespace(assemblySymbol.GlobalNamespace, rawRequests, rawNotifications, rawMiddleware);
        }
    }

    /// <summary>Recursively walks a namespace symbol, collecting handler/notification/middleware types.</summary>
    private void WalkNamespace(
        INamespaceSymbol ns,
        List<RawRequest> rawRequests,
        Dictionary<string, RawNotification> rawNotifications,
        List<RawMiddleware> rawMiddleware)
    {
        foreach (var member in ns.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol childNs:
                    WalkNamespace(childNs, rawRequests, rawNotifications, rawMiddleware);
                    break;

                case INamedTypeSymbol typeSymbol
                    when !typeSymbol.IsAbstract &&
                         !HasExcludeFromAutoDiscoveryAttribute(typeSymbol) &&
                         IsAccessibleFromCurrentAssembly(typeSymbol):
                    // Open-generic types cannot be closed for handler DI, but support middleware.
                    if (!typeSymbol.IsGenericType)
                    {
                        TryCollectRequestHandler(typeSymbol, rawRequests);
                        TryCollectNotificationHandler(typeSymbol, rawNotifications);
                    }
                    TryCollectMiddleware(typeSymbol, rawMiddleware);
                    break;
            }
        }
    }

    private sealed class RawMiddleware
    {
        public RawMiddleware(
            INamedTypeSymbol symbol,
            MiddlewareKind kind,
            bool isConditional,
            ITypeSymbol? closedRequest,
            ITypeSymbol? closedResponse,
            ITypeSymbol? constraintType = null)
        {
            Symbol          = symbol;
            Kind            = kind;
            IsConditional   = isConditional;
            ClosedRequest   = closedRequest;
            ClosedResponse  = closedResponse;
            ConstraintType  = constraintType;
        }

        public INamedTypeSymbol Symbol         { get; }
        public MiddlewareKind   Kind           { get; }
        public bool             IsConditional  { get; }
        public ITypeSymbol?     ClosedRequest  { get; }
        public ITypeSymbol?     ClosedResponse { get; }
        /// <summary>For constrained notification middleware (INotificationMiddleware&lt;T&gt;), the constraint type T. Null otherwise.</summary>
        public ITypeSymbol?     ConstraintType { get; }
    }
}
