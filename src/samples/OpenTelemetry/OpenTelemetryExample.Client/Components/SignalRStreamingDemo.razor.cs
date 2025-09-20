using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Client.Components;

/// <summary>
/// Component for demonstrating SignalR streaming functionality with real-time metrics and telemetry.
/// Provides connection management, streaming controls, and live result display.
/// </summary>
public partial class SignalRStreamingDemo : ComponentBase
{
    // Self-contained component state - no parameters needed for these
    private int count = 50;
    private int delay = 100;
    private int batchSize = 5;
    private bool isStreaming;
    private int itemsReceived;
    private long duration;
    private double throughput;
    private readonly List<StreamResponseDto<string>> results = new();
    private DateTime startTime;
    private HubConnection? hubConnection;
    private bool wasManuallyDisconnected;

    [Parameter, EditorRequired] public NavigationManager Navigation { get; set; } = null!;

    [Parameter, EditorRequired] public IJSRuntime JSRuntimeService { get; set; } = null!;

    [Parameter, EditorRequired] public HttpClient Http { get; set; } = null!;

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
    /// Gets or sets the batch size for streaming operations.
    /// </summary>
    protected int BatchSize 
    { 
        get => batchSize; 
        set => batchSize = value; 
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
    /// Gets the current SignalR hub connection.
    /// </summary>
    protected HubConnection? HubConnection => hubConnection;

    /// <summary>
    /// Disposes of the SignalR connection when the component is disposed.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (hubConnection != null)
        {
            await hubConnection.DisposeAsync();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the SignalR connection is established.
    /// </summary>
    private bool IsConnected => hubConnection?.State == HubConnectionState.Connected;

    /// <summary>
    /// Establishes a connection to the SignalR hub and configures event handlers.
    /// </summary>
    private async Task ConnectSignalR()
    {
        if (hubConnection?.State == HubConnectionState.Connected) return;

        wasManuallyDisconnected = false; // Reset disconnect flag when connecting

        // Use the same base URL as the HttpClient to ensure we connect to the API server
        var apiBaseUrl = Http.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:64688";
        var hubUrl = $"{apiBaseUrl}/streaming-hub";

        hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<StreamResponseDto<string>>("ReceiveStreamItem", async (item) =>
        {
            await AddSignalRItem(item);
        });

        hubConnection.On<object>("StreamCompleted", async (result) =>
        {
            isStreaming = false;
            await InvokeAsync(StateHasChanged);
        });

        hubConnection.On<string>("StreamError", async (error) =>
        {
            await JSRuntimeService.InvokeVoidAsync("console.error", "SignalR Streaming Error", error);
            isStreaming = false;
            await InvokeAsync(StateHasChanged);
        });

        try
        {
            await hubConnection.StartAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            await JSRuntimeService.InvokeVoidAsync("console.error", "SignalR Connection Error", ex.Message);
        }
    }

    /// <summary>
    /// Disconnects from the SignalR hub and cleans up resources.
    /// </summary>
    private async Task DisconnectSignalR()
    {
        if (hubConnection != null)
        {
            wasManuallyDisconnected = true; // Mark as manually disconnected
            await hubConnection.DisposeAsync();
            hubConnection = null;
            
            isStreaming = false;
            
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Initiates SignalR streaming with the current configuration parameters.
    /// </summary>
    private async Task StartStreaming()
    {
        if (!IsConnected || isStreaming) return;

        // Reset state for new streaming session - only metrics, preserve user input
        isStreaming = true;
        itemsReceived = 0;
        duration = 0;
        throughput = 0;
        results.Clear();
        startTime = DateTime.Now;

        StateHasChanged();

        try
        {
            var request = new
            {   // Use current user values
                Count = count,
                DelayMs = delay,
                BatchSize = batchSize
            };

            await hubConnection!.SendAsync("StartStreaming", request);
        }
        catch (Exception ex)
        {
            await JSRuntimeService.InvokeVoidAsync("console.error", "SignalR Streaming Error", ex.Message);
            
            isStreaming = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Stops the current SignalR streaming operation.
    /// </summary>
    private async Task StopStreaming()
    {
        if (hubConnection != null && IsConnected)
        {
            try
            {
                await hubConnection.SendAsync("StopStreaming");
            }
            catch (Exception)
            {
                // Log error but continue with cleanup
            }
        }
        
        isStreaming = false;
        StateHasChanged();
    }

    /// <summary>
    /// Processes a received SignalR streaming item and updates metrics.
    /// </summary>
    /// <param name="item">The streaming item received from SignalR.</param>
    private async Task AddSignalRItem(StreamResponseDto<string> item)
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
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Gets the CSS class for the connection status display.
    /// </summary>
    private string GetConnectionStatusClass() => hubConnection?.State switch
    {
        HubConnectionState.Connected => "alert-success",
        HubConnectionState.Connecting => "alert-warning",
        HubConnectionState.Disconnected => "alert-secondary",
        HubConnectionState.Reconnecting => "alert-info",
        _ => wasManuallyDisconnected ? "alert-secondary" : "alert-secondary"
    };

    /// <summary>
    /// Gets the icon name for the connection status display.
    /// </summary>
    private string GetConnectionIcon() => hubConnection?.State switch
    {
        HubConnectionState.Connected => "check-circle",
        HubConnectionState.Connecting => "arrow-clockwise",
        HubConnectionState.Disconnected => "x-circle",
        HubConnectionState.Reconnecting => "arrow-repeat",
        _ => wasManuallyDisconnected ? "x-circle" : "question-circle"
    };

    /// <summary>
    /// Gets the text description of the current connection status.
    /// </summary>
    private string GetConnectionStatus() => hubConnection?.State switch
    {
        HubConnectionState.Connected => "Connected",
        HubConnectionState.Connecting => "Connecting...",
        HubConnectionState.Disconnected => "Disconnected",
        HubConnectionState.Reconnecting => "Reconnecting...",
        _ => wasManuallyDisconnected ? "Disconnected" : "Unknown"
    };
}