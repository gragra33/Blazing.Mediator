using System.Diagnostics;
using System.Runtime.CompilerServices;
using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for StreamUsersWithMetadataQuery that demonstrates streaming with metadata and OpenTelemetry tracing.
/// </summary>
public sealed class StreamUsersWithMetadataHandler(ApplicationDbContext context) 
    : IStreamRequestHandler<StreamUsersWithMetadataQuery, StreamResponseDto<UserDto>>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(StreamUsersWithMetadataHandler)}";

    public async IAsyncEnumerable<StreamResponseDto<UserDto>> Handle(
        StreamUsersWithMetadataQuery request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", $"{HandlerName}.Handle");
        activity?.SetTag("stream.count", request.Count);
        activity?.SetTag("stream.delay_ms", request.DelayMs);
        activity?.SetTag("includeInactive", request.IncludeInactive);
        activity?.SetTag("searchTerm", request.SearchTerm);

        var startTime = DateTime.UtcNow;
        var batchId = Guid.NewGuid().ToString("N")[..8];
        
        activity?.SetTag("stream.batch_id", batchId);
        activity?.SetTag("stream.start_time", startTime.ToString("O"));

        // Get all available users from database first
        List<User> availableUsers;
        try
        {
            var query = context.Users.AsQueryable();
            
            if (!request.IncludeInactive)
            {
                query = query.Where(u => u.IsActive);
            }
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(u => u.Name.Contains(request.SearchTerm) 
                                         || u.Email.Contains(request.SearchTerm));
            }

            availableUsers = await query.ToListAsync(cancellationToken);
            
            activity?.SetTag("stream.available_users", availableUsers.Count);
            activity?.SetTag("stream.total_items", request.Count);
            activity?.SetTag("stream.will_loop", request.Count > availableUsers.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            throw;
        }

        // Handle empty results
        if (availableUsers.Count == 0)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "No users found matching criteria");
            yield break;
        }

        // Stream the requested number of users with metadata, looping through available data if needed
        for (int i = 0; i < request.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var itemStartTime = DateTime.UtcNow;
            
            // Simulate processing delay for demo purposes
            if (request.DelayMs > 0)
            {
                await Task.Delay(request.DelayMs, cancellationToken);
            }
            
            var processingTime = DateTime.UtcNow - itemStartTime;
            
            // Loop through available users if request.Count > availableUsers.Count
            var userIndex = i % availableUsers.Count;
            var user = availableUsers[userIndex];
            var loopCycle = i / availableUsers.Count + 1;
            
            // Add item-specific activity event
            activity?.AddEvent(new ActivityEvent($"stream.item.{i}", default, new ActivityTagsCollection
            {
                ["item.number"] = i + 1,
                ["item.user_id"] = user.Id,
                ["item.user_index"] = userIndex,
                ["item.loop_cycle"] = loopCycle,
                ["item.processing_ms"] = processingTime.TotalMilliseconds,
                ["item.timestamp"] = DateTime.UtcNow.ToString("O"),
                ["item.metadata_included"] = true
            }));
            
            yield return new StreamResponseDto<UserDto>
            {
                Data = user.ToDto(),
                Metadata = new StreamMetadataDto
                {
                    ItemNumber = i + 1,
                    TotalEstimated = request.Count,
                    Timestamp = DateTime.UtcNow,
                    ProcessingTimeMs = processingTime.TotalMilliseconds,
                    IsLast = i == request.Count - 1,
                    BatchId = batchId
                }
            };
        }
        
        var totalTime = DateTime.UtcNow - startTime;
        var loopCycles = (request.Count - 1) / Math.Max(availableUsers.Count, 1) + 1;
        
        activity?.SetTag("stream.total_duration_ms", totalTime.TotalMilliseconds);
        activity?.SetTag("stream.throughput_items_per_sec", request.Count / Math.Max(totalTime.TotalSeconds, 0.001));
        activity?.SetTag("stream.loop_cycles", loopCycles);
        activity?.SetStatus(ActivityStatusCode.Ok, $"Successfully streamed {request.Count} users with metadata ({loopCycles} cycles through {availableUsers.Count} available users)");
    }
}