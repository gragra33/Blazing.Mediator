using ECommerce.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container following dependency inversion principle
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline following single responsibility principle
app.ConfigurePipeline();

await app.RunAsync();

/// <summary>
/// Represents the entry point for the E-Commerce API application.
/// This application demonstrates the Blazing.Mediator library implementing CQRS pattern
/// with command and query handlers for e-commerce operations including product and order management.
/// </summary>
// Make Program class accessible for testing
public partial class Program { }
