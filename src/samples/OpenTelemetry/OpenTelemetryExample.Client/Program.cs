using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpenTelemetryExample.Client;
using OpenTelemetryExample.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to the API server
var apiBaseUrl = GetApiBaseUrl(builder.Configuration, builder.HostEnvironment);

Console.WriteLine($"[*] Blazor Client starting...");
Console.WriteLine($"[*] API Base URL: {apiBaseUrl}");
Console.WriteLine($"[*] Running in: {(builder.HostEnvironment.IsDevelopment() ? "Development" : "Production")} mode");

// Use AddHttpClient for DI and configuration
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register a typed HttpClient for DI usage
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddScoped<IApiConnectivityService, ApiConnectivityService>();

// Add logging for debugging
builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
});

var app = builder.Build();

// Test API connectivity on startup in development
if (builder.HostEnvironment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var connectivityService = scope.ServiceProvider.GetRequiredService<IApiConnectivityService>();

        Console.WriteLine("[*] Testing API connectivity...");
        var isConnected = await connectivityService.TestApiConnectivityAsync();

        if (isConnected)
        {
            Console.WriteLine("[+] API connectivity test: SUCCESS");
        }
        else
        {
            Console.WriteLine("[-] API connectivity test: FAILED");
            Console.WriteLine("[!] Please check that the API server is running and accessible");
            Console.WriteLine($"[!] Expected API URL: {apiBaseUrl}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[-] API connectivity test error: {ex.Message}");
    }
}

await app.RunAsync();

// Helper method to determine API base URL with proper configuration hierarchy
static string GetApiBaseUrl(IConfiguration configuration, IWebAssemblyHostEnvironment environment)
{
    Console.WriteLine("[*] API URL Resolution Process:");

    // 1. Aspire service discovery (highest priority when running through Aspire)
    var aspireHttpsUrl = configuration.GetValue<string>("services:OpenTelemetry-api-server:https:0");
    if (!string.IsNullOrEmpty(aspireHttpsUrl))
    {
        Console.WriteLine($"[*] ? Found Aspire HTTPS service URL: {aspireHttpsUrl}");
        return aspireHttpsUrl;
    }
    Console.WriteLine("[*] ? Aspire HTTPS service URL not found");

    var aspireHttpUrl = configuration.GetValue<string>("services:OpenTelemetry-api-server:http:0");
    if (!string.IsNullOrEmpty(aspireHttpUrl))
    {
        Console.WriteLine($"[*] ? Found Aspire HTTP service URL: {aspireHttpUrl}");
        return aspireHttpUrl;
    }
    Console.WriteLine("[*] ? Aspire HTTP service URL not found");

    // 2. Application configuration (appsettings.json)
    var configApiUrl = configuration.GetValue<string>("ApiBaseUrl");
    if (!string.IsNullOrEmpty(configApiUrl))
    {
        Console.WriteLine($"[*] ? Found configured API URL: {configApiUrl}");
        return configApiUrl;
    }
    Console.WriteLine("[*] ? Configured API URL not found");

    // 3. Environment-specific defaults
    if (environment.IsDevelopment())
    {
        // In development, try the port that's currently shown in your Swagger UI
        var developmentUrl = aspireHttpsUrl;
        Console.WriteLine($"[*] ? Using development API URL: {developmentUrl}");
        Console.WriteLine("[!] Note: If API server is on a different port, update appsettings.Development.json");
        return developmentUrl ?? "https://localhost:64688";
    }

    // 4. Production default
    var productionUrl = "https://api.opentelemetryexample.com"; // Update this for production
    Console.WriteLine($"[*] ? Using production API URL: {productionUrl}");
    return productionUrl;
}