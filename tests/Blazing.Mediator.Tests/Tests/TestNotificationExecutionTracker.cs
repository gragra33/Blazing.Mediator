namespace Blazing.Mediator.Tests;

/// <summary>
/// Static class to track execution order in tests.
/// </summary>
public static class TestNotificationExecutionTracker
{
    public static List<string> ExecutionOrder { get; } = [];

    public static void Reset()
    {
        ExecutionOrder.Clear();
    }
}