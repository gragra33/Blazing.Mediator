using Blazing.Mediator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Middleware;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register application services.
/// Follows dependency inversion principle by abstracting service registration logic.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services including database, validation, and mediator.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddApiServices();
        services.AddDatabaseServices(configuration, environment);
        services.AddValidationServices();
        services.AddMediatorServices();

        return services;
    }

    /// <summary>
    /// Registers API-related services.
    /// </summary>
    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    /// <summary>
    /// Registers database services with environment-specific configurations.
    /// </summary>
    private static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Use In-Memory database for development/demo
            services.AddDbContext<UserManagementDbContext>(options =>
                options.UseInMemoryDatabase("UserManagementDb"));
        }
        else
        {
            // Use SQL Server for production
            services.AddDbContext<UserManagementDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        return services;
    }

    /// <summary>
    /// Registers FluentValidation services.
    /// </summary>
    private static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        return services;
    }

    /// <summary>
    /// Registers Mediator with CQRS handlers and general logging middleware.
    /// </summary>
    private static IServiceCollection AddMediatorServices(this IServiceCollection services)
    {
        services.AddMediator(config =>
        {
            // Add general logging middleware for all requests (queries with responses)
            config.AddMiddleware(typeof(GeneralLoggingMiddleware<,>));
            // Add general logging middleware for all commands (void commands)
            config.AddMiddleware(typeof(GeneralCommandLoggingMiddleware<>));
        }, typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}
