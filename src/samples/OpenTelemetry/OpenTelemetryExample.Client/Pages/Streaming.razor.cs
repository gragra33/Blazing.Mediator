namespace OpenTelemetryExample.Client.Pages;

public partial class Streaming : IAsyncDisposable
{
    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        // No resources to dispose in the simplified version
        // Components handle their own disposal
        await Task.CompletedTask;
    }

    #endregion
}