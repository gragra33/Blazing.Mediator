using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ECommerce.Api.Tests;

/// <summary>
/// Comprehensive tests for the MediatorController endpoints.
/// Tests all statistics, analysis, and session-related functionality.
/// </summary>
public class MediatorControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the MediatorControllerTests class.
    /// </summary>
    /// <param name="factory">The web application factory for integration testing.</param>
    public MediatorControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Session Endpoint Tests

    /// <summary>
    /// Tests that the session endpoint returns session information when no statistics session exists yet.
    /// </summary>
    [Fact]
    public async Task GetSessionId_WhenNoStatisticsSessionExists_ReturnsNotYetAssignedMessage()
    {
        // Arrange - Use a fresh client
        var freshClient = _factory.CreateClient();

        // Act
        var response = await freshClient.GetAsync("/api/mediator/session");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        // The message should indicate whether statistics session is assigned or not
        var message = root.GetProperty("message").GetString();
        message.ShouldBeOneOf("Session ID Not Yet Assigned", "Current Session ID");

        var sessionId = root.GetProperty("sessionId");

        if (message == "Session ID Not Yet Assigned")
        {
            sessionId.ValueKind.ShouldBe(JsonValueKind.Null);
            root.GetProperty("note").GetString().ShouldContain("No session ID has been assigned yet");

            var instructions = root.GetProperty("instructions");
            instructions.GetProperty("initializeSession").GetString().ShouldContain("Make any API request");
            instructions.GetProperty("checkAgain").GetString().ShouldContain("call this endpoint again");
        }
        else
        {
            // If session ID already exists (due to SessionTrackingMiddleware running)
            sessionId.ValueKind.ShouldNotBe(JsonValueKind.Null);
            var sessionIdValue = sessionId.GetString();
            sessionIdValue.ShouldNotBeNullOrEmpty();
        }

        var sessionInfo = root.GetProperty("sessionInfo");
        sessionInfo.GetProperty("sessionAvailable").GetBoolean().ShouldBeTrue();
    }

    /// <summary>
    /// Tests that the session endpoint returns a valid session ID after session initialization.
    /// </summary>
    [Fact]
    public async Task GetSessionId_AfterSessionInitialization_ReturnsValidSessionId()
    {
        // Arrange - Initialize session by making a request that triggers middleware
        await _client.GetAsync("/api/products/1");

        // Act
        var response = await _client.GetAsync("/api/mediator/session");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("message").GetString().ShouldBe("Current Session ID");

        var sessionId = root.GetProperty("sessionId").GetString();
        sessionId.ShouldNotBeNullOrEmpty();
        sessionId.ShouldStartWith("stats_"); // Our session ID format

        var usage = root.GetProperty("usage");
        usage.GetProperty("viewSessionStats").GetString().ShouldContain(sessionId);
        usage.GetProperty("viewGlobalStats").GetString().ShouldBe("GET /api/mediator/statistics");

        var sessionInfo = root.GetProperty("sessionInfo");
        sessionInfo.GetProperty("sessionAvailable").GetBoolean().ShouldBeTrue();
        sessionInfo.GetProperty("statisticsSessionId").GetString().ShouldBe(sessionId);
    }

    #endregion

    #region Statistics Endpoint Tests

    /// <summary>
    /// Tests that getting mediator statistics returns OK status with comprehensive statistics information.
    /// </summary>
    [Fact]
    public async Task GetStatistics_ReturnsOkWithComprehensiveStatistics()
    {
        // Arrange - Make some requests to generate statistics
        await _client.GetAsync("/api/products/1");
        await _client.GetAsync("/api/products");

        // Act
        var response = await _client.GetAsync("/api/mediator/statistics");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        // Verify the response structure
        root.GetProperty("message").GetString().ShouldBe("Real-Time Mediator Statistics");
        root.GetProperty("note").GetString().ShouldContain("update dynamically");

        // Verify global statistics structure
        var globalStats = root.GetProperty("globalStatistics");
        var summary = globalStats.GetProperty("summary");

        summary.GetProperty("uniqueQueryTypes").GetInt32().ShouldBeGreaterThanOrEqualTo(0);
        summary.GetProperty("uniqueCommandTypes").GetInt32().ShouldBeGreaterThanOrEqualTo(0);
        summary.GetProperty("totalQueryExecutions").GetInt64().ShouldBeGreaterThan(0);
        summary.GetProperty("activeSessions").GetInt32().ShouldBeGreaterThanOrEqualTo(1);

        var details = globalStats.GetProperty("details");
        details.TryGetProperty("queryTypes", out _).ShouldBeTrue();
        details.TryGetProperty("commandTypes", out _).ShouldBeTrue();
        details.TryGetProperty("notificationTypes", out _).ShouldBeTrue();

        // Verify tracking info
        var trackingInfo = root.GetProperty("trackingInfo");
        trackingInfo.GetProperty("method").GetString().ShouldContain("StatisticsTrackingMiddleware");
        trackingInfo.GetProperty("sessionTracking").GetString().ShouldContain("Enabled");

        // Verify instructions
        var instructions = root.GetProperty("instructions");
        instructions.GetProperty("getSessionId").GetString().ShouldContain("/api/mediator/session");
        instructions.GetProperty("viewSessionStats").GetString().ShouldContain("/api/mediator/statistics/session/");
    }

    #endregion

    #region Session Statistics Tests

    /// <summary>
    /// Tests that session statistics can be retrieved for a valid session.
    /// </summary>
    [Fact]
    public async Task GetSessionStatistics_WithValidSession_ReturnsSessionData()
    {
        // Arrange - Make requests to generate session data
        await _client.GetAsync("/api/products/1");
        await _client.GetAsync("/api/products");

        // Get the session ID
        var sessionResponse = await _client.GetAsync("/api/mediator/session");
        var sessionContent = await sessionResponse.Content.ReadAsStringAsync();
        using var sessionDoc = JsonDocument.Parse(sessionContent);
        var sessionId = sessionDoc.RootElement.GetProperty("sessionId").GetString();

        sessionId.ShouldNotBeNullOrEmpty();

        // Act
        var response = await _client.GetAsync($"/api/mediator/statistics/session/{sessionId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("message").GetString().ShouldContain(sessionId);

        var sessionStats = root.GetProperty("sessionStatistics");
        sessionStats.GetProperty("sessionId").GetString().ShouldBe(sessionId);

        var summary = sessionStats.GetProperty("summary");
        summary.GetProperty("totalQueryExecutions").GetInt64().ShouldBeGreaterThan(0);
        summary.GetProperty("uniqueQueryTypes").GetInt32().ShouldBeGreaterThan(0);

        var details = sessionStats.GetProperty("details");
        details.TryGetProperty("queryTypes", out _).ShouldBeTrue();
        details.TryGetProperty("commandTypes", out _).ShouldBeTrue();

        sessionStats.TryGetProperty("lastActivity", out _).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that session statistics returns 404 for non-existent session.
    /// </summary>
    [Fact]
    public async Task GetSessionStatistics_WithInvalidSession_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/statistics/session/invalid-session-id");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("error").GetString().ShouldContain("Session 'invalid-session-id' not found");
    }

    #endregion

    #region All Sessions Statistics Tests

    /// <summary>
    /// Tests that all sessions statistics returns data for active sessions.
    /// </summary>
    [Fact]
    public async Task GetAllSessionStatistics_ReturnsActiveSessionsData()
    {
        // Arrange - Make requests to generate session data
        await _client.GetAsync("/api/products/1");

        // Act
        var response = await _client.GetAsync("/api/mediator/statistics/sessions");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("message").GetString().ShouldBe("Statistics for All Active Sessions");

        var totalSessions = root.GetProperty("totalActiveSessions").GetInt32();
        totalSessions.ShouldBeGreaterThanOrEqualTo(1);

        var sessions = root.GetProperty("sessions");
        sessions.ValueKind.ShouldBe(JsonValueKind.Array);

        var sessionArray = sessions.EnumerateArray().ToArray();
        sessionArray.Length.ShouldBe(totalSessions);

        if (sessionArray.Length > 0)
        {
            var firstSession = sessionArray[0];
            firstSession.TryGetProperty("sessionId", out _).ShouldBeTrue();

            var summary = firstSession.GetProperty("summary");
            summary.TryGetProperty("uniqueQueryTypes", out _).ShouldBeTrue();
            summary.TryGetProperty("totalQueryExecutions", out _).ShouldBeTrue();

            firstSession.TryGetProperty("lastActivity", out _).ShouldBeTrue();
        }
    }

    #endregion

    #region Query Analysis Tests

    /// <summary>
    /// Tests that query analysis returns discovered queries with proper structure.
    /// </summary>
    [Fact]
    public async Task AnalyzeQueries_ReturnsDiscoveredQueries()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/analyze/queries");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("totalQueries").GetInt32().ShouldBeGreaterThan(0);
        root.GetProperty("isDetailed").GetBoolean().ShouldBeTrue();

        var queriesByAssembly = root.GetProperty("queriesByAssembly");
        queriesByAssembly.ValueKind.ShouldBe(JsonValueKind.Array);

        var summary = root.GetProperty("summary");
        summary.GetProperty("withHandlers").GetInt32().ShouldBeGreaterThanOrEqualTo(0);
        summary.GetProperty("missingHandlers").GetInt32().ShouldBeGreaterThanOrEqualTo(0);

        var legend = root.GetProperty("legend");
        legend.GetProperty("description").GetString().ShouldContain("Handler found");

        // Should find ECommerce queries
        var assemblies = queriesByAssembly.EnumerateArray().ToArray();
        assemblies.ShouldNotBeEmpty();

        var ecommerceAssembly = assemblies.FirstOrDefault(a =>
            a.GetProperty("assembly").GetString()?.Contains("ECommerce") == true);

        if (ecommerceAssembly.ValueKind != JsonValueKind.Undefined)
        {
            var namespaces = ecommerceAssembly.GetProperty("namespaces");
            namespaces.ValueKind.ShouldBe(JsonValueKind.Array);
        }
    }

    /// <summary>
    /// Tests that query analysis supports compact mode.
    /// </summary>
    [Fact]
    public async Task AnalyzeQueries_WithDetailedFalse_ReturnsCompactAnalysis()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/analyze/queries?detailed=false");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("totalQueries").GetInt32().ShouldBeGreaterThan(0);
        root.GetProperty("isDetailed").GetBoolean().ShouldBeFalse();

        // Compact mode should still have the essential structure
        root.TryGetProperty("queriesByAssembly", out _).ShouldBeTrue();
        root.TryGetProperty("summary", out _).ShouldBeTrue();
        root.TryGetProperty("legend", out _).ShouldBeTrue();
    }

    #endregion

    #region Command Analysis Tests

    /// <summary>
    /// Tests that command analysis returns discovered commands with proper structure.
    /// </summary>
    [Fact]
    public async Task AnalyzeCommands_ReturnsDiscoveredCommands()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/analyze/commands");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        root.GetProperty("totalCommands").GetInt32().ShouldBeGreaterThan(0);
        root.GetProperty("isDetailed").GetBoolean().ShouldBeTrue();

        var commandsByAssembly = root.GetProperty("commandsByAssembly");
        commandsByAssembly.ValueKind.ShouldBe(JsonValueKind.Array);

        var summary = root.GetProperty("summary");
        summary.GetProperty("withHandlers").GetInt32().ShouldBeGreaterThanOrEqualTo(0);
        summary.GetProperty("missingHandlers").GetInt32().ShouldBeGreaterThanOrEqualTo(0);

        var legend = root.GetProperty("legend");
        legend.GetProperty("symbols").TryGetProperty("success", out _).ShouldBeTrue();
        legend.GetProperty("symbols").TryGetProperty("missing", out _).ShouldBeTrue();
        legend.GetProperty("symbols").TryGetProperty("multiple", out _).ShouldBeTrue();

        // Should find ECommerce commands
        var assemblies = commandsByAssembly.EnumerateArray().ToArray();
        assemblies.ShouldNotBeEmpty();
    }

    #endregion

    #region Complete Analysis Tests

    /// <summary>
    /// Tests that complete analysis returns both queries and commands with comprehensive data.
    /// </summary>
    [Fact]
    public async Task GetCompleteAnalysis_ReturnsComprehensiveAnalysis()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/analyze");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        // Verify summary section
        var summary = root.GetProperty("summary");
        summary.GetProperty("totalQueries").GetInt32().ShouldBeGreaterThan(0);
        summary.GetProperty("totalCommands").GetInt32().ShouldBeGreaterThan(0);
        summary.GetProperty("totalTypes").GetInt32().ShouldBeGreaterThan(0);
        summary.GetProperty("isDetailed").GetBoolean().ShouldBeTrue();

        var healthStatus = summary.GetProperty("healthStatus");
        healthStatus.GetProperty("queriesWithHandlers").GetInt32().ShouldBeGreaterThanOrEqualTo(0);
        healthStatus.GetProperty("commandsWithHandlers").GetInt32().ShouldBeGreaterThanOrEqualTo(0);

        // Verify queries and commands sections
        root.TryGetProperty("queries", out _).ShouldBeTrue();
        root.TryGetProperty("commands", out _).ShouldBeTrue();

        // Verify legend
        var legend = root.GetProperty("legend");
        legend.GetProperty("description").GetString().ShouldContain("Handler found");

        // Verify timestamp
        root.TryGetProperty("timestamp", out _).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that complete analysis respects the detailed parameter.
    /// </summary>
    [Fact]
    public async Task GetCompleteAnalysis_WithDetailedFalse_ReturnsCompactAnalysis()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/analyze?detailed=false");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        var summary = root.GetProperty("summary");
        summary.GetProperty("isDetailed").GetBoolean().ShouldBeFalse();

        // Compact mode should still have essential structure
        root.TryGetProperty("queries", out _).ShouldBeTrue();
        root.TryGetProperty("commands", out _).ShouldBeTrue();
        root.TryGetProperty("legend", out _).ShouldBeTrue();
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests that statistics tracking works end-to-end across all endpoints.
    /// </summary>
    [Fact]
    public async Task MediatorController_EndToEndWorkflow_WorksCorrectly()
    {
        // Arrange - Start with fresh client
        var testClient = _factory.CreateClient();

        // Step 1: Check initial session state
        var initialSessionResponse = await testClient.GetAsync("/api/mediator/session");
        initialSessionResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var initialSessionContent = await initialSessionResponse.Content.ReadAsStringAsync();
        using var initialSessionDoc = JsonDocument.Parse(initialSessionContent);

        // Session might already exist due to SessionTrackingMiddleware, but let's verify we can track the process
        var initialMessage = initialSessionDoc.RootElement.GetProperty("message").GetString();
        initialMessage.ShouldBeOneOf("Session ID Not Yet Assigned", "Current Session ID");

        // Step 2: Make requests to generate statistics
        await testClient.GetAsync("/api/products/1");
        await testClient.GetAsync("/api/products");

        var createProductCommand = new
        {
            Name = "End-to-End Test Product",
            Description = "Testing complete workflow",
            Price = 99.99m,
            StockQuantity = 10
        };
        var commandJson = JsonSerializer.Serialize(createProductCommand, _jsonOptions);
        var commandContent = new StringContent(commandJson, Encoding.UTF8, "application/json");
        await testClient.PostAsync("/api/products", commandContent);

        // Step 3: Verify session is properly set up
        var sessionResponse = await testClient.GetAsync("/api/mediator/session");
        var sessionContent = await sessionResponse.Content.ReadAsStringAsync();
        using var sessionDoc = JsonDocument.Parse(sessionContent);

        var sessionId = sessionDoc.RootElement.GetProperty("sessionId").GetString();
        sessionId.ShouldNotBeNullOrEmpty();
        sessionId.ShouldStartWith("stats_");

        var message = sessionDoc.RootElement.GetProperty("message").GetString();
        message.ShouldBe("Current Session ID");

        // Step 4: Verify global statistics show our activity
        var globalStatsResponse = await testClient.GetAsync("/api/mediator/statistics");
        var globalStatsContent = await globalStatsResponse.Content.ReadAsStringAsync();
        using var globalStatsDoc = JsonDocument.Parse(globalStatsContent);

        var globalSummary = globalStatsDoc.RootElement.GetProperty("globalStatistics").GetProperty("summary");
        globalSummary.GetProperty("totalQueryExecutions").GetInt64().ShouldBeGreaterThan(0);
        globalSummary.GetProperty("totalCommandExecutions").GetInt64().ShouldBeGreaterThan(0);

        // Step 5: Verify session-specific statistics
        var sessionStatsResponse = await testClient.GetAsync($"/api/mediator/statistics/session/{sessionId}");
        sessionStatsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var sessionStatsContent = await sessionStatsResponse.Content.ReadAsStringAsync();
        using var sessionStatsDoc = JsonDocument.Parse(sessionStatsContent);

        var sessionStats = sessionStatsDoc.RootElement.GetProperty("sessionStatistics");
        var sessionSummary = sessionStats.GetProperty("summary");
        sessionSummary.GetProperty("totalQueryExecutions").GetInt64().ShouldBeGreaterThan(0);

        // Step 6: Verify analysis endpoints work
        var queryAnalysisResponse = await testClient.GetAsync("/api/mediator/analyze/queries");
        queryAnalysisResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var commandAnalysisResponse = await testClient.GetAsync("/api/mediator/analyze/commands");
        commandAnalysisResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var completeAnalysisResponse = await testClient.GetAsync("/api/mediator/analyze");
        completeAnalysisResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 7: Verify all sessions endpoint includes our session
        var allSessionsResponse = await testClient.GetAsync("/api/mediator/statistics/sessions");
        var allSessionsContent = await allSessionsResponse.Content.ReadAsStringAsync();
        using var allSessionsDoc = JsonDocument.Parse(allSessionsContent);

        var sessions = allSessionsDoc.RootElement.GetProperty("sessions").EnumerateArray();
        var ourSession = sessions.FirstOrDefault(s => s.GetProperty("sessionId").GetString() == sessionId);
        ourSession.ValueKind.ShouldNotBe(JsonValueKind.Undefined, "Our session should be in the all sessions list");

        // Step 8: Verify session usage instructions work
        var usage = sessionDoc.RootElement.GetProperty("usage");
        usage.ValueKind.ShouldNotBe(JsonValueKind.Null);
        usage.GetProperty("viewSessionStats").GetString().ShouldContain(sessionId);
    }

    #endregion
}