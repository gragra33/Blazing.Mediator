using Blazing.Mediator;
using Streaming.Api.Requests;
using Streaming.Api.Services;
using Streaming.Api.Shared.DTOs;

namespace Streaming.Api.Handlers;

/// <summary>
/// Handler for streaming contacts
/// Implements IStreamRequestHandler to support streaming responses
/// </summary>
public class StreamContactsHandler(IContactService contactService, ILogger<StreamContactsHandler> logger)
    : IStreamRequestHandler<StreamContactsRequest, ContactDto>
{
    public async IAsyncEnumerable<ContactDto> Handle(StreamContactsRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting streaming contacts with search term: {SearchTerm}", request.SearchTerm);

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            await foreach (var contact in contactService.StreamContactsAsync(cancellationToken))
            {
                yield return contact;
            }
        }
        else
        {
            await foreach (var contact in contactService.StreamContactsAsync(request.SearchTerm, cancellationToken))
            {
                yield return contact;
            }
        }

        logger.LogInformation("Completed streaming contacts");
    }
}

/// <summary>
/// Handler for streaming contacts with metadata
/// </summary>
public class StreamContactsWithMetadataHandler(
    IContactService contactService,
    ILogger<StreamContactsWithMetadataHandler> logger)
    : IStreamRequestHandler<StreamContactsWithMetadataRequest, StreamResponse<ContactDto>>
{
    public async IAsyncEnumerable<StreamResponse<ContactDto>> Handle(StreamContactsWithMetadataRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting streaming contacts with metadata, search term: {SearchTerm}", request.SearchTerm);

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            await foreach (var response in contactService.StreamContactsWithMetadataAsync(cancellationToken))
            {
                yield return response;
            }
        }
        else
        {
            await foreach (var response in contactService.StreamContactsWithMetadataAsync(request.SearchTerm, cancellationToken))
            {
                yield return response;
            }
        }

        logger.LogInformation("Completed streaming contacts with metadata");
    }
}

/// <summary>
/// Handler for getting contact count (regular request handler)
/// </summary>
public class GetContactCountHandler(IContactService contactService, ILogger<GetContactCountHandler> logger)
    : IRequestHandler<GetContactCountRequest, int>
{
    public async Task<int> Handle(GetContactCountRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting contact count");
        var count = await contactService.GetTotalCountAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Contact count: {Count}", count);
        return count;
    }
}

/// <summary>
/// Handler for getting all contacts at once (bulk load, regular request handler)
/// </summary>
public class GetAllContactsHandler(IContactService contactService, ILogger<GetAllContactsHandler> logger)
    : IRequestHandler<GetAllContactsRequest, ContactDto[]>
{
    public async Task<ContactDto[]> Handle(GetAllContactsRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting all contacts, search term: {SearchTerm}", request.SearchTerm);

        var contacts = new List<ContactDto>();

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            await foreach (var contact in contactService.StreamContactsAsync(cancellationToken))
            {
                contacts.Add(contact);
            }
        }
        else
        {
            await foreach (var contact in contactService.StreamContactsAsync(request.SearchTerm, cancellationToken))
            {
                contacts.Add(contact);
            }
        }

        logger.LogInformation("Retrieved {Count} contacts", contacts.Count);
        return contacts.ToArray();
    }
}
