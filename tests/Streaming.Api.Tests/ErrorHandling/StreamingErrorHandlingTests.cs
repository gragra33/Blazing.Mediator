using System.Net;

namespace Streaming.Api.Tests.ErrorHandling;

/// <summary>
/// Tests for error handling and edge cases in streaming endpoints
/// </summary>
public class StreamingErrorHandlingTests(StreamingApiWebApplicationFactory factory)
    : IClassFixture<StreamingApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task StreamEndpoints_HandleVeryLongSearchTerms()
    {
        // Arrange
        var longSearchTerm = new string('a', 1000); // 1000 character search term

        // Act & Assert - Should handle gracefully without crashing
        var response1 = await _client.GetAsync($"/api/contacts/all?search={longSearchTerm}").ConfigureAwait(false);
        var response2 = await _client.GetAsync($"/api/contacts/stream?search={longSearchTerm}").ConfigureAwait(false);
        var response3 = await _client.GetAsync($"/api/contacts/stream/sse?search={longSearchTerm}").ConfigureAwait(false);

        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
        response3.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task StreamEndpoints_HandleSpecialCharactersInSearch()
    {
        // Arrange
        var specialCharacterTerms = new[]
        {
            "john@doe.com",
            "user+tag@example.com",
            "name with spaces",
            "name-with-hyphens",
            "name_with_underscores",
            "name.with.dots",
            "name'with'quotes",
            "name\"with\"doublequotes"
        };

        foreach (var searchTerm in specialCharacterTerms)
        {
            // Act
            var encodedSearchTerm = Uri.EscapeDataString(searchTerm);
            var response1 = await _client.GetAsync($"/api/contacts/all?search={encodedSearchTerm}").ConfigureAwait(false);
            var response2 = await _client.GetAsync($"/api/contacts/stream?search={encodedSearchTerm}").ConfigureAwait(false);

            // Assert
            response1.EnsureSuccessStatusCode();
            response2.EnsureSuccessStatusCode();
        }
    }

    [Fact]
    public async Task StreamSSE_HandlesClientDisconnection()
    {
        // Arrange
        var cts = new CancellationTokenSource();

        // Act - Start SSE stream and cancel quickly
        using var response = await _client.GetAsync("/api/contacts/stream/sse", HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        // Read a few lines then cancel
        var linesRead = 0;
        try
        {
            while (linesRead < 5 && await reader.ReadLineAsync(cts.Token).ConfigureAwait(false) != null)
            {
                linesRead++;
            }
            await cts.CancelAsync(); // Simulate client disconnection

            // Try to read more - should handle cancellation gracefully
            await reader.ReadLineAsync(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation occurs
        }

        // Assert
        linesRead.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task StreamEndpoints_HandleEmptyResultSets()
    {
        // Arrange - Use a search term that should return no results
        var noResultsSearchTerm = "zzzznonexistentterm12345";

        // Act
        var response1 = await _client.GetAsync($"/api/contacts/all?search={noResultsSearchTerm}").ConfigureAwait(false);
        var response2 = await _client.GetAsync($"/api/contacts/stream?search={noResultsSearchTerm}").ConfigureAwait(false);
        var response3 = await _client.GetAsync($"/api/contacts/stream/sse?search={noResultsSearchTerm}").ConfigureAwait(false);

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();
        response3.EnsureSuccessStatusCode();

        // Verify responses handle empty results gracefully
        var content1 = await response1.Content.ReadAsStringAsync().ConfigureAwait(false);
        var content2 = await response2.Content.ReadAsStringAsync().ConfigureAwait(false);
        var content3 = await response3.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Should return empty arrays or appropriate empty responses
        var contacts1 = JsonSerializer.Deserialize<ContactDto[]>(content1, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var contacts2 = JsonSerializer.Deserialize<ContactDto[]>(content2, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        contacts1?.Length.ShouldBe(0);
        contacts2?.Length.ShouldBe(0);

        // SSE should still include start/end events even with no data
        content3.ShouldContain("event: start");
    }

    [Fact]
    public async Task StreamEndpoints_HandleInvalidHttpMethods()
    {
        // Act & Assert - Test non-GET methods
        var postResponse = await _client.PostAsync("/api/contacts/stream", null).ConfigureAwait(false);
        var putResponse = await _client.PutAsync("/api/contacts/stream", null).ConfigureAwait(false);
        var deleteResponse = await _client.DeleteAsync("/api/contacts/stream").ConfigureAwait(false);

        postResponse.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task StreamEndpoints_HandleMalformedUrls()
    {
        // Arrange
        var malformedUrls = new[]
        {
            "/api/contacts/stream?search=",
            "/api/contacts/stream?search=%",
            "/api/contacts/stream?invalidparam=test",
            "/api/contacts/stream?search=test&search=duplicate"
        };

        foreach (var url in malformedUrls)
        {
            // Act
            var response = await _client.GetAsync(url).ConfigureAwait(false);

            // Assert - Should handle gracefully, not crash
            response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task StreamSSE_HandlesLongRunningConnectionInterruption()
    {
        // Arrange
        var maxTestDuration = TimeSpan.FromSeconds(30);
        var cts = new CancellationTokenSource(maxTestDuration);
        var dataReceived = false;

        try
        {
            // Act
            using var response = await _client.GetAsync("/api/contacts/stream/sse", HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync(cts.Token).ConfigureAwait(false)) != null && !cts.Token.IsCancellationRequested)
            {
                if (line.StartsWith("data: {"))
                {
                    dataReceived = true;
                    // Simulate connection interruption after receiving some data
                    await cts.CancelAsync();
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation occurs
        }

        // Assert
        dataReceived.ShouldBeTrue();
    }

    [Fact]
    public async Task ContactCount_HandlesHighLoadScenarios()
    {
        // Arrange
        const int concurrentRequests = 50;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Hammer the count endpoint
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_client.GetAsync("/api/contacts/count"));
        }

        var responses = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert
        responses.ShouldAllBe(r => r.IsSuccessStatusCode);

        // All responses should return the same count
        var contents = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync())).ConfigureAwait(false);
        var firstContent = contents.First();
        contents.ShouldAllBe(c => c == firstContent);
    }

    [Fact]
    public async Task StreamEndpoints_HandleTimeoutScenarios()
    {
        // Arrange
        var shortTimeout = TimeSpan.FromMilliseconds(100);
        var cts = new CancellationTokenSource(shortTimeout);

        // Act & Assert - Very short timeout should trigger cancellation
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            var response = await _client.GetAsync("/api/contacts/stream/sse", cts.Token).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            // Try to read the entire stream with short timeout
            while (await reader.ReadLineAsync(cts.Token).ConfigureAwait(false) != null)
            {
                // Keep reading until timeout
            }
        }).ConfigureAwait(false);
    }
}
