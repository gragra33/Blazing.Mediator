using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazing.Mediator;

public sealed partial class Mediator
{
    /// <summary>
    /// Sends a command request through the middleware pipeline to its corresponding handler.
    /// </summary>
    /// <param name="request">The command request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A ValueTask representing the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public ValueTask Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (GetDispatcher() is { } d)
            return d.SendAsync(request, cancellationToken);
        return new ValueTask(SendReflection(request, cancellationToken));
    }

    /// <summary>
    /// Sends a query request through the middleware pipeline to its corresponding handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from the handler</typeparam>
    /// <param name="request">The query request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A ValueTask containing the response from the handler</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type or the handler returns null</exception>
    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (GetDispatcher() is { } d)
            return d.SendAsync(request, cancellationToken);
        return new ValueTask<TResponse>(SendReflection<TResponse>(request, cancellationToken));
    }

    /// <summary>
    /// Reflection-based send implementation for commands (preserved for fallback and comparison).
    /// </summary>
    private async Task SendReflection(IRequest request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var requestTypeName = requestType.Name;
        var sanitizedRequestName = SanitizeTypeName(requestTypeName);

        using var activity = IsTelemetryEnabled ? ActivitySource.StartActivity($"{_mediatorSendActivity}{sanitizedRequestName}") : null;
        activity?.SetStatus(ActivityStatusCode.Ok);

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;
        var executedMiddleware = new List<string>();
        long startMemory = 0;

        // Debug logging: Send operation started
        _logger?.SendOperationStarted(requestTypeName, IsTelemetryEnabled);

        // Record initial memory if performance counters are enabled
        if (_statistics != null && HasPerformanceCountersEnabled())
        {
            startMemory = GC.GetTotalMemory(false);
        }

        try
        {
            _statistics?.IncrementCommand(requestTypeName);
            
            Type handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

            // Debug logging: Request type classification
            _logger?.SendRequestTypeClassification(requestTypeName, "command");

            // Debug logging: Handler resolution
            _logger?.SendHandlerResolution(PipelineUtilities.FormatTypeName(handlerType), requestTypeName);

            // Get middleware pipeline information for telemetry
            if (IsTelemetryEnabled && ShouldCaptureMiddlewareDetails && _pipelineBuilder is IMiddlewarePipelineInspector inspector)
            {
                var middlewareInfo = inspector.GetDetailedMiddlewareInfo(_serviceProvider);
                var applicableMiddleware = middlewareInfo.Where(m => IsMiddlewareApplicable(m.Type, requestType));
                var distinctMiddleware = GetDistinctMiddlewareNames(applicableMiddleware);

                activity?.SetTag("middleware.pipeline", string.Join(",", distinctMiddleware));
            }

            async Task FinalHandler()
            {
                // Check for multiple handler registrations
                IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
                object[] handlerArray = handlers.Where(h => h != null).ToArray()!;
                switch (handlerArray)
                {
                    case { Length: 0 }:
                        _logger?.NoHandlerFoundWarning(requestTypeName);
                        throw new InvalidOperationException(string.Format(_handlerNotFoundFormat, requestTypeName));
                    case { Length: > 1 }:
                        var handlerNames = string.Join(", ", handlerArray.Select(h => h.GetType().Name));
                        _logger?.MultipleHandlersFoundWarning(requestTypeName, handlerNames);
                        throw new InvalidOperationException(string.Format(_multipleHandlersFoundFormat, requestTypeName));
                }
                object handler = handlerArray[0];

                // Debug logging: Handler found
                var handlerTypeName = handler.GetType().Name;
                _logger?.SendHandlerFound(handlerTypeName, requestTypeName);

                MethodInfo method = handlerType.GetMethod(_handleMethodName) ?? throw new InvalidOperationException(string.Format(_handleMethodNotFoundFormat, handlerType.Name));
                try
                {
                    if (IsTelemetryEnabled && ShouldCaptureHandlerDetails)
                    {
                        var sanitizedHandlerName = SanitizeTypeName(handler.GetType().Name);
                        activity?.SetTag("handler.type", sanitizedHandlerName);
                    }

                    // Handle ValueTask (returned by IRequestHandler<TRequest>.Handle after ValueTask migration)
                    var invokeResult = method.Invoke(handler, [request, cancellationToken]);
                    if (invokeResult != null)
                    {
                        if (invokeResult is ValueTask vt)
                            await vt.ConfigureAwait(false);
                        else if (invokeResult is Task t)
                            await t.ConfigureAwait(false);
                    }
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }

            // Execute through middleware pipeline with enhanced tracking
            await ExecuteWithMiddlewareTracking(request, FinalHandler, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Debug logging: Send operation completed
            _logger?.SendOperationCompleted(requestTypeName, stopwatch.Elapsed.TotalMilliseconds, exception == null);

            // Enhanced statistics recording
            if (_statistics != null)
            {
                // Record execution time if performance counters are enabled
                if (HasPerformanceCountersEnabled())
                {
                    _statistics.RecordExecutionTime(requestTypeName, stopwatch.ElapsedMilliseconds, exception == null);

                    // Record memory allocation if available
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

                // Record detailed analysis if enabled
                if (HasDetailedAnalysisEnabled())
                {
                    _statistics.RecordExecutionPattern(requestTypeName, DateTime.UtcNow);
                }

                // Record middleware execution metrics if enabled
                if (HasMiddlewareMetricsEnabled())
                {
                    foreach (var middlewareName in executedMiddleware)
                    {
                        // Record that middleware was executed (duration would be tracked in pipeline)
                        _statistics.RecordMiddlewareExecution(middlewareName, 0, true);
                    }
                }
            }

            if (IsTelemetryEnabled)
            {
                // Record metrics with appropriate tags
                var tags = new TagList
                {
                    { "request_name", sanitizedRequestName },
                    { "request_type", "command" }
                };

                if (executedMiddleware.Count > 0)
                {
                    tags.Add("middleware.executed", string.Join(",", executedMiddleware));
                }

                MediatorMetrics.SendDurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

                if (exception == null)
                {
                    MediatorMetrics.SendSuccessCounter.Add(1, tags);
                    // Ensure successful completion is marked with Ok status
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    var sanitizedExceptionType = SanitizeTypeName(exception.GetType().Name);
                    tags.Add("exception.type", sanitizedExceptionType);
                    if (ShouldCaptureExceptionDetails)
                    {
                        tags.Add("exception.message", SanitizeExceptionMessage(exception.Message));
                    }
                    MediatorMetrics.SendFailureCounter.Add(1, tags);

                    // Add exception details to activity if enabled
                    if (ShouldCaptureExceptionDetails)
                    {
                        activity?.SetTag("exception.type", sanitizedExceptionType);
                        activity?.SetTag("exception.message", SanitizeExceptionMessage(exception.Message));

                        // Add stack trace if available and configured
                        var sanitizedStackTrace = SanitizeStackTrace(exception.StackTrace);
                        if (!string.IsNullOrEmpty(sanitizedStackTrace))
                        {
                            activity?.SetTag("exception.stack_trace", sanitizedStackTrace);
                        }
                    }
                    activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(exception.Message));
                }

                // Add activity tags
                activity?.SetTag("request_name", sanitizedRequestName);
                activity?.SetTag("request_type", "command");
                activity?.SetTag("duration_ms", stopwatch.Elapsed.TotalMilliseconds);
                if (executedMiddleware.Count > 0)
                {
                    activity?.SetTag("middleware.executed", string.Join(",", executedMiddleware));
                }

                // Increment health check counter
                MediatorMetrics.TelemetryHealthCounter.Add(1, new TagList { { "operation", "send" } });
            }
        }
    }

    /// <summary>
    /// Reflection-based send implementation for queries (preserved for fallback and comparison).
    /// </summary>
    private async Task<TResponse> SendReflection<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var requestTypeName = requestType.Name;
        var sanitizedRequestName = SanitizeTypeName(requestTypeName);
        var sanitizedResponseType = SanitizeTypeName(typeof(TResponse).Name);
        var isQuery = DetermineIfQuery(request);

        using var activity = IsTelemetryEnabled ? ActivitySource.StartActivity($"{_mediatorSendActivity}{sanitizedRequestName}") : null;
        activity?.SetStatus(ActivityStatusCode.Ok);

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;
        var executedMiddleware = new List<string>();
        long startMemory = 0;

        // Debug logging: Send operation started
        _logger?.SendOperationStarted(requestTypeName, IsTelemetryEnabled);

        // Record initial memory if performance counters are enabled
        if (_statistics != null && HasPerformanceCountersEnabled())
        {
            startMemory = GC.GetTotalMemory(false);
        }

        try
        {
            if (_statistics != null)
            {
                if (isQuery)
                {
                    _statistics.IncrementQuery(requestTypeName);
                    // Debug logging: Request type classification
                    _logger?.SendRequestTypeClassification(requestTypeName, "query");
                }
                else
                {
                    _statistics.IncrementCommand(requestTypeName);
                    // Debug logging: Request type classification
                    _logger?.SendRequestTypeClassification(requestTypeName, "command");
                }
            }

            Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            // Debug logging: Handler resolution
            _logger?.SendHandlerResolution(PipelineUtilities.FormatTypeName(handlerType), requestTypeName);

            // Get middleware pipeline information for telemetry
            if (IsTelemetryEnabled && ShouldCaptureMiddlewareDetails && _pipelineBuilder is IMiddlewarePipelineInspector inspector)
            {
                var middlewareInfo = inspector.GetDetailedMiddlewareInfo(_serviceProvider);
                var applicableMiddleware = middlewareInfo.Where(m => IsMiddlewareApplicable(m.Type, requestType, typeof(TResponse)));
                var distinctMiddleware = GetDistinctMiddlewareNames(applicableMiddleware);

                activity?.SetTag("middleware.pipeline", string.Join(",", distinctMiddleware));
            }

            async Task<TResponse> FinalHandler()
            {
                IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
                object[] handlerArray = handlers.Where(h => h != null).ToArray()!;
                switch (handlerArray)
                {
                    case { Length: 0 }:
                        _logger?.NoHandlerFoundWarning(requestTypeName);
                        throw new InvalidOperationException(string.Format(_handlerNotFoundFormat, requestTypeName));
                    case { Length: > 1 }:
                        var handlerNames = string.Join(", ", handlerArray.Select(h => h.GetType().Name));
                        _logger?.MultipleHandlersFoundWarning(requestTypeName, handlerNames);
                        throw new InvalidOperationException(string.Format(_multipleHandlersFoundFormat, requestTypeName));
                }
                object handler = handlerArray[0];

                // Debug logging: Handler found
                var handlerTypeName = handler.GetType().Name;
                _logger?.SendHandlerFound(handlerTypeName, requestTypeName);

                MethodInfo method = handlerType.GetMethod(_handleMethodName) ?? throw new InvalidOperationException(string.Format(_handleMethodNotFoundFormat, handlerType.Name));
                try
                {
                    if (IsTelemetryEnabled && ShouldCaptureHandlerDetails)
                    {
                        var sanitizedHandlerName = SanitizeTypeName(handler.GetType().Name);
                        activity?.SetTag("handler.type", sanitizedHandlerName);
                        activity?.SetTag("response_type", sanitizedResponseType);
                    }

                    // Handle ValueTask<TResponse> (returned by IRequestHandler<TRequest,TResponse>.Handle after ValueTask migration)
                    var invokeResult = method.Invoke(handler, [request, cancellationToken]);
                    if (invokeResult == null)
                        throw new InvalidOperationException(string.Format(_handlerReturnedNullFormat, requestTypeName));
                    if (invokeResult is ValueTask<TResponse> vt)
                    {
                        var result = await vt.ConfigureAwait(false);
                        if (result is null)
                            throw new InvalidOperationException(string.Format(_handlerReturnedNullFormat, requestTypeName));
                        return result;
                    }
                    if (invokeResult is Task<TResponse> t)
                    {
                        var result = await t.ConfigureAwait(false);
                        if (result is null)
                            throw new InvalidOperationException(string.Format(_handlerReturnedNullFormat, requestTypeName));
                        return result;
                    }
                    throw new InvalidOperationException(
                        $"Handler returned unexpected type '{invokeResult.GetType().FullName}'. Expected ValueTask<{typeof(TResponse).Name}>.");
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }

            // Execute through middleware pipeline with enhanced tracking
            return await ExecuteWithMiddlewareTracking(request, FinalHandler, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Debug logging: Send operation completed
            _logger?.SendOperationCompleted(requestTypeName, stopwatch.Elapsed.TotalMilliseconds, exception == null);

            // Enhanced statistics recording
            if (_statistics != null)
            {
                // Record execution time if performance counters are enabled
                if (HasPerformanceCountersEnabled())
                {
                    _statistics.RecordExecutionTime(requestTypeName, stopwatch.ElapsedMilliseconds, exception == null);

                    // Record memory allocation if available
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

                // Record detailed analysis if enabled
                if (HasDetailedAnalysisEnabled())
                {
                    _statistics.RecordExecutionPattern(requestTypeName, DateTime.UtcNow);
                }

                // Record middleware execution metrics if enabled
                if (HasMiddlewareMetricsEnabled())
                {
                    foreach (var middlewareName in executedMiddleware)
                    {
                        // Record that middleware was executed (duration would be tracked in pipeline)
                        _statistics.RecordMiddlewareExecution(middlewareName, 0, true);
                    }
                }
            }

            if (IsTelemetryEnabled)
            {
                // Record metrics with appropriate tags
                var tags = new TagList
                {
                    { "request_name", sanitizedRequestName },
                    { "request_type", isQuery ? "query" : "command" },
                    { "response_type", sanitizedResponseType }
                };

                if (executedMiddleware.Count > 0)
                {
                    tags.Add("middleware.executed", string.Join(",", executedMiddleware));
                }

                MediatorMetrics.SendDurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

                if (exception == null)
                {
                    MediatorMetrics.SendSuccessCounter.Add(1, tags);
                    // Ensure successful completion is marked with Ok status
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    var sanitizedExceptionType = SanitizeTypeName(exception.GetType().Name);
                    tags.Add("exception.type", sanitizedExceptionType);
                    tags.Add("exception.message", SanitizeExceptionMessage(exception.Message));
                    MediatorMetrics.SendFailureCounter.Add(1, tags);

                    // Add exception details to activity
                    activity?.SetTag("exception.type", sanitizedExceptionType);
                    activity?.SetTag("exception.message", SanitizeExceptionMessage(exception.Message));
                    activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(exception.Message));
                }

                // Add activity tags
                activity?.SetTag("request_name", sanitizedRequestName);
                activity?.SetTag("request_type", isQuery ? "query" : "command");
                activity?.SetTag("response_type", sanitizedResponseType);
                activity?.SetTag("duration_ms", stopwatch.Elapsed.TotalMilliseconds);
                if (executedMiddleware.Count > 0)
                {
                    activity?.SetTag("middleware.executed", string.Join(",", executedMiddleware));
                }

                // Increment health check counter
                MediatorMetrics.TelemetryHealthCounter.Add(1, new TagList { { "operation", "send_with_response" } });
            }
        }
    }
}