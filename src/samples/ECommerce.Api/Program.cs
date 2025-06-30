using Blazing.Mediator;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
if (builder.Environment.IsDevelopment())
{
    // Use In-Memory database for development/demo
    builder.Services.AddDbContext<ECommerceDbContext>(options =>
        options.UseInMemoryDatabase("ECommerceDb"));
}
else
{
    // Use SQL Server for production
    builder.Services.AddDbContext<ECommerceDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Register Mediator with CQRS handlers
builder.Services.AddMediator(typeof(Program).Assembly);

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Ensure database is created and seeded in development
    using IServiceScope? scope = app.Services.CreateScope();
    ECommerceDbContext? context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Represents the entry point for the E-Commerce API application.
/// This application demonstrates the Blazing.Mediator library implementing CQRS pattern
/// with command and query handlers for e-commerce operations including product and order management.
/// </summary>
// Make Program class accessible for testing
public partial class Program { }
