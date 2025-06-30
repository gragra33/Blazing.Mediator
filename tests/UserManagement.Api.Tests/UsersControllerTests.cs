using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Queries;

namespace UserManagement.Api.Tests;

/// <summary>
/// Integration tests for UsersController endpoints
/// </summary>
public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsOkWithUser()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/users/2");  // Use User ID 2 instead of 1

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        UserDto? user = JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);

        user.ShouldNotBeNull();
        user!.Id.ShouldBe(2);  // Expect User ID 2
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/users/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsers_WithDefaultParameters_ReturnsPagedResult()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        PagedResult<UserDto>? result = JsonSerializer.Deserialize<PagedResult<UserDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Items.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task GetUsers_WithCustomParameters_ReturnsFilteredResult()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/users?page=1&pageSize=5&searchTerm=test&includeInactive=true");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        PagedResult<UserDto>? result = JsonSerializer.Deserialize<PagedResult<UserDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Page.ShouldBe(1);
        result.PageSize.ShouldBe(5);
    }

    [Fact]
    public async Task GetActiveUsers_ReturnsListOfActiveUsers()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/users/active");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        List<UserDto>? users = JsonSerializer.Deserialize<List<UserDto>>(content, _jsonOptions);

        users.ShouldNotBeNull();
        users!.ShouldAllBe(u => u.IsActive);
    }

    [Fact]
    public async Task GetUserStatistics_WithValidId_ReturnsStatistics()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/users/2/statistics");  // Use User ID 2 instead of 1

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? content = await response.Content.ReadAsStringAsync();
        UserStatisticsDto? statistics = JsonSerializer.Deserialize<UserStatisticsDto>(content, _jsonOptions);

        statistics.ShouldNotBeNull();
        statistics!.UserId.ShouldBe(2);  // Expect User ID 2
    }

    [Fact]
    public async Task GetUserStatistics_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage? response = await _client.GetAsync("/api/users/999/statistics");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        CreateUserCommand? command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        CreateUserCommand? command = new CreateUserCommand
        {
            FirstName = "", // Invalid - empty
            LastName = "User",
            Email = "invalid-email", // Invalid format
            DateOfBirth = DateTime.Now.AddYears(1) // Invalid - future date
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserWithId_WithValidData_ReturnsCreatedWithId()
    {
        // Arrange
        CreateUserWithIdCommand? command = new CreateUserWithIdCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/users/with-id", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        string? responseContent = await response.Content.ReadAsStringAsync();
        int userId = JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
        userId.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsNoContent()
    {
        // Arrange
        UpdateUserCommand? command = new UpdateUserCommand
        {
            UserId = 1,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/users/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUser_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        UpdateUserCommand? command = new UpdateUserCommand
        {
            UserId = 2, // Different from URL parameter
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/users/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        UpdateUserCommand? command = new UpdateUserCommand
        {
            UserId = 999,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/users/999", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserWithResult_WithValidData_ReturnsOkWithResult()
    {
        // Arrange
        UpdateUserWithResultCommand? command = new UpdateUserWithResultCommand
        {
            UserId = 1,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string? json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent? content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage? response = await _client.PutAsync("/api/users/1/with-result", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string? responseContent = await response.Content.ReadAsStringAsync();
        OperationResult? result = JsonSerializer.Deserialize<OperationResult>(responseContent, _jsonOptions);
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage? response = await _client.DeleteAsync("/api/users/1?reason=Test deletion");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage? response = await _client.DeleteAsync("/api/users/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateUser_WithValidId_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/users/1/activate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ActivateUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/users/999/activate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateUser_WithValidId_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/users/1/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeactivateUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage? response = await _client.PostAsync("/api/users/999/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
