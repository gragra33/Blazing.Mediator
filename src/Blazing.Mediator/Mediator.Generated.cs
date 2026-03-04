#nullable enable

namespace Blazing.Mediator;

/// <summary>
/// Partial extension of <see cref="Mediator"/> that wires the source-generated dispatcher.
/// </summary>
/// <remarks>
/// <para>
/// When a consuming project calls <c>services.AddMediator()</c>, the generated
/// <c>ContainerMetadata : MediatorDispatcherBase</c> is registered in DI.
/// <see cref="GetDispatcher"/> returns the instance pre-resolved in the constructor.
/// </para>
/// <para>
/// If <c>AddMediator()</c> was not called, <see cref="GetDispatcher"/> returns
/// <c>null</c>. Subsequent calls into <c>Mediator.Send.cs</c> and
/// <c>Mediator.SendStream.cs</c> will throw <see cref="System.InvalidOperationException"/>
/// when the dispatcher is missing; no reflection-based fallback is used.
/// </para>
/// </remarks>
public sealed partial class Mediator
{
    /// <summary>
    /// Returns the source-generated <see cref="MediatorDispatcherBase"/> resolved eagerly
    /// in the constructor.  Zero allocations, zero DI on every dispatch — just a field read.
    /// Returns <c>null</c> when the dispatcher has not been registered via <c>AddMediator()</c>.
    /// </summary>
    [global::System.Runtime.CompilerServices.MethodImpl(
        global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal MediatorDispatcherBase? GetDispatcher() => _dispatcher;
}
