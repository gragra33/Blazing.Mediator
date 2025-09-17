using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Use the project's source directory to resolve paths correctly
var sourceDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
var aspirePath = Path.GetFullPath(Path.Combine(sourceDirectory!, "..", "..", "..", "OpenTelemetryExample.Aspire"));
var projectRoot = Path.GetDirectoryName(aspirePath)!;

// Add the OpenTelemetryExample API server
var apiService = builder.AddProject("opentelemetryexample-api", 
    Path.Combine(projectRoot, "OpenTelemetryExample", "OpenTelemetryExample.csproj"))
    .WithExternalHttpEndpoints();

// Add the OpenTelemetryExample.Client Blazor WebAssembly app
var webApp = builder.AddProject("opentelemetryexample-client", 
    Path.Combine(projectRoot, "OpenTelemetryExample.Client", "OpenTelemetryExample.Client.csproj"))
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

// Build and run the distributed application
var app = builder.Build();

app.Run();