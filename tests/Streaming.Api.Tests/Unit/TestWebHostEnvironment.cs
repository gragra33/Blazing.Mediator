using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Streaming.Api.Tests.Unit;

/// <summary>
/// Test implementation of IWebHostEnvironment for unit testing
/// </summary>
public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Testing";
    public string ApplicationName { get; set; } = "Streaming.Api.Tests";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

    // Point to the test project root directory so ContactService can find "data/Mock_Contacts.json"
    public string WebRootPath { get; set; } = GetTestProjectRoot();
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

    private static string GetTestProjectRoot()
    {
        // Navigate from bin/Debug/net9.0 back to project root
        var currentDir = Directory.GetCurrentDirectory();
        var projectRoot = currentDir;

        // If we're in a bin directory, go up to find the project root
        while (Path.GetFileName(projectRoot) != "Streaming.Api.Tests" &&
               Directory.GetParent(projectRoot) != null)
        {
            projectRoot = Directory.GetParent(projectRoot)!.FullName;
        }

        return projectRoot;
    }
}