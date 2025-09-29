using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Streaming.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory for Streaming.Api integration tests
/// </summary>
public class StreamingApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override services for testing if needed
            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Warning);
        });

        builder.UseEnvironment("Testing");

        // Configure the web root path to point to the test project's root directory
        // This ensures the integration tests use the test data at tests/Streaming.Api.Tests/data/Mock_Contacts.json
        var testProjectPath = FindTestProjectPath();
        
        if (Directory.Exists(testProjectPath))
        {
            builder.UseWebRoot(testProjectPath);
        }

        // Configure static web assets for testing
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Disable static web assets development features that cause ETag issues in tests
            context.HostingEnvironment.EnvironmentName = "Testing";
        });
    }

    private static string FindTestProjectPath()
    {
        // Start from current directory and navigate to find the test project root
        var currentDir = Directory.GetCurrentDirectory();
        var searchDir = currentDir;

        // Navigate up to find the test project directory (containing data folder)
        while (searchDir != null && Path.GetFileName(searchDir) != "Streaming.Api.Tests")
        {
            searchDir = Directory.GetParent(searchDir)?.FullName;
        }

        if (searchDir == null)
        {
            throw new DirectoryNotFoundException("Could not find Streaming.Api.Tests project directory");
        }

        // Verify the test data file exists
        var testDataPath = Path.Combine(searchDir, "data", "Mock_Contacts.json");
        if (!File.Exists(testDataPath))
        {
            throw new FileNotFoundException($"Test data file not found at: {testDataPath}");
        }

        return searchDir;
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
