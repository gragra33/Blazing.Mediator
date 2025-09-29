using Blazing.Mediator;
using Streaming.Api.Requests;
using Streaming.Api.Shared.DTOs;
using System.Text.Json;

namespace Streaming.Api.Endpoints;

/// <summary>
/// API endpoints for streaming contacts
/// Following REST conventions and supporting Server-Sent Events (SSE)
/// </summary>
public static class ContactEndpoints
{
    /// <summary>
    /// Configure contact-related endpoints
    /// </summary>
    public static void MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contacts")
            .WithTags("Contacts")
            .WithOpenApi();

        // Get contact count
        group.MapGet("/count", async (IMediator mediator) =>
        {
            var count = await mediator.Send(new GetContactCountRequest()).ConfigureAwait(false);
            return Results.Ok(new { count });
        })
        .WithName("GetContactCount")
        .WithSummary("Get total number of contacts")
        .Produces<object>();

        // Get all contacts at once (for non-streaming bulk load)
        group.MapGet("/all", async (IMediator mediator, string? search, CancellationToken cancellationToken) =>
        {
            var request = new GetAllContactsRequest { SearchTerm = search };
            var contacts = await mediator.Send(request, cancellationToken).ConfigureAwait(false);

            return Results.Json(contacts, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        })
        .WithName("GetAllContacts")
        .WithSummary("Get all contacts at once (bulk load)")
        .Produces<ContactDto[]>();

        // Stream all contacts as JSON array
        group.MapGet("/stream", (IMediator mediator, string? search, CancellationToken cancellationToken) =>
        {
            var request = new StreamContactsRequest { SearchTerm = search };

            return Results.Stream(async stream =>
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                await JsonSerializer.SerializeAsync(stream, mediator.SendStream(request, cancellationToken), options, cancellationToken).ConfigureAwait(false);
            }, contentType: "application/json");
        })
        .WithName("StreamContacts")
        .WithSummary("Stream contacts as JSON");

        // Stream contacts with Server-Sent Events (SSE) - True Streaming with immediate flush
        group.MapGet("/stream/sse", (IMediator mediator, string? search, CancellationToken cancellationToken) =>
        {
            var request = new StreamContactsRequest { SearchTerm = search };

            return Results.Stream(async stream =>
            {
                var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = false };
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                try
                {
                    // Send start event
                    await writer.WriteLineAsync("event: start").ConfigureAwait(false);
                    await writer.WriteLineAsync("data: {\"message\": \"Starting stream\"}").ConfigureAwait(false);
                    await writer.WriteLineAsync().ConfigureAwait(false);
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);  // Force immediate flush to client

                    var count = 0;
                    await foreach (var contact in mediator.SendStream(request, cancellationToken))
                    {
                        count++;

                        // Send contact data with immediate flush for true streaming
                        await writer.WriteLineAsync("event: data").ConfigureAwait(false);
                        await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(contact, options)}").ConfigureAwait(false);
                        await writer.WriteLineAsync().ConfigureAwait(false);
                        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);  // Critical: Force immediate flush

                        // Send progress every 50 items
                        if (count % 50 == 0)
                        {
                            await writer.WriteLineAsync("event: progress").ConfigureAwait(false);
                            await writer.WriteLineAsync($"data: {{\"processed\": {count}}}").ConfigureAwait(false);
                            await writer.WriteLineAsync().ConfigureAwait(false);
                            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);  // Immediate flush for progress
                        }
                    }

                    // Send completion event
                    await writer.WriteLineAsync("event: complete").ConfigureAwait(false);
                    await writer.WriteLineAsync($"data: {{\"message\": \"Stream completed\", \"total\": {count}}}").ConfigureAwait(false);
                    await writer.WriteLineAsync().ConfigureAwait(false);
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    await writer.DisposeAsync().ConfigureAwait(false);
                }
            }, contentType: "text/event-stream");
        })
        .WithName("StreamContactsSSE")
        .WithSummary("Stream contacts using Server-Sent Events");

        // Stream contacts with metadata
        group.MapGet("/stream/metadata", (IMediator mediator, string? search, CancellationToken cancellationToken) =>
        {
            var request = new StreamContactsWithMetadataRequest { SearchTerm = search };

            return Results.Stream(async stream =>
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                await JsonSerializer.SerializeAsync(stream, mediator.SendStream(request, cancellationToken), options, cancellationToken).ConfigureAwait(false);
            }, contentType: "application/json");
        })
        .WithName("StreamContactsWithMetadata")
        .WithSummary("Stream contacts with metadata and statistics");

        // Stream contacts with metadata using SSE - True Streaming with immediate flush
        group.MapGet("/stream/metadata/sse", (IMediator mediator, string? search, CancellationToken cancellationToken) =>
        {
            var request = new StreamContactsWithMetadataRequest { SearchTerm = search };

            return Results.Stream(async stream =>
            {
                var writer = new StreamWriter(stream, leaveOpen: true) { AutoFlush = false };
                try
                {
                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                    await foreach (var response in mediator.SendStream(request, cancellationToken))
                    {
                        var eventType = response.IsComplete ? "complete" : "data";

                        // Each response is immediately flushed for true streaming
                        await writer.WriteLineAsync($"event: {eventType}").ConfigureAwait(false);
                        await writer.WriteLineAsync($"data: {JsonSerializer.Serialize(response, options)}").ConfigureAwait(false);
                        await writer.WriteLineAsync().ConfigureAwait(false);
                        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);  // Critical: Force immediate flush
                    }
                }
                finally
                {
                    await writer.DisposeAsync().ConfigureAwait(false);
                }
            }, contentType: "text/event-stream");
        })
        .WithName("StreamContactsWithMetadataSSE")
        .WithSummary("Stream contacts with metadata using Server-Sent Events");

        // TRUE STREAMING: Direct HTTP Response approach - bypasses all buffering
        group.MapGet("/stream/direct", async (HttpContext context, IMediator mediator, string? search, CancellationToken cancellationToken) =>
        {
            var request = new StreamContactsRequest { SearchTerm = search };

            // Set headers for SSE - disable all buffering
            context.Response.Headers.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers.Connection = "keep-alive";
            context.Response.Headers.AccessControlAllowOrigin = "*";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            try
            {
                // Send start event immediately
                await context.Response.WriteAsync("event: start\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                await context.Response.WriteAsync("data: {\"message\": \"Starting stream\"}\n\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                await context.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);

                var count = 0;
                await foreach (var contact in mediator.SendStream(request, cancellationToken))
                {
                    count++;

                    // Write directly to HTTP response - completely bypasses buffering
                    await context.Response.WriteAsync("event: data\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                    await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(contact, options)}\n\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                    await context.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);

                    // Send progress every 50 items
                    if (count % 50 == 0)
                    {
                        await context.Response.WriteAsync("event: progress\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                        await context.Response.WriteAsync($"data: {{\"processed\": {count}}}\n\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                        await context.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                }

                // Send completion event
                await context.Response.WriteAsync("event: complete\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                await context.Response.WriteAsync($"data: {{\"message\": \"Stream completed\", \"total\": {count}}}\n\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                await context.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await context.Response.WriteAsync("event: error\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                await context.Response.WriteAsync($"data: {{\"error\": \"{ex.Message}\"}}\n\n", cancellationToken: cancellationToken).ConfigureAwait(false);
                await context.Response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        })
        .WithName("StreamContactsDirect")
        .WithSummary("TRUE STREAMING: Direct HTTP response streaming - bypasses all buffering");
    }
}
