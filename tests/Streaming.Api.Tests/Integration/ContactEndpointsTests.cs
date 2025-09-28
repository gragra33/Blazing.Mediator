namespace Streaming.Api.Tests.Integration;

/// <summary>
/// Integration tests for Contact streaming endpoints
/// </summary>
public class ContactEndpointsTests(StreamingApiWebApplicationFactory factory)
    : IClassFixture<StreamingApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly StreamingApiWebApplicationFactory _factory = factory;

    private const int ExpectedTotalContacts = 50; // Total contacts in test Mock_Contacts.json

    [Fact]
    public async Task GetContactCount_ReturnsValidCount()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/count").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<dynamic>(content);

        // Should return a count object
        content.ShouldContain("count");
    }

    [Fact]
    public async Task GetAllContacts_ReturnsBulkContactData()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/all").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var contacts = JsonSerializer.Deserialize<ContactDto[]>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        contacts.ShouldNotBeNull();
        contacts.Length.ShouldBe(ExpectedTotalContacts); // Expect exactly 50 contacts from test data

        // Verify contact structure
        var firstContact = contacts.First();
        firstContact.Id.ShouldBeGreaterThan(0);
        firstContact.FirstName.ShouldNotBeNullOrEmpty();
        firstContact.LastName.ShouldNotBeNullOrEmpty();
        firstContact.Email.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAllContacts_WithSearchTerm_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/all?search=john").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var contacts = JsonSerializer.Deserialize<ContactDto[]>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        contacts.ShouldNotBeNull();
        contacts.Length.ShouldBeLessThanOrEqualTo(ExpectedTotalContacts); // Filtered results should be within total
        // Verify filtering worked (assuming test data contains contacts with "john")
        if (contacts.Length > 0)
        {
            contacts.ShouldAllBe(c =>
                c.FirstName.Contains("john", StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains("john", StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains("john", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task StreamContacts_ReturnsJsonStreamData()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/stream").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var contacts = JsonSerializer.Deserialize<ContactDto[]>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        contacts.ShouldNotBeNull();
        contacts.Length.ShouldBe(ExpectedTotalContacts); // Expect exactly 50 contacts from test data

        // Verify contact structure
        var firstContact = contacts.First();
        firstContact.Id.ShouldBeGreaterThan(0);
        firstContact.FirstName.ShouldNotBeNullOrEmpty();
        firstContact.LastName.ShouldNotBeNullOrEmpty();
        firstContact.Email.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task StreamContacts_WithSearchTerm_ReturnsFilteredStream()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/stream?search=doe").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var contacts = JsonSerializer.Deserialize<ContactDto[]>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        contacts.ShouldNotBeNull();
        contacts.Length.ShouldBeLessThanOrEqualTo(ExpectedTotalContacts); // Filtered results should be within total
        // Verify filtering worked
        if (contacts.Length > 0)
        {
            contacts.ShouldAllBe(c =>
                c.FirstName.Contains("doe", StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains("doe", StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains("doe", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task StreamContactsSSE_ReturnsServerSentEvents()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/stream/sse").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Verify SSE format
        content.ShouldContain("event: start");
        content.ShouldContain("data:");
        content.ShouldContain("event: data");

        // Should contain contact data events
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Find contact data by looking for "event: data" followed by "data: " lines
        var contactDataLines = new List<string>();
        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (lines[i].Trim() == "event: data" && lines[i + 1].StartsWith("data: "))
            {
                contactDataLines.Add(lines[i + 1]);
            }
        }

        contactDataLines.Count.ShouldBe(ExpectedTotalContacts); // Should stream all 50 contacts

        // Verify at least one contact data line can be parsed
        var firstDataLine = contactDataLines.First();
        var jsonData = firstDataLine.Substring(6); // Remove "data: " prefix
        var contact = JsonSerializer.Deserialize<ContactDto>(jsonData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        contact.ShouldNotBeNull();
        contact.Id.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task StreamContactsSSE_WithSearchTerm_ReturnsFilteredSSE()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/stream/sse?search=china").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/event-stream");

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Verify SSE format and filtering
        content.ShouldContain("event: start");
        content.ShouldContain("event: data");

        // Extract and verify contact data
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Find contact data by looking for "event: data" followed by "data: " lines
        var contactDataLines = new List<string>();
        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (lines[i].Trim() == "event: data" && lines[i + 1].StartsWith("data: "))
            {
                contactDataLines.Add(lines[i + 1]);
            }
        }

        contactDataLines.Count.ShouldBeLessThanOrEqualTo(ExpectedTotalContacts); // Filtered should be within total

        if (contactDataLines.Count > 0)
        {
            foreach (var dataLine in contactDataLines)
            {
                var jsonData = dataLine.Substring(6); // Remove "data: " prefix
                var contact = JsonSerializer.Deserialize<ContactDto>(jsonData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Verify filtering
                if (contact != null)
                {
                    (contact.FirstName.Contains("china", StringComparison.OrdinalIgnoreCase) ||
                     contact.LastName.Contains("china", StringComparison.OrdinalIgnoreCase) ||
                     contact.Email.Contains("china", StringComparison.OrdinalIgnoreCase) ||
                     contact.Company.Contains("china", StringComparison.OrdinalIgnoreCase) ||
                     contact.City.Contains("china", StringComparison.OrdinalIgnoreCase) ||
                     contact.Country.Contains("china", StringComparison.OrdinalIgnoreCase))
                    .ShouldBeTrue($"Contact {contact.FirstName} {contact.LastName} should contain 'china'");
                }
            }
        }
    }

    [Fact]
    public async Task StreamContactsSSE_IncludesProgressEvents()
    {
        // Act
        var response = await _client.GetAsync("/api/contacts/stream/sse").ConfigureAwait(false);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Should include progress events for datasets with batch processing
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var progressLines = lines.Where(line => line.StartsWith("event: progress")).ToArray();

        // With 50 contacts and batch processing every 10, we should have progress events
        var dataLines = lines.Where(line => line.StartsWith("data: {")).ToArray();
        if (dataLines.Length >= 10)
        {
            progressLines.Length.ShouldBeGreaterThan(0);

            // Verify progress data format
            var progressDataLines = lines.Where(line => line.StartsWith("data: {\"processed\"")).ToArray();
            progressDataLines.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public async Task ContactEndpoints_HandlesConcurrentRequests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act - Make multiple concurrent requests
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.GetAsync("/api/contacts/count"));
            tasks.Add(_client.GetAsync("/api/contacts/all"));
            tasks.Add(_client.GetAsync("/api/contacts/stream"));
        }

        var responses = await Task.WhenAll(tasks).ConfigureAwait(false);

        // Assert
        responses.ShouldAllBe(r => r.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ContactEndpoints_HandleInvalidSearchTerms()
    {
        // Test with various edge cases
        var testCases = new[]
        {
            "/api/contacts/all?search=",
            "/api/contacts/all?search=%20", // Space
            "/api/contacts/stream?search=",
            "/api/contacts/stream/sse?search="
        };

        foreach (var testCase in testCases)
        {
            // Act
            var response = await _client.GetAsync(testCase).ConfigureAwait(false);

            // Assert
            response.EnsureSuccessStatusCode();
            // Should handle empty/invalid search terms gracefully
        }
    }
}
