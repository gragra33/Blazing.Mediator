namespace Blazing.Mediator.Tests.Middleware;

/// <summary>
/// Default implementation of middleware execution tracker.
/// </summary>
public class MiddlewareExecutionTracker : IMiddlewareExecutionTracker
{
    private readonly List<string> _executedMiddleware = new();

    /// <inheritdoc />
    public void RecordExecution(Type middlewareType)
    {
        var name = middlewareType.Name;
        // Clean up generic type names
        if (name.Contains('`'))
        {
            name = name.Substring(0, name.IndexOf('`'));
        }
        _executedMiddleware.Add(name);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetExecutedMiddleware()
    {
        return _executedMiddleware.AsReadOnly();
    }

    /// <inheritdoc />
    public void Clear()
    {
        _executedMiddleware.Clear();
    }
}