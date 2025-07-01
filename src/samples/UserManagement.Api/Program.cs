using UserManagement.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container following dependency inversion principle
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline following single responsibility principle
app.ConfigurePipeline();

await app.RunAsync();

/// <summary>
/// Represents the entry point for the User Management API application.
/// This application demonstrates the Blazing.Mediator library implementing CQRS pattern
/// with command and query handlers for user management operations.
/// </summary>
public partial class Program
{
    // Make Program class accessible for testing
}