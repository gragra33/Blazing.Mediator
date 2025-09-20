using Blazing.Mediator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UserManagement.Api.Application.Middleware;
using UserManagement.Api.Infrastructure.Data;
using UserManagement.Api.Middleware;
using UserManagement.Api.Services;

namespace UserManagement.Api.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register application services.
/// Follows single responsibility principle by separating service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all application services to the container following dependency inversion principle.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddHttpServices();
        services.AddDatabaseServices(configuration, environment);
        services.AddMediatorServices();
        services.AddValidationServices();
        services.AddStatisticsServices();

        return services;
    }

    /// <summary>
    /// Adds HTTP-related services.
    /// </summary>
    private static void AddHttpServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();

        // Add session services for statistics tracking
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(2);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = "UserManagementApi.Session";
        });
    }

    /// <summary>
    /// Adds database services following interface segregation principle.
    /// </summary>
    private static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddDbContext<UserManagementDbContext>(options => options.UseInMemoryDatabase("UserManagementDb"));
        }
        else
        {
            services.AddDbContext<UserManagementDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }
    }

    /// <summary>
    /// Adds mediator services with auto-discovery and middleware registration.
    /// </summary>
    private static void AddMediatorServices(this IServiceCollection services)
    {
        services.AddMediator(config =>
        {
            // Keep existing middleware that was working - use typeof for generic middleware
            config.AddMiddleware(typeof(GeneralLoggingMiddleware<,>));
            config.AddMiddleware(typeof(GeneralCommandLoggingMiddleware<>));

            // Add statistics tracking middleware for both typed and void requests
            config.AddMiddleware(typeof(StatisticsTrackingMiddleware<,>));
            config.AddMiddleware(typeof(StatisticsTrackingVoidMiddleware<>));
        }, Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Adds FluentValidation services following dependency inversion principle.
    /// </summary>
    private static void AddValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Adds statistics tracking services.
    /// </summary>
    private static void AddStatisticsServices(this IServiceCollection services)
    {
        // Register statistics tracking services
        services.AddSingleton<MediatorStatisticsTracker>();

        // Register background cleanup service
        services.AddHostedService<StatisticsCleanupService>();
    }
}
