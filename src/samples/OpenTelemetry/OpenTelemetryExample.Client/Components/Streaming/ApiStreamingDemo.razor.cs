using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenTelemetryExample.Shared.Models;
using System.Net.Http.Json;

namespace OpenTelemetryExample.Client.Components.Streaming;

/// <summary>
/// Component for demonstrating API streaming functionality with real-time metrics and telemetry.
/// Provides user controls for configuring streaming parameters and displays live results.
/// </summary>
public partial class ApiStreamingDemo : ComponentBase
{
    // Self-contained component state - no parameters needed for these
    private int count = 50;
    private int delay = 100;
    private bool isStreaming;
    private int itemsReceived;
    private long duration;
    private double throughput;
    private readonly List<StreamResponseDto<string>> results = [];
    private CancellationTokenSource? cancellationTokenSource;
    private DateTime startTime;

    [Parameter, EditorRequired] public HttpClient Http { get; set; } = null!;

    [Parameter, EditorRequired] public IJSRuntime JSRuntimeService { get; set; } = null!;

    // Expose properties for the Razor template

    /// <summary>
    /// Gets or sets the number of items to stream.
    /// </summary>
    protected int Count
    {
        get => count;
        set => count = value;
    }

    /// <summary>
    /// Gets or sets the delay between streaming items in milliseconds.
    /// </summary>
    protected int Delay
    {
        get => delay;
        set => delay = value;
    }

    /// <summary>
    /// Gets a value indicating whether streaming is currently active.
    /// </summary>
    protected bool IsStreaming => isStreaming;

    /// <summary>
    /// Gets the total number of items received so far.
    /// </summary>
    protected int ItemsReceived => itemsReceived;

    /// <summary>
    /// Gets the total duration of the streaming session in milliseconds.
    /// </summary>
    protected long Duration => duration;

    /// <summary>
    /// Gets the current throughput in items per second.
    /// </summary>
    protected double Throughput => throughput;

    /// <summary>
    /// Gets the collection of received streaming results.
    /// </summary>
    protected List<StreamResponseDto<string>> Results => results;

    /// <summary>
    /// Initiates the API streaming process with current configuration parameters.
    /// </summary>
    private async Task StartStreaming()
    {
        if (isStreaming) return;

        // Reset state for new streaming session
        isStreaming = true;
        itemsReceived = 0;
        duration = 0;
        throughput = 0;
        results.Clear();
        cancellationTokenSource = new CancellationTokenSource();
        startTime = DateTime.Now;

        StateHasChanged();

        try
        {
            var url = $"/api/streaming/stream-data?Count={count}&DelayMs={delay}";

            await foreach (var item in Http.GetFromJsonAsAsyncEnumerable<StreamResponseDto<string>>(
                url, cancellationTokenSource.Token))
            {
                if (item != null)
                {
                    await AddItem(item);
                }
                StateHasChanged();
                await Task.Yield();
            }
        }
        catch (OperationCanceledException)
        {
            // Streaming was cancelled by user or system
        }
        catch (Exception ex)
        {
            await JSRuntimeService.InvokeVoidAsync("console.error", "API Streaming Error", ex.Message);
        }
        finally
        {
            isStreaming = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Stops the current streaming operation and cleans up resources.
    /// </summary>
    private Task StopStreaming()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;

        isStreaming = false;
        StateHasChanged();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Processes a received streaming item and updates metrics.
    /// </summary>
    /// <param name="item">The streaming item to process.</param>
    private Task AddItem(StreamResponseDto<string> item)
    {
        results.Add(item);
        itemsReceived++;

        // Calculate metrics in real-time
        var elapsed = DateTime.Now - startTime;
        duration = (long)elapsed.TotalMilliseconds;
        throughput = itemsReceived / Math.Max(elapsed.TotalSeconds, 0.001);

        // Update UI frequently for real-time feel (every 2 items or first 5)
        if (itemsReceived % 2 == 0 || itemsReceived <= 5)
        {
            StateHasChanged();
        }

        return Task.CompletedTask;
    }
}
