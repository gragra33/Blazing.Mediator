using OpenTelemetryExample.Application.Services;
using OpenTelemetryExample.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add all application services using extension method
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Inspect logging configuration using helper class
LoggerInspectionHelper.PerformFullInspection(app.Services, "ProgramStartupTest");

// Configure the HTTP request pipeline using extension method
app.ConfigurePipeline();

// Initialize database and verify telemetry logging setup
await DatabaseInitializationHelper.InitializeAndSeedDatabaseAsync(app.Services);

// Generate startup test logs
//await DatabaseInitializationHelper.GenerateStartupTestLogsAsync(app.Services);

app.Run();

public partial class Program { }