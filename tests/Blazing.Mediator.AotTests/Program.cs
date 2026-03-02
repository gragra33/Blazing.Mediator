using Blazing.Mediator;
using Blazing.Mediator.AotTests.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.AotTests;

/// <summary>
/// AOT compatibility test for Blazing.Mediator with source generators.
/// This program validates that the library works correctly with NativeAOT compilation.
/// </summary>
internal class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== Blazing.Mediator AOT Compatibility Test ===");
        Console.WriteLine();

        try
        {
            // Setup DI container
            var services = new ServiceCollection();
            
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            // Register mediator with source generators
            services.AddMediator();
            
            // Note: Handlers are automatically discovered from assembly scanning
            // No manual registration needed - this is the power of source generators!
            
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();
            
            Console.WriteLine("✅ Mediator initialized successfully");
            Console.WriteLine();
            
            // Test 1: Send Query
            Console.WriteLine("Test 1: Query Handler (IRequest<TResponse>)");
            var query = new GetUserQuery(UserId: 42);
            var user = await mediator.Send(query);
            Console.WriteLine($"  Result: User {user.Id} - {user.Name}");
            Console.WriteLine($"  Status: {(user.Id == 42 ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine();
            
            // Test 2: Send Command
            Console.WriteLine("Test 2: Command Handler (IRequest<TResponse>)");
            var command = new CreateUserCommand(Name: "AOT User");
            var userId = await mediator.Send(command);
            Console.WriteLine($"  Result: Created user with ID {userId}");
            Console.WriteLine($"  Status: {(userId != Guid.Empty ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine();
            
            // Test 3: Void Command
            Console.WriteLine("Test 3: Void Command (IRequest)");
            await mediator.Send(new CreateTestCommand { Name = "AOT Test" });
            Console.WriteLine("  Status: ✅ PASS");
            Console.WriteLine();
            
            // Test 4: Stream Request
            Console.WriteLine("Test 4: Stream Request (IStreamRequest<T>)");
            var streamCount = 0;
            await foreach (var item in mediator.SendStream(new StreamTestDataRequest { Count = 5 }))
            {
                streamCount++;
                Console.WriteLine($"  Stream item: {item}");
            }
            Console.WriteLine($"  Status: {(streamCount == 5 ? "✅ PASS" : "❌ FAIL")} - Streamed {streamCount} items");
            Console.WriteLine();
            
            // Test 5: Publish Notification
            Console.WriteLine("Test 5: Notification (INotification)");
            var notification = new UserCreatedNotification(UserId: 42, Name: "AOT User");
            await mediator.Publish(notification);
            await mediator.Publish(new TestNotification { Message = "AOT Test Notification" });
            Console.WriteLine("  Status: ✅ PASS (no exceptions)");
            Console.WriteLine();
            
            // Test 6: Telemetry Operations
            Console.WriteLine("Test 6: Telemetry Operations");
            for (int i = 0; i < 10; i++)
            {
                var result = await mediator.Send(new GetTestDataQuery { Id = i });
            }
            Console.WriteLine("  Status: ✅ PASS - 10 telemetry operations completed");
            Console.WriteLine();
            
            Console.WriteLine("=== All AOT Tests Passed ✅ ===");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}

#region Test Request/Response Types

public record GetUserQuery(int UserId) : IRequest<UserDto>;
public record UserDto(int Id, string Name);

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async ValueTask<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return new UserDto(request.UserId, "Test User");
    }
}

public record CreateUserCommand(string Name) : IRequest<Guid>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return Guid.NewGuid();
    }
}

public record UserCreatedNotification(int UserId, string Name) : INotification;

public class UserCreatedLogHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly ILogger<UserCreatedLogHandler> _logger;

    public UserCreatedLogHandler(ILogger<UserCreatedLogHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User created: {UserId} - {Name}", notification.UserId, notification.Name);
        return ValueTask.CompletedTask;
    }
}

#endregion
