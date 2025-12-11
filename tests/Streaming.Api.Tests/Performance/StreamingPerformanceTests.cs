using System.Diagnostics;

namespace Streaming.Api.Tests.Performance;

/// <summary>
/// Performance tests for streaming functionality
/// </summary>
public class StreamingPerformanceTests(StreamingApiWebApplicationFactory factory)
    : IClassFixture<StreamingApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private const int BatchSize = 10;
    private const int MaxAvailableContacts = 50; // Total contacts in Mock_Contacts.json

    [Fact]
    public async Task StreamContacts_CompletesWithinReasonableTime()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var maxAllowedTime = TimeSpan.FromSeconds(30); // 30 seconds max

        // Act
        var response = await _client.GetAsync($"/api/contacts/stream?batchSize={BatchSize}");
        stopwatch.Stop();

        // Assert
        response.EnsureSuccessStatusCode();
        stopwatch.Elapsed.ShouldBeLessThan(maxAllowedTime);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        var contacts = JsonSerializer.Deserialize<ContactDto[]>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        contacts.ShouldNotBeNull();
        contacts.Length.ShouldBe(MaxAvailableContacts); // Should return all 50 contacts
    }

    [Fact]
    public async Task StreamContactsSSE_StreamsDataImmediately()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var firstDataReceived = TimeSpan.Zero;
        int dataCount = 0;

        // Act
        using var response = await _client.GetAsync($"/api/contacts/stream/sse?batchSize={BatchSize}", HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync() is { } line)
        {
            if (line.StartsWith("data: {") && firstDataReceived == TimeSpan.Zero)
            {
                firstDataReceived = stopwatch.Elapsed;
            }
            if (line.StartsWith("data: {"))
            {
                dataCount++;
                if (dataCount >= MaxAvailableContacts) // Stop after all available contacts
                    break;
            }
        }

        stopwatch.Stop();

        // Assert
        firstDataReceived.ShouldBeGreaterThan(TimeSpan.Zero);
        firstDataReceived.ShouldBeLessThan(TimeSpan.FromSeconds(5)); // First data within 5 seconds
        dataCount.ShouldBe(MaxAvailableContacts); // Should stream all 50 contacts
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
            tasks.Add(_client.GetAsync($"/api/contacts/stream?batchSize={BatchSize}"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.ShouldAllBe(r => r.IsSuccessStatusCode);
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMinutes(1)); // All requests within 1 minute

        // Verify all responses have content and return all available contacts
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();
            var contacts = JsonSerializer.Deserialize<ContactDto[]>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            contacts.ShouldNotBeNull();
            contacts.Length.ShouldBe(MaxAvailableContacts); // Should return all 50 contacts
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
        var streamResponse = await _client.GetAsync($"/api/contacts/stream?batchSize={BatchSize}");
        streamStopwatch.Stop();

        // Assert
        bulkResponse.EnsureSuccessStatusCode();
        streamResponse.EnsureSuccessStatusCode();

        var bulkContent = await bulkResponse.Content.ReadAsStringAsync();
        var streamContent = await streamResponse.Content.ReadAsStringAsync();

        // Both should return same data structure with all 50 contacts
        var bulkContacts = JsonSerializer.Deserialize<ContactDto[]>(bulkContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var streamContacts = JsonSerializer.Deserialize<ContactDto[]>(streamContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        bulkContacts?.Length.ShouldBe(streamContacts?.Length ?? 0);
        bulkContacts?.Length.ShouldBe(MaxAvailableContacts); // Both should return all 50 contacts

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
        using var response = await _client.GetAsync($"/api/contacts/stream/sse?batchSize={BatchSize}", HttpCompletionOption.ResponseHeadersRead, cts.Token);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream);

        try
        {
            while (await reader.ReadLineAsync(cts.Token) is { } line && !cts.Token.IsCancellationRequested)
            {
                if (line.StartsWith("data: {"))
                {
                    dataPointsReceived++;
                    if (dataPointsReceived >= MaxAvailableContacts) // Stop after all available contacts
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when timeout occurs
        }

        // Assert
        dataPointsReceived.ShouldBeGreaterThan(0);
        dataPointsReceived.ShouldBeLessThanOrEqualTo(MaxAvailableContacts); // Should receive up to 50 contacts
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
            tasks.Add(_client.GetAsync($"/api/contacts/stream?search={searchTerm}&batchSize={BatchSize}"));
            tasks.Add(_client.GetAsync($"/api/contacts/stream/sse?search={searchTerm}&batchSize={BatchSize}"));
        }

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.ShouldAllBe(r => r.IsSuccessStatusCode);
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMinutes(2)); // All filtered requests within 2 minutes

        // Verify filtered responses have content and are within available data limits
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();
            
            // Check if this is an SSE response or JSON response
            if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
            {
                // Handle SSE response - just verify it contains event data
                content.ShouldContain("event:");
                content.ShouldContain("data:");
            }
            else
            {
                // Handle JSON response - deserialize and validate
                var contacts = JsonSerializer.Deserialize<ContactDto[]>(content, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                contacts.ShouldNotBeNull();
                contacts.Length.ShouldBeLessThanOrEqualTo(MaxAvailableContacts); // Should be within available data
            }
        }
    }
}
