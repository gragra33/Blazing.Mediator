namespace Blazing.Mediator;

/// <summary>
/// Marks a class as excluded from Blazing.Mediator source-generator auto-discovery.
/// Apply to middleware, handler, or notification handler classes that should NOT be
/// automatically included in generated pipelines — for example, test-only or
/// intentionally-broken middleware classes used in edge-case unit tests.
/// </summary>
/// <remarks>
/// Without this attribute the source generator discovers every class in the assembly
/// that implements a Blazing.Mediator interface (e.g. <c>IRequestMiddleware&lt;,&gt;</c>)
/// and wires it into the appropriate generated pipeline.  Annotate a class with
/// <c>[ExcludeFromAutoDiscovery]</c> to suppress that behaviour.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ExcludeFromAutoDiscoveryAttribute : Attribute;
