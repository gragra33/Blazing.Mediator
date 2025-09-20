using Blazing.Mediator;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for CreateUserCommand using Entity Framework Core.
/// </summary>
public sealed class CreateUserHandler(ApplicationDbContext context)
    : IRequestHandler<CreateUserCommand, int>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(CreateUserHandler)}";

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", "CreateUserHandler.Handle");
        activity?.SetTag("user.name", request.Name);
        activity?.SetTag("user.email", request.Email);

        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);
        // Simulate potential failures for testing
        if (request.Name.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            var message = "Simulated CreateUser error for testing telemetry";
            activity?.SetStatus(ActivityStatusCode.Error, message);
            activity?.SetTag("handler.simulated_error", true);

            throw new InvalidOperationException(message);
        }
        // Create new user entity
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        // Add to database
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("handler.created", true);
        activity?.SetTag("user.id", user.Id);
        Console.WriteLine($"Created user: {user.Name} ({user.Email}) with ID {user.Id}");

        return user.Id;
    }
}