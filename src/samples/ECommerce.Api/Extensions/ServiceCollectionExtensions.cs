using Blazing.Mediator;
using ECommerce.Api.Application.Middleware;
using ECommerce.Api.Application.Services;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Extensions;

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
        services.AddNotificationServices();

        return services;
    }

    /// <summary>
    /// Registers API-related services.
    /// </summary>
    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();
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
            services.AddDbContext<ECommerceDbContext>(options =>
                options.UseInMemoryDatabase("ECommerceDb"));
        }
        else
        {
            // Use SQL Server for production
            services.AddDbContext<ECommerceDbContext>(options =>
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
    /// Registers Mediator with CQRS handlers and conditional middleware.
    /// </summary>
    private static IServiceCollection AddMediatorServices(this IServiceCollection services)
    {
        services.AddMediator(config =>
        {
            // Add conditional middleware for order operations
            config.AddMiddleware(typeof(OrderLoggingMiddleware<,>));
            
            // Add conditional middleware for product operations  
            config.AddMiddleware(typeof(ProductLoggingMiddleware<,>));

            // Add notification middleware for logging and metrics
            config.AddNotificationMiddleware<NotificationLoggingMiddleware>();
            config.AddNotificationMiddleware<NotificationMetricsMiddleware>();
        }, typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }

    /// <summary>
    /// Registers notification-related background services.
    /// </summary>
    private static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        // Register background services for handling notifications
        services.AddHostedService<EmailNotificationService>();
        services.AddHostedService<InventoryManagementService>();

        return services;
    }
}
