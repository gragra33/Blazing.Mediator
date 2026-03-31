namespace Blazing.Mediator;

/// <summary>
/// Specifies the execution order for a middleware class in the pipeline.
/// Lower values execute first. Use this attribute on middleware classes
/// to ensure the source generator emits the correct order, including for
/// middleware in referenced assemblies where the <c>Order</c> interface
/// property is not visible to the source generator at compile time.
/// </summary>
/// <remarks>
/// The source generator reads this attribute from compiled metadata, making
/// it the correct cross-assembly mechanism. The <c>Order</c> interface property
/// is only readable from same-assembly syntax trees and should not be relied on
/// for middleware in NuGet packages or referenced projects.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class OrderAttribute(int order) : Attribute
{
    /// <summary>Gets the execution order value. Lower numbers execute first.</summary>
    public int Order { get; } = order;
}