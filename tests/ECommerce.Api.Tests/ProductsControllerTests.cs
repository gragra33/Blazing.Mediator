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
        HttpResponseMessage? response = await _client.GetAsync("/api/products/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        ProductDto? product = JsonSerializer.Deserialize<ProductDto>(content, _jsonOptions);

        product.ShouldNotBeNull();
        product!.Id.ShouldBe(1);
    }

    [Fact]
    public async Task GetProducts_WithDefaultParameters_ReturnsPagedResult()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        PagedResult<ProductDto>? result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Items.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task GetProducts_WithCustomParameters_ReturnsFilteredResult()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/products?page=1&pageSize=5&searchTerm=test&inStockOnly=true&activeOnly=true");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        PagedResult<ProductDto>? result = JsonSerializer.Deserialize<PagedResult<ProductDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Page.ShouldBe(1);
        result.PageSize.ShouldBe(5);
    }

    [Fact]
    public async Task GetLowStockProducts_WithDefaultThreshold_ReturnsLowStockProducts()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/products/low-stock");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        List<ProductDto>? products = JsonSerializer.Deserialize<List<ProductDto>>(content, _jsonOptions);

        products.ShouldNotBeNull();
        products!.ShouldAllBe(p => p.StockQuantity <= 10);
    }

    [Fact]
    public async Task GetLowStockProducts_WithCustomThreshold_ReturnsFilteredProducts()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/products/low-stock?threshold=5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        List<ProductDto>? products = JsonSerializer.Deserialize<List<ProductDto>>(content, _jsonOptions);

        products.ShouldNotBeNull();
        products!.ShouldAllBe(p => p.StockQuantity <= 5);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedWithId()
    {
        // Arrange
        CreateProductCommand? command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 29.99m,
            StockQuantity = 100
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        string? responseContent = await response.Content.ReadAsStringAsync();
        int productId = JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
        productId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsNoContent()
    {
        // Arrange
        UpdateProductCommand? command = new UpdateProductCommand
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 39.99m,
            StockQuantity = 150
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/products/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProductStock_WithValidData_ReturnsNoContent()
    {
        // Arrange
        UpdateProductStockCommand? command = new UpdateProductStockCommand
        {
            StockQuantity = 200
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/products/1/stock", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeactivateProduct_WithValidId_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/products/1/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    // Validation Tests
    [Fact]
    public async Task CreateProduct_WithInvalidData_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange
        CreateProductCommand? command = new CreateProductCommand
        {
            Name = "", // Invalid - empty name
            Description = "Invalid product",
            Price = -10.00m, // Invalid - negative price
            StockQuantity = -5 // Invalid - negative stock
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string? responseContent = await response.Content.ReadAsStringAsync();
        string[]? errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.ShouldNotBeNull();
        errors!.ShouldContain("Product name is required");
        errors.ShouldContain("Price must be greater than 0");
        errors.ShouldContain("Stock quantity must be 0 or greater");
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_ReturnsBadRequestWithValidationError()
    {
        // Arrange
        CreateProductCommand? command = new CreateProductCommand
        {
            Name = "", // Invalid - empty name
            Description = "Valid description",
            Price = 29.99m,
            StockQuantity = 100
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string? responseContent = await response.Content.ReadAsStringAsync();
        string[]? errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.ShouldNotBeNull();
        errors!.ShouldContain("Product name is required");
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidData_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange
        UpdateProductCommand? command = new UpdateProductCommand
        {
            Name = "", // Invalid - empty name
            Description = "Invalid update",
            Price = -5.00m, // Invalid - negative price
            StockQuantity = -10 // Invalid - negative stock
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/products/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string? responseContent = await response.Content.ReadAsStringAsync();
        string[]? errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.ShouldNotBeNull();
        errors!.ShouldContain("Product name is required");
        errors.ShouldContain("Product price must be greater than 0");
        errors.ShouldContain("Stock quantity cannot be negative");
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        UpdateProductCommand? command = new UpdateProductCommand
        {
            Name = "Non-existent Product",
            Description = "This should fail",
            Price = 99.99m,
            StockQuantity = 10
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/products/9999", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        string? responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("Product with ID 9999 not found");
    }

    [Fact]
    public async Task UpdateProductStock_WithNegativeStock_ReturnsBadRequestWithValidationError()
    {
        // Arrange
        UpdateProductStockCommand? command = new UpdateProductStockCommand
        {
            StockQuantity = -5 // Invalid - negative stock
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/products/1/stock", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        string? responseContent = await response.Content.ReadAsStringAsync();
        string[]? errors = JsonSerializer.Deserialize<string[]>(responseContent, _jsonOptions);
        
        errors.ShouldNotBeNull();
        errors!.ShouldContain("Stock quantity cannot be negative");
    }

    [Fact]
    public async Task UpdateProductStock_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        UpdateProductStockCommand? command = new UpdateProductStockCommand
        {
            StockQuantity = 100
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/products/9999/stock", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        string? responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("Product with ID 9999 not found");
    }
}