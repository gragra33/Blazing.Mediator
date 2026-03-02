using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazing.Mediator;

public sealed partial class Mediator
{
    #region Notification Methods

    /// <summary>
    /// Publishes a notification to all subscribers and handlers following the observer pattern.
    /// Publishers blindly send notifications without caring about recipients.
    /// Supports both manual subscribers (INotificationSubscriber) and automatic handlers (INotificationHandler).
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to publish</typeparam>
    /// <param name="notification">The notification to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ValueTask representing the operation</returns>
    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Fast path: source-gen dispatcher has a pre-baked handler chain for this notification type.
        // • No async state machine — returns the ValueTask directly from the pre-resolved wrapper.
        // • No try/catch — IsNotificationHandled<T>() is a compile-time type check; the JIT can
        //   constant-fold it to 'true' or 'false' when TNotification is statically known, so the
        //   branch becomes a predicted-not-taken no-op on the fast path.
        if (GetDispatcher() is { } d && d.IsNotificationHandled<TNotification>())
            return d.PublishAsync(notification, cancellationToken);

        // Slow path: reflection-based dispatch + manual subscriber support.
        // Reached when (a) source-gen dispatcher is not registered, or (b) the notification type
        // was not in the compile-time model (e.g. only manual INotificationSubscriber<T> registrations).
        _logger?.PublishOperationStarted(typeof(TNotification).Name, IsTelemetryEnabled);
        return new ValueTask(PublishReflection(notification, cancellationToken));
    }

    /// <summary>
    /// Reflection-based publish implementation (preserved for fallback and comparison).
    /// </summary>
    private async Task PublishReflection<TNotification>(TNotification notification, CancellationToken cancellationToken) where TNotification : INotification
    {
        var notificationTypeName = typeof(TNotification).Name;
        var sanitizedNotificationName = SanitizeTypeName(notificationTypeName);

        using var activity = IsTelemetryEnabled ? ActivitySource.StartActivity($"{_mediatorPublishActivityPrefix}{sanitizedNotificationName}") : null;
        // Only set status if activity was actually created
        if (activity != null && IsTelemetryEnabled)
        {
            activity.SetStatus(ActivityStatusCode.Ok);
            activity.SetTag("notification.type", sanitizedNotificationName);
            activity.SetTag("operation", "publish");
            activity.SetTag("mediator_operation", "notification_publish");
        }

        var stopwatch = Stopwatch.StartNew();
        List<string> executedMiddleware = [];
        List<string> allMiddleware = [];
        Exception? exception = null;
        int subscriberCount = 0;
        int handlerCount = 0;
        var subscriberResults = new List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)>();
        long startMemory = 0;

        // Record initial memory if performance counters are enabled
        if (_statistics != null && HasPerformanceCountersEnabled())
        {
            startMemory = GC.GetTotalMemory(false);
        }

        try
        {
            // Track notification statistics
            _statistics?.IncrementNotification(notificationTypeName);

            // Get all registered notification middleware (types and order)
            var pipelineInspector = _notificationPipelineBuilder as INotificationMiddlewarePipelineInspector;
            var middlewareInfo = pipelineInspector?.GetDetailedMiddlewareInfo(_serviceProvider);
            if (middlewareInfo != null && IsTelemetryEnabled && ShouldCaptureNotificationMiddlewareDetails)
            {
                allMiddleware = middlewareInfo.OrderBy(m => m.Order).Select(m1 => SanitizeMiddlewareName(m1.Type)).ToList();

                activity?.SetTag("notification_middleware.pipeline", string.Join(",", allMiddleware));
            }

            // Execute through notification middleware
            async ValueTask SubscriberAndHandlerProcessor(TNotification n, CancellationToken ct)
            {
                // PATTERN 1: Manual Subscribers (existing pattern) ===
                var subscribers = new List<INotificationSubscriber<TNotification>>();
                if (_specificSubscribers.TryGetValue(typeof(TNotification), out var specific))
                {
                    subscribers.AddRange(specific.OfType<INotificationSubscriber<TNotification>>());
                }

                // Add generic subscribers that can handle any notification
                var genericSubscriberList = _genericSubscribers.ToList();

                // PATTERN 2: Automatic Handlers (new pattern) ===
                // Discover handlers from DI container (including covariant handlers)
                var handlers = GetCovariantNotificationHandlers<TNotification>(n).ToList();

                subscriberCount = subscribers.Count + genericSubscriberList.Count;
                handlerCount = handlers.Count;
                
                var totalProcessors = subscriberCount + handlerCount;
                _logger?.PublishSubscriberResolution(totalProcessors, notificationTypeName);

                if (IsTelemetryEnabled && ShouldCaptureNotificationHandlerDetails)
                {
                    activity?.SetTag("notification.handler_count", handlerCount);
                    activity?.SetTag("notification.subscriber_count", subscriberCount);
                    activity?.SetTag("notification.execution_pattern", DetermineExecutionPattern());
                }
                
                var processingExceptions = new List<Exception>();

                // Process manual subscribers
                foreach (var subscriber in subscribers)
                {
                    await ProcessSubscriberWithTelemetry(subscriber, n, activity, ct, subscriberResults, processingExceptions, notificationTypeName, sanitizedNotificationName);
                }

                // Process generic subscribers
                foreach (var genericSubscriber in genericSubscriberList)
                {
                    await ProcessGenericSubscriberWithTelemetry(genericSubscriber, n, activity, ct, subscriberResults, processingExceptions, notificationTypeName, sanitizedNotificationName);
                }

                foreach (var handler in handlers)
                {
                    await ProcessHandlerWithTelemetry(handler, n, activity, ct, subscriberResults, processingExceptions, notificationTypeName, sanitizedNotificationName);
                }

                // After all processors have been called, throw the first exception if any occurred
                if (processingExceptions.Count > 0)
                {
                    throw processingExceptions[0];
                }
            }

            // Execute through middleware pipeline (or directly if no pipeline builder is registered)
            if (_notificationPipelineBuilder is not null)
                await _notificationPipelineBuilder.ExecutePipeline(notification, _serviceProvider, SubscriberAndHandlerProcessor, cancellationToken).ConfigureAwait(false);
            else
                await SubscriberAndHandlerProcessor(notification, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
            if (!IsTelemetryEnabled)
            {
                throw;
            }

            activity?.SetStatus(ActivityStatusCode.Error);

            var sanitizedExceptionType = SanitizeTypeName(ex.GetType().Name);

            var failureTags = new TagList
            {
                { "notification_name", sanitizedNotificationName },
                { "exception.type", sanitizedExceptionType },
                { "exception.message", SanitizeExceptionMessage(ex.Message) }
            };
            MediatorMetrics.PublishFailureCounter.Add(1, failureTags);

            activity?.SetTag("exception.type", sanitizedExceptionType);
            activity?.SetTag("exception.message", SanitizeExceptionMessage(ex.Message));
            activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(ex.Message));
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Debug logging: Publish operation completed
            _logger?.PublishOperationCompleted(notificationTypeName, stopwatch.Elapsed.TotalMilliseconds, exception == null, subscriberCount + handlerCount);

            // Enhanced statistics recording for notifications using existing infrastructure
            if (_statistics != null)
            {
                // Record execution time if performance counters are enabled (using existing infrastructure)
                if (HasPerformanceCountersEnabled())
                {
                    // Use existing RecordExecutionTime method with "Notification:" prefix
                    _statistics.RecordExecutionTime($"Notification:{notificationTypeName}", stopwatch.ElapsedMilliseconds, exception == null);

                    // Record memory allocation if available (using existing infrastructure)
                    if (startMemory > 0)
                    {
                        var endMemory = GC.GetTotalMemory(false);
                        var memoryDelta = endMemory - startMemory;
                        if (memoryDelta > 0)
                        {
                            _statistics.RecordMemoryAllocation(memoryDelta);
                        }
                    }
                }

                // Record detailed analysis if enabled (using existing infrastructure)
                if (HasDetailedAnalysisEnabled())
                {
                    _statistics.RecordExecutionPattern($"Notification:{notificationTypeName}", DateTime.UtcNow);
                }

                // Record middleware execution metrics if enabled (using existing infrastructure)
                if (HasMiddlewareMetricsEnabled())
                {
                    foreach (var middlewareName in executedMiddleware)
                    {
                        _statistics.RecordMiddlewareExecution(middlewareName, 0, true);
                    }
                }
            }

            if (IsTelemetryEnabled)
            {
                var totalProcessors = subscriberCount + handlerCount;
                var tags = new TagList
                {
                    { "notification_name", sanitizedNotificationName },
                    { "subscriber_count", subscriberCount },
                    { "handler_count", handlerCount },
                    { "total_processors", totalProcessors }
                };

                if (executedMiddleware.Count > 0)
                {
                    tags.Add("notification_middleware.executed", string.Join(",", executedMiddleware));
                }

                MediatorMetrics.PublishDurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

                // Analyze handler/subscriber results for partial/total failure metrics
                var successCount = subscriberResults.Count(r => r.Success);
                var failureCount = subscriberResults.Count(r => !r.Success);
                
                if (exception == null)
                {
                    MediatorMetrics.PublishSuccessCounter.Add(1, tags);
                    // Ensure successful completion is marked with Ok status
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    // Partial vs total failure classification
                    if (successCount > 0 && failureCount > 0)
                    {
                        // Mixed results: some succeeded, some failed = partial failure
                        MediatorMetrics.PublishPartialFailureCounter.Add(1, tags);
                    }
                    else if (failureCount > 0 && successCount == 0)
                    {
                        // All failed = total failure
                        MediatorMetrics.PublishTotalFailureCounter.Add(1, tags);
                    }
                    // Note: If successCount > 0 && failureCount == 0, then exception shouldn't have occurred
                    // This would be an edge case where collection succeeded but middleware/pipeline threw
                }

                // Add activity tags
                activity?.SetTag("notification_name", sanitizedNotificationName);
                activity?.SetTag("notification_middleware.executed", string.Join(",", executedMiddleware));
                activity?.SetTag("notification_middleware.pipeline", string.Join(",", allMiddleware));
                activity?.SetTag("duration_ms", stopwatch.Elapsed.TotalMilliseconds);
                activity?.SetTag("subscriber_count", subscriberCount);
                activity?.SetTag("handler_count", handlerCount);
                activity?.SetTag("total_processors", totalProcessors);

                // Add per-processor results as activity events
                foreach (var result in subscriberResults)
                {
                    activity?.AddEvent(new ActivityEvent($"subscriber:{result.SubscriberType}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                    {
                        ["subscriber_type"] = result.SubscriberType,
                        ["success"] = result.Success,
                        ["duration_ms"] = result.DurationMs,
                        ["exception_type"] = result.ExceptionType,
                        ["exception_message"] = result.ExceptionMessage
                    }));
                }

                // Increment health check counter
                MediatorMetrics.TelemetryHealthCounter.Add(1, new TagList { { "operation", "publish" } });
            }
        }
    }

    /// <summary>
    /// Processes a subscriber with telemetry tracking.
    /// </summary>
    private async Task ProcessSubscriberWithTelemetry<TNotification>(
        INotificationSubscriber<TNotification> subscriber,
        TNotification notification,
        Activity? parentActivity,
        CancellationToken cancellationToken,
        List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)> subscriberResults,
        List<Exception> processingExceptions,
        string notificationTypeName,
        string sanitizedNotificationName) where TNotification : INotification
    {
        var subType = SanitizeTypeName(subscriber.GetType().Name);
        _logger?.PublishSubscriberProcessing($"{subType}(Subscriber)", notificationTypeName);
        var subStopwatch = Stopwatch.StartNew();

        try
        {
            await subscriber.OnNotification(notification, cancellationToken).ConfigureAwait(false);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var successTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1, successTags);
            }

            subscriberResults.Add(($"{subType}(Subscriber)", true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
            _logger?.PublishSubscriberCompleted($"{subType}(Subscriber)", notificationTypeName, subStopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            processingExceptions.Add(ex);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var sanitizedException = SanitizeTypeName(ex.GetType().Name);
                var failureTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "subscriber" }, { "processor_name", subType }, { "exception.type", sanitizedException }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1, failureTags);
            }

            subscriberResults.Add(($"{subType}(Subscriber)", false, subStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
            _logger?.PublishSubscriberCompleted($"{subType}(Subscriber)", notificationTypeName, subStopwatch.Elapsed.TotalMilliseconds, false);
        }
        finally
        {
            subStopwatch.Stop();
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var durationTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(subStopwatch.Elapsed.TotalMilliseconds, durationTags);
            }
        }
    }

    /// <summary>
    /// Processes a generic subscriber with telemetry tracking.
    /// </summary>
    private async Task ProcessGenericSubscriberWithTelemetry<TNotification>(
        INotificationSubscriber genericSubscriber,
        TNotification notification,
        Activity? parentActivity,
        CancellationToken cancellationToken,
        List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)> subscriberResults,
        List<Exception> processingExceptions,
        string notificationTypeName,
        string sanitizedNotificationName) where TNotification : INotification
    {
        var subType = SanitizeTypeName(genericSubscriber.GetType().Name);
        _logger?.PublishSubscriberProcessing($"{subType}(Generic)", notificationTypeName);
        var subStopwatch = Stopwatch.StartNew();

        try
        {
            await genericSubscriber.OnNotification(notification, cancellationToken).ConfigureAwait(false);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var successTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "generic_subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1, successTags);
            }

            subscriberResults.Add(($"{subType}(Generic)", true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
            _logger?.PublishSubscriberCompleted($"{subType}(Generic)", notificationTypeName, subStopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            processingExceptions.Add(ex);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var sanitizedException = SanitizeTypeName(ex.GetType().Name);
                var failureTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "generic_subscriber" }, { "processor_name", subType }, { "exception.type", sanitizedException }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1, failureTags);
            }

            subscriberResults.Add(($"{subType}(Generic)", false, subStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
            _logger?.PublishSubscriberCompleted($"{subType}(Generic)", notificationTypeName, subStopwatch.Elapsed.TotalMilliseconds, false);
        }
        finally
        {
            subStopwatch.Stop();
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var durationTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "generic_subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(subStopwatch.Elapsed.TotalMilliseconds, durationTags);
            }
        }
    }

    /// <summary>
    /// Processes a notification handler with individual child span telemetry tracking.
    /// </summary>
    private async Task ProcessHandlerWithTelemetry<TNotification>(
        object handler,
        TNotification notification,
        Activity? parentActivity,
        CancellationToken cancellationToken,
        List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)> subscriberResults,
        List<Exception> processingExceptions,
        string notificationTypeName,
        string sanitizedNotificationName) where TNotification : INotification
    {
        var handlerTypeName = SanitizeTypeName(handler.GetType().Name);
        _logger?.PublishSubscriberProcessing($"{handlerTypeName}(Handler)", notificationTypeName);
        
        // Create child span for individual handler
        using var handlerActivity = IsTelemetryEnabled && ShouldCreateHandlerChildSpans 
            ? ActivitySource.StartActivity($"{_mediatorPublishActivityPrefix}Handler.{handlerTypeName}", ActivityKind.Internal, parentActivity?.Context ?? default) 
            : null;

        if (handlerActivity != null && IsTelemetryEnabled && ShouldCaptureNotificationHandlerDetails)
        {
            handlerActivity.SetTag("handler.type", handlerTypeName);
            handlerActivity.SetTag("notification.type", sanitizedNotificationName);
            handlerActivity.SetTag("operation", "handle_notification");
        }

        var handlerStopwatch = Stopwatch.StartNew();

        try
        {
            var typedHandler = (INotificationHandler<TNotification>)handler;
            await typedHandler.Handle(notification, cancellationToken).ConfigureAwait(false);
            
            handlerStopwatch.Stop();
            
            // Record metrics
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(handlerStopwatch.ElapsedMilliseconds);
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1);
                
                var successTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "handler" }, { "processor_name", handlerTypeName } };
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1, successTags);
            }
            
            if (handlerActivity != null && IsTelemetryEnabled)
            {
                handlerActivity.SetStatus(ActivityStatusCode.Ok);
                handlerActivity.SetTag("duration_ms", handlerStopwatch.ElapsedMilliseconds);
                handlerActivity.SetTag("success", true);
            }

            subscriberResults.Add(($"{handlerTypeName}(Handler)", true, handlerStopwatch.Elapsed.TotalMilliseconds, null, null));
            _logger?.PublishSubscriberCompleted($"{handlerTypeName}(Handler)", notificationTypeName, handlerStopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            handlerStopwatch.Stop();
            processingExceptions.Add(ex);
            
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1);
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(handlerStopwatch.ElapsedMilliseconds);
                
                var sanitizedException = SanitizeTypeName(ex.GetType().Name);
                var failureTags = new TagList { { "notification_name", sanitizedNotificationName }, { "processor_type", "handler" }, { "processor_name", handlerTypeName }, { "exception.type", sanitizedException }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1, failureTags);
            }
            
            if (handlerActivity != null && IsTelemetryEnabled && ShouldCaptureExceptionDetails)
            {
                var sanitizedException = SanitizeTypeName(ex.GetType().Name);
                handlerActivity.SetStatus(ActivityStatusCode.Error, ex.Message);
                handlerActivity.SetTag("exception.type", sanitizedException);
                handlerActivity.SetTag("exception.message", SanitizeExceptionMessage(ex.Message));
                handlerActivity.SetTag("success", false);
            }

            subscriberResults.Add(($"{handlerTypeName}(Handler)", false, handlerStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
            _logger?.PublishSubscriberCompleted($"{handlerTypeName}(Handler)", notificationTypeName, handlerStopwatch.Elapsed.TotalMilliseconds, false);
        }
    }

    /// <summary>
    /// Processes a subscriber with telemetry tracking.
    /// </summary>
    private async Task ProcessSubscriberWithTelemetry<TNotification>(
        INotificationSubscriber<TNotification> subscriber,
        TNotification notification,
        Activity? parentActivity,
        CancellationToken cancellationToken,
        List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)> subscriberResults,
        List<Exception> processingExceptions) where TNotification : INotification
    {
        var subType = SanitizeTypeName(subscriber.GetType().Name);
        _logger?.PublishSubscriberProcessing($"{subType}(Subscriber)", typeof(TNotification).Name);
        var subStopwatch = Stopwatch.StartNew();

        try
        {
            await subscriber.OnNotification(notification, cancellationToken).ConfigureAwait(false);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var successTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1, successTags);
            }

            subscriberResults.Add(($"{subType}(Subscriber)", true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
            _logger?.PublishSubscriberCompleted($"{subType}(Subscriber)", typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            processingExceptions.Add(ex);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var failureTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "subscriber" }, { "processor_name", subType }, { "exception.type", SanitizeTypeName(ex.GetType().Name) }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1, failureTags);
            }

            subscriberResults.Add(($"{subType}(Subscriber)", false, subStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
            _logger?.PublishSubscriberCompleted($"{subType}(Subscriber)", typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, false);
        }
        finally
        {
            subStopwatch.Stop();
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var durationTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(subStopwatch.Elapsed.TotalMilliseconds, durationTags);
            }
        }
    }

    /// <summary>
    /// Processes a generic subscriber with telemetry tracking.
    /// </summary>
    private async Task ProcessGenericSubscriberWithTelemetry<TNotification>(
        INotificationSubscriber genericSubscriber,
        TNotification notification,
        Activity? parentActivity,
        CancellationToken cancellationToken,
        List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)> subscriberResults,
        List<Exception> processingExceptions) where TNotification : INotification
    {
        var subType = SanitizeTypeName(genericSubscriber.GetType().Name);
        _logger?.PublishSubscriberProcessing($"{subType}(Generic)", typeof(TNotification).Name);
        var subStopwatch = Stopwatch.StartNew();

        try
        {
            await genericSubscriber.OnNotification(notification, cancellationToken).ConfigureAwait(false);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var successTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "generic_subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1, successTags);
            }

            subscriberResults.Add(($"{subType}(Generic)", true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
            _logger?.PublishSubscriberCompleted($"{subType}(Generic)", typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            processingExceptions.Add(ex);
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var failureTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "generic_subscriber" }, { "processor_name", subType }, { "exception.type", SanitizeTypeName(ex.GetType().Name) }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1, failureTags);
            }

            subscriberResults.Add(($"{subType}(Generic)", false, subStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
            _logger?.PublishSubscriberCompleted($"{subType}(Generic)", typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, false);
        }
        finally
        {
            subStopwatch.Stop();
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                var durationTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "generic_subscriber" }, { "processor_name", subType } };
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(subStopwatch.Elapsed.TotalMilliseconds, durationTags);
            }
        }
    }

    /// <summary>
    /// Processes a notification handler with individual child span telemetry tracking.
    /// </summary>
    private async Task ProcessHandlerWithTelemetry<TNotification>(
        object handler,
        TNotification notification,
        Activity? parentActivity,
        CancellationToken cancellationToken,
        List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)> subscriberResults,
        List<Exception> processingExceptions) where TNotification : INotification
    {
        var handlerTypeName = SanitizeTypeName(handler.GetType().Name);
        _logger?.PublishSubscriberProcessing($"{handlerTypeName}(Handler)", typeof(TNotification).Name);
        
        // Create child span for individual handler
        using var handlerActivity = IsTelemetryEnabled && ShouldCreateHandlerChildSpans 
            ? ActivitySource.StartActivity($"{_mediatorPublishActivityPrefix}Handler.{handlerTypeName}", ActivityKind.Internal, parentActivity?.Context ?? default) 
            : null;

        if (handlerActivity != null && IsTelemetryEnabled && ShouldCaptureNotificationHandlerDetails)
        {
            handlerActivity.SetTag("handler.type", handlerTypeName);
            handlerActivity.SetTag("notification.type", SanitizeTypeName(typeof(TNotification).Name));
            handlerActivity.SetTag("operation", "handle_notification");
        }

        var handlerStopwatch = Stopwatch.StartNew();

        try
        {
            var typedHandler = (INotificationHandler<TNotification>)handler;
            await typedHandler.Handle(notification, cancellationToken).ConfigureAwait(false);
            
            handlerStopwatch.Stop();
            
            // Record metrics
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(handlerStopwatch.ElapsedMilliseconds);
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1);
                
                var successTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "handler" }, { "processor_name", handlerTypeName } };
                MediatorMetrics.PublishSubscriberSuccessCounter.Add(1, successTags);
            }
            
            if (handlerActivity != null && IsTelemetryEnabled)
            {
                handlerActivity.SetStatus(ActivityStatusCode.Ok);
                handlerActivity.SetTag("duration_ms", handlerStopwatch.ElapsedMilliseconds);
                handlerActivity.SetTag("success", true);
            }

            subscriberResults.Add(($"{handlerTypeName}(Handler)", true, handlerStopwatch.Elapsed.TotalMilliseconds, null, null));
            _logger?.PublishSubscriberCompleted($"{handlerTypeName}(Handler)", typeof(TNotification).Name, handlerStopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            handlerStopwatch.Stop();
            processingExceptions.Add(ex);
            
            if (IsTelemetryEnabled && ShouldCaptureSubscriberMetrics)
            {
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1);
                MediatorMetrics.PublishSubscriberDurationHistogram.Record(handlerStopwatch.ElapsedMilliseconds);
                
                var failureTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "processor_type", "handler" }, { "processor_name", handlerTypeName }, { "exception.type", SanitizeTypeName(ex.GetType().Name) }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                MediatorMetrics.PublishSubscriberFailureCounter.Add(1, failureTags);
            }
            
            if (handlerActivity != null && IsTelemetryEnabled && ShouldCaptureExceptionDetails)
            {
                var sanitizedException = SanitizeTypeName(ex.GetType().Name);
                handlerActivity.SetStatus(ActivityStatusCode.Error, ex.Message);
                handlerActivity.SetTag("exception.type", sanitizedException);
                handlerActivity.SetTag("exception.message", SanitizeExceptionMessage(ex.Message));
                handlerActivity.SetTag("success", false);
            }

            subscriberResults.Add(($"{handlerTypeName}(Handler)", false, handlerStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
            _logger?.PublishSubscriberCompleted($"{handlerTypeName}(Handler)", typeof(TNotification).Name, handlerStopwatch.Elapsed.TotalMilliseconds, false);
        }
    }

    // ** Helper methods for telemetry configuration **

    /// <summary>
    /// Gets whether notification handler details should be captured based on options (default true).
    /// </summary>
    private bool ShouldCaptureNotificationHandlerDetails => _telemetryOptions?.CaptureNotificationHandlerDetails ?? true;

    /// <summary>
    /// Gets whether to create child spans for individual handlers based on options (default true).
    /// </summary>
    private bool ShouldCreateHandlerChildSpans => _telemetryOptions?.CreateHandlerChildSpans ?? true;

    /// <summary>
    /// Gets whether to capture subscriber metrics based on options (default true).
    /// </summary>
    private bool ShouldCaptureSubscriberMetrics => _telemetryOptions?.CaptureSubscriberMetrics ?? true;

    /// <summary>
    /// Gets whether notification middleware details should be captured based on options (default true).
    /// </summary>
    private bool ShouldCaptureNotificationMiddlewareDetails => _telemetryOptions?.CaptureNotificationMiddlewareDetails ?? true;

    /// <summary>
    /// Determines the execution pattern for the notification processing.
    /// </summary>
    private static string DetermineExecutionPattern()
    {
        return "standard"; // Simple implementation for now
    }

    #endregion

    #region Covariant Notification Handler Support

    /// <summary>
    /// Gets covariant notification handlers for the specified notification type.
    /// Finds handlers that can handle the notification type or any of its base types/interfaces,
    /// enabling covariant notification handling where handlers can process base types and derived types.
    /// </summary>
    /// <typeparam name="TNotification">The notification type to find handlers for</typeparam>
    /// <param name="notification">The notification instance</param>
    /// <returns>An enumerable of handler instances that can handle the notification</returns>
    /// <remarks>
    /// This method supports covariant notification handling by:
    /// 1. Finding handlers for the exact notification type
    /// 2. Finding handlers for base classes that the notification inherits from
    /// 3. Finding handlers for interfaces that the notification implements
    /// 4. Ensuring each handler is only returned once (deduplication)
    /// 
    /// Example:
    /// - If notification is OrderCreatedNotification : DomainEvent : INotification
    /// - It will find handlers for: OrderCreatedNotification, DomainEvent, INotification
    /// </remarks>
    private IEnumerable<object> GetCovariantNotificationHandlers<TNotification>(TNotification notification) 
        where TNotification : INotification
    {
        var notificationType = notification.GetType();
        var foundHandlers = new HashSet<object>(ReferenceEqualityComparer.Instance);

        // Get all types in the inheritance hierarchy and interfaces
        var candidateTypes = GetNotificationTypeHierarchy(notificationType);

        foreach (var candidateType in candidateTypes)
        {
            // Create the handler interface type for this candidate type
            var handlerInterfaceType = typeof(INotificationHandler<>).MakeGenericType(candidateType);
            
            // Get all handlers from the service provider
            var handlers = _serviceProvider.GetServices(handlerInterfaceType);
            
            foreach (var handler in handlers)
            {
                if (handler != null && foundHandlers.Add(handler))
                {
                    yield return handler;
                }
            }
        }
    }

    /// <summary>
    /// Gets all types in the notification type hierarchy that could have handlers.
    /// This includes the notification type itself, all base classes, and all implemented interfaces.
    /// </summary>
    /// <param name="notificationType">The notification type to analyze</param>
    /// <returns>An enumerable of types that could have notification handlers</returns>
    private static IEnumerable<Type> GetNotificationTypeHierarchy(Type notificationType)
    {
        var processedTypes = new HashSet<Type>();
        var typesToProcess = new Queue<Type>();
        
        typesToProcess.Enqueue(notificationType);

        while (typesToProcess.Count > 0)
        {
            var currentType = typesToProcess.Dequeue();
            
            if (!processedTypes.Add(currentType))
            {
                continue; // Already processed this type
            }

            // Only yield types that implement INotification
            if (typeof(INotification).IsAssignableFrom(currentType))
            {
                yield return currentType;
            }

            // Add base type if it exists and is not object
            if (currentType.BaseType != null && currentType.BaseType != typeof(object))
            {
                typesToProcess.Enqueue(currentType.BaseType);
            }

            // Add all implemented interfaces
            foreach (var interfaceType in currentType.GetInterfaces())
            {
                typesToProcess.Enqueue(interfaceType);
            }
        }
    }

    #endregion
}