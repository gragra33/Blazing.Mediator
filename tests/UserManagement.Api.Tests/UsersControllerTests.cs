using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Queries;

namespace UserManagement.Api.Tests;

/// <summary>
/// Integration tests for UsersController endpoints.
/// Tests all user-related API endpoints including CRUD operations, filtering, statistics, and validation scenarios.
/// </summary>
public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the UsersControllerTests class.
    /// Sets up the test client and JSON serialization options for API testing.
    /// </summary>
    /// <param name="factory">The web application factory for creating test clients.</param>
    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Tests that getting a user by valid ID returns OK status with user details.
    /// </summary>
    [Fact]
    public async Task GetUser_WithValidId_ReturnsOkWithUser()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/users/2");  // Use User ID 2 instead of 1

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        UserDto? user = JsonSerializer.Deserialize<UserDto>(content, _jsonOptions);

        user.ShouldNotBeNull();
        user!.Id.ShouldBe(2);  // Expect User ID 2
    }

    /// <summary>
    /// Tests that getting a user by invalid ID returns NotFound status.
    /// </summary>
    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/users/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that getting users with default parameters returns a paginated result with default pagination settings.
    /// </summary>
    [Fact]
    public async Task GetUsers_WithDefaultParameters_ReturnsPagedResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PagedResult<UserDto>? result = JsonSerializer.Deserialize<PagedResult<UserDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Items.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
    }

    /// <summary>
    /// Tests that getting users with custom pagination and filtering parameters returns filtered results.
    /// </summary>
    [Fact]
    public async Task GetUsers_WithCustomParameters_ReturnsFilteredResult()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/users?page=1&pageSize=5&searchTerm=test&includeInactive=true");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PagedResult<UserDto>? result = JsonSerializer.Deserialize<PagedResult<UserDto>>(content, _jsonOptions);

        result.ShouldNotBeNull();
        result!.Page.ShouldBe(1);
        result.PageSize.ShouldBe(5);
    }

    /// <summary>
    /// Tests that getting active users returns a list containing only active users.
    /// </summary>
    [Fact]
    public async Task GetActiveUsers_ReturnsListOfActiveUsers()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/users/active");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        List<UserDto>? users = JsonSerializer.Deserialize<List<UserDto>>(content, _jsonOptions);

        users.ShouldNotBeNull();
        users!.ShouldAllBe(u => u.IsActive);
    }

    /// <summary>
    /// Tests that getting user statistics by valid ID returns OK status with statistics details.
    /// </summary>
    [Fact]
    public async Task GetUserStatistics_WithValidId_ReturnsStatistics()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/users/2/statistics");  // Use User ID 2 instead of 1

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        UserStatisticsDto? statistics = JsonSerializer.Deserialize<UserStatisticsDto>(content, _jsonOptions);

        statistics.ShouldNotBeNull();
        statistics!.UserId.ShouldBe(2);  // Expect User ID 2
    }

    /// <summary>
    /// Tests that getting user statistics by invalid ID returns NotFound status.
    /// </summary>
    [Fact]
    public async Task GetUserStatistics_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/users/999/statistics");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that creating a user with valid data returns Created status.
    /// </summary>
    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    /// <summary>
    /// Tests that creating a user with invalid data returns BadRequest status.
    /// </summary>
    [Fact]
    public async Task CreateUser_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        CreateUserCommand command = new()
        {
            FirstName = "", // Invalid - empty
            LastName = "User",
            Email = "invalid-email", // Invalid format
            DateOfBirth = DateTime.Now.AddYears(1) // Invalid - future date
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that creating a user with ID returns Created status with the new user ID.
    /// </summary>
    [Fact]
    public async Task CreateUserWithId_WithValidData_ReturnsCreatedWithId()
    {
        // Arrange
        CreateUserWithIdCommand command = new()
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test{Guid.NewGuid()}@example.com",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users/with-id", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        string responseContent = await response.Content.ReadAsStringAsync();
        int userId = JsonSerializer.Deserialize<int>(responseContent, _jsonOptions);
        userId.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Tests that updating a user with valid data returns NoContent status indicating successful update.
    /// </summary>
    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsNoContent()
    {
        // Arrange
        UpdateUserCommand command = new()
        {
            UserId = 1,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/users/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that updating a user with mismatched ID in URL and body returns BadRequest status.
    /// </summary>
    [Fact]
    public async Task UpdateUser_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        UpdateUserCommand command = new()
        {
            UserId = 2, // Different from URL parameter
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/users/1", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that updating a non-existent user returns NotFound status.
    /// </summary>
    [Fact]
    public async Task UpdateUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        UpdateUserCommand command = new()
        {
            UserId = 999,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/users/999", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that updating a user with result returns OK status with operation result.
    /// </summary>
    [Fact]
    public async Task UpdateUserWithResult_WithValidData_ReturnsOkWithResult()
    {
        // Arrange
        UpdateUserWithResultCommand command = new()
        {
            UserId = 1,
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        string json = JsonSerializer.Serialize(command, _jsonOptions);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await _client.PutAsync("/api/users/1/with-result", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        string responseContent = await response.Content.ReadAsStringAsync();
        OperationResult? result = JsonSerializer.Deserialize<OperationResult>(responseContent, _jsonOptions);
        result.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that deleting a user with valid ID returns NoContent status indicating successful deletion.
    /// </summary>
    [Fact]
    public async Task DeleteUser_WithValidId_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage response = await _client.DeleteAsync("/api/users/1?reason=Test deletion");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that deleting a non-existent user returns NotFound status.
    /// </summary>
    [Fact]
    public async Task DeleteUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await _client.DeleteAsync("/api/users/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that activating a user with valid ID returns NoContent status indicating successful activation.
    /// </summary>
    [Fact]
    public async Task ActivateUser_WithValidId_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users/1/activate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that activating a non-existent user returns NotFound status.
    /// </summary>
    [Fact]
    public async Task ActivateUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users/999/activate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests that deactivating a user with valid ID returns NoContent status indicating successful deactivation.
    /// </summary>
    [Fact]
    public async Task DeactivateUser_WithValidId_ReturnsNoContent()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users/1/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Tests that deactivating a non-existent user returns NotFound status.
    /// </summary>
    [Fact]
    public async Task DeactivateUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        HttpResponseMessage response = await _client.PostAsync("/api/users/999/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
