using Blazing.Mediator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
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
    private readonly IServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;
    private readonly IMediator _mediator;

    public StreamContactsHandlerTests()
    {
        _services = new ServiceCollection();
        _services.AddLogging(builder => builder.AddConsole());

        // Mock IWebHostEnvironment for ContactService
        _services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());

        // Register ContactService first
        _services.AddSingleton<IContactService, ContactService>();

        // Add Mediator with manual handler registration since assembly scanning has issues in test context
        _services.AddMediator(Array.Empty<Assembly>());

        // Manually register all the handlers we need for testing
        _services.AddScoped<IStreamRequestHandler<StreamContactsRequest, ContactDto>, StreamContactsHandler>();
        _services.AddScoped<IStreamRequestHandler<StreamContactsWithMetadataRequest, StreamResponse<ContactDto>>, StreamContactsWithMetadataHandler>();
        _services.AddScoped<IRequestHandler<GetContactCountRequest, int>, GetContactCountHandler>();
        _services.AddScoped<IRequestHandler<GetAllContactsRequest, ContactDto[]>, GetAllContactsHandler>();

        _serviceProvider = _services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
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

        // Assert
        results.ShouldNotBeEmpty();
        results.Count.ShouldBeGreaterThan(0);

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
                if (results.Count == 2)
                {
                    cancellationTokenSource.Cancel();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation occurs
        }

        // Should have processed some items before cancellation
        results.Count.ShouldBeGreaterThan(0);
        results.Count.ShouldBeLessThanOrEqualTo(100); // Shouldn't complete full stream
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

        // Assert
        results.ShouldNotBeEmpty();

        // Verify stream response structure
        var firstResponse = results.First();
        firstResponse.ShouldNotBeNull();
        firstResponse.Data.ShouldNotBeNull();
        firstResponse.Data.Id.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetContactCountHandler_ReturnsValidCount()
    {
        // Arrange
        var request = new GetContactCountRequest();

        // Act
        var count = await _mediator.Send(request);

        // Assert
        count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllContactsHandler_ReturnsContactArray()
    {
        // Arrange
        var request = new GetAllContactsRequest();

        // Act
        var contacts = await _mediator.Send(request);

        // Assert
        contacts.ShouldNotBeNull();
        contacts.Length.ShouldBeGreaterThan(0);

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

/// <summary>
/// Test implementation of IWebHostEnvironment for unit testing
/// </summary>
public class TestWebHostEnvironment : IWebHostEnvironment
{
    public string EnvironmentName { get; set; } = "Testing";
    public string ApplicationName { get; set; } = "Streaming.Api.Tests";
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

    // Point to the test project root directory so ContactService can find "data/Mock_Contacts.json"
    public string WebRootPath { get; set; } = GetTestProjectRoot();
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

    private static string GetTestProjectRoot()
    {
        // Navigate from bin/Debug/net9.0 back to project root
        var currentDir = Directory.GetCurrentDirectory();
        var projectRoot = currentDir;

        // If we're in a bin directory, go up to find the project root
        while (Path.GetFileName(projectRoot) != "Streaming.Api.Tests" &&
               Directory.GetParent(projectRoot) != null)
        {
            projectRoot = Directory.GetParent(projectRoot)!.FullName;
        }

        return projectRoot;
    }
}
