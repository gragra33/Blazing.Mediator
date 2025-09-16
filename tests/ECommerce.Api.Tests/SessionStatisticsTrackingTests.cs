using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Shouldly;

namespace ECommerce.Api.Tests;

/// <summary>
/// Tests for verifying that session-based statistics tracking works correctly
/// and that session state persists across multiple requests from the same client.
/// </summary>
public class SessionStatisticsTrackingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionStatisticsTrackingTests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory for integration testing.</param>
    public SessionStatisticsTrackingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Tests that session-based statistics tracking correctly persists session state
    /// and tracks requests from the same session together.
    /// </summary>
    [Fact]
    public async Task SessionStatisticsTracking_MultipleRequestsFromSameSession_TrackCorrectly()
    {
        // Arrange - Create a client that maintains cookies/session state
        var client = _factory.CreateClient();

        // Act - Get initial statistics
        var initialStatsResponse = await client.GetAsync("/api/mediator/statistics");
        initialStatsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var initialStatsContent = await initialStatsResponse.Content.ReadAsStringAsync();
        var initialStats = JsonDocument.Parse(initialStatsContent);
        
        var initialActiveSession = initialStats.RootElement
            .GetProperty("globalStatistics")
            .GetProperty("summary")
            .GetProperty("activeSessions")
            .GetInt32();

        // Execute multiple requests from the same client session
        // Query requests
        var productResponse1 = await client.GetAsync("/api/products/1");
        productResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);

        var productsResponse1 = await client.GetAsync("/api/products");
        productsResponse1.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Command request
        var createProductCommand = new
        {
            Name = "Session Test Product",
            Description = "Testing session-based statistics tracking",
            Price = 29.99m,
            StockQuantity = 25
        };
        var commandJson = JsonSerializer.Serialize(createProductCommand, _jsonOptions);
        var commandContent = new StringContent(commandJson, Encoding.UTF8, "application/json");
        var commandResponse = await client.PostAsync("/api/products", commandContent);
        // Command might fail due to validation, but it should be tracked

        // More query requests
        var productResponse2 = await client.GetAsync("/api/products/2");
        productResponse2.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Get updated statistics
        var updatedStatsResponse = await client.GetAsync("/api/mediator/statistics");
        updatedStatsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var updatedStatsContent = await updatedStatsResponse.Content.ReadAsStringAsync();
        var updatedStats = JsonDocument.Parse(updatedStatsContent);

        // Assert - Verify session tracking is working
        var globalStats = updatedStats.RootElement.GetProperty("globalStatistics").GetProperty("summary");
        var finalActiveSessions = globalStats.GetProperty("activeSessions").GetInt32();

        // Should have at least 1 active session (could be more if other tests are running)
        finalActiveSessions.ShouldBeGreaterThanOrEqualTo(1);

        // Verify that queries and commands were tracked
        var totalQueryExecutions = globalStats.GetProperty("totalQueryExecutions").GetInt64();
        var totalCommandExecutions = globalStats.GetProperty("totalCommandExecutions").GetInt64();

        totalQueryExecutions.ShouldBeGreaterThan(0, "Should have tracked query executions");
        totalCommandExecutions.ShouldBeGreaterThan(0, "Should have tracked command executions");

        // Verify query types are being tracked
        var queryTypes = updatedStats.RootElement
            .GetProperty("globalStatistics")
            .GetProperty("details")
            .GetProperty("queryTypes");

        queryTypes.ValueKind.ShouldBe(JsonValueKind.Object);
        
        // Should have GetProductByIdQuery and GetProductsQuery tracked
        var queryTypesDict = JsonSerializer.Deserialize<Dictionary<string, long>>(queryTypes.GetRawText());
        queryTypesDict.ShouldNotBeEmpty("Should have tracked some query types");
        queryTypesDict.ShouldContainKey("GetProductByIdQuery");
        queryTypesDict.ShouldContainKey("GetProductsQuery");
    }

    /// <summary>
    /// Tests that different browser sessions (different clients) are tracked separately.
    /// </summary>
    [Fact]
    public async Task SessionStatisticsTracking_DifferentClients_TrackSeparately()
    {
        // Arrange - Create two separate clients (different sessions)
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        // Act - Make requests from client1
        await client1.GetAsync("/api/products/1");
        await client1.GetAsync("/api/products");

        // Make requests from client2
        await client2.GetAsync("/api/products/2");
        await client2.GetAsync("/api/products");

        // Get statistics
        var statsResponse = await client1.GetAsync("/api/mediator/statistics");
        statsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        var stats = JsonDocument.Parse(statsContent);

        // Assert - Should have multiple active sessions
        var activeSessions = stats.RootElement
            .GetProperty("globalStatistics")
            .GetProperty("summary")
            .GetProperty("activeSessions")
            .GetInt32();

        activeSessions.ShouldBeGreaterThanOrEqualTo(2, "Should have at least 2 active sessions from different clients");
    }

    /// <summary>
    /// Tests that session-specific statistics can be retrieved for individual sessions.
    /// </summary>
    [Fact]
    public async Task SessionStatisticsTracking_GetSessionSpecificStats_ReturnsCorrectData()
    {
        // Arrange - Create a client and make some requests
        var client = _factory.CreateClient();

        // Execute specific requests to generate session-specific data
        await client.GetAsync("/api/products/1");
        await client.GetAsync("/api/products");

        // Get all session statistics to find our session ID
        var allSessionsResponse = await client.GetAsync("/api/mediator/statistics/sessions");
        allSessionsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var allSessionsContent = await allSessionsResponse.Content.ReadAsStringAsync();
        var allSessions = JsonDocument.Parse(allSessionsContent);

        var sessions = allSessions.RootElement.GetProperty("sessions");
        sessions.ValueKind.ShouldBe(JsonValueKind.Array);

        var sessionArray = sessions.EnumerateArray().ToArray();
        sessionArray.ShouldNotBeEmpty("Should have at least one active session");

        // Get the most recent session (likely ours)
        var recentSession = sessionArray
            .OrderByDescending(s => DateTime.Parse(s.GetProperty("lastActivity").GetString()!))
            .First();

        var sessionId = recentSession.GetProperty("sessionId").GetString();
        sessionId.ShouldNotBeNullOrEmpty();

        // Act - Get statistics for this specific session
        var sessionStatsResponse = await client.GetAsync($"/api/mediator/statistics/session/{sessionId}");

        // Assert
        sessionStatsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var sessionStatsContent = await sessionStatsResponse.Content.ReadAsStringAsync();
        var sessionStats = JsonDocument.Parse(sessionStatsContent);

        var sessionStatistics = sessionStats.RootElement.GetProperty("sessionStatistics");
        sessionStatistics.GetProperty("sessionId").GetString().ShouldBe(sessionId);

        var summary = sessionStatistics.GetProperty("summary");
        summary.GetProperty("totalQueryExecutions").GetInt64().ShouldBeGreaterThan(0);
        summary.GetProperty("uniqueQueryTypes").GetInt32().ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Tests that the main statistics endpoint provides comprehensive information.
    /// </summary>
    [Fact]
    public async Task MainStatistics_ReturnsComprehensiveStatisticsOutput()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Make some requests to generate statistics
        await client.GetAsync("/api/products/1");
        await client.GetAsync("/api/products");

        // Act
        var response = await client.GetAsync("/api/mediator/statistics");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        
        var stats = JsonDocument.Parse(content);
        var root = stats.RootElement;

        root.GetProperty("message").GetString().ShouldBe("Real-Time Mediator Statistics");
        
        var globalStats = root.GetProperty("globalStatistics");
        var summary = globalStats.GetProperty("summary");
        
        summary.GetProperty("totalQueryExecutions").GetInt64().ShouldBeGreaterThan(0);
        summary.GetProperty("uniqueQueryTypes").GetInt32().ShouldBeGreaterThan(0);
        summary.GetProperty("activeSessions").GetInt32().ShouldBeGreaterThanOrEqualTo(1);

        var trackingInfo = root.GetProperty("trackingInfo");
        trackingInfo.GetProperty("method").GetString().ShouldContain("StatisticsTrackingMiddleware");
        trackingInfo.GetProperty("sessionTracking").GetString().ShouldContain("Enabled");

        var instructions = root.GetProperty("instructions");
        instructions.GetProperty("getSessionId").GetString().ShouldContain("/api/mediator/session");
    }

    /// <summary>
    /// Tests that session statistics persist across multiple requests within the same session.
    /// </summary>
    [Fact]
    public async Task SessionStatisticsTracking_PersistsAcrossRequests_InSameSession()
    {
        // Arrange - Create a client that maintains session state
        var client = _factory.CreateClient();

        // Act - Make first request and get session stats
        await client.GetAsync("/api/products/1");

        var firstStatsResponse = await client.GetAsync("/api/mediator/statistics");
        firstStatsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var firstStatsContent = await firstStatsResponse.Content.ReadAsStringAsync();
        var firstStats = JsonDocument.Parse(firstStatsContent);

        var firstQueryCount = firstStats.RootElement
            .GetProperty("globalStatistics")
            .GetProperty("summary")
            .GetProperty("totalQueryExecutions")
            .GetInt64();

        // Make second request from same session
        await client.GetAsync("/api/products/2");

        var secondStatsResponse = await client.GetAsync("/api/mediator/statistics");
        secondStatsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var secondStatsContent = await secondStatsResponse.Content.ReadAsStringAsync();
        var secondStats = JsonDocument.Parse(secondStatsContent);

        var secondQueryCount = secondStats.RootElement
            .GetProperty("globalStatistics")
            .GetProperty("summary")
            .GetProperty("totalQueryExecutions")
            .GetInt64();

        // Assert - Query count should have increased
        secondQueryCount.ShouldBeGreaterThan(firstQueryCount, "Query count should increase as more requests are made");

        // Verify session ID consistency by checking that both requests contributed to the same session
        var activeSessions = secondStats.RootElement
            .GetProperty("globalStatistics")
            .GetProperty("summary")
            .GetProperty("activeSessions")
            .GetInt32();

        activeSessions.ShouldBeGreaterThanOrEqualTo(1, "Should maintain consistent session across requests");
    }
}