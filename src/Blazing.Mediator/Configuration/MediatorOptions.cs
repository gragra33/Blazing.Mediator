namespace Blazing.Mediator.Configuration;

/// <summary>
/// Controls when <c>ContainerMetadata</c> initialises handler wrappers for the Singleton lifetime.
/// </summary>
public enum CachingMode
{
    /// <summary>
    /// All wrappers are initialised eagerly when <c>ContainerMetadata</c> is first resolved.
    /// Provides the best steady-state performance — all closures are pre-baked before the
    /// first request arrives. <strong>Default.</strong>
    /// </summary>
    Eager = 0,

    /// <summary>
    /// Each wrapper is initialised on the first dispatch of its request type.
    /// Useful when the application only dispatches a small subset of registered types.
    /// </summary>
    Lazy = 1,
}

/// <summary>
/// Selects the built-in notification publisher strategy.
/// Custom strategies can be registered by implementing <see cref="INotificationPublisher"/> directly.
/// </summary>
public enum NotificationPublisherType
{
    /// <summary>
    /// Invokes handlers sequentially using <c>foreach await</c>.
    /// Handlers with 2–4 entries use an unrolled fast path (no loop overhead, no allocation).
    /// <strong>Default.</strong>
    /// </summary>
    Sequential = 0,

    /// <summary>
    /// Starts all handlers synchronously before the first <c>await</c>, then awaits only the
    /// non-completed tasks. Aggregates exceptions from all handlers rather than stopping on
    /// the first failure. Does not use <c>Task.WhenAll</c> — avoids its array allocation.
    /// </summary>
    Concurrent = 1,
}

/// <summary>
/// Runtime options for configuring the generated <c>Mediator</c> and its DI registrations.
/// Pass an <c>Action&lt;MediatorOptions&gt;</c> to <c>services.AddMediator()</c> to override defaults.
/// </summary>
public sealed class MediatorOptions
{
    /// <summary>
    /// The DI lifetime used for every mediator component: handlers, middleware, the
    /// <c>Mediator</c> class itself, and the publisher.
    /// <para>
    /// <c>RequestHandlerWrapper</c>, <c>NotificationHandlerWrapper</c>, and
    /// <c>ContainerMetadata</c> are <strong>always Singleton</strong> — they are stateless
    /// data structures that cache the pre-baked handler closures.
    /// </para>
    /// Default: <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Controls when handler wrappers are initialised.
    /// Only relevant when <see cref="ServiceLifetime"/> is <see cref="ServiceLifetime.Singleton"/>.
    /// Default: <see cref="CachingMode.Eager"/>.
    /// </summary>
    public CachingMode CachingMode { get; set; } = CachingMode.Eager;

    /// <summary>
    /// Selects the built-in notification publisher strategy.
    /// Default: <see cref="NotificationPublisherType.Sequential"/>.
    /// </summary>
    public NotificationPublisherType NotificationPublisher { get; set; } = NotificationPublisherType.Sequential;

    /// <summary>
    /// The number of registered request types above which a <c>FrozenDictionary&lt;Type, object&gt;</c>
    /// is used for the generic <c>Send&lt;TResponse&gt;</c> dispatch fallback path instead of a linear
    /// switch statement. Tunable for applications with many handler types.
    /// Default: 20.
    /// </summary>
    public int FrozenDictionaryThreshold { get; set; } = 20;
}
