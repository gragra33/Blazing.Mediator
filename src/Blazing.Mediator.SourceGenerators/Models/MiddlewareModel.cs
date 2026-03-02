namespace Blazing.Mediator.SourceGenerators.Models;

/// <summary>
/// Describes a single middleware implementation discovered in the compilation.
/// </summary>
internal sealed class MiddlewareModel
{
    public MiddlewareModel(
        string middlewareType,
        int order,
        bool isOpenGeneric,
        bool isConditional,
        bool isStream,
        bool isNotification,
        bool isVoidCommand,
        string? closedRequestType,
        string? closedResponseType,
        string? openGenericBaseType = null,
        string? notificationConstraintType = null)
    {
        MiddlewareType = middlewareType;
        Order = order;
        IsOpenGeneric = isOpenGeneric;
        IsConditional = isConditional;
        IsStream = isStream;
        IsNotification = isNotification;
        IsVoidCommand = isVoidCommand;
        ClosedRequestType = closedRequestType;
        ClosedResponseType = closedResponseType;
        OpenGenericBaseType = openGenericBaseType;
        NotificationConstraintType = notificationConstraintType;
    }

    /// <summary>Fully qualified middleware type name.</summary>
    public string MiddlewareType { get; }

    /// <summary>Execution order (lower runs first). Sourced from the [Order] attribute or the <c>Order</c> property/field.</summary>
    public int Order { get; }

    /// <summary>
    /// True when the middleware implements an open-generic interface
    /// (<c>IRequestMiddleware&lt;TReq, TRes&gt;</c>) and therefore applies to every request type.
    /// </summary>
    public bool IsOpenGeneric { get; }

    /// <summary>True when the middleware additionally implements <c>IConditionalMiddleware</c>.</summary>
    public bool IsConditional { get; }

    /// <summary>True when this is a stream middleware (<c>IStreamRequestMiddleware&lt;,&gt;</c>).</summary>
    public bool IsStream { get; }

    /// <summary>True when this is a notification middleware (<c>INotificationMiddleware</c>).</summary>
    public bool IsNotification { get; }

    /// <summary>
    /// True when this middleware implements <c>IRequestMiddleware&lt;TRequest&gt;</c> (single type-arg, void-command variant).
    /// False when it implements <c>IRequestMiddleware&lt;TRequest, TResponse&gt;</c> (two type-args, response variant).
    /// </summary>
    public bool IsVoidCommand { get; }

    /// <summary>
    /// Fully qualified request type when this is a closed-generic middleware
    /// (<c>IRequestMiddleware&lt;PingRequest, PingResponse&gt;</c>). Null for open-generic.
    /// </summary>
    public string? ClosedRequestType { get; }

    /// <summary>
    /// Fully qualified response type when this is a closed-generic middleware. Null for open-generic.
    /// </summary>
    public string? ClosedResponseType { get; }

    /// <summary>
    /// For open-generic middleware, the namespace-qualified class name WITHOUT type parameters
    /// (e.g. "MyApp.LoggingMiddleware" for "MyApp.LoggingMiddleware&lt;TReq, TRes&gt;").
    /// Used by <c>MediatorCodeWriter</c> to construct closed-generic type references.
    /// Null for concrete (non-open-generic) middleware.
    /// </summary>
    public string? OpenGenericBaseType { get; }

    /// <summary>
    /// For constrained notification middleware (<c>INotificationMiddleware&lt;T&gt;</c>),
    /// the fully qualified constraint type name (e.g. "global::MyApp.IOrderNotification").
    /// Null for non-constrained or non-notification middleware.
    /// Used by <c>MediatorCodeWriter</c> to only apply constrained middleware to compatible notification types.
    /// </summary>
    public string? NotificationConstraintType { get; }
}
