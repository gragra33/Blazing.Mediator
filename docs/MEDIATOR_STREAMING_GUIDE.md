# Blazing.Mediator - Streaming Implementation Guide

## Overview

**Blazing.Mediator** provides powerful streaming capabilities through `IAsyncEnumerable<T>`, enabling real-time data processing with minimal memory overhead. The streaming implementation allows you to process large datasets efficiently, send data to clients as it becomes available, and build responsive real-time applications.

Streaming in **Blazing.Mediator** is built on top of the standard CQRS pattern with specialised interfaces for stream requests and handlers, supporting middleware pipelines and maintaining the same clean architecture principles.

### Key Streaming Features

-   **🌊 Memory-Efficient Streaming**: Process large datasets without loading everything into memory using `IAsyncEnumerable<T>`
-   **⚡ Real-Time Data Flow**: Send data to clients as soon as it's available, reducing perceived latency
-   **🔧 Middleware Pipeline Support**: Apply cross-cutting concerns like logging, monitoring, and filtering to streaming requests
-   **🤖 Auto-Discovery**: Optional automatic streaming middleware discovery and registration from assemblies
-   **🎯 Multiple Streaming Patterns**: Support for asynchronous send/receive
-   **🚀 High Performance**: Optimised for throughput with minimal overhead and efficient resource utilisation
-   **🧪 Fully Testable**: Easy to unit test streaming handlers and middleware
-   **🔒 Type Safety**: Compile-time type checking for stream requests, handlers, and responses

## Table of Contents

