using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Configuration;
using Blazing.Mediator.Exceptions;
using System.Reflection;

namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// Builds and executes notification middleware pipelines with support for generic types and conditional execution.
/// </summary>
public sealed class NotificationPipelineBuilder : INotificationPipelineBuilder, INotificationMiddlewarePipelineInspector
{
    private const string OrderPropertyName = "Order";
    private readonly List<NotificationMiddlewareInfo> _middlewareInfos = [];
    private readonly MediatorLogger? _mediatorLogger;

    /// <summary>
    /// Information about registered notification middleware.
    /// </summary>
    /// <param name="Type">The middleware type</param>
    /// <param name="Order">The execution order</param>
    /// <param name="Configuration">Optional configuration object</param>
    private sealed record NotificationMiddlewareInfo(Type Type, int Order, object? Configuration = null);

    /// <summary>
    /// Initializes a new instance of the NotificationPipelineBuilder with optional logging.
    /// </summary>
    /// <param name="mediatorLogger">Optional MediatorLogger for enhanced logging.</param>
    public NotificationPipelineBuilder(MediatorLogger? mediatorLogger = null)
    {
        _mediatorLogger = mediatorLogger;
    }

    #region AddMiddleware overloads

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class, INotificationMiddleware
    {
        var middlewareType = typeof(TMiddleware);
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order));

        return this;
    }

    /// <summary>
    /// Adds notification middleware with configuration to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type that implements INotificationMiddleware.</typeparam>
    /// <param name="configuration">Optional configuration object for the middleware.</param>
    /// <returns>The pipeline builder for chaining.</returns>
    public INotificationPipelineBuilder AddMiddleware<TMiddleware>(object? configuration)
        where TMiddleware : class, INotificationMiddleware
    {
        var middlewareType = typeof(TMiddleware);
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order, configuration));

        return this;
    }

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware(Type middlewareType)
    {
        if (!typeof(INotificationMiddleware).IsAssignableFrom(middlewareType))
        {
            throw new ArgumentException($"Type {middlewareType.Name} does not implement INotificationMiddleware", nameof(middlewareType));
        }

        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order));

        return this;
    }

    #endregion

    #region Build overloads

    /// <inheritdoc />
    [Obsolete("Use ExecutePipeline method instead for better performance and consistency")]
    public NotificationDelegate<TNotification> Build<TNotification>(
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler)
        where TNotification : INotification
    {
        // Legacy build method - maintained for compatibility but ExecutePipeline is preferred
        return BuildLegacyPipeline(serviceProvider, finalHandler);
    }

    /// <summary>
    /// Legacy build method for backward compatibility.
    /// </summary>
    private NotificationDelegate<TNotification> BuildLegacyPipeline<TNotification>(
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler)
        where TNotification : INotification
    {
        // Sort middleware by order (ascending - lower numbers execute first)
        var sortedMiddleware = _middlewareInfos
            .OrderBy(m => m.Order)
            .ToList();

        // Build pipeline from right to left (last middleware first)
        NotificationDelegate<TNotification> pipeline = finalHandler;

        for (int i = sortedMiddleware.Count - 1; i >= 0; i--)
        {
            var middlewareInfo = sortedMiddleware[i];
            var currentPipeline = pipeline;

            pipeline = async (notification, cancellationToken) =>
            {
                var middleware = (INotificationMiddleware)serviceProvider.GetRequiredService(middlewareInfo.Type);

                // Check if it's conditional middleware
                if (middleware is IConditionalNotificationMiddleware conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(notification))
                {
                    await currentPipeline(notification, cancellationToken).ConfigureAwait(false);
                    return;
                }

                await middleware.InvokeAsync(notification, currentPipeline, cancellationToken).ConfigureAwait(false);
            };
        }

        return pipeline;
    }

    #endregion

    #region ExecutePipeline - Enhanced Implementation with Logging

    /// <summary>
    /// Executes the notification middleware pipeline with enhanced support for generic types, 
    /// conditional execution, and comprehensive logging.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being processed</typeparam>
    /// <param name="notification">The notification to process</param>
    /// <param name="serviceProvider">Service provider for middleware resolution</param>
    /// <param name="finalHandler">Final handler to execute after all middleware</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task ExecutePipeline<TNotification>(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        Type actualNotificationType = notification.GetType();
        var pipelineId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _mediatorLogger?.NotificationPipelineStarted(actualNotificationType.Name, _middlewareInfos.Count, pipelineId);

        NotificationDelegate<TNotification> pipeline = finalHandler;
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach (NotificationMiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                var genericParams = middlewareType.GetGenericArguments();
                switch (genericParams)
                {
                    case { Length: 1 }:
                        bool canSatisfyConstraints = true; // Always true now, as constraint validation is removed
                        if (!canSatisfyConstraints)
                        {
                            continue;
                        }
                        try
                        {
                            actualMiddlewareType = middlewareType.MakeGenericType(actualNotificationType);
                        }
                        catch (ArgumentException)
                        {
                            continue;
                        }
                        break;
                    default:
                        continue;
                }
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }

            bool isCompatible = typeof(INotificationMiddleware).IsAssignableFrom(actualMiddlewareType);
            if (!isCompatible)
            {
                continue;
            }

            var constrainedInterfaces = actualMiddlewareType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                .ToArray();

            if (constrainedInterfaces.Length > 0)
            {
                bool hasCompatibleConstraint = constrainedInterfaces.Any(constrainedInterface =>
                {
                    var constraintType = constrainedInterface.GetGenericArguments()[0];
                    return constraintType.IsAssignableFrom(actualNotificationType);
                });

                if (!hasCompatibleConstraint)
                {
                    continue;
                }
            }

            int actualOrder = GetActualMiddlewareOrder(middlewareInfo, actualMiddlewareType, serviceProvider);
            applicableMiddleware.Add((actualMiddlewareType, actualOrder));
        }

        applicableMiddleware.Sort((a, b) =>
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;
            int indexA = GetOriginalRegistrationIndex(a.Type);
            int indexB = GetOriginalRegistrationIndex(b.Type);
            return indexA.CompareTo(indexB);
        });

        int executedCount = 0;
        int skippedCount = _middlewareInfos.Count - applicableMiddleware.Count;

        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            (Type middlewareType, int order) = applicableMiddleware[i];
            NotificationDelegate<TNotification> currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;

            pipeline = async (notif, ct) =>
            {
                var middlewareStopwatch = System.Diagnostics.Stopwatch.StartNew();
                object? middlewareInstance = serviceProvider.GetService(middlewareType);
                if (middlewareInstance == null)
                {
                    throw new InvalidOperationException(
                        $"Could not create instance of notification middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                }
                if (middlewareInstance is not INotificationMiddleware middleware)
                {
                    throw new InvalidOperationException(
                        $"Middleware {middlewareName} does not implement INotificationMiddleware.");
                }
                if (middleware is IConditionalNotificationMiddleware conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(notif))
                {
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }
                if (IsTypeConstrainedMiddleware(middleware, actualNotificationType, notif))
                {
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }
                bool executionSuccessful = false;
                try
                {
                    bool invokedConstrainedMethod = await TryInvokeConstrainedMethodAsync(middleware, notif, currentPipeline, ct, actualNotificationType);
                    if (!invokedConstrainedMethod)
                    {
                        await middleware.InvokeAsync(notif, currentPipeline, ct).ConfigureAwait(false);
                    }
                    executionSuccessful = true;
                    executedCount++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    executionSuccessful = false;
                    throw;
                }
                finally
                {
                    middlewareStopwatch.Stop();
                }
            };
        }

        await pipeline(notification, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();
    }

    #endregion

    #region Inspector Methods

    /// <inheritdoc />
    public IReadOnlyList<Type> GetRegisteredMiddleware()
    {
        return _middlewareInfos.Select(info => info.Type).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration()
    {
        return _middlewareInfos.Select(info => (info.Type, info.Configuration)).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null)
    {
        if (serviceProvider == null)
        {
            return _middlewareInfos.Select(info => (info.Type, info.Order, info.Configuration)).ToList();
        }
        var result = new List<(Type Type, int Order, object? Configuration)>();
        foreach (var middlewareInfo in _middlewareInfos)
        {
            int actualOrder = middlewareInfo.Order;
            try
            {
                if (middlewareInfo.Type.IsGenericTypeDefinition)
                {
                    var genericParams = middlewareInfo.Type.GetGenericArguments();
                    Type? actualMiddlewareType = null;
                    if (genericParams.Length == 1)
                    {
                        actualMiddlewareType = TryCreateConcreteNotificationMiddlewareType(middlewareInfo.Type);
                    }
                    if (actualMiddlewareType != null)
                    {
                        actualOrder = GetActualMiddlewareOrder(middlewareInfo, actualMiddlewareType, serviceProvider);
                    }
                }
                else
                {
                    actualOrder = GetActualMiddlewareOrder(middlewareInfo, middlewareInfo.Type, serviceProvider);
                }
            }
            catch
            {
            }
            result.Add((middlewareInfo.Type, actualOrder, middlewareInfo.Configuration));
        }
        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true)
    {
        var middlewareInfos = GetDetailedMiddlewareInfo(serviceProvider);
        var analysisResults = new List<MiddlewareAnalysis>();
        foreach (var (type, order, configuration) in middlewareInfos.OrderBy(m => m.Order))
        {
            var orderDisplay = order == int.MaxValue ? "Default" : order.ToString();
            var className = GetCleanTypeName(type);
            var typeParameters = type.IsGenericType ?
                $"<{string.Join(", ", type.GetGenericArguments().Select(t => t.Name))}>" :
                string.Empty;
            var detailed = isDetailed ?? true;
            var genericConstraints = detailed ? GetGenericConstraints(type) : string.Empty;
            var handlerInfo = detailed ? configuration : null;
            analysisResults.Add(new MiddlewareAnalysis(
                Type: type,
                Order: order,
                OrderDisplay: orderDisplay,
                ClassName: className,
                TypeParameters: detailed ? typeParameters : string.Empty,
                GenericConstraints: genericConstraints,
                Configuration: handlerInfo
            ));
        }
        return analysisResults;
    }

    /// <inheritdoc />
    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider)
    {
        return AnalyzeMiddleware(serviceProvider, true);
    }

    #endregion

    #region Helper Methods

    private async Task<bool> TryInvokeConstrainedMethodAsync<TNotification>(
        INotificationMiddleware middleware, 
        TNotification notification, 
        NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken,
        Type actualNotificationType)
        where TNotification : INotification
    {
        var constrainedInterfaces = middleware.GetType().GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            .ToArray();
        if (constrainedInterfaces.Length == 0)
        {
            return false;
        }
        foreach (var constrainedInterface in constrainedInterfaces)
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            bool isCompatible = constraintType.IsAssignableFrom(actualNotificationType);
            if (isCompatible)
            {
                var constrainedMethod = constrainedInterface.GetMethod("InvokeAsync");
                if (constrainedMethod != null)
                {
                    try
                    {
                        var delegateType = typeof(NotificationDelegate<>).MakeGenericType(constraintType);
                        var constrainedNext = Delegate.CreateDelegate(delegateType, next.Target, next.Method);
                        var task = (Task?)constrainedMethod.Invoke(middleware, [notification, constrainedNext, cancellationToken]);
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                            return true;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }
        return false;
    }

    private static bool IsTypeConstrainedMiddleware(INotificationMiddleware middleware, Type notificationType, object notification)
    {
        var middlewareType = middleware.GetType();
        var genericMiddlewareInterfaces = middlewareType.GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            .ToArray();
        if (genericMiddlewareInterfaces.Length == 0)
        {
            return false;
        }
        foreach (var constrainedInterface in genericMiddlewareInterfaces)
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            if (constraintType.IsAssignableFrom(notificationType))
            {
                return false;
            }
        }
        return true;
    }

    private int GetMiddlewareOrder(Type middlewareType)
    {
        var orderProperty = middlewareType.GetProperty("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
        {
            int staticOrder = (int)orderProperty.GetValue(null)!;
            return staticOrder;
        }
        var orderField = middlewareType.GetField("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderField != null && orderField.FieldType == typeof(int))
        {
            int staticOrder = (int)orderField.GetValue(null)!;
            return staticOrder;
        }
        var orderAttribute = middlewareType.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name == "OrderAttribute");
        if (orderAttribute != null)
        {
            var orderProp = orderAttribute.GetType().GetProperty("Order");
            if (orderProp != null && orderProp.PropertyType == typeof(int))
            {
                int attrOrder = (int)orderProp.GetValue(orderAttribute)!;
                return attrOrder;
            }
        }
        var instanceOrderProperty = middlewareType.GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
        if (instanceOrderProperty != null && instanceOrderProperty.PropertyType == typeof(int))
        {
            if (middlewareType.IsGenericTypeDefinition)
            {
                return int.MaxValue - 1000000;
            }
            else
            {
                try
                {
                    object? instance = Activator.CreateInstance(middlewareType);
                    if (instance != null)
                    {
                        int orderValue = (int)instanceOrderProperty.GetValue(instance)!;
                        return orderValue;
                    }
                }
                catch
                {
                }
            }
        }
        int unorderedCount = _middlewareInfos.Count(m => m.Order >= 1 && m.Order < 100);
        return unorderedCount + 1;
    }

    private int GetActualMiddlewareOrder(NotificationMiddlewareInfo middlewareInfo, Type actualMiddlewareType, IServiceProvider serviceProvider)
    {
        int actualOrder = middlewareInfo.Order;
        try
        {
            var instance = serviceProvider.GetService(actualMiddlewareType);
            if (instance != null)
            {
                var orderProperty = instance.GetType().GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
                if (orderProperty != null && orderProperty.PropertyType == typeof(int))
                {
                    actualOrder = (int)orderProperty.GetValue(instance)!;
                }
            }
        }
        catch
        {
        }
        return actualOrder;
    }

    private int GetOriginalRegistrationIndex(Type middlewareType)
    {
        return _middlewareInfos.FindIndex(info =>
            info.Type == middlewareType ||
            (info.Type.IsGenericTypeDefinition && middlewareType.IsGenericType && 
             info.Type == middlewareType.GetGenericTypeDefinition()));
    }

    private static Type? TryCreateConcreteNotificationMiddlewareType(Type middlewareTypeDefinition)
    {
        if (!middlewareTypeDefinition.IsGenericTypeDefinition)
            return middlewareTypeDefinition;
        var genericParams = middlewareTypeDefinition.GetGenericArguments();
        if (genericParams.Length != 1)
            return null;
        var candidateTypes = new List<Type>();
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .OrderBy(a =>
                {
                    var name = a.FullName ?? "";
                    if (name.Contains("Test")) return 0;
                    if (!name.StartsWith("System") && !name.StartsWith("Microsoft")) return 1;
                    return 2;
                })
                .ToArray();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && t.IsPublic)
                        .Where(t => typeof(INotification).IsAssignableFrom(t))
                        .Where(t => t.GetConstructor(Type.EmptyTypes) != null);
                    candidateTypes.AddRange(types);
                }
                catch
                {
                    continue;
                }
            }
        }
        catch
        {
            candidateTypes.AddRange([typeof(MinimalNotification)]);
        }
        foreach (var notificationType in candidateTypes)
        {
            if (TryMakeGenericType(middlewareTypeDefinition, [notificationType], out var concreteType))
            {
                return concreteType;
            }
        }
        return null;
    }

    private static bool TryMakeGenericType(Type genericTypeDefinition, Type[] typeArguments, out Type? concreteType)
    {
        concreteType = null;
        try
        {
            concreteType = genericTypeDefinition.MakeGenericType(typeArguments);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string GetGenericConstraints(Type type)
    {
        if (!type.IsGenericTypeDefinition)
            return string.Empty;
        var genericParameters = type.GetGenericArguments();
        if (genericParameters.Length == 0)
            return string.Empty;
        var constraintParts = new List<string>();
        foreach (var parameter in genericParameters)
        {
            var parameterConstraints = new List<string>();
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                parameterConstraints.Add("class");
            }
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                parameterConstraints.Add("struct");
            }
            var typeConstraints = parameter.GetGenericParameterConstraints();
            parameterConstraints.AddRange(typeConstraints
                .Where(constraint => constraint.IsInterface || constraint.IsClass)
                .Select(FormatTypeName));
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                parameterConstraints.Add("new()");
            }
            if (parameterConstraints.Count > 0)
            {
                var constraintText = $"where {parameter.Name} : {string.Join(", ", parameterConstraints)}";
                constraintParts.Add(constraintText);
            }
        }
        return constraintParts.Count > 0 ? string.Join(" ", constraintParts) : string.Empty;
    }

    private static string FormatTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;
        var genericTypeName = type.Name;
        var backtickIndex = genericTypeName.IndexOf('`');
        if (backtickIndex > 0)
        {
            genericTypeName = genericTypeName[..backtickIndex];
        }
        var genericArgs = type.GetGenericArguments();
        var genericArgNames = genericArgs.Select(arg => arg.IsGenericParameter ? arg.Name : FormatTypeName(arg));
        return $"{genericTypeName}<{string.Join(", ", genericArgNames)}>`";
    }

    private static string GetCleanTypeName(Type type)
    {
        var typeName = type.Name;
        var backtickIndex = typeName.IndexOf('`');
        return backtickIndex > 0 ? typeName[..backtickIndex] : typeName;
    }

    private sealed class MinimalNotification : INotification { }

    #endregion
}
