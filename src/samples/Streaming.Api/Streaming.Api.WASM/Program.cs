using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Streaming.Api.WASM;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Add HttpClient for API communication
// Point to the main Streaming.Api server for API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7021/") // Main API server HTTPS URL
});

await builder.Build().RunAsync();
