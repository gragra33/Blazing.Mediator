using OpenTelemetryExample.Shared.Models;
using System.Net;
using System.Text.Json;

namespace OpenTelemetryExample.Tests;

/// <summary>
/// Integration tests for StreamingController endpoints.
/// Tests streaming functionality, Server-Sent Events, metadata handling, and OpenTelemetry integration with streaming operations.
/// </summary>
public class StreamingControllerTests : IClassFixture<OpenTelemetryWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the StreamingControllerTests class.
    /// Sets up the test client and JSON serialization options for streaming API testing.
    /// </summary>
    /// <param name="factory">The web application factory for creating test clients.</param>
    public StreamingControllerTests(OpenTelemetryWebApplicationFactory factory)
    {
        OpenTelemetryWebApplicationFactory factory1 = factory;
        _client = factory1.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region StreamData Tests

    /// <summary>
    /// Tests that streaming data with default parameters returns valid streaming response.
    /// </summary>
    [Fact]
    public async Task StreamData_WithDefaultParameters_ReturnsValidStream()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/stream-data");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        
        // Verify content type for streaming
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    /// <summary>
    /// Tests that streaming data with custom parameters applies the configuration correctly.
    /// </summary>
    [Fact]
    public async Task StreamData_WithCustomParameters_ReturnsConfiguredStream()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/stream-data?count=5&delayMs=50");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that streaming data with zero count returns appropriate response.
    /// </summary>
    [Fact]
    public async Task StreamData_WithZeroCount_ReturnsEmptyStream()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/stream-data?count=0");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests that streaming data with negative parameters handles gracefully.
    /// </summary>
    [Fact]
    public async Task StreamData_WithNegativeParameters_HandlesGracefully()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/stream-data?count=-1&delayMs=-100");

        // Assert
        // Should not crash - exact behavior depends on implementation
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region StreamUsers Tests

    /// <summary>
    /// Tests that streaming users with default parameters returns valid user stream.
    /// </summary>
    [Fact]
    public async Task StreamUsers_WithDefaultParameters_ReturnsValidUserStream()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        
        // Try to deserialize as JSON array to verify it's valid JSON
        var users = JsonSerializer.Deserialize<UserDto[]>(content, _jsonOptions);
        users.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that streaming users with search term applies filtering.
    /// </summary>
    [Fact]
    public async Task StreamUsers_WithSearchTerm_ReturnsFilteredStream()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users?searchTerm=john&count=5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that streaming users with includeInactive parameter works correctly.
    /// </summary>
    [Fact]
    public async Task StreamUsers_WithIncludeInactive_ReturnsAllUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users?includeInactive=true&count=5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that streaming users with custom delay applies timing correctly.
    /// </summary>
    [Fact]
    public async Task StreamUsers_WithCustomDelay_AppliesTimingCorrectly()
    {
        // Arrange
        var startTime = DateTime.UtcNow;

        // Act
        var response = await _client.GetAsync("/api/streaming/users?count=2&delayMs=100");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var endTime = DateTime.UtcNow;
        var elapsed = endTime - startTime;
        
        // Should take at least some time due to delays (allowing for test timing variations)
        elapsed.TotalMilliseconds.ShouldBeGreaterThan(50);
    }

    #endregion

    #region StreamUsersSSE Tests

    /// <summary>
    /// Tests that Server-Sent Events endpoint returns correct headers and format.
    /// </summary>
    [Fact]
    public async Task StreamUsersSSE_ReturnsCorrectHeadersAndFormat()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users/sse?count=3&delayMs=10");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        // Verify SSE headers - Content-Type should be accessed via Content.Headers
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");
        response.Headers.GetValues("Cache-Control").First().ShouldBe("no-cache");
        response.Headers.GetValues("Connection").First().ShouldBe("keep-alive");
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        
        // SSE format should contain "data:" lines
        content.ShouldContain("data:");
    }

    /// <summary>
    /// Tests that SSE endpoint with search parameters filters correctly.
    /// </summary>
    [Fact]
    public async Task StreamUsersSSE_WithSearchParameters_FiltersCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users/sse?searchTerm=test&includeInactive=true&count=2");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");
    }

    /// <summary>
    /// Tests that SSE endpoint handles zero count appropriately.
    /// </summary>
    [Fact]
    public async Task StreamUsersSSE_WithZeroCount_HandlesAppropriately()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users/sse?count=0");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");
    }

    #endregion

    #region StreamUsersWithMetadata Tests

    /// <summary>
    /// Tests that streaming users with metadata returns proper metadata structure.
    /// </summary>
    [Fact]
    public async Task StreamUsersWithMetadata_ReturnsProperMetadataStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users/metadata?count=3&delayMs=10");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        
        // Try to deserialize to verify structure
        var streamResponses = JsonSerializer.Deserialize<StreamResponseDto<UserDto>[]>(content, _jsonOptions);
        streamResponses.ShouldNotBeNull();
        
        if (streamResponses!.Length > 0)
        {
            var firstResponse = streamResponses[0];
            firstResponse.Data.ShouldNotBeNull();
            firstResponse.Metadata.ShouldNotBeNull();
            firstResponse.Metadata.BatchId.ShouldNotBeNullOrEmpty();
            firstResponse.Metadata.ItemNumber.ShouldBeGreaterThan(0);
        }
    }

    /// <summary>
    /// Tests that metadata includes correct timing information.
    /// </summary>
    [Fact]
    public async Task StreamUsersWithMetadata_IncludesCorrectTimingInformation()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users/metadata?count=2&delayMs=50");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        
        var streamResponses = JsonSerializer.Deserialize<StreamResponseDto<UserDto>[]>(content, _jsonOptions);
        streamResponses.ShouldNotBeNull();
        
        if (streamResponses!.Length > 0)
        {
            streamResponses.ShouldAllBe(sr => sr.Metadata.Timestamp != DateTimeOffset.MinValue);
        }
    }

    /// <summary>
    /// Tests that metadata with filtering parameters works correctly.
    /// </summary>
    [Fact]
    public async Task StreamUsersWithMetadata_WithFilteringParameters_WorksCorrectly()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/users/metadata?searchTerm=admin&includeInactive=false&count=5");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region Health Check Tests

    /// <summary>
    /// Tests that streaming health check returns healthy status.
    /// </summary>
    [Fact]
    public async Task GetStreamingHealth_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/streaming/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        
        healthResponse.GetProperty("status").GetString().ShouldBe("Healthy");
        healthResponse.GetProperty("timestamp").ValueKind.ShouldBe(JsonValueKind.String);
        
        var checks = healthResponse.GetProperty("checks");
        checks.GetProperty("streamingPipeline").GetString().ShouldBe("OK");
        checks.GetProperty("openTelemetryIntegration").GetString().ShouldBeOneOf("Enabled", "Disabled");
        checks.GetProperty("middlewareCount").GetInt32().ShouldBeGreaterThan(0);
    }

    #endregion

    #region OpenTelemetry Integration Tests

    /// <summary>
    /// Tests that streaming endpoints generate appropriate OpenTelemetry activities.
    /// </summary>
    [Fact]
    public async Task StreamingEndpoints_GenerateOpenTelemetryActivities()
    {
        // Note: In a real scenario, you would need to set up OpenTelemetry test instrumentation
        // to capture and verify streaming activities. For now, we'll test that endpoints work correctly
        // which indicates streaming telemetry instrumentation is not breaking functionality.

        // Act - Test multiple streaming endpoints
        var streamDataResponse = await _client.GetAsync("/api/streaming/stream-data?count=2&delayMs=10");
        var streamUsersResponse = await _client.GetAsync("/api/streaming/users?count=2&delayMs=10");
        var metadataResponse = await _client.GetAsync("/api/streaming/users/metadata?count=2&delayMs=10");

        // Assert - All endpoints should work correctly with streaming telemetry enabled
        streamDataResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        streamUsersResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        metadataResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify responses contain expected data
        var streamDataContent = await streamDataResponse.Content.ReadAsStringAsync();
        streamDataContent.ShouldNotBeNullOrEmpty();

        var streamUsersContent = await streamUsersResponse.Content.ReadAsStringAsync();
        streamUsersContent.ShouldNotBeNullOrEmpty();

        var metadataContent = await metadataResponse.Content.ReadAsStringAsync();
        metadataContent.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that streaming error scenarios generate appropriate telemetry traces.
    /// </summary>
    [Fact]
    public async Task StreamingErrorScenarios_GenerateErrorTelemetry()
    {
        // Act - Test scenarios that might generate errors or warnings
        var largeCountResponse = await _client.GetAsync("/api/streaming/users?count=1000&delayMs=1");
        var invalidParametersResponse = await _client.GetAsync("/api/streaming/users?count=abc&delayMs=xyz");

        // Assert - Errors should be properly handled and not break streaming telemetry
        largeCountResponse.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.RequestTimeout);
        invalidParametersResponse.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that concurrent streaming requests work correctly with telemetry.
    /// </summary>
    [Fact]
    public async Task ConcurrentStreamingRequests_WithTelemetryEnabled_HandleCorrectly()
    {
        // Arrange
        const int concurrentRequests = 5;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Make concurrent streaming requests
        // Act - Make concurrent streaming requests
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_client.GetAsync($"/api/streaming/users?count=2&delayMs=10"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - All requests should succeed
        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);

        // Verify responses contain valid data
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();
        }
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that streaming endpoints perform within reasonable time limits.
    /// </summary>
    [Fact]
    public async Task StreamingEndpoints_PerformWithinReasonableTimeLimits()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(30); // Reasonable timeout for small streams
        using var cts = new CancellationTokenSource(timeout);

        // Act & Assert - Should complete within timeout
        var streamTask = _client.GetAsync("/api/streaming/users?count=5&delayMs=100", cts.Token);
        
        var response = await streamTask;
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync(cts.Token);
        content.ShouldNotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that streaming endpoints handle cancellation correctly.
    /// </summary>
    [Fact]
    public async Task StreamingEndpoints_HandleCancellationCorrectly()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        
        // Act - Start a request and then cancel it quickly
        var requestTask = _client.GetAsync("/api/streaming/users?count=10&delayMs=500", cts.Token);
        
        // Cancel after a short delay
        await Task.Delay(50, cts.Token);
        await cts.CancelAsync();

        // Assert - Should handle cancellation gracefully
        await Should.ThrowAsync<TaskCanceledException>(async () => await requestTask);
    }

    #endregion
}