namespace Blazing.Mediator.Pipeline;

/// <summary>
/// Shared record structure for middleware information across both request and notification pipelines.
/// Optimized for performance with built-in caching capabilities.
/// </summary>
public sealed record MiddlewareInfo(
    Type Type,
    int Order,
    object? Configuration = null,
    // When true the Order value was supplied at registration time (e.g. by the source generator)
    // and must not be re-derived at runtime via IL analysis or instance creation.
    bool IsOrderKnown = false,
    // Performance optimization fields for runtime caching
    string? CachedTypeName = null,
    bool? IsGenericTypeDefinition = null,
    Type[]? CachedInterfaces = null)
{
    // Performance-optimized properties with lazy evaluation
    public string CleanTypeName => CachedTypeName ?? PipelineUtilities.GetCleanTypeName(Type);
    public bool IsGeneric => IsGenericTypeDefinition ?? Type.IsGenericTypeDefinition;
    public Type[] Interfaces => CachedInterfaces ?? Type.GetInterfaces();

    /// <summary>
    /// Pre-caching method for registration-time optimization.
    /// Populates all cached fields to minimize runtime reflection calls.
    /// </summary>
    /// <returns>A new MiddlewareInfo instance with populated cache fields.</returns>
    public MiddlewareInfo WithCache() => this with
    {
        CachedTypeName = PipelineUtilities.GetCleanTypeName(Type),
        IsGenericTypeDefinition = Type.IsGenericTypeDefinition,
        CachedInterfaces = Type.GetInterfaces()
    };
}