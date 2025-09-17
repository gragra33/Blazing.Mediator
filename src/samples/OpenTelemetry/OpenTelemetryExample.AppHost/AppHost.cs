using Arshid.Aspire.ApiDocs.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> apiService = builder
    .AddProject<Projects.OpenTelemetryExample>("OpenTelemetry-api-server")
    .WithSwagger();

builder.AddProject<Projects.OpenTelemetryExample_Client>("OpenTelemetry-blazor-client")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
