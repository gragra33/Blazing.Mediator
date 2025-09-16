using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Streaming.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory for Streaming.Api integration tests
/// </summary>
public class StreamingApiWebApplicationFactory : WebApplicationFactory<Streaming.Api.Components.App>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override services for testing if needed
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Warning);
        });

        builder.UseEnvironment("Testing");

        // Configure the web root path to point to the actual Streaming.Api application's wwwroot
        // This ensures the integration tests can find the Mock_Contacts.json file
        var streamingApiPath = FindStreamingApiPath();
        var webRootPath = Path.Combine(streamingApiPath, "wwwroot");

        if (Directory.Exists(webRootPath))
        {
            builder.UseWebRoot(webRootPath);
        }
    }

    private static string FindStreamingApiPath()
    {
        // Start from current directory and navigate to find the Streaming.Api project
        var currentDir = Directory.GetCurrentDirectory();
        var searchDir = currentDir;

        // Navigate up to find the repository root (containing src folder)
        while (searchDir != null && !Directory.Exists(Path.Combine(searchDir, "src")))
        {
            searchDir = Directory.GetParent(searchDir)?.FullName;
        }

        if (searchDir == null)
        {
            throw new DirectoryNotFoundException("Could not find repository root containing 'src' folder");
        }

        // Build path to Streaming.Api project
        var streamingApiPath = Path.Combine(searchDir, "src", "samples", "Streaming.Api", "Streaming.Api");

        if (!Directory.Exists(streamingApiPath))
        {
            throw new DirectoryNotFoundException($"Could not find Streaming.Api project at: {streamingApiPath}");
        }

        return streamingApiPath;
    }
}
