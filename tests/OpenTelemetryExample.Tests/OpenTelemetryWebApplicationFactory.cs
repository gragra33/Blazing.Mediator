using Microsoft.AspNetCore.Mvc.Testing;

namespace OpenTelemetryExample.Tests;

/// <summary>
/// Custom web application factory for OpenTelemetry example testing.
/// Handles the static Program class by creating a proper test host.
/// </summary>
public class OpenTelemetryWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        // Override services for testing if needed
        builder.ConfigureServices(services =>
        {
            // Add any test-specific service configurations here
            // For example, you could mock external dependencies
        });
        
        base.ConfigureWebHost(builder);
    }
}