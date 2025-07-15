namespace Blazing.Mediator;

/// <summary>
/// Implementation of the Mediator pattern that dispatches requests to their corresponding handlers.
/// </summary>
/// <remarks>
/// The Mediator class serves as a centralized request dispatcher that decouples the request sender 
/// from the request handler. It uses dependency injection to resolve handlers at runtime and 
/// supports both void and typed responses.
/// </remarks>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMiddlewarePipelineBuilder _pipelineBuilder;
    private readonly INotificationPipelineBuilder _notificationPipelineBuilder;
    
    // Thread-safe collections for notification subscribers
    private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _specificSubscribers = new();
    private readonly ConcurrentBag<INotificationSubscriber> _genericSubscribers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handlers.</param>
    /// <param name="pipelineBuilder">The middleware pipeline builder.</param>
    /// <param name="notificationPipelineBuilder">The notification middleware pipeline builder.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider, pipelineBuilder, or notificationPipelineBuilder is null.</exception>
    public Mediator(IServiceProvider serviceProvider, IMiddlewarePipelineBuilder pipelineBuilder, INotificationPipelineBuilder notificationPipelineBuilder)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        _notificationPipelineBuilder = notificationPipelineBuilder ?? throw new ArgumentNullException(nameof(notificationPipelineBuilder));
    }

    /// <summary>
    /// Sends a command request through the middleware pipeline to its corresponding handler.
    /// </summary>
    /// <param name="request">The command request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        Type requestType = request.GetType();
        Type handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        // Create final handler delegate that executes the actual handler
        RequestHandlerDelegate finalHandler = async () =>
        {
            // Check for multiple handler registrations
            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
            object[] handlerArray = handlers.Where(h => h != null).ToArray()!;

            switch (handlerArray)
            {
                case { Length: 0 }:
                    throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
                case { Length: > 1 }:
                    throw new InvalidOperationException($"Multiple handlers found for request type {requestType.Name}. Only one handler per request type is allowed.");
            }

            object handler = handlerArray[0];
            MethodInfo method = handlerType.GetMethod("Handle")
                                ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

            try
            {
                Task? task = (Task?)method.Invoke(handler, [request, cancellationToken]);

                if (task != null)
                {
                    await task;
                }
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        };

        // Execute through middleware pipeline using reflection to call the generic method
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
            // Fallback to direct execution if pipeline method not found
            await finalHandler();
            return;
        }

        MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(requestType);
        Task? pipelineTask = (Task?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, finalHandler, cancellationToken]);

        if (pipelineTask != null)
        {
            await pipelineTask;
        }
    }

    /// <summary>
    /// Sends a query request through the middleware pipeline to its corresponding handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from the handler</typeparam>
    /// <param name="request">The query request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task containing the response from the handler</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type or the handler returns null</exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Type requestType = request.GetType();
        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        // Create final handler delegate that executes the actual handler
        RequestHandlerDelegate<TResponse> finalHandler = async () =>
        {
            // Check for multiple handler registrations
            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
            object[] handlerArray = handlers.Where(h => h != null).ToArray()!;

            switch (handlerArray)
            {
                case { Length: 0 }:
                    throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
                case { Length: > 1 }:
                    throw new InvalidOperationException($"Multiple handlers found for request type {requestType.Name}. Only one handler per request type is allowed.");
            }

            object handler = handlerArray[0];
            MethodInfo method = handlerType.GetMethod("Handle")
                                ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

            try
            {
                Task<TResponse>? task = (Task<TResponse>?)method.Invoke(handler, [request, cancellationToken]);

                if (task != null)
                {
                    return await task;
                }

                throw new InvalidOperationException($"Handler for {requestType.Name} returned null");
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        };

        // Execute through middleware pipeline using reflection to call the generic method with the correct runtime type
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
            // Fallback to direct execution if pipeline method not found
            return await finalHandler();
        }

        MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(requestType, typeof(TResponse));
        Task<TResponse>? pipelineTask = (Task<TResponse>?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, finalHandler, cancellationToken]);

        if (pipelineTask != null)
        {
            return await pipelineTask;
        }

        return await finalHandler();
    }
    
    /// <summary>
    /// Sends a stream request through the middleware pipeline to its corresponding handler and returns an async enumerable.
    /// </summary>
    /// <typeparam name="TResponse">The type of response items in the stream</typeparam>
    /// <param name="request">The stream request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>An async enumerable of response items</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public IAsyncEnumerable<TResponse> SendStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Type requestType = request.GetType();
        Type handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        // Create final handler delegate that executes the actual stream handler
        StreamRequestHandlerDelegate<TResponse> finalHandler = () =>
        {
            // Check for multiple handler registrations
            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
            object[] handlerArray = handlers.Where(h => h != null).ToArray()!;
            
            switch (handlerArray)
            {
                case { Length: 0 }:
                    throw new InvalidOperationException($"No handler found for stream request type {requestType.Name}");
                case { Length: > 1 }:
                    throw new InvalidOperationException($"Multiple handlers found for stream request type {requestType.Name}. Only one handler per request type is allowed.");
            }

            object handler = handlerArray[0];
            MethodInfo method = handlerType.GetMethod("Handle")
                                ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

            try
            {
                IAsyncEnumerable<TResponse>? result = (IAsyncEnumerable<TResponse>?)method.Invoke(handler, [request, cancellationToken]);
                return result ?? throw new InvalidOperationException($"Handler for {requestType.Name} returned null");
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        };

        // Execute through middleware pipeline using reflection to call the generic method
        MethodInfo? executeMethod = _pipelineBuilder
            .GetType()
            .GetMethods()
            .FirstOrDefault(m =>
                m.Name == "ExecuteStreamPipeline" &&
                m.GetParameters().Length == 4 &&
                m.GetParameters()[2].ParameterType.IsGenericType &&
                m.IsGenericMethodDefinition);

        if (executeMethod == null)
        {
            // Fallback to direct execution if pipeline method not found
            return finalHandler();
        }

        MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(requestType, typeof(TResponse));
        IAsyncEnumerable<TResponse>? pipelineResult = (IAsyncEnumerable<TResponse>?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, finalHandler, cancellationToken]);

        return pipelineResult ?? finalHandler();
    }

    #region Notification Methods

    /// <summary>
    /// Publishes a notification to all subscribers following the observer pattern.
    /// Publishers blindly send notifications without caring about recipients.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to publish</typeparam>
    /// <param name="notification">The notification to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        // Create final handler delegate that notifies all subscribers
        NotificationDelegate<TNotification> finalHandler = async (notif, token) =>
        {
            var tasks = new List<Task>();

            // Notify specific subscribers
            if (_specificSubscribers.TryGetValue(typeof(TNotification), out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    if (subscriber is INotificationSubscriber<TNotification> typedSubscriber)
                    {
                        tasks.Add(SafeInvokeSubscriber(async () => await typedSubscriber.OnNotification(notif, token)));
                    }
                }
            }

            // Notify generic subscribers
            foreach (var genericSubscriber in _genericSubscribers)
            {
                tasks.Add(SafeInvokeSubscriber(async () => await genericSubscriber.OnNotification(notif, token)));
            }

            // Wait for all subscribers to complete
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        };

        // Execute through notification middleware pipeline
        await _notificationPipelineBuilder.ExecutePipeline(notification, _serviceProvider, finalHandler, cancellationToken);
    }

    /// <summary>
    /// Subscribe to notifications of a specific type.
    /// Subscribers actively choose to listen to notifications they're interested in.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to subscribe to</typeparam>
    /// <param name="subscriber">The subscriber that will receive notifications</param>
    public void Subscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        _specificSubscribers.AddOrUpdate(
            typeof(TNotification),
            new ConcurrentBag<object> { subscriber },
            (key, existing) =>
            {
                existing.Add(subscriber);
                return existing;
            });
    }

    /// <summary>
    /// Subscribe to all notifications (generic/broadcast).
    /// Subscribers actively choose to listen to all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber that will receive all notifications</param>
    public void Subscribe(INotificationSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        _genericSubscribers.Add(subscriber);
    }

    /// <summary>
    /// Unsubscribe from notifications of a specific type.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to unsubscribe from</typeparam>
    /// <param name="subscriber">The subscriber to remove</param>
    public void Unsubscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        if (_specificSubscribers.TryGetValue(typeof(TNotification), out var subscribers))
        {
            // Create new bag without the subscriber
            var newSubscribers = new ConcurrentBag<object>();
            foreach (var existing in subscribers)
            {
                if (!ReferenceEquals(existing, subscriber))
                {
                    newSubscribers.Add(existing);
                }
            }

            if (newSubscribers.IsEmpty)
            {
                _specificSubscribers.TryRemove(typeof(TNotification), out _);
            }
            else
            {
                _specificSubscribers.TryUpdate(typeof(TNotification), newSubscribers, subscribers);
            }
        }
    }

    /// <summary>
    /// Unsubscribe from all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber to remove from all notifications</param>
    public void Unsubscribe(INotificationSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        // Remove from generic subscribers
        var newGenericSubscribers = new ConcurrentBag<INotificationSubscriber>();
        foreach (var existing in _genericSubscribers)
        {
            if (!ReferenceEquals(existing, subscriber))
            {
                newGenericSubscribers.Add(existing);
            }
        }

        // Replace the entire bag
        _genericSubscribers.Clear();
        foreach (var sub in newGenericSubscribers)
        {
            _genericSubscribers.Add(sub);
        }
    }

    /// <summary>
    /// Safely invokes a subscriber method, catching and logging any exceptions.
    /// Ensures that exceptions in one subscriber don't affect other subscribers.
    /// </summary>
    /// <param name="subscriberAction">The subscriber action to invoke</param>
    /// <returns>A task representing the operation</returns>
    private static async Task SafeInvokeSubscriber(Func<Task> subscriberAction)
    {
        try
        {
            await subscriberAction();
        }
        catch (Exception ex)
        {
            // Log the exception but don't let it propagate to other subscribers
            // In a real implementation, you might want to use a logger here
            Console.WriteLine($"Exception in notification subscriber: {ex.Message}");
        }
    }

    #endregion
}
