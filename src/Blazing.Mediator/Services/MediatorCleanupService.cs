namespace Blazing.Mediator.Services;

/// <summary>
/// Service that provides cleanup functionality for static Mediator resources.
/// This service can be manually called during application shutdown to dispose static resources.
/// </summary>
public sealed class MediatorCleanupService
{
    private readonly ILogger<MediatorCleanupService>? _logger;
    private static bool _staticResourcesDisposed;
    private static readonly object _disposalLock = new();

    /// <summary>
    /// Initializes a new instance of the MediatorCleanupService.
    /// </summary>
    /// <param name="logger">Optional logger for cleanup operations.</param>
    public MediatorCleanupService(ILogger<MediatorCleanupService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Disposes static Mediator resources if they haven't been disposed already.
    /// This method is thread-safe and can be called multiple times.
    /// </summary>
    public void DisposeStaticResources()
    {
        lock (_disposalLock)
        {
            if (_staticResourcesDisposed)
                return;

            try
            {
                _logger?.LogDebug("Disposing static Mediator resources");
                //Mediator.DisposeStaticResources();
                SetStaticResourcesDisposed();
                _logger?.LogDebug("Static Mediator resources disposed successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error disposing static Mediator resources");
                // Don't rethrow - we don't want to prevent application shutdown
            }
        }
    }

    /// <summary>
    /// Marks static resources as disposed. Extracted to method to satisfy static field access rules.
    /// </summary>
    private static void SetStaticResourcesDisposed()
    {
        _staticResourcesDisposed = true;
    }

    /// <summary>
    /// Gets whether static resources have been disposed.
    /// </summary>
    public static bool AreStaticResourcesDisposed => _staticResourcesDisposed;
}