using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserManagement.Api.Services;

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
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the background service that might be causing issues in tests
                var statisticsCleanupService = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService) && 
                    d.ImplementationType == typeof(StatisticsCleanupService));
                if (statisticsCleanupService != null)
                {
                    services.Remove(statisticsCleanupService);
                }
            });
        });
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
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

    /// <summary>
    /// Tests that the analysis health endpoint returns OK status.
    /// </summary>
    [Fact]
    public async Task GetAnalysisHealth_ReturnsOkWithHealthInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/analysis/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Healthy");
        content.ShouldContain("User Management API - Mediator Analysis");
    }

    /// <summary>
    /// Tests that the session ID endpoint returns OK status.
    /// </summary>
    [Fact]
    public async Task GetSessionId_ReturnsOkWithSessionInfo()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/session");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Session");
    }

    /// <summary>
    /// Tests that the global statistics endpoint returns OK status.
    /// </summary>
    [Fact]
    public async Task GetStatistics_ReturnsOkWithGlobalStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/statistics");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Real-Time Mediator Statistics");
    }

    /// <summary>
    /// Tests that the all sessions statistics endpoint returns OK status.
    /// </summary>
    [Fact]
    public async Task GetAllSessionStatistics_ReturnsOkWithSessionsList()
    {
        // Act
        var response = await _client.GetAsync("/api/mediator/statistics/sessions");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Statistics for All Active Sessions");
    }
}