using OpenTelemetryExample.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add all application services using extension method
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline using extension method
app.ConfigurePipeline();

// Display startup information
Console.WriteLine("[*] OpenTelemetry API Server is ready!");
Console.WriteLine($"[*] Blazing.Mediator Telemetry: {(Blazing.Mediator.Mediator.TelemetryEnabled ? "ENABLED" : "DISABLED")}");
Console.WriteLine($"[*] Environment: {app.Environment.EnvironmentName}");

app.Run();

public partial class Program { }