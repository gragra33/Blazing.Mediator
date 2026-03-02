using System.Runtime.CompilerServices;
using Blazing.Mediator;

namespace Blazing.Mediator.AotTests.Handlers;

// Test Query (IRequest<TResponse>)
public record GetTestDataQuery : IRequest<TestDataResponse>
{
    public int Id { get; init; }
}

public record TestDataResponse
{
    public int Id { get; init; }
    public string Data { get; init; } = string.Empty;
}

public class GetTestDataQueryHandler : IRequestHandler<GetTestDataQuery, TestDataResponse>
{
    public async ValueTask<TestDataResponse> Handle(GetTestDataQuery request, CancellationToken cancellationToken)
    {
        return new TestDataResponse 
        { 
            Id = request.Id, 
            Data = $"Test data for {request.Id}" 
        };
    }
}

// Test Void Command (IRequest)
public record CreateTestCommand : IRequest
{
    public string Name { get; init; } = string.Empty;
}

public class CreateTestCommandHandler : IRequestHandler<CreateTestCommand>
{
    public ValueTask Handle(CreateTestCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Creating test: {request.Name}");
        return ValueTask.CompletedTask;
    }
}

// Test Stream Request
public record StreamTestDataRequest : IStreamRequest<int>
{
    public int Count { get; init; }
}

public class StreamTestDataHandler : IStreamRequestHandler<StreamTestDataRequest, int>
{
    public async IAsyncEnumerable<int> Handle(
        StreamTestDataRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < request.Count; i++)
        {
            await Task.Delay(10, cancellationToken);
            yield return i;
        }
    }
}

// Test Notification
public record TestNotification : INotification
{
    public string Message { get; init; } = string.Empty;
}

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Handling notification: {notification.Message}");
        return ValueTask.CompletedTask;
    }
}

// Test Middleware
public class TestLoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 100;

    public async ValueTask<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Before: {typeof(TRequest).Name}");
        var response = await next();
        Console.WriteLine($"After: {typeof(TRequest).Name}");
        return response;
    }
}
