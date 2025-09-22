using Arshid.Aspire.ApiDocs.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> apiService = builder
    .AddProject<Projects.OpenTelemetryExample>("OpenTelemetry-api-server")
    .WithSwagger();

builder.AddProject<Projects.OpenTelemetryExample_Client>("OpenTelemetry-blazor-client")
    .WithReference(apiService)
    .WaitFor(apiService);

// Note: Serilog is already configured in the API service to send logs to OTLP endpoint
// which Aspire automatically configures via OTEL_EXPORTER_OTLP_ENDPOINT environment variable.
// The logs will appear in the Aspire dashboard automatically.

builder.Build().Run();
