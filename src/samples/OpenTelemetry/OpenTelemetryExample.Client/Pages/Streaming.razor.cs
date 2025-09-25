namespace OpenTelemetryExample.Client.Pages;

public partial class Streaming : IAsyncDisposable
{
    #region IAsyncDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        // No resources to dispose in the simplified version
        // Components handle their own disposal
        await Task.CompletedTask;
    }

    #endregion
}