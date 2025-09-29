using Streaming.Api.Models;
using Streaming.Api.Shared.DTOs;
using System.Text.Json;

namespace Streaming.Api.Services;

/// <summary>
/// Service for handling contact data operations with streaming support
/// Following DRY, KISS, SOLID, and YAGNI principles
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Stream all contacts asynchronously
    /// </summary>
    IAsyncEnumerable<ContactDto> StreamContactsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stream contacts with search filter
    /// </summary>
    IAsyncEnumerable<ContactDto> StreamContactsAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stream contacts with response wrapper including statistics
    /// </summary>
    IAsyncEnumerable<StreamResponse<ContactDto>> StreamContactsWithMetadataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stream contacts with metadata and search filtering
    /// </summary>
    IAsyncEnumerable<StreamResponse<ContactDto>> StreamContactsWithMetadataAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total count of contacts
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of contact service using JSON streaming
/// </summary>
public class ContactService(IWebHostEnvironment environment, ILogger<ContactService> logger)
    : IContactService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async IAsyncEnumerable<ContactDto> StreamContactsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(environment.WebRootPath, "data", "Mock_Contacts.json");

        if (!File.Exists(filePath))
        {
            logger.LogWarning("Contact data file not found at: {FilePath}", filePath);
            yield break;
        }

        logger.LogInformation("Starting to stream contacts from: {FilePath}", filePath);

        var processedCount = 0;
        Contact[]? jsonData = null;

        try
        {
            await using var fileStream = File.OpenRead(filePath);
            jsonData = await JsonSerializer.DeserializeAsync<Contact[]>(fileStream, _jsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error parsing JSON file: {FilePath}", filePath);
            yield break;
        }

        if (jsonData != null)
        {
            foreach (var contact in jsonData)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                yield return MapToDto(contact);
                processedCount++;            // Add small delay to simulate real streaming behavior
                if (processedCount % 10 == 0)
                {
                    await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        logger.LogInformation("Completed streaming {Count} contacts", processedCount);
    }

    public async IAsyncEnumerable<ContactDto> StreamContactsAsync(string searchTerm, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(normalizedSearchTerm))
        {
            await foreach (var contact in StreamContactsAsync(cancellationToken))
            {
                yield return contact;
            }
            yield break;
        }

        logger.LogInformation("Streaming contacts with search term: {SearchTerm}", searchTerm);

        await foreach (var contact in StreamContactsAsync(cancellationToken))
        {
            if (ContactMatchesSearch(contact, normalizedSearchTerm))
            {
                yield return contact;
            }
        }
    }

    public async IAsyncEnumerable<StreamResponse<ContactDto>> StreamContactsWithMetadataAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var processedCount = 0;
        var totalCount = await GetTotalCountAsync(cancellationToken).ConfigureAwait(false);

        await foreach (var contact in StreamContactsAsync(cancellationToken))
        {
            processedCount++;
            var elapsed = DateTime.UtcNow - startTime;

            yield return new StreamResponse<ContactDto>
            {
                Data = contact,
                Statistics = new StreamStatistics
                {
                    TotalItems = totalCount,
                    ProcessedItems = processedCount,
                    ElapsedTime = elapsed
                },
                IsComplete = false,
                Message = $"Processing contact {processedCount} of {totalCount}"
            };
        }

        // Send completion signal
        yield return new StreamResponse<ContactDto>
        {
            Data = new ContactDto(),
            Statistics = new StreamStatistics
            {
                TotalItems = totalCount,
                ProcessedItems = processedCount,
                ElapsedTime = DateTime.UtcNow - startTime
            },
            IsComplete = true,
            Message = "Streaming completed successfully"
        };
    }

    public async IAsyncEnumerable<StreamResponse<ContactDto>> StreamContactsWithMetadataAsync(string searchTerm, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(normalizedSearchTerm))
        {
            await foreach (var response in StreamContactsWithMetadataAsync(cancellationToken))
            {
                yield return response;
            }
            yield break;
        }

        logger.LogInformation("Streaming contacts with metadata and search term: {SearchTerm}", searchTerm);

        var startTime = DateTime.UtcNow;
        var count = 0;

        await foreach (var contact in StreamContactsAsync(cancellationToken))
        {
            if (ContactMatchesSearch(contact, normalizedSearchTerm))
            {
                count++;
                var elapsedTime = DateTime.UtcNow - startTime;

                var response = new StreamResponse<ContactDto>
                {
                    Data = contact,
                    Statistics = new StreamStatistics
                    {
                        ProcessedItems = count,
                        TotalItems = count, // This will be updated at the end
                        ElapsedTime = elapsedTime
                    }
                };

                yield return response;

                // Small delay to demonstrate streaming
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            }
        }

        // Final completion response
        var finalElapsedTime = DateTime.UtcNow - startTime;
        yield return new StreamResponse<ContactDto>
        {
            Data = new ContactDto(),
            IsComplete = true,
            Statistics = new StreamStatistics
            {
                ProcessedItems = count,
                TotalItems = count,
                ElapsedTime = finalElapsedTime
            }
        };
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(environment.WebRootPath, "data", "Mock_Contacts.json");

        if (!File.Exists(filePath))
        {
            return 0;
        }

        try
        {
            await using var fileStream = File.OpenRead(filePath);
            var jsonData = await JsonSerializer.DeserializeAsync<Contact[]>(fileStream, _jsonOptions, cancellationToken);
            return jsonData?.Length ?? 0;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error parsing JSON file for count: {FilePath}", filePath);
            return 0;
        }
    }

    private static ContactDto MapToDto(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email,
            Company = contact.Company.Name,
            City = contact.Address.City,
            Country = contact.Address.Country,
            Phone = contact.Phone,
            Avatar = contact.Avatar
        };
    }

    private static bool ContactMatchesSearch(ContactDto contact, string searchTerm)
    {
        return contact.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               contact.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               contact.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               contact.Company.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               contact.City.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
               contact.Country.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}
