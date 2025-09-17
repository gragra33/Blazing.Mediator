using OpenTelemetryExample.Extensions;
using OpenTelemetryExample.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add all application services using extension method
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline using extension method
app.ConfigurePipeline();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    Console.WriteLine("[*] In-memory database created and seeded with initial user data");
}

// Display startup information
Console.WriteLine("[*] OpenTelemetry API Server is ready!");
Console.WriteLine($"[*] Blazing.Mediator Telemetry: {(Blazing.Mediator.Mediator.TelemetryEnabled ? "ENABLED" : "DISABLED")}");
Console.WriteLine($"[*] Environment: {app.Environment.EnvironmentName}");

app.Run();

public partial class Program { }