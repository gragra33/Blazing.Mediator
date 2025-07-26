# Blazing.Mediator Examples

This project contains examples demonstrating all the core features of **Blazing.Mediator**, converted from the original MediatR examples.

## Purpose

These examples show how to:

1. **Migrate from MediatR to Blazing.Mediator**: See the differences in interfaces and patterns
2. **Use Core Features**: Understand requests, handlers, notifications, and streaming
3. **Implement Middleware**: Replace MediatR pipeline behaviors with Blazing.Mediator middleware
4. **Performance Benefits**: Experience faster execution and lower memory usage

## Key Examples

### 1. Basic Request/Response (Ping/Pong)

```csharp
// Request using Blazing.Mediator.IRequest<T>
public class Ping : IRequest<Pong>
{
    public string Message { get; set; }
}

// Handler using Blazing.Mediator.IRequestHandler<T,R>
public class PingHandler : IRequestHandler<Ping, Pong>
{
    public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new Pong { Message = request.Message + " Pong" };
    }
}
```

### 2. Void Commands (Jing)

```csharp
// Void request using Blazing.Mediator.IRequest
public class Jing : IRequest
{
    public string Message { get; set; }
}

// Void handler using Blazing.Mediator.IRequestHandler<T>
public class JingHandler : IRequestHandler<Jing>
{
    public async Task Handle(Jing request, CancellationToken cancellationToken)
    {
        // Command logic here
    }
}
```

### 3. Notifications (Pinged)

```csharp
// Notification using Blazing.Mediator.INotification
public class Pinged : INotification
{
    public DateTime Timestamp { get; set; }
}

// Multiple handlers can exist for the same notification
public class PingedHandler : INotificationHandler<Pinged>
{
    public async Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        // Handle notification
    }
}
```

### 4. Streaming Requests (Sing/Song)

```csharp
// Streaming request using Blazing.Mediator.IStreamRequest<T>
public class Sing : IStreamRequest<Song>
{
    public string Message { get; set; }
}

// Streaming handler using Blazing.Mediator.IStreamRequestHandler<T,R>
public class SingHandler : IStreamRequestHandler<Sing, Song>
{
    public async IAsyncEnumerable<Song> Handle(Sing request, CancellationToken cancellationToken)
    {
        var notes = new[] { "do", "re", "mi", "fa", "so", "la", "ti", "do" };
        foreach (var note in notes)
        {
            yield return new Song { Message = $"Singing {note}" };
            await Task.Delay(10, cancellationToken);
        }
    }
}
```

### 5. Middleware (instead of Pipeline Behaviors)

```csharp
// Middleware using Blazing.Mediator middleware pattern
public class GenericRequestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Pre-processing
        Console.WriteLine("Before handler");

        var response = await next();

        // Post-processing
        Console.WriteLine("After handler");

        return response;
    }
}
```

## Migration from MediatR

| MediatR                          | Blazing.Mediator                        | Notes                               |
| -------------------------------- | --------------------------------------- | ----------------------------------- |
| `MediatR.IMediator`              | `Blazing.Mediator.IMediator`            | Same interface, different namespace |
| `MediatR.IRequest<T>`            | `Blazing.Mediator.IRequest<T>`          | Same pattern, better performance    |
| `MediatR.IRequestHandler<T,R>`   | `Blazing.Mediator.IRequestHandler<T,R>` | Same interface                      |
| `MediatR.INotification`          | `Blazing.Mediator.INotification`        | Same interface                      |
| `MediatR.IPipelineBehavior<T,R>` | `IRequestMiddleware<T,R>`               | Different pattern, more flexible    |
| `MediatR.IStreamRequest<T>`      | `Blazing.Mediator.IStreamRequest<T>`    | Same interface                      |

## Running the Examples

1. **Prerequisites**: .NET 9.0 SDK
2. **Build**: `dotnet build`
3. **Run**: `dotnet run`

## Expected Output

The examples will demonstrate:

-   ✅ Request Handler
-   ✅ Void Request Handler
-   ✅ Middleware Behavior
-   ✅ Pre-Processor
-   ✅ Post-Processor
-   ✅ Constrained Post-Processor
-   ✅ Ordered Behaviors
-   ✅ Notification Handler
-   ✅ Notification Handlers (Multiple)
-   ✅ Stream Request Handler
-   ✅ Stream Middleware Behavior
-   ✅ Stream Ordered Behaviors

## Performance Benefits

Compared to MediatR, these examples benefit from:

-   **Faster Request Handling**: Optimized reflection and caching
-   **Lower Memory Allocation**: Reduced garbage collection pressure
-   **Better Throughput**: Higher requests per second
-   **Startup Performance**: Faster application startup

## Next Steps

After understanding these examples:

1. Try the **ECommerce.Api**, and **UserManagement.Api** samples for a real-world implementation
2. Build your own applications with Blazing.Mediator!
