using Blazing.Mediator.Examples.Streams;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Main runner class that demonstrates all Blazing.Mediator features.
/// This is the equivalent of the MediatR Runner but converted to use Blazing.Mediator.
/// Compare with MediatR version: uses Blazing.Mediator.IMediator instead of MediatR.IMediator.
/// </summary>
public static class Runner
{
    /// <summary>
    /// Runs all the Blazing.Mediator examples.
    /// </summary>
    /// <param name="mediator">The Blazing.Mediator instance.</param>
    /// <param name="writer">The wrapping writer for output.</param>
    /// <param name="projectName">The name of the project being demonstrated.</param>
    /// <param name="testStreams">Whether to test streaming features.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Run(IMediator mediator, WrappingWriter writer, string projectName, bool testStreams = true)
    {
        await writer.WriteLineAsync("===============");
        await writer.WriteLineAsync(projectName);
        await writer.WriteLineAsync("===============");
        await writer.WriteLineAsync();

        await writer.WriteLineAsync("Sending Ping...");
        var pong = await mediator.Send(new Ping { Message = "Ping" });
        await writer.WriteLineAsync("Received: " + pong.Message);
        await writer.WriteLineAsync();

        await writer.WriteLineAsync("Publishing Pinged...");
        await mediator.Publish(new Pinged());
        await writer.WriteLineAsync();

        await writer.WriteLineAsync("Publishing Ponged...");
        var failedPong = false;
        try
        {
            await mediator.Publish(new Ponged());
        }
        catch (Exception e)
        {
            failedPong = true;
            await writer.WriteLineAsync(e.ToString());
        }
        await writer.WriteLineAsync();

        var failedJing = false;
        await writer.WriteLineAsync("Sending Jing...");
        try
        {
            await mediator.Send(new Jing { Message = "Jing" });
        }
        catch (Exception e)
        {
            failedJing = true;
            await writer.WriteLineAsync(e.ToString());
        }
        await writer.WriteLineAsync();

        bool failedSing = false;
        if (testStreams)
        {
            await writer.WriteLineAsync("Sending Sing...");
            try
            {
                int i = 0;
                await foreach (Song s in mediator.SendStream(new Sing { Message = "Sing" }))
                {
                    if (i == 0)
                    {
                        failedSing = !(s.Message.Contains("Singing do"));
                    }
                    else if (i == 1)
                    {
                        failedSing = !(s.Message.Contains("Singing re"));
                    }
                    else if (i == 2)
                    {
                        failedSing = !(s.Message.Contains("Singing mi"));
                    }
                    else if (i == 3)
                    {
                        failedSing = !(s.Message.Contains("Singing fa"));
                    }
                    else if (i == 4)
                    {
                        failedSing = !(s.Message.Contains("Singing so"));
                    }
                    else if (i == 5)
                    {
                        failedSing = !(s.Message.Contains("Singing la"));
                    }
                    else if (i == 6)
                    {
                        failedSing = !(s.Message.Contains("Singing ti"));
                    }
                    else if (i == 7)
                    {
                        failedSing = !(s.Message.Contains("Singing do"));
                    }

                    failedSing = failedSing || (++i) > 10;
                }
            }
            catch (Exception e)
            {
                failedSing = true;
                await writer.WriteLineAsync(e.ToString());
            }
            await writer.WriteLineAsync();
        }

        await writer.WriteLineAsync("---------------");
        var contents = writer.Contents;

        var order = new[] {
            contents.IndexOf("- Starting Up", StringComparison.OrdinalIgnoreCase),
            contents.IndexOf("-- Handling Request", StringComparison.OrdinalIgnoreCase),
            contents.IndexOf("--- Handled Ping", StringComparison.OrdinalIgnoreCase),
            contents.IndexOf("-- Finished Request", StringComparison.OrdinalIgnoreCase),
            contents.IndexOf("- All Done", StringComparison.OrdinalIgnoreCase),
            contents.IndexOf("- All Done with Ping", StringComparison.OrdinalIgnoreCase),
        };

        var streamOrder = testStreams ? [
            contents.IndexOf("-- Handling StreamRequest", StringComparison.OrdinalIgnoreCase),
            contents.IndexOf("--- Handled Sing: Sing, Song", StringComparison.OrdinalIgnoreCase),
            contents.IndexOf("-- Finished StreamRequest", StringComparison.OrdinalIgnoreCase),
        ] : Array.Empty<int>();

        var results = new RunResults
        {
            RequestHandlers = contents.Contains("--- Handled Ping:"),
            VoidRequestsHandlers = contents.Contains("--- Handled Jing:"),
            MiddlewareBehaviors = contents.Contains("-- Handling Request"),
            RequestPreProcessors = contents.Contains("- Starting Up"),
            RequestPostProcessors = contents.Contains("- All Done"),
            ConstrainedGenericBehaviors = contents.Contains("- All Done with Ping") && !failedJing,
            OrderedMiddlewareBehaviors = order.SequenceEqual(order.OrderBy(i => i)),
            NotificationHandler = contents.Contains("Got pinged async"),
            MultipleNotificationHandlers = contents.Contains("Got pinged async") && contents.Contains("Got pinged also async"),
            ConstrainedGenericNotificationHandler = contents.Contains("Got pinged constrained async") && !failedPong,
            CovariantNotificationHandler = contents.Contains("Got notified"),

            // Streams
            StreamRequestHandlers = testStreams && contents.Contains("--- Handled Sing: Sing, Song") && !failedSing,
            StreamMiddlewareBehaviors = testStreams && contents.Contains("-- Handling StreamRequest"),
            StreamOrderedMiddlewareBehaviors = testStreams && streamOrder.SequenceEqual(streamOrder.OrderBy(i => i))
        };

        await writer.WriteLineAsync($"Request Handler....................................................{(results.RequestHandlers ? "Y" : "N")}");
        await writer.WriteLineAsync($"Void Request Handler...............................................{(results.VoidRequestsHandlers ? "Y" : "N")}");
        await writer.WriteLineAsync($"Middleware Behavior................................................{(results.MiddlewareBehaviors ? "Y" : "N")}");
        await writer.WriteLineAsync($"Pre-Processor......................................................{(results.RequestPreProcessors ? "Y" : "N")}");
        await writer.WriteLineAsync($"Post-Processor.....................................................{(results.RequestPostProcessors ? "Y" : "N")}");
        await writer.WriteLineAsync($"Constrained Post-Processor.........................................{(results.ConstrainedGenericBehaviors ? "Y" : "N")}");
        await writer.WriteLineAsync($"Ordered Behaviors..................................................{(results.OrderedMiddlewareBehaviors ? "Y" : "N")}");
        await writer.WriteLineAsync($"Notification Handler...............................................{(results.NotificationHandler ? "Y" : "N")}");
        await writer.WriteLineAsync($"Notification Handlers..............................................{(results.MultipleNotificationHandlers ? "Y" : "N")}");
        await writer.WriteLineAsync($"Constrained Notification Handler...................................{(results.ConstrainedGenericNotificationHandler ? "Y" : "N")}");
        await writer.WriteLineAsync($"Covariant Notification Handler.....................................{(results.CovariantNotificationHandler ? "Y" : "N")}");

        //if (testStreams)
        {
            await writer.WriteLineAsync($"Stream Request Handler.............................................{(results.StreamRequestHandlers ? "Y" : "N")}");
            await writer.WriteLineAsync($"Stream Middleware Behavior.........................................{(results.StreamMiddlewareBehaviors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Stream Ordered Behaviors...........................................{(results.StreamOrderedMiddlewareBehaviors ? "Y" : "N")}");
        }

        await writer.WriteLineAsync();
    }
}
