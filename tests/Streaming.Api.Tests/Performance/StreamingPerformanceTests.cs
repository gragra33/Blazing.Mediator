using System.Diagnostics;

namespace Streaming.Api.Tests.Performance;

/// <summary>
/// Performance tests for streaming functionality
/// </summary>
public class StreamingPerformanceTests : IClassFixture<StreamingApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly StreamingApiWebApplicationFactory _factory;

    public StreamingPerformanceTests(StreamingApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StreamContacts_CompletesWithinReasonableTime()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var maxAllowedTime = TimeSpan.FromSeconds(30); // 30 seconds max

        // Act
        var response = await _client.GetAsync("/api/contacts/stream");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.Elapsed.ShouldBeLessThan(maxAllowedTime);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task StreamContactsSSE_StreamsDataImmediately()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var firstDataReceived = TimeSpan.Zero;

        // Act
        using var response = await _client.GetAsync("/api/contacts/stream/sse", HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: {") && firstDataReceived == TimeSpan.Zero)
            {
                firstDataReceived = stopwatch.Elapsed;
                break; // We got the first data point
            }
        }

        stopwatch.Stop();

        // Assert
        firstDataReceived.ShouldBeGreaterThan(TimeSpan.Zero);
        firstDataReceived.ShouldBeLessThan(TimeSpan.FromSeconds(5)); // First data within 5 seconds
    }

    [Fact]
    public async Task ConcurrentStreamRequests_HandleLoadEffectively()
    {
        // Arrange
        const int concurrentRequests = 10;
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_client.GetAsync("/api/contacts/stream"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.ShouldAllBe(r => r.IsSuccessStatusCode);
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMinutes(1)); // All requests within 1 minute

        // Verify all responses have content
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task StreamingVsBulk_PerformanceComparison()
    {
        // Arrange
        var bulkStopwatch = Stopwatch.StartNew();

        // Act - Bulk request
        var bulkResponse = await _client.GetAsync("/api/contacts/all");
        bulkStopwatch.Stop();

        var streamStopwatch = Stopwatch.StartNew();

        // Act - Stream request
        var streamResponse = await _client.GetAsync("/api/contacts/stream");
        streamStopwatch.Stop();

        // Assert
        bulkResponse.EnsureSuccessStatusCode();
        streamResponse.EnsureSuccessStatusCode();

        var bulkContent = await bulkResponse.Content.ReadAsStringAsync();
        var streamContent = await streamResponse.Content.ReadAsStringAsync();

        // Both should return same data structure
        var bulkContacts = JsonSerializer.Deserialize<ContactDto[]>(bulkContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var streamContacts = JsonSerializer.Deserialize<ContactDto[]>(streamContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        bulkContacts?.Length.ShouldBe(streamContacts?.Length ?? 0);

        // Performance characteristics may vary, but both should complete reasonably
        bulkStopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(30));
        streamStopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task SSEStream_HandlesLongRunningConnections()
    {
        // Arrange
        var dataPointsReceived = 0;
        var connectionDuration = TimeSpan.FromSeconds(10); // Test for 10 seconds
        var cts = new CancellationTokenSource(connectionDuration);

        // Act
        using var response = await _client.GetAsync("/api/contacts/stream/sse", HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        try
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null && !cts.Token.IsCancellationRequested)
            {
                if (line.StartsWith("data: {"))
                {
                    dataPointsReceived++;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when timeout occurs
        }

        // Assert
        dataPointsReceived.ShouldBeGreaterThan(0);
        // Should have received data throughout the connection period
    }

    [Fact]
    public async Task FilteredStream_PerformanceWithSearchTerms()
    {
        // Arrange
        var searchTerms = new[] { "john", "doe", "smith", "johnson", "brown" };
        var tasks = new List<Task<HttpResponseMessage>>();
        var stopwatch = Stopwatch.StartNew();

        // Act - Test multiple filtered streams concurrently
        foreach (var searchTerm in searchTerms)
        {
            tasks.Add(_client.GetAsync($"/api/contacts/stream?search={searchTerm}"));
            tasks.Add(_client.GetAsync($"/api/contacts/stream/sse?search={searchTerm}"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.ShouldAllBe(r => r.IsSuccessStatusCode);
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMinutes(2)); // All filtered requests within 2 minutes

        // Verify filtered responses have content
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();
        }
    }
}
