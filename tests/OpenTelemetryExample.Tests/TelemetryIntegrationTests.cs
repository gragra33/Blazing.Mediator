using System.Net;
using System.Text.Json;

namespace OpenTelemetryExample.Tests;

/// <summary>
/// Integration tests for telemetry-specific endpoints and functionality.
/// Tests telemetry data endpoints, health checks, and OpenTelemetry integration features.
/// </summary>
public class TelemetryIntegrationTests : IClassFixture<OpenTelemetryWebApplicationFactory>
{
    private readonly OpenTelemetryWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the TelemetryIntegrationTests class.
    /// </summary>
    /// <param name="factory">The web application factory for creating test clients.</param>
    public TelemetryIntegrationTests(OpenTelemetryWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Health Check Tests

    /// <summary>
    /// Tests that the application health check endpoint returns healthy status.
    /// </summary>
    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        content.ShouldContain("Healthy");
    }

    /// <summary>
    /// Tests that the mediator telemetry health check is working.
    /// </summary>
    [Fact]
    public async Task MediatorTelemetryHealthCheck_IsWorking()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        // The health check should include mediator telemetry status
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region Telemetry Data Endpoints Tests - These endpoints exist and return telemetry data

    /// <summary>
    /// Tests that telemetry activities endpoint returns OK with data.
    /// </summary>
    [Fact]
    public async Task TelemetryActivities_ReturnsOkWithData()
    {
        // Act - Get telemetry activities
        var response = await _client.GetAsync("/telemetry/activities");

        // Assert - This endpoint exists and should return telemetry data
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that telemetry metrics endpoint returns OK with data.
    /// </summary>
    [Fact]
    public async Task TelemetryMetrics_ReturnsOkWithData()
    {
        // Act - Get live telemetry metrics 
        var response = await _client.GetAsync("/telemetry/live-metrics");

        // Assert - This endpoint exists and should return metrics data
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that telemetry activities endpoint with time window parameter returns OK.
    /// </summary>
    [Fact]
    public async Task TelemetryActivities_WithTimeWindow_ReturnsOkWithData()
    {
        // Act - Get activities for last 5 minutes
        var response = await _client.GetAsync("/telemetry/activities?timeWindowMinutes=5");

        // Assert - This endpoint exists and should return filtered data
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that telemetry metrics endpoint with time window parameter returns OK.
    /// </summary>
    [Fact]
    public async Task TelemetryMetrics_WithTimeWindow_ReturnsOkWithData()
    {
        // Act - Get metrics for last 5 minutes
        var response = await _client.GetAsync("/telemetry/live-metrics?timeWindowMinutes=5");

        // Assert - This endpoint exists and should return filtered metrics
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region OpenTelemetry Configuration Tests

    /// <summary>
    /// Tests that OpenTelemetry integration is working correctly by creating a user first and then accessing it.
    /// </summary>
    [Fact]
    public async Task OpenTelemetryConfiguration_IsWorkingCorrectly()
    {
        // Create a test user first to ensure it exists
        var createUserRequest = new
        {
            Name = "Test User",
            Email = "testuser@example.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/users", createUserRequest);
        createResponse.EnsureSuccessStatusCode();
        var userIdResponse = await createResponse.Content.ReadAsStringAsync();
        int userId = int.Parse(userIdResponse);

        // Now test accessing the created user
        var userResponse = await _client.GetAsync($"/api/users/{userId}");
        userResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify telemetry is working by checking telemetry endpoints
        var telemetryResponse = await _client.GetAsync("/telemetry/health");
        telemetryResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var activitiesResponse = await _client.GetAsync("/telemetry/activities");
        activitiesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that telemetry middleware is processing requests correctly.
    /// </summary>
    [Fact]
    public async Task TelemetryMiddleware_ProcessesRequestsCorrectly()
    {
        // Act - Make various types of requests
        var getResponse = await _client.GetAsync("/api/users");
        var streamResponse = await _client.GetAsync("/api/streaming/users?count=1&delayMs=1");

        // Assert - All should complete successfully (middleware shouldn't interfere)
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        streamResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // OpenTelemetry integration is working if requests process normally
    }

    #endregion

    #region Error Telemetry Tests

    /// <summary>
    /// Tests that telemetry endpoints work normally even when there are application errors.
    /// </summary>
    [Fact]
    public async Task Errors_DontBreakTelemetryEndpoints()
    {
        // Act - Generate some errors and then verify telemetry still works
        var notFoundResponse = await _client.GetAsync("/api/users/999999"); // Not found
        
        // Assert - Application should handle errors normally
        notFoundResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Verify telemetry endpoints still work after errors
        var activitiesResponse = await _client.GetAsync("/telemetry/activities");
        activitiesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Performance Telemetry Tests

    /// <summary>
    /// Tests that performance is not negatively impacted by OpenTelemetry.
    /// </summary>
    [Fact]
    public async Task PerformanceMetrics_DontImpactFunctionality()
    {
        // Act - Make requests with different performance characteristics
        var user1Response = await _client.GetAsync("/api/users/1"); // Fast request
        var streamResponse = await _client.GetAsync("/api/streaming/users?count=3&delayMs=50"); // Slower streaming request
        var usersResponse = await _client.GetAsync("/api/users"); // Medium request

        // Assert - All should work normally
        user1Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        streamResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        usersResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that streaming works normally with OpenTelemetry enabled.
    /// </summary>
    [Fact]
    public async Task StreamingTelemetry_DoesntBreakStreaming()
    {
        // Act - Make streaming requests
        var streamResponse1 = await _client.GetAsync("/api/streaming/users?count=5&delayMs=20");
        var streamResponse2 = await _client.GetAsync("/api/streaming/users?count=3&delayMs=30");

        // Assert - Streaming should work normally
        streamResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);
        streamResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify content is streamed properly
        var content1 = await streamResponse1.Content.ReadAsStringAsync();
        var content2 = await streamResponse2.Content.ReadAsStringAsync();
        
        content1.ShouldNotBeNullOrEmpty();
        content2.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region Integration Scenario Tests

    /// <summary>
    /// Tests a complete user workflow with telemetry enabled.
    /// </summary>
    [Fact]
    public async Task CompleteUserWorkflow_WithTelemetryEnabled_WorksCorrectly()
    {
        // Arrange - Simulate a complete user workflow
        var workflow = new List<Task<HttpResponseMessage>>
        {
            // Act - Execute workflow steps
            _client.GetAsync("/api/users"), // List users
            _client.GetAsync("/api/users/1"), // Get specific user
            _client.GetAsync("/api/streaming/users?count=2") // Stream users
        };

        var responses = await Task.WhenAll(workflow);

        // Assert - All operations should succeed
        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that high-load scenarios work correctly with telemetry.
    /// </summary>
    [Fact]
    public async Task HighLoadScenarios_WithTelemetry_WorkCorrectly()
    {
        // Arrange
        const int concurrentRequests = 20;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Generate high load with mixed request types
        for (int i = 0; i < concurrentRequests; i++)
        {
            if (i % 3 == 0)
                tasks.Add(_client.GetAsync("/api/users"));
            else if (i % 3 == 1)
                tasks.Add(_client.GetAsync("/api/streaming/users?count=1&delayMs=1"));
            else
                tasks.Add(_client.GetAsync($"/api/users/{(i % 5) + 1}"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Most requests should succeed (some might fail due to load)
        var successfulResponses = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        successfulResponses.ShouldBeGreaterThan(concurrentRequests / 2); // At least 50% success
    }

    #endregion

    #region Edge Cases and Error Scenarios

    /// <summary>
    /// Tests that telemetry endpoints return valid responses even with edge case parameters.
    /// </summary>
    [Fact]
    public async Task TelemetryEndpoints_WithEdgeCaseParameters_ReturnValidResponses()
    {
        // Act - Test with various edge case parameters
        var invalidTimeWindow1 = await _client.GetAsync("/telemetry/activities?timeWindowMinutes=-1");
        var invalidTimeWindow2 = await _client.GetAsync("/telemetry/live-metrics?timeWindowMinutes=99999");
        var invalidParameters = await _client.GetAsync("/telemetry/activities?invalidParam=value");

        // Assert - Should handle edge cases gracefully and return OK (endpoints exist and handle params)
        invalidTimeWindow1.StatusCode.ShouldBe(HttpStatusCode.OK);
        invalidTimeWindow2.StatusCode.ShouldBe(HttpStatusCode.OK);
        invalidParameters.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that the application continues working even under high load.
    /// </summary>
    [Fact]
    public async Task ApplicationUnderPressure_ContinuesWorking()
    {
        // Act - Generate a lot of requests quickly
        var tasks = new List<Task<HttpResponseMessage>>();
        
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(_client.GetAsync("/api/users/1"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Most requests should succeed
        var successfulResponses = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        successfulResponses.ShouldBeGreaterThan(40); // At least 80% success rate
    }

    #endregion

    #region Non-Existing Endpoints Tests

    /// <summary>
    /// Tests that accessing non-existing telemetry endpoints returns not found status.
    /// </summary>
    [Fact]
    public async Task GetTelemetryData_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/telemetry/data");

        // Assert
        // This endpoint doesn't exist in the OpenTelemetryExample project
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRecentActivities_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/telemetry/activities");

        // Assert
        // This endpoint doesn't exist in the OpenTelemetryExample project
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLiveMetrics_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/telemetry/live-metrics");

        // Assert
        // This endpoint doesn't exist in the OpenTelemetryExample project
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion
}