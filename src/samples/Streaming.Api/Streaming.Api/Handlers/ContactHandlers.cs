using Blazing.Mediator;
using Streaming.Api.Requests;
using Streaming.Api.Services;
using Streaming.Api.Shared.DTOs;

namespace Streaming.Api.Handlers;

/// <summary>
/// Handler for streaming contacts
/// Implements IStreamRequestHandler to support streaming responses
/// </summary>
public class StreamContactsHandler : IStreamRequestHandler<StreamContactsRequest, ContactDto>
{
    private readonly IContactService _contactService;
    private readonly ILogger<StreamContactsHandler> _logger;

    public StreamContactsHandler(IContactService contactService, ILogger<StreamContactsHandler> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    public async IAsyncEnumerable<ContactDto> Handle(StreamContactsRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting streaming contacts with search term: {SearchTerm}", request.SearchTerm);

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            await foreach (var contact in _contactService.StreamContactsAsync(cancellationToken))
            {
                yield return contact;
            }
        }
        else
        {
            await foreach (var contact in _contactService.StreamContactsAsync(request.SearchTerm, cancellationToken))
            {
                yield return contact;
            }
        }

        _logger.LogInformation("Completed streaming contacts");
    }
}

/// <summary>
/// Handler for streaming contacts with metadata
/// </summary>
public class StreamContactsWithMetadataHandler : IStreamRequestHandler<StreamContactsWithMetadataRequest, StreamResponse<ContactDto>>
{
    private readonly IContactService _contactService;
    private readonly ILogger<StreamContactsWithMetadataHandler> _logger;

    public StreamContactsWithMetadataHandler(IContactService contactService, ILogger<StreamContactsWithMetadataHandler> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    public async IAsyncEnumerable<StreamResponse<ContactDto>> Handle(StreamContactsWithMetadataRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting streaming contacts with metadata, search term: {SearchTerm}", request.SearchTerm);

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            await foreach (var response in _contactService.StreamContactsWithMetadataAsync(cancellationToken))
            {
                yield return response;
            }
        }
        else
        {
            await foreach (var response in _contactService.StreamContactsWithMetadataAsync(request.SearchTerm, cancellationToken))
            {
                yield return response;
            }
        }

        _logger.LogInformation("Completed streaming contacts with metadata");
    }
}

/// <summary>
/// Handler for getting contact count (regular request handler)
/// </summary>
public class GetContactCountHandler : IRequestHandler<GetContactCountRequest, int>
{
    private readonly IContactService _contactService;
    private readonly ILogger<GetContactCountHandler> _logger;

    public GetContactCountHandler(IContactService contactService, ILogger<GetContactCountHandler> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    public async Task<int> Handle(GetContactCountRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting contact count");
        var count = await _contactService.GetTotalCountAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Contact count: {Count}", count);
        return count;
    }
}

/// <summary>
/// Handler for getting all contacts at once (bulk load, regular request handler)
/// </summary>
public class GetAllContactsHandler : IRequestHandler<GetAllContactsRequest, ContactDto[]>
{
    private readonly IContactService _contactService;
    private readonly ILogger<GetAllContactsHandler> _logger;

    public GetAllContactsHandler(IContactService contactService, ILogger<GetAllContactsHandler> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    public async Task<ContactDto[]> Handle(GetAllContactsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all contacts, search term: {SearchTerm}", request.SearchTerm);

        var contacts = new List<ContactDto>();

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            await foreach (var contact in _contactService.StreamContactsAsync(cancellationToken))
            {
                contacts.Add(contact);
            }
        }
        else
        {
            await foreach (var contact in _contactService.StreamContactsAsync(request.SearchTerm, cancellationToken))
            {
                contacts.Add(contact);
            }
        }

        _logger.LogInformation("Retrieved {Count} contacts", contacts.Count);
        return contacts.ToArray();
    }
}
