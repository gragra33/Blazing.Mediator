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

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handlers.</param>
    /// <param name="pipelineBuilder">The middleware pipeline builder.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider or pipelineBuilder is null.</exception>
    public Mediator(IServiceProvider serviceProvider, IMiddlewarePipelineBuilder pipelineBuilder)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
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
}
