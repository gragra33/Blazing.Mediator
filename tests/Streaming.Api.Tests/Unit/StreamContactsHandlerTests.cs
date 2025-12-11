using Blazing.Mediator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Streaming.Api.Handlers;
using Streaming.Api.Requests;
using Streaming.Api.Services;
using System.Reflection;

namespace Streaming.Api.Tests.Unit;

/// <summary>
/// Unit tests for streaming request handlers
/// </summary>
public class StreamContactsHandlerTests
{
    private readonly IMediator _mediator;

    public StreamContactsHandlerTests()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        // Mock IWebHostEnvironment for ContactService
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());

        // Register ContactService first
        services.AddSingleton<IContactService, ContactService>();

        // Add Mediator with manual handler registration since assembly scanning has issues in test context
        services.AddMediator(Array.Empty<Assembly>());

        // Manually register all the handlers we need for testing
        services.AddScoped<IStreamRequestHandler<StreamContactsRequest, ContactDto>, StreamContactsHandler>();
        services.AddScoped<IStreamRequestHandler<StreamContactsWithMetadataRequest, StreamResponse<ContactDto>>, StreamContactsWithMetadataHandler>();
        services.AddScoped<IRequestHandler<GetContactCountRequest, int>, GetContactCountHandler>();
        services.AddScoped<IRequestHandler<GetAllContactsRequest, ContactDto[]>, GetAllContactsHandler>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task StreamContactsHandler_WithoutSearchTerm_ReturnsAllContacts()
    {
        // Arrange
        var request = new StreamContactsRequest();
        var results = new List<ContactDto>();

        // Act
        await foreach (var contact in _mediator.SendStream(request))
        {
            results.Add(contact);
        }

        // Assert - With 50 contacts in Mock_Contacts.json, we expect all 50
        results.ShouldNotBeEmpty();
        results.Count.ShouldBe(50); // Exact count from Mock_Contacts.json

        // Verify contact structure
        var firstContact = results.First();
        firstContact.Id.ShouldBeGreaterThan(0);
        firstContact.FirstName.ShouldNotBeNullOrEmpty();
        firstContact.LastName.ShouldNotBeNullOrEmpty();
        firstContact.Email.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task StreamContactsHandler_WithSearchTerm_ReturnsFilteredContacts()
    {
        // Arrange
        var request = new StreamContactsRequest { SearchTerm = "john" };
        var results = new List<ContactDto>();

        // Act
        await foreach (var contact in _mediator.SendStream(request))
        {
            results.Add(contact);
        }

        // Assert
        if (results.Count > 0)
        {
            results.ShouldAllBe(c =>
                c.FirstName.Contains("john", StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains("john", StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains("john", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task StreamContactsHandler_SupportsCancellation()
    {
        // Arrange
        var request = new StreamContactsRequest();
        var cancellationTokenSource = new CancellationTokenSource();
        var results = new List<ContactDto>();

        // Act & Assert
        try
        {
            await foreach (var contact in _mediator.SendStream(request, cancellationTokenSource.Token))
            {
                results.Add(contact);
                if (results.Count == 5) // Cancel after 5 items
                {
                    await cancellationTokenSource.CancelAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation occurs
        }

        // Should have processed some items before cancellation
        results.Count.ShouldBeGreaterThan(0);
        results.Count.ShouldBeLessThanOrEqualTo(50); // Max available in test data
    }

    [Fact]
    public async Task StreamContactsWithMetadataHandler_ReturnsStreamResponseData()
    {
        // Arrange
        var request = new StreamContactsWithMetadataRequest();
        var results = new List<StreamResponse<ContactDto>>();

        // Act
        await foreach (var response in _mediator.SendStream(request))
        {
            results.Add(response);
        }

        // Assert - With 50 contacts in Mock_Contacts.json, we expect 50 + 1 completion signal = 51
        results.ShouldNotBeEmpty();
        results.Count.ShouldBe(51); // 50 contacts + 1 completion signal

        // Verify that the first 50 responses contain contact data
        var contactResponses = results.Take(50).ToList();
        contactResponses.ShouldAllBe(r => !r.IsComplete && r.Data.Id > 0);

        // Verify the last response is a completion signal
        var completionResponse = results.Last();
        completionResponse.IsComplete.ShouldBeTrue();
        completionResponse.Message.ShouldBe("Streaming completed successfully");
    }

    [Fact]
    public async Task GetContactCountHandler_ReturnsValidCount()
    {
        // Arrange
        var request = new GetContactCountRequest();

        // Act
        var count = await _mediator.Send(request);

        // Assert - Mock_Contacts.json contains exactly 50 contacts
        count.ShouldBe(50);
    }

    [Fact]
    public async Task GetAllContactsHandler_ReturnsContactArray()
    {
        // Arrange
        var request = new GetAllContactsRequest();

        // Act
        var contacts = await _mediator.Send(request);

        // Assert - Mock_Contacts.json contains exactly 50 contacts
        contacts.ShouldNotBeNull();
        contacts.Length.ShouldBe(50);

        // Verify contact structure
        var firstContact = contacts.First();
        firstContact.Id.ShouldBeGreaterThan(0);
        firstContact.FirstName.ShouldNotBeNullOrEmpty();
        firstContact.LastName.ShouldNotBeNullOrEmpty();
        firstContact.Email.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAllContactsHandler_WithSearchTerm_ReturnsFilteredArray()
    {
        // Arrange
        var request = new GetAllContactsRequest { SearchTerm = "doe" };

        // Act
        var contacts = await _mediator.Send(request);

        // Assert
        contacts.ShouldNotBeNull();

        if (contacts.Length > 0)
        {
            contacts.ShouldAllBe(c =>
                c.FirstName.Contains("doe", StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains("doe", StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains("doe", StringComparison.OrdinalIgnoreCase));
        }
    }
}