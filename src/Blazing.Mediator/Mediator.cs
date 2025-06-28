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

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handlers.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

        try
        {
            var task = (Task?)method.Invoke(handler, [request, cancellationToken]);

            if (task != null)
            {
                await task;
            }
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    /// <inheritdoc />
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

        try
        {
            var task = (Task<TResponse>?)method.Invoke(handler, [request, cancellationToken]);

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
    }
}
