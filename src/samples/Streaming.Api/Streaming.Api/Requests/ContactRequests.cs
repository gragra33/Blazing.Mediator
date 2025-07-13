using Blazing.Mediator;
using Streaming.Api.Shared.DTOs;

namespace Streaming.Api.Requests;

/// <summary>
/// Stream request to get all contacts
/// </summary>
public class StreamContactsRequest : IStreamRequest<ContactDto>
{
    public string? SearchTerm { get; set; }
    public bool IncludeMetadata { get; set; }
}

/// <summary>
/// Stream request to get contacts with metadata and statistics
/// </summary>
public class StreamContactsWithMetadataRequest : IStreamRequest<StreamResponse<ContactDto>>
{
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Request to get contact count (regular request, not streaming)
/// </summary>
public class GetContactCountRequest : IRequest<int>
{
}

/// <summary>
/// Request to get all contacts at once (bulk load, not streaming)
/// </summary>
public class GetAllContactsRequest : IRequest<ContactDto[]>
{
    public string? SearchTerm { get; set; }
}
