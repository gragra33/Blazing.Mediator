using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Infrastructure.Telemetry;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for StreamUsersWithMetadataQuery that demonstrates streaming with metadata and OpenTelemetry tracing.
/// Demonstrates OpenTelemetry best practices with static ActivitySource usage.
/// </summary>
public sealed class StreamUsersWithMetadataHandler(ApplicationDbContext context)
    : IStreamRequestHandler<StreamUsersWithMetadataQuery, StreamResponseDto<UserDto>>
{
    private const string ActivityName = "StreamUsersWithMetadataHandler.Handle";

    public async IAsyncEnumerable<StreamResponseDto<UserDto>> Handle(
        StreamUsersWithMetadataQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Use static ActivitySource for optimal performance
        using var activity = ApplicationActivitySources.Handlers.StartActivity(ActivityName);

        // Set comprehensive telemetry tags
        activity?.SetTag("handler.name", nameof(StreamUsersWithMetadataHandler));
        activity?.SetTag("handler.type", "StreamQueryHandler");
        activity?.SetTag("stream.count", request.Count);
        activity?.SetTag("stream.delay_ms", request.DelayMs);
        activity?.SetTag("query.include_inactive", request.IncludeInactive);
        activity?.SetTag("query.search_term", request.SearchTerm ?? "none");
        activity?.SetTag("stream.includes_metadata", true);
        activity?.SetTag("operation.type", "stream_users_with_metadata");
        activity?.SetTag("data.source", "database");

        var startTime = DateTime.UtcNow;
        var batchId = Guid.NewGuid().ToString("N")[..8];

        activity?.SetTag("stream.batch_id", batchId);
        activity?.SetTag("stream.start_time", startTime.ToString("O"));

        // Get all available users from database first
        List<User> availableUsers;
        try
        {
            // Add operation start event
            activity?.AddEvent(new ActivityEvent("handler.execution.started", DateTimeOffset.UtcNow));

            var query = context.Users.AsQueryable();

            if (!request.IncludeInactive)
            {
                query = query.Where(u => u.IsActive);
                activity?.SetTag("query.active_filter_applied", true);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(u => u.Name.Contains(request.SearchTerm)
                                         || u.Email.Contains(request.SearchTerm));
                activity?.SetTag("query.search_filter_applied", true);
                activity?.SetTag("query.search_term_length", request.SearchTerm.Length);
            }

            availableUsers = await query.ToListAsync(cancellationToken);

            activity?.SetTag("stream.available_users", availableUsers.Count);
            activity?.SetTag("stream.total_items", request.Count);
            activity?.SetTag("stream.will_loop", request.Count > availableUsers.Count);

            activity?.AddEvent(new ActivityEvent("stream.data_loaded", DateTimeOffset.UtcNow,
                new ActivityTagsCollection { ["available_users"] = availableUsers.Count }));
        }
        catch (Exception ex)
        {
            // Comprehensive error telemetry
            activity?.SetTag("handler.result", "error");
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("handler.exception", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["exception.type"] = ex.GetType().Name,
                    ["exception.source"] = ex.Source ?? "unknown"
                }));
            throw;
        }

        // Handle empty results
        if (availableUsers.Count == 0)
        {
            activity?.SetTag("handler.result", "no_data");
            activity?.SetStatus(ActivityStatusCode.Ok, "No users found matching criteria");
            activity?.AddEvent(new ActivityEvent("stream.no_data", DateTimeOffset.UtcNow));
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
                await Task.Delay(request.DelayMs, cancellationToken).ConfigureAwait(false);
            }
            var processingTime = DateTime.UtcNow - itemStartTime;

            // Loop through available users if request.Count > availableUsers.Count
            var userIndex = i % availableUsers.Count;
            var user = availableUsers[userIndex];
            var loopCycle = i / availableUsers.Count + 1;

            // Enhanced item-specific activity event
            activity?.AddEvent(new ActivityEvent($"stream.item.{i}", default, new ActivityTagsCollection
            {
                ["item.number"] = i + 1,
                ["item.user_id"] = user.Id,
                ["item.user_name"] = user.Name,
                ["item.user_index"] = userIndex,
                ["item.loop_cycle"] = loopCycle,
                ["item.processing_ms"] = processingTime.TotalMilliseconds,
                ["item.timestamp"] = DateTime.UtcNow.ToString("O"),
                ["item.metadata_included"] = true,
                ["item.is_active"] = user.IsActive,
                ["item.is_last"] = i == request.Count - 1
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

        // Success telemetry with comprehensive metrics
        activity?.SetTag("handler.result", "success");
        activity?.SetTag("stream.total_duration_ms", totalTime.TotalMilliseconds);
        activity?.SetTag("stream.throughput_items_per_sec", request.Count / Math.Max(totalTime.TotalSeconds, 0.001));
        activity?.SetTag("stream.loop_cycles", loopCycles);
        activity?.SetTag("stream.items_streamed", request.Count);
        activity?.SetTag("stream.metadata_enriched", true);
        activity?.SetStatus(ActivityStatusCode.Ok, $"Successfully streamed {request.Count} users with metadata ({loopCycles} cycles through {availableUsers.Count} available users)");

        activity?.AddEvent(new ActivityEvent("handler.execution.completed", DateTimeOffset.UtcNow,
            new ActivityTagsCollection
            {
                ["items_streamed"] = request.Count,
                ["loop_cycles"] = loopCycles,
                ["duration_ms"] = totalTime.TotalMilliseconds,
                ["metadata_included"] = true
            }));
    }
}
