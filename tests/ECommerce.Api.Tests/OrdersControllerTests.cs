using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerce.Api.Tests;

/// <summary>
/// Integration tests for OrdersController endpoints
/// </summary>
public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsOkWithOrder()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<OrderDto>(content, _jsonOptions);

        order.Should().NotBeNull();
        order!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetOrders_WithDefaultParameters_ReturnsPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<OrderDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetOrders_WithCustomParameters_ReturnsFilteredResult()
    {
        // Act
        var response = await _client.GetAsync("/api/orders?page=1&pageSize=5&customerId=1&status=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<OrderDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetCustomerOrders_WithValidCustomerId_ReturnsOrders()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/customer/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderDto>>(content, _jsonOptions);

        orders.Should().NotBeNull();
        orders!.Should().OnlyContain(o => o.CustomerId == 1);
    }

    [Fact]
    public async Task GetCustomerOrders_WithDateRange_ReturnsFilteredOrders()
    {
        // Arrange
        var fromDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
        var toDate = DateTime.Today.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/1?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderDto>>(content, _jsonOptions);

        orders.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrderStatistics_WithoutDateRange_ReturnsStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/orders/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var statistics = JsonSerializer.Deserialize<OrderStatisticsDto>(content, _jsonOptions);

        statistics.Should().NotBeNull();
        statistics!.TotalOrders.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetOrderStatistics_WithDateRange_ReturnsFilteredStatistics()
    {
        // Arrange
        var fromDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
        var toDate = DateTime.Today.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/orders/statistics?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var statistics = JsonSerializer.Deserialize<OrderStatisticsDto>(content, _jsonOptions);

        statistics.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedWithResult()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = 1,
            CustomerEmail = "test@example.com",
            ShippingAddress = "123 Test Street, Test City, TC 12345",
            Items = [new() { ProductId = 2, Quantity = 1 }]
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Debug: Log response content for debugging
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CreateOrder validation error: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = 0, // Invalid customer ID
            CustomerEmail = "invalid-email", // Invalid email format
            ShippingAddress = "", // Invalid - empty address
            Items = [] // Invalid - empty items
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.Errors.Should().NotBeNull();
        result.Errors!.Should().Contain("Customer ID must be greater than 0");
        result.Errors.Should().Contain("Invalid email format");
        result.Errors.Should().Contain("Shipping address is required");
        result.Errors.Should().Contain("Order must contain at least one item");
    }

    [Fact]
    public async Task ProcessOrder_WithValidData_ReturnsOkWithResult()
    {
        // Arrange
        var command = new ProcessOrderCommand
        {
            CustomerId = 1,
            CustomerEmail = "test@example.com",
            ShippingAddress = "123 Test Street, Test City, TC 12345",
            Items = [new() { ProductId = 3, Quantity = 1 }],
            PaymentMethod = "Credit Card"
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders/process", content);

        // Debug: Log response content for debugging
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ProcessOrder validation error: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult<ProcessOrderResponse>>(responseContent, _jsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var command = new UpdateOrderStatusCommand
        {
            Status = OrderStatus.Processing
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/orders/1/status", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CancelOrder_WithValidId_ReturnsOkWithResult()
    {
        // Arrange
        var command = new CancelOrderCommand
        {
            Reason = "Customer request"
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders/1/cancel", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult<bool>>(responseContent, _jsonOptions);
        result.Should().NotBeNull();
    }

    // Additional validation and error scenario tests
    [Fact]
    public async Task CreateOrder_WithNonExistentProduct_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = 999,
            CustomerEmail = "test@example.com",
            ShippingAddress = "123 Test St",
            Items = [
                new OrderItemRequest
                {
                    ProductId = 9999, // Non-existent product
                    Quantity = 1
                }
            ]
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Contain("Product with ID 9999 not found or inactive");
    }

    [Fact]
    public async Task CreateOrder_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = 123,
            CustomerEmail = "invalid-email", // Invalid email format
            ShippingAddress = "123 Test St",
            Items = [
                new OrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 1
                }
            ]
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.Errors.Should().Contain("Invalid email format");
    }

    [Fact]
    public async Task CancelOrder_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        var command = new CancelOrderCommand
        {
            Reason = "Test cancellation"
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders/9999/cancel", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult<bool>>(responseContent, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Order with ID 9999 not found");
    }
}