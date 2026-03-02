using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazing.Mediator;

public sealed partial class Mediator
{
    /// <summary>
    /// Enhanced middleware execution with detailed tracking for statistics.
    /// </summary>
    private async Task ExecuteWithMiddlewareTracking<TRequest>(
        TRequest request,
        Func<Task> finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        // Create the proper delegate
        async ValueTask HandlerDelegate() => await finalHandler().ConfigureAwait(false);

        // Execute through middleware pipeline
        MethodInfo? executeMethod = _pipelineBuilder
            .GetType()
            .GetMethods()
            .FirstOrDefault(m =>
                m.Name == "ExecutePipeline" &&
                m.GetParameters().Length == 4 &&
                m.GetParameters()[2].ParameterType == typeof(RequestHandlerDelegate) &&
                m.IsGenericMethodDefinition);

        if (executeMethod == null)
        {
            await finalHandler().ConfigureAwait(false);
            return;
        }

        MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(typeof(TRequest));
        var pipelineResult = genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (RequestHandlerDelegate)HandlerDelegate, cancellationToken]);
        if (pipelineResult is ValueTask valueTask)
        {
            await valueTask.ConfigureAwait(false);
        }
        else if (pipelineResult is Task pipelineTask)
        {
            await pipelineTask.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Enhanced middleware execution with detailed tracking for statistics (with response).
    /// </summary>
    private async Task<TResponse> ExecuteWithMiddlewareTracking<TRequest, TResponse>(
        TRequest request,
        Func<Task<TResponse>> finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        // Create the proper delegate
        async ValueTask<TResponse> HandlerDelegate() => await finalHandler().ConfigureAwait(false);

        // Execute through middleware pipeline
        MethodInfo? executeMethod = _pipelineBuilder
            .GetType()
            .GetMethods()
            .FirstOrDefault(m =>
                m.Name == "ExecutePipeline" &&
                m.GetParameters().Length == 4 &&
                m.GetParameters()[2].ParameterType.IsGenericType &&
                m.IsGenericMethodDefinition);

        if (executeMethod == null)
        {
            return await finalHandler().ConfigureAwait(false);
        }

        MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(typeof(TRequest), typeof(TResponse));
        var pipelineResult = genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (RequestHandlerDelegate<TResponse>)HandlerDelegate, cancellationToken]);
        if (pipelineResult is ValueTask<TResponse> valueTask)
        {
            return await valueTask.ConfigureAwait(false);
        }
        if (pipelineResult is Task<TResponse> pipelineTask)
        {
            return await pipelineTask.ConfigureAwait(false);
        }
        return await finalHandler().ConfigureAwait(false);
    }

    /// <summary>
    /// Helper method to check if performance counters are enabled in statistics options.
    /// </summary>
    private bool HasPerformanceCountersEnabled()
    {
        return _statistics?.GetType()
            .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(_statistics) is StatisticsOptions { EnablePerformanceCounters: true };
    }

    /// <summary>
    /// Helper method to check if detailed analysis is enabled in statistics options.
    /// </summary>
    private bool HasDetailedAnalysisEnabled()
    {
        return _statistics?.GetType()
            .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(_statistics) is StatisticsOptions { EnableDetailedAnalysis: true };
    }

    /// <summary>
    /// Helper method to check if middleware metrics are enabled in statistics options.
    /// </summary>
    private bool HasMiddlewareMetricsEnabled()
    {
        return _statistics?.GetType()
            .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(_statistics) is StatisticsOptions { EnableMiddlewareMetrics: true };
    }
}