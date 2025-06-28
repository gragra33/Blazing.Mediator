using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerce.Api.Tests;

/// <summary>
/// Integration tests for ProductsController endpoints
/// </summary>
public class ProductsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsOkWithProduct()
    {
        // Act
        var response = await _client.GetAsync("/api/products/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<ProductDto>(content, _jsonOptions);

        product.Should().NotBeNull();
        product!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetProducts_WithDefaultParameters_ReturnsPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetProducts_WithCustomParameters_ReturnsFilteredResult()
    {
        // Act
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5&searchTerm=test&inStockOnly=true&activeOnly=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetLowStockProducts_WithDefaultThreshold_ReturnsLowStockProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/low-stock");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductDto>>(content, _jsonOptions);

        products.Should().NotBeNull();
        products!.Should().OnlyContain(p => p.StockQuantity <= 10);
    }

    [Fact]
    public async Task GetLowStockProducts_WithCustomThreshold_ReturnsFilteredProducts()
    {
        // Act
        var response = await _client.GetAsync("/api/products/low-stock?threshold=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<ProductDto>>(content, _jsonOptions);

        products.Should().NotBeNull();
        products!.Should().OnlyContain(p => p.StockQuantity <= 5);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedWithId()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            StockQuantity = 100
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var productId = JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
        productId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 39.99m,
            StockQuantity = 150
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/products/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProductStock_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var command = new UpdateProductStockCommand
        {
            StockQuantity = 200
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/products/1/stock", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeactivateProduct_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.PostAsync("/api/products/1/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // Validation Tests
    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "", // Invalid - empty name
            Description = "Invalid product",
            Price = -10.00m, // Invalid - negative price
            StockQuantity = -5 // Invalid - negative stock
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.Should().NotBeNull();
        errors!.Should().Contain("Product name is required");
        errors.Should().Contain("Price must be greater than 0");
        errors.Should().Contain("Stock quantity must be 0 or greater");
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_ReturnsBadRequestWithValidationError()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "", // Invalid - empty name
            Description = "Valid description",
            Price = 29.99m,
            StockQuantity = 100
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.Should().NotBeNull();
        errors!.Should().Contain("Product name is required");
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidData_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Name = "", // Invalid - empty name
            Description = "Invalid update",
            Price = -5.00m, // Invalid - negative price
            StockQuantity = -10 // Invalid - negative stock
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/products/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.Should().NotBeNull();
        errors!.Should().Contain("Product name is required");
        errors.Should().Contain("Product price must be greater than 0");
        errors.Should().Contain("Stock quantity cannot be negative");
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Name = "Non-existent Product",
            Description = "This should fail",
            Price = 99.99m,
            StockQuantity = 10
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/products/9999", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Product with ID 9999 not found");
    }

    [Fact]
    public async Task UpdateProductStock_WithNegativeStock_ReturnsBadRequestWithValidationError()
    {
        // Arrange
        var command = new UpdateProductStockCommand
        {
            StockQuantity = -5 // Invalid - negative stock
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/products/1/stock", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        var errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.Should().NotBeNull();
        errors!.Should().Contain("Stock quantity cannot be negative");
    }

    [Fact]
    public async Task UpdateProductStock_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var command = new UpdateProductStockCommand
        {
            StockQuantity = 100
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/products/9999/stock", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("Product with ID 9999 not found");
    }
}