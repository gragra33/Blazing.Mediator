using UserManagement.Api.Endpoints;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Extensions;

/// <summary>
/// Extension methods for WebApplication to configure the HTTP request pipeline.
/// Follows single responsibility principle by separating pipeline configuration.
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
        app.ConfigureEndpoints();
        
        return app;
    }

    /// <summary>
    /// Configures development-specific settings.
    /// </summary>
    private static void ConfigureDevelopmentEnvironment(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        app.UseSwagger();
        app.UseSwaggerUI();
        app.EnsureDatabaseCreated();
    }

    /// <summary>
    /// Configures middleware pipeline.
    /// </summary>
    private static void ConfigureMiddleware(this WebApplication app)
    {
        app.UseHttpsRedirection();
    }

    /// <summary>
    /// Configures API endpoints following interface segregation principle.
    /// </summary>
    private static void ConfigureEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/users")
            .WithTags("Users");

        // Separate query and command endpoints for better organization
        api.MapUserQueryEndpoints();
        api.MapUserCommandEndpoints();
    }

    /// <summary>
    /// Ensures database is created and seeded in development.
    /// </summary>
    private static void EnsureDatabaseCreated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
        context.Database.EnsureCreated();
    }
}
