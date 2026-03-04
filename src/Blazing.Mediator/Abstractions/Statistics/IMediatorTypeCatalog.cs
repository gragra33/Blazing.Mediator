namespace Blazing.Mediator.Statistics;

/// <summary>
/// Classifies a mediator request/handler into CQRS categories.
/// Emitted by <c>MediatorCodeWriter</c> at compile time — zero runtime reflection.
/// </summary>
public enum CqrsCategory
{
    /// <summary>The category could not be determined.</summary>
    Unknown,

    /// <summary>A query that returns data without side-effects.</summary>
    Query,

    /// <summary>A command that performs a write / mutation operation.</summary>
    Command,

    /// <summary>A domain notification (fan-out to zero or more handlers).</summary>
    Notification,

    /// <summary>A streaming request that yields multiple results via <c>IAsyncEnumerable&lt;T&gt;</c>.</summary>
    Stream,
}

/// <summary>
/// Describes a single request/command/query handler discovered by the source generator.
/// Populated at compile time — all types are embedded as <c>typeof(…)</c> literals.
/// </summary>
/// <param name="RequestName">The short name of the request type (e.g. <c>"GetOrderQuery"</c>).</param>
/// <param name="RequestType">The CLR type of the request.</param>
/// <param name="ResponseType">The CLR type of the response, or <see langword="null"/> for void handlers.</param>
/// <param name="HandlerType">The CLR type of the handler implementation.</param>
/// <param name="Category">The CQRS category inferred by the source generator.</param>
/// <param name="Section">An optional grouping label used for statistics rendering (e.g. namespace segment).</param>
public sealed record HandlerTypeInfo(
    string RequestName,
    Type RequestType,
    Type? ResponseType,
    Type HandlerType,
    CqrsCategory Category,
    string? Section = null);

/// <summary>
/// Describes a single notification type and all handlers registered with it by the source generator.
/// Populated at compile time — all types are embedded as <c>typeof(…)</c> literals.
/// </summary>
/// <param name="NotificationName">The short name of the notification type.</param>
/// <param name="NotificationType">The CLR type of the notification.</param>
/// <param name="HandlerTypes">All handler types registered for this notification.</param>
public sealed record NotificationTypeInfo(
    string NotificationName,
    Type NotificationType,
    IReadOnlyList<Type> HandlerTypes);

/// <summary>
/// Describes a single middleware type discovered by the source generator.
/// Populated at compile time — all types are embedded as <c>typeof(…)</c> literals.
/// </summary>
/// <param name="Name">The short class name of the middleware (e.g. <c>"ValidationMiddleware"</c>).</param>
/// <param name="MiddlewareType">The CLR type of the middleware implementation.</param>
/// <param name="Order">The registered execution order value.</param>
/// <param name="IsOpenGeneric">
/// <see langword="true"/> when the middleware type is open-generic (e.g.
/// <c>LoggingMiddleware&lt;&gt;</c>); the <see cref="MiddlewareType"/> will be the open
/// form in that case.
/// </param>
/// <param name="IsNotification">
/// <see langword="true"/> for notification-pipeline middleware;
/// <see langword="false"/> for request-pipeline middleware.
/// </param>
public sealed record MiddlewareTypeInfo(
    string Name,
    Type MiddlewareType,
    int Order,
    bool IsOpenGeneric,
    bool IsNotification);

/// <summary>
/// A compile-time catalog of every handler and notification type discovered by
/// <c>Blazing.Mediator.SourceGenerators</c>.
/// <para>
/// Implement this interface by consuming the generated <c>MediatorTypeCatalog</c> class, or
/// pass it to analysis overloads such as
/// <c>MediatorStatistics.AnalyzeQueries(IMediatorTypeCatalog)</c> for fully AOT-clean statistics.
/// The reflection-based <c>AnalyzeQueries(IServiceProvider)</c> overload is annotated with
/// <c>[RequiresUnreferencedCode]</c> for non-source-gen scenarios.
/// </para>
/// </summary>
public interface IMediatorTypeCatalog
{
    /// <summary>
    /// All request, command, query, and stream handlers discovered at compile time.
    /// </summary>
    IReadOnlyList<HandlerTypeInfo> RequestHandlers { get; }

    /// <summary>
    /// All notification types and their compile-time-registered handler types.
    /// </summary>
    IReadOnlyList<NotificationTypeInfo> NotificationHandlers { get; }

    /// <summary>
    /// All request-pipeline middleware discovered at compile time.
    /// Returns an empty list when no source generator implements this member
    /// (default interface implementation — non-breaking for existing implementors).
    /// </summary>
    IReadOnlyList<MiddlewareTypeInfo> RequestMiddleware => [];

    /// <summary>
    /// All notification-pipeline middleware discovered at compile time.
    /// Returns an empty list when no source generator implements this member
    /// (default interface implementation — non-breaking for existing implementors).
    /// </summary>
    IReadOnlyList<MiddlewareTypeInfo> NotificationMiddleware => [];
}
