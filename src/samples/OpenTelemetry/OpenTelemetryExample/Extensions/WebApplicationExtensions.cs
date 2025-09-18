namespace OpenTelemetryExample.Extensions;

/// <summary>
/// Extension methods for WebApplication to configure the HTTP request pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for method chaining.</returns>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.ConfigureDevelopmentEnvironment();
        app.ConfigureMiddleware();
        app.ConfigureRouting();
        app.ConfigureEndpoints();

        return app;
    }

    /// <summary>
    /// Configures development-specific settings.
    /// </summary>
    private static void ConfigureDevelopmentEnvironment(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenTelemetry Example API v1");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableValidator();
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });
    }

    /// <summary>
    /// Configures middleware pipeline.
    /// </summary>
    private static void ConfigureMiddleware(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseCors();
    }

    /// <summary>
    /// Configures routing.
    /// </summary>
    private static void ConfigureRouting(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapControllers();
    }

    /// <summary>
    /// Configures minimal API endpoints.
    /// </summary>
    private static void ConfigureEndpoints(this WebApplication app)
    {
        app.MapTelemetryEndpoints();
        app.MapDebugEndpoints();
        app.MapTestingEndpoints();
    }
}