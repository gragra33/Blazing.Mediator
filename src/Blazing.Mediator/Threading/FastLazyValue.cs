using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Threading;

/// <summary>
/// Thread-safe lazy-initialisation struct that avoids heap allocation for the lazy wrapper.
/// Uses a volatile <see cref="int"/> state flag and <see cref="Interlocked.CompareExchange(ref int,int,int)"/>
/// to guarantee that <typeparamref name="T"/> is initialised exactly once across concurrent callers.
/// </summary>
/// <typeparam name="T">The lazily-initialised value type (must be a reference type).</typeparam>
/// <typeparam name="TArg">The initialisation argument type passed to the factory on first access.</typeparam>
/// <remarks>
/// Intended for Singleton-lifetime components that need cheap, one-time lazy init without
/// allocating a <see cref="Lazy{T}"/> or <see cref="System.Threading.LazyInitializer"/> wrapper.
/// Example usage in generated <c>ContainerMetadata</c>:
/// <code>
/// private FastLazyValue&lt;ContainerMetadata, IServiceProvider&gt; _lazy;
/// public ContainerMetadata Get(IServiceProvider sp)
///     =&gt; _lazy.GetOrCreate(sp, static root =&gt; new ContainerMetadata(root));
/// </code>
/// </remarks>
internal struct FastLazyValue<T, TArg> where T : class
{
    // State machine: 0 = uninitialised, 1 = initialising, 2 = complete
    // No volatile keyword: all accesses use Volatile.Read, Volatile.Write, or Interlocked to avoid CS0420.
    private int _state;
    private T? _value;

    /// <summary>
    /// Gets the cached value, invoking <paramref name="factory"/> with <paramref name="arg"/>
    /// on the first call.  Concurrent callers block on a <see cref="SpinWait"/> until
    /// the winning thread has finished initialisation.
    /// </summary>
    /// <param name="arg">Argument forwarded to <paramref name="factory"/>; not stored after init.</param>
    /// <param name="factory">Factory delegate invoked exactly once to produce the value.</param>
    /// <returns>The initialised value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetOrCreate(TArg arg, Func<TArg, T> factory)
    {
        if (Volatile.Read(ref _state) == 2)
            return _value!;

        return GetOrCreateSlow(arg, factory);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private T GetOrCreateSlow(TArg arg, Func<TArg, T> factory)
    {
        // Spin until either we win the CAS and initialise, or another thread completes.
        while (true)
        {
            // Try to claim the initialising slot.
            if (Interlocked.CompareExchange(ref _state, 1, 0) == 0)
            {
                try
                {
                    _value = factory(arg);
                    Volatile.Write(ref _state, 2);
                    return _value;
                }
                catch
                {
                    // Reset so the next caller can retry.
                    Volatile.Write(ref _state, 0);
                    throw;
                }
            }

            // Another thread is initialising — spin until complete.
            var spin = new SpinWait();
            while (Volatile.Read(ref _state) != 2)
                spin.SpinOnce();

            return _value!;
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the value has been successfully initialised.
    /// </summary>
    public bool IsValueCreated => Volatile.Read(ref _state) == 2;
}
