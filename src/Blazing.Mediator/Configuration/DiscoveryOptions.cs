namespace Blazing.Mediator.Configuration;

/// <summary>
/// Options for controlling the discovery of middleware and handlers in the Blazing.Mediator library.
/// </summary>
public class DiscoveryOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatically discover middleware.
    /// </summary>
    public bool DiscoverMiddleware { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically discover notification middleware.
    /// </summary>
    public bool DiscoverNotificationMiddleware { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically discover constrained middleware.
    /// </summary>
    public bool DiscoverConstrainedMiddleware { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically discover notification handlers.
    /// </summary>
    public bool DiscoverNotificationHandlers { get; set; } = true;
}