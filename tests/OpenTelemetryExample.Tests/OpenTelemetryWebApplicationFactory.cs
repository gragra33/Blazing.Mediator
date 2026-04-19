using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetryExample.Infrastructure.Data;

namespace OpenTelemetryExample.Tests;

/// <summary>
/// Custom web application factory for OpenTelemetry example testing.
/// Handles the static Program class by creating a proper test host.
/// </summary>
/// <remarks>
/// Each factory instance uses an isolated in-memory database (unique name per instance).
/// This prevents <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.EnsureDeletedAsync"/>
/// in one factory's startup from destroying seeded data in another concurrently-initializing
/// factory — the root cause of flaky test failures when multiple <c>IClassFixture&lt;T&gt;</c>
/// classes start in parallel and all share the same named EF Core in-memory database.
/// </remarks>
public class OpenTelemetryWebApplicationFactory : WebApplicationFactory<Program>
{
    // Unique per-instance name so concurrent factory startups each operate on their own
    // isolated in-memory database rather than racing to delete/recreate a shared one.
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace the shared named in-memory database with a per-factory isolated one.
            // RemoveAll removes every DbContextOptions<ApplicationDbContext> descriptor so
            // the subsequent AddDbContext registration is the sole authoritative one.
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });
        });

        base.ConfigureWebHost(builder);
    }
}