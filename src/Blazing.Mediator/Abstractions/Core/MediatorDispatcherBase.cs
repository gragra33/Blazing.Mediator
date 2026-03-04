namespace Blazing.Mediator;

/// <summary>
/// Abstract base class that bridges the pre-compiled library and the source-generated
/// <c>ContainerMetadata</c> in each consuming project.
/// </summary>
/// <remarks>
/// <para>
/// The source generator emits <c>internal sealed class ContainerMetadata : MediatorDispatcherBase</c>
/// for every project that has handlers. The generated <c>AddMediator()</c> extension registers
/// <c>ContainerMetadata</c> as the singleton <see cref="MediatorDispatcherBase"/> in the DI container.
/// </para>
/// <para>
/// The <see cref="Mediator"/> class resolves <c>MediatorDispatcherBase</c> from DI via
/// <see cref="Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService{T}"/>
/// on first call and caches it. If the service is not registered (i.e. the consuming project has not
/// called <c>AddMediator()</c>), the return value is <c>null</c> and the reflection-based
/// fallback path in <c>Mediator.Send.cs</c> / <c>Mediator.SendStream.cs</c> is used instead.
/// </para>
/// <para>
/// This design eliminates all <c>#if USE_SOURCE_GENERATORS</c> conditional compilation from the runtime
/// library, making source-generator dispatch the primary path at runtime without requiring a compile-time
/// symbol in the library project itself.
/// </para>
/// </remarks>
public abstract class MediatorDispatcherBase
{
    /// <summary>
    /// Dispatches a void command to its source-generated, pre-resolved handler wrapper.
    /// </summary>
    /// <param name="request">The void command to dispatch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public abstract ValueTask SendAsync(IRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Dispatches a request with a typed response to its source-generated, pre-resolved handler wrapper.
    /// </summary>
    /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
    /// <param name="request">The request to dispatch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResponse}"/> containing the handler's response.</returns>
    public abstract ValueTask<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);

    /// <summary>
    /// Dispatches a stream request to its source-generated, pre-resolved handler wrapper.
    /// </summary>
    /// <typeparam name="TResponse">The type of each item in the response stream.</typeparam>
    /// <param name="request">The stream request to dispatch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="IAsyncEnumerable{TResponse}"/> of response items.</returns>
    public abstract IAsyncEnumerable<TResponse> SendStreamAsync<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken);

    /// <summary>
    /// Dispatches a notification to its source-generated, pre-resolved handler wrappers.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="notification">The notification instance to dispatch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when all handlers have been invoked.</returns>
    public abstract ValueTask PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification;

    /// <summary>
    /// Returns <see langword="true"/> when <typeparamref name="TNotification"/> was discovered at compile
    /// time and a pre-cached <c>NotificationHandlerWrapper_*</c> field exists for it in the generated
    /// <c>ContainerMetadata</c>.  The JIT can constant-fold this check when <typeparamref name="TNotification"/>
    /// is known statically (i.e. the entire fast-path becomes a predicted-not-taken branch).
    /// Returns <see langword="false"/> for notification types that only have manual subscribers or that were
    /// not present in the compilation, directing them to the <c>PublishReflection</c> fallback.
    /// </summary>
    /// <typeparam name="TNotification">The notification type to probe.</typeparam>
    /// <returns><see langword="true"/> if the source-gen dispatch chain can handle this type.</returns>
    public abstract bool IsNotificationHandled<TNotification>()
        where TNotification : INotification;
}
