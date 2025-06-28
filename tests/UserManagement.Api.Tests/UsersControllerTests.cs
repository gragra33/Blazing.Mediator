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
        var response = await _client.GetAsync("/api/users/2");  // Use User ID 2 instead of 1

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);

        user.Should().NotBeNull();
        user!.Id.Should().Be(2);  // Expect User ID 2
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/users/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsers_WithDefaultParameters_ReturnsPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<UserDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetUsers_WithCustomParameters_ReturnsFilteredResult()
    {
        // Act
        var response = await _client.GetAsync("/api/users?page=1&pageSize=5&searchTerm=test&includeInactive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<UserDto>>(content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetActiveUsers_ReturnsListOfActiveUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/users/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<UserDto>>(content, _jsonOptions);

        users.Should().NotBeNull();
        users!.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task GetUserStatistics_WithValidId_ReturnsStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/users/2/statistics");  // Use User ID 2 instead of 1

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var statistics = JsonSerializer.Deserialize<UserStatisticsDto>(content, _jsonOptions);

        statistics.Should().NotBeNull();
        statistics!.UserId.Should().Be(2);  // Expect User ID 2
    }

    [Fact]
    public async Task GetUserStatistics_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/users/999/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "", // Invalid - empty
            LastName = "User",
            Email = "invalid-email", // Invalid format
            DateOfBirth = DateTime.Now.AddYears(1) // Invalid - future date
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserWithId_WithValidData_ReturnsCreatedWithId()
    {
        // Arrange
        var command = new CreateUserWithIdCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/with-id", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var userId = JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
        userId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = 1,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/users/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateUser_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = 2, // Different from URL parameter
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/users/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = 999,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/users/999", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserWithResult_WithValidData_ReturnsOkWithResult()
    {
        // Arrange
        var command = new UpdateUserWithResultCommand
        {
            UserId = 1,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        var json = JsonSerializer.Serialize(command, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/users/1/with-result", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OperationResult>(responseContent, _jsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/users/1?reason=Test deletion");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/users/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateUser_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.PostAsync("/api/users/1/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ActivateUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync("/api/users/999/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateUser_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.PostAsync("/api/users/1/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeactivateUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync("/api/users/999/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
