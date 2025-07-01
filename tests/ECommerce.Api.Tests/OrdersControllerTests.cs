using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerce.Api.Tests;

/// <summary>
/// Integration tests for OrdersController endpoints.
/// Tests all order-related API endpoints including CRUD operations, filtering, statistics, and validation scenarios.
/// </summary>
public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the OrdersControllerTests class.
    /// Sets up the test client and JSON serialization options for API testing.
    /// </summary>
    /// <param name="factory">The web application factory for creating test clients.</param>
    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Tests that getting an order by valid ID returns OK status with order details.
    /// </summary>
    [Fact]
    public async Task GetOrder_WithValidId_ReturnsOkWithOrder()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/orders/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        OrderDto? order = JsonSerializer.Deserialize<OrderDto>(content, _jsonOptions);

        order.ShouldNotBeNull();
        order!.Id.ShouldBe(1);
    }

    /// <summary>
    /// Tests that getting orders with default parameters returns a paginated result with default pagination settings.
    /// </summary>
    [Fact]
    public async Task GetOrders_WithDefaultParameters_ReturnsPagedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PagedResult<OrderDto>? result = JsonSerializer.Deserialize<PagedResult<OrderDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Items.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
    }

    /// <summary>
    /// Tests that getting orders with custom pagination and filtering parameters returns filtered results.
    /// </summary>
    [Fact]
    public async Task GetOrders_WithCustomParameters_ReturnsFilteredResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/orders?page=1&pageSize=5&customerId=1&status=1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PagedResult<OrderDto>? result = JsonSerializer.Deserialize<PagedResult<OrderDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Page.ShouldBe(1);
        result.PageSize.ShouldBe(5);
    }

    /// <summary>
    /// Tests that getting orders for a specific customer ID returns only orders for that customer.
    /// </summary>
    [Fact]
    public async Task GetCustomerOrders_WithValidCustomerId_ReturnsOrders()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/orders/customer/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        List<OrderDto>? orders = JsonSerializer.Deserialize<List<OrderDto>>(content, _jsonOptions);

        orders.ShouldNotBeNull();
        orders!.ShouldAllBe(o => o.CustomerId == 1);
    }

    /// <summary>
    /// Tests that getting customer orders with a date range returns orders filtered by the specified date range.
    /// </summary>
    [Fact]
    public async Task GetCustomerOrders_WithDateRange_ReturnsFilteredOrders()
    {
        // Arrange
        string fromDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
        string toDate = DateTime.Today.ToString("yyyy-MM-dd");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/orders/customer/1?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        List<OrderDto>? orders = JsonSerializer.Deserialize<List<OrderDto>>(content, _jsonOptions);

        orders.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that getting order statistics without date range returns overall statistics.
    /// </summary>
    [Fact]
    public async Task GetOrderStatistics_WithoutDateRange_ReturnsStatistics()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/orders/statistics");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        OrderStatisticsDto? statistics = JsonSerializer.Deserialize<OrderStatisticsDto>(content, _jsonOptions);

        statistics.ShouldNotBeNull();
        statistics!.TotalOrders.ShouldBeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Tests that getting order statistics with date range returns statistics filtered by the specified date range.
    /// </summary>
    [Fact]
    public async Task GetOrderStatistics_WithDateRange_ReturnsFilteredStatistics()
    {
        // Arrange
        string fromDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
        string toDate = DateTime.Today.ToString("yyyy-MM-dd");

        // Act
        HttpResponseMessage response = await _client.GetAsync($"/api/orders/statistics?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        OrderStatisticsDto? statistics = JsonSerializer.Deserialize<OrderStatisticsDto>(content, _jsonOptions);

        statistics.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that creating an order with valid data returns Created status with the new order ID.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreatedWithResult()
    {
        // Arrange
        CreateOrderCommand command = new()
        {
            CustomerId = 1,
            CustomerEmail = "test@example.com",
            ShippingAddress = "123 Test Street, Test City, TC 12345",
            Items = [new() { ProductId = 2, Quantity = 1 }]
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/orders", content);

        // Debug: Log response content for debugging
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CreateOrder validation error: {errorContent}");
        }

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult<int>? result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        result.ShouldNotBeNull();
        result!.Success.ShouldBeTrue();
        result.Data.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Tests that creating an order with invalid data returns BadRequest status with validation errors.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        CreateOrderCommand command = new()
        {
            CustomerId = 0, // Invalid customer ID
            CustomerEmail = "invalid-email", // Invalid email format
            ShippingAddress = "", // Invalid - empty address
            Items = [] // Invalid - empty items
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult<int>? result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldBe("Validation failed");
        result.Errors.ShouldNotBeNull();
        result.Errors!.ShouldContain("Customer ID must be greater than 0");
        result.Errors.ShouldContain("Invalid email format");
        result.Errors.ShouldContain("Shipping address is required");
        result.Errors.ShouldContain("Order must contain at least one item");
    }

    /// <summary>
    /// Tests that processing an order with valid data returns OK status with processing result.
    /// </summary>
    [Fact]
    public async Task ProcessOrder_WithValidData_ReturnsOkWithResult()
    {
        // Arrange
        ProcessOrderCommand command = new()
        {
            CustomerId = 1,
            CustomerEmail = "test@example.com",
            ShippingAddress = "123 Test Street, Test City, TC 12345",
            Items = [new() { ProductId = 3, Quantity = 1 }],
            PaymentMethod = "Credit Card"
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/orders/process", content);

        // Debug: Log response content for debugging
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ProcessOrder validation error: {errorContent}");
        }

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult<ProcessOrderResponse>? result = JsonSerializer.Deserialize<OperationResult<ProcessOrderResponse>>(responseContent, _jsonOptions);
        result.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that updating order status with valid data returns NoContent status indicating successful update.
    /// </summary>
    [Fact]
    public async Task UpdateOrderStatus_WithValidData_ReturnsNoContent()
    {
        // Arrange
        UpdateOrderStatusCommand command = new()
        {
            Status = OrderStatus.Processing
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/orders/1/status", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that canceling an order with valid ID returns OK status with cancellation result.
    /// </summary>
    [Fact]
    public async Task CancelOrder_WithValidId_ReturnsOkWithResult()
    {
        // Arrange
        CancelOrderCommand command = new()
        {
            Reason = "Customer request"
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/orders/1/cancel", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult<bool>? result = JsonSerializer.Deserialize<OperationResult<bool>>(responseContent, _jsonOptions);
        result.ShouldNotBeNull();
    }

    // Additional validation and error scenario tests
    
    /// <summary>
    /// Tests that creating an order with a non-existent product returns BadRequest status with appropriate error message.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithNonExistentProduct_ReturnsBadRequest()
    {
        // Arrange
        CreateOrderCommand command = new()
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
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult<int>? result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldContain("Product with ID 9999 not found or inactive");
    }

    /// <summary>
    /// Tests that creating an order with invalid email format returns BadRequest status with validation errors.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        CreateOrderCommand command = new()
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
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/orders", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult<int>? result = JsonSerializer.Deserialize<OperationResult<int>>(responseContent, _jsonOptions);
        
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldBe("Validation failed");
        result.Errors.ShouldContain("Invalid email format");
    }

    /// <summary>
    /// Tests that canceling a non-existent order returns BadRequest status with appropriate error message.
    /// </summary>
    [Fact]
    public async Task CancelOrder_WithNonExistentId_ReturnsBadRequest()
    {
        // Arrange
        CancelOrderCommand command = new()
        {
            Reason = "Test cancellation"
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/orders/9999/cancel", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult<bool>? result = JsonSerializer.Deserialize<OperationResult<bool>>(responseContent, _jsonOptions);
        
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldBe("Order with ID 9999 not found");
    }
}