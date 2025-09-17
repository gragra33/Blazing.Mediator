var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.OpenTelemetryExample>("OpenTelemetry-api-server")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.OpenTelemetryExample_Client>("OpenTelemetry-blazor-client")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
