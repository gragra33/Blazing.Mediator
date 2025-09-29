namespace Blazing.Mediator.Services;

/// <summary>
/// Service that provides cleanup functionality for static Mediator resources.
/// This service can be manually called during application shutdown to dispose static resources.
/// </summary>
public sealed partial class CleanupService
{
    private readonly MediatorLogger? _mediatorLogger;
    private static bool _staticResourcesDisposed;
    private static readonly Lock _disposalLock = new();

    /// <summary>
    /// Initializes a new instance of the MediatorCleanupService.
    /// </summary>
    /// <param name="mediatorLogger">Optional MediatorLogger for cleanup operations.</param>
    public CleanupService(MediatorLogger? mediatorLogger = null)
    {
        _mediatorLogger = mediatorLogger;
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
                LogCleanupStarted();
                //Mediator.DisposeStaticResources();
                SetStaticResourcesDisposed();
                LogCleanupCompleted();
            }
            catch (Exception ex)
            {
                LogCleanupError(ex);
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

    #region Source Generated Logging Methods

    private void LogCleanupStarted()
    {
        _mediatorLogger?.CleanupServiceStarted();
    }

    private void LogCleanupCompleted()
    {
        _mediatorLogger?.CleanupServiceCompleted();
    }

    private void LogCleanupError(Exception ex)
    {
        _mediatorLogger?.CleanupServiceError(ex);
    }

    #endregion
}