1. [Quick Start](#quick-start)
2. [Core Streaming Concepts](#core-streaming-concepts)
3. [Creating Stream Requests](#creating-stream-requests)
4. [Implementing Stream Handlers](#implementing-stream-handlers)
5. [Streaming Middleware](#streaming-middleware)
6. [API Endpoints with Streaming](#api-endpoints-with-streaming)
7. [Blazor Streaming Scenarios](#blazor-streaming-scenarios)
8. [Performance Considerations](#performance-considerations)
9. [Testing Streaming](#testing-streaming)
10. [Best Practices](#best-practices)
11. [Sample Projects](#sample-projects)
12. [Complete Examples](#complete-examples)

## Quick Start

Get streaming up and running with **Blazing.Mediator** in minutes:

### 1. Install the Package

```bash
dotnet add package Blazing.Mediator
```

### 2. Create Your First Stream Request

```csharp
// Stream Request
public class StreamContactsRequest : IStreamRequest<ContactDto>
{
    public string? SearchTerm { get; set; }
    public int BatchSize { get; set; } = 100;
}

// Stream Handler
public class StreamContactsHandler : IStreamRequestHandler<StreamContactsRequest, ContactDto>
{
    private readonly IContactService _contactService;

    public StreamContactsHandler(IContactService contactService)
    {
        _contactService = contactService;
    }

    public async IAsyncEnumerable<ContactDto> Handle(
        StreamContactsRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var contact in _contactService.StreamContactsAsync(request.SearchTerm, cancellationToken))
        {
            yield return contact;
        }
    }
}
```

### 3. Register Services with Streaming Support

```csharp
// Program.cs - Basic registration with auto-discovery
builder.Services.AddMediator(typeof(Program).Assembly);

// With auto-discovery for all middleware (including streaming middleware)
builder.Services.AddMediator(discoverMiddleware: true, typeof(Program).Assembly);

// Manual configuration with specific streaming middleware
builder.Services.AddMediator(config =>
{
    // Optional: Add streaming-specific middleware manually
    config.AddMiddleware(typeof(StreamingLoggingMiddleware<,>));
}, typeof(Program));

// Best approach: Auto-discovery with optional manual configuration
builder.Services.AddMediator(config =>
{
    // Any additional manual middleware configuration
    config.AddMiddleware(typeof(CustomStreamingMiddleware<,>));
}, discoverMiddleware: true, typeof(Program).Assembly);
```

### 4. Use in API Endpoints

```csharp
// Minimal API with JSON streaming
app.MapGet("/api/contacts/stream", (IMediator mediator, string? search, CancellationToken ct) =>
{
    var request = new StreamContactsRequest { SearchTerm = search };

    return Results.Stream(async stream =>
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await JsonSerializer.SerializeAsync(stream, mediator.SendStream(request, ct), options, ct);
    }, contentType: "application/json");
});
```

### 5. Use in Blazor Components

```razor
@inject IMediator Mediator

<h3>Real-Time Contact Stream</h3>

@if (contacts.Any())
{
    <table>
        @foreach (var contact in contacts)
        {
            <tr>
                <td>@contact.Name</td>
                <td>@contact.Email</td>
            </tr>
        }
    </table>
}

@code {
    private List<ContactDto> contacts = new();
    private CancellationTokenSource? cancellationTokenSource;

    protected override async Task OnInitializedAsync()
    {
        cancellationTokenSource = new CancellationTokenSource();
        var request = new StreamContactsRequest();

        await foreach (var contact in Mediator.SendStream(request, cancellationTokenSource.Token))
        {
            contacts.Add(contact);
            await Task.Yield(); // Yield control to prevent UI blocking
            await InvokeAsync(StateHasChanged); // Trigger UI update for each new contact
        }
    }

    public void Dispose()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }
}
```

## Core Streaming Concepts

### Stream Request Interface

The `IStreamRequest<TResponse>` interface marks a request as streamable, extending the base `IRequest<TResponse>`:

```csharp
public interface IStreamRequest<TResponse> : IRequest<TResponse>
{
}
```

### Stream Request Handler

The `IStreamRequestHandler<TRequest, TResponse>` interface defines handlers that return `IAsyncEnumerable<TResponse>`:

```csharp
public interface IStreamRequestHandler<in TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}
```

### Mediator Streaming Method

The `IMediator` interface includes the `SendStream` method for executing stream requests:

```csharp
public interface IMediator
{
    // Regular request methods...

    IAsyncEnumerable<TResponse> SendStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
```

## Creating Stream Requests

Stream requests follow the same patterns as regular requests but implement `IStreamRequest<T>`:

### Basic Stream Request

```csharp
public class StreamProductsRequest : IStreamRequest<ProductDto>
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int BatchSize { get; set; } = 50;
}
```

### Stream Request with Metadata

```csharp
public class StreamOrdersWithMetadataRequest : IStreamRequest<StreamResponse<OrderDto>>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
}

public class StreamResponse<T>
{
    public T Data { get; set; } = default!;
    public StreamMetadata Metadata { get; set; } = new();
}

public class StreamMetadata
{
    public int ItemNumber { get; set; }
    public int TotalEstimated { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
}
```

## Implementing Stream Handlers

Stream handlers use `IAsyncEnumerable<T>` and `yield return` for memory-efficient streaming:

### Basic Stream Handler

```csharp
public class StreamProductsHandler : IStreamRequestHandler<StreamProductsRequest, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly ILogger<StreamProductsHandler> _logger;

    public StreamProductsHandler(IProductRepository repository, ILogger<StreamProductsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async IAsyncEnumerable<ProductDto> Handle(
        StreamProductsRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting product stream for category: {Category}", request.Category);

        var query = _repository.GetProductsQuery()
            .Where(p => request.Category == null || p.Category == request.Category);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice);

        await foreach (var product in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Category = product.Category
            };
        }

        _logger.LogInformation("Completed product stream");
    }
}
```

### Stream Handler with Batching

```csharp
public class BatchedStreamHandler : IStreamRequestHandler<StreamOrdersRequest, OrderBatch>
{
    private readonly IOrderService _orderService;

    public BatchedStreamHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async IAsyncEnumerable<OrderBatch> Handle(
        StreamOrdersRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<OrderDto>();
        var batchNumber = 1;

        await foreach (var order in _orderService.StreamOrdersAsync(cancellationToken))
        {
            batch.Add(order);

            if (batch.Count >= request.BatchSize)
            {
                yield return new OrderBatch
                {
                    Orders = batch.ToArray(),
                    BatchNumber = batchNumber++,
                    Timestamp = DateTime.UtcNow
                };

                batch.Clear();
            }
        }

        // Yield remaining items in final batch
        if (batch.Count > 0)
        {
            yield return new OrderBatch
            {
                Orders = batch.ToArray(),
                BatchNumber = batchNumber,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
```

## Streaming Middleware

Streaming middleware implements `IStreamRequestMiddleware<TRequest, TResponse>` to add cross-cutting concerns like logging, monitoring, and filtering to streaming requests.

### Auto-Registration of Streaming Middleware

**Blazing.Mediator** can automatically discover and register streaming middleware from your assemblies:

```csharp
// Auto-register all middleware (including streaming middleware)
builder.Services.AddMediator(discoverMiddleware: true, typeof(Program).Assembly);

// Or with additional manual configuration
builder.Services.AddMediator(config =>
{
    // Manual middleware registration for fine-grained control
    config.AddMiddleware(typeof(CustomStreamingMiddleware<,>));
}, discoverMiddleware: true, typeof(Program).Assembly);
```

When `discoverMiddleware: true` is enabled, **Blazing.Mediator** automatically scans the specified assemblies for all types implementing:

-   `IRequestMiddleware<T>` - Single-parameter middleware
-   `IRequestMiddleware<T, TResponse>` - Request/response middleware
-   `IConditionalMiddleware<T>` - Conditional single-parameter middleware
-   `IConditionalMiddleware<T, TResponse>` - Conditional request/response middleware
-   `IStreamRequestMiddleware<TRequest, TResponse>` - **Streaming middleware** ⭐

The middleware will be registered with their defined `Order` property for proper execution sequence.

### Middleware Registration Approaches

1. **Auto-Discovery (Recommended)**: Let **Blazing.Mediator** automatically find and register all middleware
2. **Manual Registration**: Explicitly register specific middleware types
3. **Hybrid Approach**: Use auto-discovery with additional manual registrations

```csharp
// 1. Pure auto-discovery
builder.Services.AddMediator(discoverMiddleware: true, typeof(Program).Assembly);

// 2. Manual registration only
builder.Services.AddMediator(config =>
{
    config.AddMiddleware(typeof(StreamingLoggingMiddleware<,>));
    config.AddMiddleware(typeof(StreamingPerformanceMiddleware<,>));
}, typeof(Program).Assembly);

// 3. Hybrid: Auto-discovery + manual additions
builder.Services.AddMediator(config =>
{
    // Auto-discovery will find most middleware
    // Add any specific middleware that needs custom configuration
    config.AddMiddleware(typeof(CustomStreamingMiddleware<,>));
}, discoverMiddleware: true, typeof(Program).Assembly);
```

### Basic Streaming Middleware

```csharp
public class StreamingLoggingMiddleware<TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly ILogger<StreamingLoggingMiddleware<TRequest, TResponse>> _logger;

    public StreamingLoggingMiddleware(ILogger<StreamingLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => 0; // Execute first

    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;
        var itemCount = 0;

        _logger.LogInformation("🚀 Starting stream: {RequestType}", requestType);

        try
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                itemCount++;

                // Log progress every 100 items
                if (itemCount % 100 == 0)
                {
                    _logger.LogInformation("📦 Streamed {ItemCount} items", itemCount);
                }

                yield return item;
            }
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("✅ Completed stream: {RequestType}, Items: {ItemCount}, Duration: {Duration}ms",
                requestType, itemCount, duration.TotalMilliseconds);
        }
    }
}
```

### Performance Monitoring Middleware

```csharp
public class StreamingPerformanceMiddleware<TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly IMetrics _metrics;

    public StreamingPerformanceMiddleware(IMetrics metrics)
    {
        _metrics = metrics;
    }

    public int Order => -10; // Execute early

    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var itemCount = 0;
        var requestType = typeof(TRequest).Name;

        try
        {
            await foreach (var item in next().WithCancellation(cancellationToken))
            {
                itemCount++;
                _metrics.IncrementCounter($"stream.{requestType}.items");

                yield return item;
            }
        }
        finally
        {
            stopwatch.Stop();
            _metrics.RecordDuration($"stream.{requestType}.duration", stopwatch.Elapsed);
            _metrics.RecordValue($"stream.{requestType}.total_items", itemCount);
        }
    }
}
```

## API Endpoints with Streaming

### JSON Array Streaming

```csharp
app.MapGet("/api/products/stream", (IMediator mediator, string? category, CancellationToken ct) =>
{
    var request = new StreamProductsRequest { Category = category };

    return Results.Stream(async stream =>
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false // Minimise payload size
        };

        await JsonSerializer.SerializeAsync(stream, mediator.SendStream(request, ct), options, ct);
    }, contentType: "application/json");
});
```

### Server-Sent Events (SSE) Streaming

```csharp
app.MapGet("/api/orders/stream/sse", (IMediator mediator, CancellationToken ct) =>
{
    var request = new StreamOrdersRequest();

    return Results.Stream(async stream =>
    {
        var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = false };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        try
        {
            // Send connection established event
            await writer.WriteLineAsync("event: connected");
            await writer.WriteLineAsync("data: {\"status\": \"connected\"}");
            await writer.WriteLineAsync();
            await writer.FlushAsync();
            await stream.FlushAsync(); // Force immediate flush

            var count = 0;
            await foreach (var order in mediator.SendStream(request, ct))
            {
                count++;

                // Send order data
                await writer.WriteLineAsync("event: data");
                await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(order, options)}");
                await writer.WriteLineAsync();
                await writer.FlushAsync();
                await stream.FlushAsync(); // Critical for real-time streaming

                // Send progress updates
                if (count % 10 == 0)
                {
                    await writer.WriteLineAsync("event: progress");
                    await writer.WriteLineAsync($"data: {{\"processed\": {count}}}");
                    await writer.WriteLineAsync();
                    await writer.FlushAsync();
                    await stream.FlushAsync();
                }
            }

            // Send completion event
            await writer.WriteLineAsync("event: complete");
            await writer.WriteLineAsync($"data: {{\"total\": {count}}}");
            await writer.WriteLineAsync();
            await writer.FlushAsync();
        }
        finally
        {
            await writer.DisposeAsync();
        }
    }, contentType: "text/event-stream");
});
```

### Streaming with Error Handling

```csharp
app.MapGet("/api/data/stream", async (IMediator mediator, CancellationToken ct) =>
{
    try
    {
        var request = new StreamDataRequest();

        return Results.Stream(async stream =>
        {
            try
            {
                await JsonSerializer.SerializeAsync(stream, mediator.SendStream(request, ct));
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation gracefully
                await stream.WriteAsync(Encoding.UTF8.GetBytes("{\"status\":\"cancelled\"}"));
            }
            catch (Exception ex)
            {
                // Log error and send error response
                var errorResponse = JsonSerializer.Serialize(new { error = ex.Message });
                await stream.WriteAsync(Encoding.UTF8.GetBytes(errorResponse));
            }
        }, contentType: "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});
```

## Blazor Streaming Scenarios

### 1. Minimal API Streaming

RESTful endpoints with JSON streaming and Server-Sent Events for web clients and mobile apps:

```csharp
// JSON Array Streaming
app.MapGet("/api/contacts/stream", (IMediator mediator, string? search, CancellationToken ct) =>
{
    return Results.Stream(async stream =>
    {
        var request = new StreamContactsRequest { SearchTerm = search };
        await JsonSerializer.SerializeAsync(stream, mediator.SendStream(request, ct));
    }, "application/json");
});
```

### 2. Blazor SSR Streaming

Server-Side Rendered Blazor with real-time streaming capabilities:

```razor
@page "/streaming-ssr"
@inject IMediator Mediator

<h3>Server-Side Streaming</h3>

<div>
    <input @bind="searchTerm" @onkeyup="StartStreaming" placeholder="Search contacts..." />
    <button @onclick="StartStreaming">Start Stream</button>
    <button @onclick="StopStreaming">Stop</button>
</div>

<div>Total: @contacts.Count contacts</div>

<table>
    @foreach (var contact in contacts)
    {
        <tr>
            <td>@contact.Name</td>
            <td>@contact.Email</td>
        </tr>
    }
</table>

@code {
    private List<ContactDto> contacts = new();
    private string searchTerm = "";
    private CancellationTokenSource? cancellationTokenSource;

    private async Task StartStreaming()
    {
        StopStreaming();
        contacts.Clear();

        cancellationTokenSource = new CancellationTokenSource();
        var request = new StreamContactsRequest { SearchTerm = searchTerm };

        try
        {
            await foreach (var contact in Mediator.SendStream(request, cancellationTokenSource.Token))
            {
                contacts.Add(contact);
                await Task.Yield(); // Yield control to prevent UI blocking
                await InvokeAsync(StateHasChanged); // Trigger UI update
            }
        }
        catch (OperationCanceledException)
        {
            // Stream was cancelled
        }
    }

    private void StopStreaming()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }
}
```

### 3. Blazor Auto Mode Streaming

Hybrid approach that starts with Server mode for fast initial load, then automatically upgrades to WebAssembly when downloaded.

> **Why Hybrid Approach?** We chose this hybrid approach over full static render because we wanted **live streaming capabilities** - static mode doesn't support real-time streaming interactions.

```razor
@page "/streaming/auto"
@rendermode InteractiveAuto
@inject IMediator Mediator

<h3>Auto Mode Streaming (Server → WebAssembly)</h3>
<p>Starts on server for fast load, upgrades to WebAssembly automatically</p>

<!-- Same streaming implementation as SSR -->
<!-- The render mode handles the transition automatically -->
```

### 4. Blazor Static SSR Streaming

Pure server-side rendering with no JavaScript dependencies:

```razor
@page "/streaming-static"
@inject IMediator Mediator

<h3>Static SSR Streaming</h3>
<p>Pure server-side rendering - no JavaScript required</p>

<!-- Static rendering doesn't support real-time updates -->
<!-- Use for SEO-optimised, accessible content -->

@{
    // Pre-load data on server for static rendering
    var contacts = await LoadContactsAsync();
}

<table>
    @foreach (var contact in contacts)
    {
        <tr>
            <td>@contact.Name</td>
            <td>@contact.Email</td>
        </tr>
    }
</table>

@code {
    private async Task<List<ContactDto>> LoadContactsAsync()
    {
        var request = new StreamContactsRequest();
        var contacts = new List<ContactDto>();

        await foreach (var contact in Mediator.SendStream(request))
        {
            contacts.Add(contact);
        }

        return contacts;
    }
}
```

### 5. Blazor Interactive Streaming (WebAssembly)

WebAssembly-powered client-side streaming using EventSource for real-time data flow:

```razor
@page "/streaming-interactive"
@rendermode InteractiveWebAssembly
@inject HttpClient Http
@implements IDisposable

<h3>WebAssembly Interactive Streaming</h3>

<div>
    <button @onclick="StartEventSourceStream">Start EventSource Stream</button>
    <button @onclick="StopStream">Stop Stream</button>
</div>

<div>Received: @contacts.Count contacts</div>

<table>
    @foreach (var contact in contacts)
    {
        <tr>
            <td>@contact.Name</td>
            <td>@contact.Email</td>
        </tr>
    }
</table>

@code {
    private List<ContactDto> contacts = new();
    private EventSource? eventSource;

    private void StartEventSourceStream()
    {
        StopStream();
        contacts.Clear();

        eventSource = new EventSource("/api/contacts/stream/sse");

        eventSource.OnMessage += (sender, e) =>
        {
            if (e.Type == "data")
            {
                var contact = JsonSerializer.Deserialize<ContactDto>(e.Data);
                if (contact != null)
                {
                    contacts.Add(contact);
                    InvokeAsync(StateHasChanged);
                }
            }
        };

        eventSource.OnError += (sender, e) =>
        {
            Console.WriteLine($"EventSource error: {e.Message}");
        };
    }

    private void StopStream()
    {
        eventSource?.Close();
        eventSource?.Dispose();
        eventSource = null;
    }

    public void Dispose()
    {
        StopStream();
    }
}
```

### 6. Blazor Non-Streaming (WebAssembly)

Traditional bulk data loading for performance comparison:

```razor
@page "/non-streaming"
@rendermode InteractiveWebAssembly
@inject HttpClient Http

<h3>Non-Streaming (Bulk Load)</h3>
<p>Traditional approach for performance comparison</p>

<div>
    <button @onclick="LoadAllContacts" disabled="@isLoading">Load All Contacts</button>
    @if (isLoading)
    {
        <span>Loading...</span>
    }
</div>

<div>Total: @contacts.Count contacts (Load time: @loadTime ms)</div>

<table>
    @foreach (var contact in contacts)
    {
        <tr>
            <td>@contact.Name</td>
            <td>@contact.Email</td>
        </tr>
    }
</table>

@code {
    private List<ContactDto> contacts = new();
    private bool isLoading = false;
    private double loadTime = 0;

    private async Task LoadAllContacts()
    {
        isLoading = true;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await Http.GetFromJsonAsync<ContactDto[]>("/api/contacts/all");
            contacts = response?.ToList() ?? new List<ContactDto>();
        }
        finally
        {
            stopwatch.Stop();
            loadTime = stopwatch.Elapsed.TotalMilliseconds;
            isLoading = false;
        }
    }
}
```

## Performance Considerations

### Memory Efficiency

```csharp
public async IAsyncEnumerable<LargeDataDto> Handle(
    StreamLargeDataRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    // ✅ Good: Process one item at a time
    await foreach (var item in _repository.StreamDataAsync(cancellationToken))
    {
        yield return MapToDto(item); // Memory released after yield
    }

    // ❌ Bad: Load everything into memory
    // var allItems = await _repository.GetAllAsync();
    // foreach (var item in allItems) yield return MapToDto(item);
}
```

### Cancellation Support

```csharp
public async IAsyncEnumerable<DataDto> Handle(
    StreamDataRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var processed = 0;

    await foreach (var item in _dataSource.StreamAsync().WithCancellation(cancellationToken))
    {
        // Check cancellation periodically
        cancellationToken.ThrowIfCancellationRequested();

        processed++;

        // Optional: Check cancellation more frequently for CPU-intensive operations
        if (processed % 100 == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        yield return ProcessItem(item);
    }
}
```

### Buffering and Batching

```csharp
public async IAsyncEnumerable<DataBatch> Handle(
    StreamBatchedDataRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var buffer = new List<DataDto>(request.BatchSize);

    await foreach (var item in _dataSource.StreamAsync().WithCancellation(cancellationToken))
    {
        buffer.Add(item);

        if (buffer.Count >= request.BatchSize)
        {
            yield return new DataBatch { Items = buffer.ToArray() };
            buffer.Clear(); // Release memory
        }
    }

    // Yield remaining items
    if (buffer.Count > 0)
    {
        yield return new DataBatch { Items = buffer.ToArray() };
    }
}
```

## Testing Streaming

### Unit Testing Stream Handlers

```csharp
[Test]
public async Task StreamHandler_Should_YieldExpectedItems()
{
    // Arrange
    var mockService = new Mock<IContactService>();
    var testContacts = new[]
    {
        new ContactDto { Id = 1, Name = "John" },
        new ContactDto { Id = 2, Name = "Jane" }
    };

    mockService.Setup(s => s.StreamContactsAsync(It.IsAny<CancellationToken>()))
              .Returns(testContacts.ToAsyncEnumerable());

    var handler = new StreamContactsHandler(mockService.Object, Mock.Of<ILogger<StreamContactsHandler>>());
    var request = new StreamContactsRequest();

    // Act
    var results = new List<ContactDto>();
    await foreach (var contact in handler.Handle(request))
    {
        results.Add(contact);
    }

    // Assert
    Assert.That(results, Has.Count.EqualTo(2));
    Assert.That(results[0].Name, Is.EqualTo("John"));
    Assert.That(results[1].Name, Is.EqualTo("Jane"));
}
```

### Testing with Cancellation

```csharp
[Test]
public async Task StreamHandler_Should_RespectCancellation()
{
    // Arrange
    var cts = new CancellationTokenSource();
    var handler = new StreamContactsHandler(mockService.Object, Mock.Of<ILogger<StreamContactsHandler>>());
    var request = new StreamContactsRequest();

    // Act & Assert
    var itemCount = 0;

    await Assert.ThrowsAsync<OperationCanceledException>(async () =>
    {
        await foreach (var contact in handler.Handle(request, cts.Token))
        {
            itemCount++;
            if (itemCount >= 5)
            {
                cts.Cancel(); // Cancel after 5 items
            }
        }
    });

    Assert.That(itemCount, Is.EqualTo(5));
}
```

### Integration Testing Streaming Endpoints

```csharp
[Test]
public async Task StreamEndpoint_Should_ReturnJsonStream()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    using var client = factory.CreateClient();

    // Act
    using var response = await client.GetAsync("/api/contacts/stream");
    using var stream = await response.Content.ReadAsStreamAsync();

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

    var contacts = await JsonSerializer.DeserializeAsync<ContactDto[]>(stream);
    Assert.That(contacts, Is.Not.Null);
    Assert.That(contacts!.Length, Is.GreaterThan(0));
}
```

## Best Practices

### 1. Always Use EnumeratorCancellation

```csharp
public async IAsyncEnumerable<T> Handle(
    TRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### 2. Implement Proper Error Handling

```csharp
public async IAsyncEnumerable<ContactDto> Handle(
    StreamContactsRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    ContactDto? lastSuccessfulContact = null;

    try
    {
        await foreach (var contact in _service.StreamContactsAsync(cancellationToken))
        {
            try
            {
                lastSuccessfulContact = contact;
                yield return contact;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing contact {ContactId}", contact?.Id);
                // Continue with next item rather than failing entire stream
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Fatal error in contact stream after {LastContact}",
                        lastSuccessfulContact?.Id);
        throw; // Re-throw fatal errors
    }
}
```

### 3. Use ConfigureAwait(false) in Handlers

```csharp
public async IAsyncEnumerable<DataDto> Handle(
    StreamDataRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    await foreach (var item in _service.StreamDataAsync(cancellationToken).ConfigureAwait(false))
    {
        yield return item;
    }
}
```

### 4. Implement Timeouts for Long-Running Streams

```csharp
public async IAsyncEnumerable<DataDto> Handle(
    StreamDataRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

    await foreach (var item in _service.StreamDataAsync(combinedCts.Token))
    {
        yield return item;
    }
}
```

### 5. Monitor Memory Usage

```csharp
public async IAsyncEnumerable<DataDto> Handle(
    StreamDataRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var processed = 0;

    await foreach (var item in _service.StreamDataAsync(cancellationToken))
    {
        processed++;

        // Periodic memory cleanup
        if (processed % 1000 == 0)
        {
            GC.Collect(0, GCCollectionMode.Optimized);
        }

        yield return item;
    }
}
```

## Sample Projects

The **Streaming.Api** sample projects demonstrate comprehensive streaming capabilities across multiple scenarios:

> **Important Note**: The **Streaming.Api** server must be running for the **Streaming.Api.WASM** project to work properly with live data.

### Project Structure

```
src/samples/Streaming.Api/
├── Streaming.Api/              # Main server (https://localhost:7021)
├── Streaming.Api.WASM/         # WebAssembly client (https://localhost:5011)
├── Streaming.Api.Client/       # Shared Razor components
└── Streaming.Api.Shared/       # Common models and DTOs
```

### 1. **Minimal API Streaming** (`/api/contacts/stream`)

RESTful API endpoints with JSON streaming and Server-Sent Events:

-   JSON Array Streaming for bulk data transfer
-   Server-Sent Events with real-time metadata
-   Search filtering and performance metrics
-   Interactive Swagger documentation at `https://localhost:7021/swagger`

### 2. **Blazor SSR Streaming** (`/streaming-ssr`)

Server-Side Rendered Blazor with real-time streaming capabilities:

-   Real-time server-side updates
-   Interactive streaming controls
-   Live statistics and performance metrics
-   Responsive table layout with search

### 3. **Blazor Auto Mode Streaming** (`/streaming/auto`)

Hybrid approach demonstrating the best of both worlds:

-   Fast initial server-side load
-   Automatic WebAssembly upgrade when available
-   **Live streaming support** (not available in static mode)
-   Seamless transition between render modes

> **Why Hybrid Approach?** We chose this hybrid approach over full static render because we wanted **live streaming capabilities** - static mode doesn't support real-time streaming interactions. This gives us the speed of server-side rendering for the initial load while maintaining the rich interactivity of WebAssembly for ongoing streaming operations.

### 4. **Blazor Static SSR Streaming** (`/streaming-static`)

Pure server-side rendering optimised for compatibility:

-   No JavaScript required
-   Maximum browser compatibility
-   SEO-optimised streaming content
-   Accessibility-focused design

### 5. **Blazor Interactive Streaming (WebAssembly)** (`https://localhost:5011/streaming-interactive`)

WebAssembly-powered client-side streaming:

-   Client-side WebAssembly performance
-   EventSource integration for real-time updates
-   Advanced UI features and interaction patterns
-   Multiple streaming modes and configurations

### 6. **Blazor Non-Streaming (WebAssembly)** (`https://localhost:5011/non-streaming`)

Traditional bulk data loading for performance comparison:

-   Classic REST API patterns
-   Bulk JSON loading comparison
-   Performance benchmarking metrics
-   Load time comparison vs streaming approaches

### Running the Samples

The **Streaming.Api** sample uses auto-discovery for middleware registration:

```csharp
// From Streaming.Api/Program.cs
builder.Services.AddMediator(config =>
{
    // Manual registration of specific streaming middleware
    config.AddMiddleware(typeof(StreamingLoggingMiddleware<,>));
}, typeof(Program));

// Alternative: Auto-discovery approach (recommended)
// builder.Services.AddMediator(discoverMiddleware: true, typeof(Program).Assembly);
```

```bash
# Start the main server
cd C:\Blazing.Mediator\src\samples\Streaming.Api\Streaming.Api
dotnet run

# In another terminal, start the WebAssembly client
cd C:\Blazing.Mediator\src\samples\Streaming.Api\Streaming.Api.WASM
dotnet run
```

### Access Points

-   **Main Server**: https://localhost:7021 (Streaming.Api)
-   **WebAssembly Client**: https://localhost:5011 (Streaming.Api.WASM)
-   **API Documentation**: https://localhost:7021/swagger
-   **Sample Endpoints**:
    -   `/api/contacts/count` - Get contact count
    -   `/api/contacts/stream` - JSON streaming
    -   `/api/contacts/stream/sse` - Server-Sent Events

## Complete Examples

### E-Commerce Product Streaming

```csharp
// Request
public class StreamProductCatalogRequest : IStreamRequest<ProductCatalogItem>
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool IncludeImages { get; set; } = true;
    public int BatchSize { get; set; } = 20;
}

// Response DTO
public class ProductCatalogItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string[]? ImageUrls { get; set; }
    public int StockQuantity { get; set; }
    public ProductMetadata Metadata { get; set; } = new();
}

public class ProductMetadata
{
    public DateTime LastUpdated { get; set; }
    public string ProcessingTime { get; set; } = string.Empty;
    public int ItemNumber { get; set; }
}

// Handler
public class StreamProductCatalogHandler : IStreamRequestHandler<StreamProductCatalogRequest, ProductCatalogItem>
{
    private readonly IProductRepository _repository;
    private readonly IImageService _imageService;
    private readonly ILogger<StreamProductCatalogHandler> _logger;

    public StreamProductCatalogHandler(
        IProductRepository repository,
        IImageService imageService,
        ILogger<StreamProductCatalogHandler> logger)
    {
        _repository = repository;
        _imageService = imageService;
        _logger = logger;
    }

    public async IAsyncEnumerable<ProductCatalogItem> Handle(
        StreamProductCatalogRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting product catalog stream for category: {Category}", request.Category);

        var query = _repository.GetProductsQuery();

        if (!string.IsNullOrEmpty(request.Category))
            query = query.Where(p => p.Category == request.Category);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice);

        var itemNumber = 0;
        var stopwatch = Stopwatch.StartNew();

        await foreach (var product in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            itemNumber++;
            var itemStopwatch = Stopwatch.StartNew();

            var catalogItem = new ProductCatalogItem
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                StockQuantity = product.StockQuantity,
                Metadata = new ProductMetadata
                {
                    LastUpdated = product.LastUpdated,
                    ItemNumber = itemNumber
                }
            };

            // Optionally load images
            if (request.IncludeImages)
            {
                catalogItem.ImageUrls = await _imageService.GetProductImagesAsync(product.Id, cancellationToken);
            }

            itemStopwatch.Stop();
            catalogItem.Metadata.ProcessingTime = $"{itemStopwatch.ElapsedMilliseconds}ms";

            yield return catalogItem;

            // Optional: Add small delay to prevent overwhelming the client
            if (itemNumber % request.BatchSize == 0)
            {
                await Task.Delay(10, cancellationToken);
            }
        }

        stopwatch.Stop();
        _logger.LogInformation("Completed product catalog stream. Items: {ItemCount}, Total time: {TotalTime}ms",
                              itemNumber, stopwatch.ElapsedMilliseconds);
    }
}

// API Endpoint
app.MapGet("/api/products/catalog/stream", (
    IMediator mediator,
    string? category,
    decimal? minPrice,
    decimal? maxPrice,
    bool includeImages = true,
    CancellationToken ct = default) =>
{
    var request = new StreamProductCatalogRequest
    {
        Category = category,
        MinPrice = minPrice,
        MaxPrice = maxPrice,
        IncludeImages = includeImages
    };

    return Results.Stream(async stream =>
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        await JsonSerializer.SerializeAsync(stream, mediator.SendStream(request, ct), options, ct);
    }, "application/json");
})
.WithName("StreamProductCatalog")
.WithSummary("Stream product catalog with filtering")
.WithOpenApi();
```

### Real-Time Order Processing Stream

```csharp
// Request
public class StreamOrderProcessingRequest : IStreamRequest<OrderProcessingUpdate>
{
    public DateTime? FromDate { get; set; }
    public string[]? Statuses { get; set; }
    public bool IncludeCustomerInfo { get; set; } = false;
}

// Response DTO
public class OrderProcessingUpdate
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StatusChangeTime { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderProcessingMetrics Metrics { get; set; } = new();
}

public class OrderProcessingMetrics
{
    public TimeSpan ProcessingTime { get; set; }
    public int QueuePosition { get; set; }
    public string ProcessingStage { get; set; } = string.Empty;
}

// Handler with sophisticated processing
public class StreamOrderProcessingHandler : IStreamRequestHandler<StreamOrderProcessingRequest, OrderProcessingUpdate>
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IMetricsCollector _metrics;
    private readonly ILogger<StreamOrderProcessingHandler> _logger;

    public StreamOrderProcessingHandler(
        IOrderService orderService,
        ICustomerService customerService,
        IMetricsCollector metrics,
        ILogger<StreamOrderProcessingHandler> logger)
    {
        _orderService = orderService;
        _customerService = customerService;
        _metrics = metrics;
        _logger = logger;
    }

    public async IAsyncEnumerable<OrderProcessingUpdate> Handle(
        StreamOrderProcessingRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting order processing stream from {FromDate}", request.FromDate);

        var queuePosition = 0;
        var processedOrders = 0;

        await foreach (var order in _orderService.StreamOrderUpdatesAsync(request.FromDate, cancellationToken))
        {
            queuePosition++;
            var processingStart = DateTime.UtcNow;

            // Filter by status if specified
            if (request.Statuses != null && !request.Statuses.Contains(order.Status))
                continue;

            var update = new OrderProcessingUpdate
            {
                OrderId = order.Id,
                Status = order.Status,
                StatusChangeTime = order.LastUpdated,
                TotalAmount = order.TotalAmount,
                Metrics = new OrderProcessingMetrics
                {
                    QueuePosition = queuePosition,
                    ProcessingStage = "DataMapping"
                }
            };

            // Optionally include customer info
            if (request.IncludeCustomerInfo)
            {
                update.Metrics.ProcessingStage = "CustomerLookup";
                var customer = await _customerService.GetCustomerAsync(order.CustomerId, cancellationToken);
                update.CustomerName = customer?.FullName;
            }

            var processingEnd = DateTime.UtcNow;
            update.Metrics.ProcessingTime = processingEnd - processingStart;
            update.Metrics.ProcessingStage = "Complete";

            // Record metrics
            _metrics.RecordProcessingTime("order_stream_item", update.Metrics.ProcessingTime);

            processedOrders++;
            yield return update;

            // Throttling to prevent overwhelming downstream systems
            if (processedOrders % 50 == 0)
            {
                _logger.LogDebug("Processed {ProcessedOrders} orders, queue position {QueuePosition}",
                               processedOrders, queuePosition);
                await Task.Delay(100, cancellationToken);
            }
        }

        _logger.LogInformation("Completed order processing stream. Processed: {ProcessedOrders}", processedOrders);
    }
}

// SSE Endpoint for Real-Time Updates
app.MapGet("/api/orders/processing/stream", (
    IMediator mediator,
    DateTime? fromDate,
    string? statuses,
    bool includeCustomerInfo = false,
    CancellationToken ct = default) =>
{
    var request = new StreamOrderProcessingRequest
    {
        FromDate = fromDate,
        Statuses = statuses?.Split(','),
        IncludeCustomerInfo = includeCustomerInfo
    };

    return Results.Stream(async stream =>
    {
        var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = false };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        try
        {
            await writer.WriteLineAsync("event: stream-start");
            await writer.WriteLineAsync($"data: {{\"timestamp\": \"{DateTime.UtcNow:O}\"}}");
            await writer.WriteLineAsync();
            await writer.FlushAsync();
            await stream.FlushAsync();

            await foreach (var update in mediator.SendStream(request, ct))
            {
                await writer.WriteLineAsync("event: order-update");
                await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(update, options)}");
                await writer.WriteLineAsync();
                await writer.FlushAsync();
                await stream.FlushAsync();
            }

            await writer.WriteLineAsync("event: stream-complete");
            await writer.WriteLineAsync($"data: {{\"timestamp\": \"{DateTime.UtcNow:O}\"}}");
            await writer.WriteLineAsync();
            await writer.FlushAsync();
        }
        finally
        {
            await writer.DisposeAsync();
        }
    }, "text/event-stream");
})
.WithName("StreamOrderProcessing")
.WithSummary("Real-time order processing updates via Server-Sent Events")
.WithOpenApi();
```

---

**Blazing.Mediator** streaming provides a powerful, memory-efficient way to handle real-time data processing scenarios. Whether you're building APIs, Blazor applications, or complex data processing pipelines, the streaming capabilities enable you to create responsive, scalable applications that can handle large datasets efficiently.

For more examples and detailed implementations, explore the **Streaming.Api** sample projects in the repository, which demonstrate all the patterns and scenarios covered in this guide.
