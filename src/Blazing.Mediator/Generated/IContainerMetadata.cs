namespace Blazing.Mediator.Generated;

/// <summary>
/// Marker interface implemented by the source-generated <c>ContainerMetadata</c> class.
/// Allows the library to reference the generated container metadata via DI without
/// coupling to the concrete generated type, which lives in the consuming assembly.
/// </summary>
/// <remarks>
/// Register and resolve this via DI using the generated <c>AddMediator()</c> extension.
/// When registered, the generated <c>RequestDispatcher</c> activates the Singleton fast-path
/// (zero per-call DI lookups, zero per-call allocations for the 0-middleware case).
/// </remarks>
internal interface IContainerMetadata
{
}
