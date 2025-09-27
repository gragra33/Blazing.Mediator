namespace Blazing.Mediator.Tests;

/// <summary>
/// Simple test interface for testing generic constraint functionality.
/// Used to verify that middleware with generic constraints work correctly.
/// This is different from the removed constraint validation feature.
/// </summary>
public interface ITestConstraintEntity
{
    int Id { get; set; }
    string Name { get; set; }
}