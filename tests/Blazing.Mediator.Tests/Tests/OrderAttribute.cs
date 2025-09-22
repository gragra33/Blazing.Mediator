namespace Blazing.Mediator.Tests;

/// <summary>
/// Simple OrderAttribute for testing.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class OrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}