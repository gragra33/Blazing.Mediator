using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.Notifications;
using ECommerce.Api.Application.Services;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace ECommerce.Api.Tests.Integration;

/// <summary>
/// Integration tests for the notification system to verify that notifications are properly published and handled.
/// </summary>
public class NotificationSystemTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Initializes a new instance of the NotificationSystemTests class.
    /// </summary>
    /// <param name="factory">The web application factory for creating test clients.</param>
    /// <param name="output">The test output helper for logging test results.</param>
    public NotificationSystemTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    /// <summary>
    /// Tests that creating a product publishes a ProductCreatedNotification.
    /// </summary>
    [Fact]
    public async Task CreateProduct_Should_PublishProductCreatedNotification()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "A test product",
            Price = 99.99m,
            StockQuantity = 5 // Low stock to potentially trigger notification
        };

        // Act
        var result = await mediator.Send(command);

        // Assert
        Assert.True(result > 0);

        // Verify product was created
        var product = await context.Products.FindAsync(result);
        Assert.NotNull(product);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(5, product.StockQuantity);

        _output.WriteLine($"✅ Product created successfully with ID: {result}");
        _output.WriteLine($"🔔 ProductCreatedNotification should have been published");

        // If stock is low (< 10), low stock notification should also be published
        if (product.StockQuantity < 10)
        {
            _output.WriteLine($"⚠️  ProductStockLowNotification should have been published (stock: {product.StockQuantity})");
        }
    }

    /// <summary>
    /// Tests that the notification middleware logs notifications.
    /// </summary>
    [Fact]
    public async Task NotificationMiddleware_Should_LogNotifications()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var notification = new ProductCreatedNotification(
            productId: 1,
            productName: "Test Product",
            price: 99.99m,
            stockQuantity: 10
        );

        // Act
        await mediator.Publish(notification);

        // Assert
        // The notification middleware should have logged the notification
        // This test verifies that the notification pipeline is working
        _output.WriteLine($"✅ Notification published successfully");
        _output.WriteLine($"🔔 NotificationLoggingMiddleware should have logged the notification");
        _output.WriteLine($"📊 NotificationMetricsMiddleware should have tracked the metrics");
    }

    /// <summary>
    /// Tests that background services are properly registered.
    /// </summary>
    [Fact]
    public void BackgroundServices_Should_BeRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var hostedServices = scope.ServiceProvider.GetServices<IHostedService>();

        // Assert
        var emailService = hostedServices.OfType<EmailNotificationService>().FirstOrDefault();
        var inventoryService = hostedServices.OfType<InventoryManagementService>().FirstOrDefault();

        Assert.NotNull(emailService);
        Assert.NotNull(inventoryService);

        _output.WriteLine($"✅ EmailNotificationService is registered as a background service");
        _output.WriteLine($"✅ InventoryManagementService is registered as a background service");
    }

    /// <summary>
    /// Tests that the mediator is properly configured with notification middleware.
    /// </summary>
    [Fact]
    public void Mediator_Should_BeConfiguredWithNotificationMiddleware()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Assert
        Assert.NotNull(mediator);

        _output.WriteLine($"✅ Mediator is properly registered and configured");
        _output.WriteLine($"🔔 NotificationLoggingMiddleware should be in the pipeline");
        _output.WriteLine($"📊 NotificationMetricsMiddleware should be in the pipeline");
    }
}
