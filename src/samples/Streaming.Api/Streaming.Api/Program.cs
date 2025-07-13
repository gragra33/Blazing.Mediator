using Blazing.Mediator;
using Streaming.Api.Client.Pages;
using Streaming.Api.Components;
using Streaming.Api.Endpoints;
using Streaming.Api.Services;
using Streaming.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Blazing.Mediator with streaming support and middleware
builder.Services.AddMediator(config =>
{
    // Add streaming middleware for logging - only applies to IStreamRequest<T>
    config.AddMiddleware(typeof(StreamingLoggingMiddleware<,>));
}, typeof(Program));

// Add custom services
builder.Services.AddScoped<IContactService, ContactService>();

// Add HttpContextAccessor for antiforgery token access
builder.Services.AddHttpContextAccessor();

// Add HttpClient for components that need it
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// Add CORS for API access
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Streaming API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAntiforgery();

app.MapStaticAssets();

// Map API endpoints
app.MapContactEndpoints();

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Streaming.Api.Client._Imports).Assembly);

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }
