namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Test types specifically for MediatorStatistics analysis testing.
/// These types are designed to be discovered by the analysis methods.
/// </summary>
public static class StatisticsTestTypesDescription
{
    // This class serves as documentation for the test types below
}

#region Query Test Types

/// <summary>
/// Simple query test type implementing IQuery&lt;T&gt;.
/// </summary>
public class SimpleTestQuery : IQuery<string>
{
    public string SearchTerm { get; set; } = string.Empty;
}

/// <summary>
/// Query test type implementing IRequest&lt;T&gt; with Query in name.
/// </summary>
public class RequestBasedTestQuery : IRequest<List<string>>
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}

/// <summary>
/// Generic query test type.
/// </summary>
public class GenericTestQuery<T> : IQuery<T>
{
    public string Filter { get; set; } = string.Empty;
}

/// <summary>
/// Complex query with multiple generic parameters.
/// </summary>
public class ComplexGenericQuery<TInput, TOutput> : IQuery<TOutput>
{
    public TInput? Input { get; set; }
}

#endregion

#region Command Test Types

/// <summary>
/// Simple command test type implementing ICommand.
/// </summary>
public class SimpleTestCommand : ICommand
{
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Command test type implementing ICommand&lt;T&gt;.
/// </summary>
public class ReturningTestCommand : ICommand<int>
{
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Command test type implementing IRequest with Command in name.
/// </summary>
public class RequestBasedTestCommand : IRequest
{
    public bool Flag { get; set; }
}

/// <summary>
/// Command test type implementing IRequest&lt;T&gt; with Command in name.
/// </summary>
public class RequestReturningTestCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Generic command test type.
/// </summary>
public class GenericTestCommand<T> : ICommand<T>
{
    public T? Data { get; set; }
}

/// <summary>
/// Command with type constraints.
/// </summary>
public class ConstrainedTestCommand<T> : ICommand<bool> where T : class, new()
{
    public T? Entity { get; set; }
}

#endregion

#region Edge Case Test Types

/// <summary>
/// Type that implements both query and command interfaces (edge case).
/// </summary>
public class HybridTestType : IQuery<string>, ICommand<int>
{
    public string QueryData { get; set; } = string.Empty;
    public int CommandData { get; set; }
}

/// <summary>
/// Type with confusing name that doesn't implement mediator interfaces.
/// </summary>
public class FakeQueryCommand
{
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Abstract query type that should be excluded from analysis.
/// </summary>
public abstract class AbstractTestQuery : IQuery<string>
{
    public abstract string GetData();
}

/// <summary>
/// Interface that should be excluded from analysis.
/// </summary>
public interface ITestQueryInterface : IQuery<string>
{
    string Data { get; set; }
}

#endregion

#region Request Types Without Clear Query/Command Naming

/// <summary>
/// Request type without clear Query/Command naming pattern.
/// </summary>
public class AmbiguousRequest : IRequest<string>
{
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Void request type without clear naming pattern.
/// </summary>
public class AmbiguousVoidRequest : IRequest
{
    public int Value { get; set; }
}

#endregion

#region Handlers for Test Types

/// <summary>
/// Handler for SimpleTestQuery.
/// </summary>
public class SimpleTestQueryHandler : IRequestHandler<SimpleTestQuery, string>
{
    public Task<string> Handle(SimpleTestQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Result for: {request.SearchTerm}");
    }
}

/// <summary>
/// Handler for SimpleTestCommand.
/// </summary>
public class SimpleTestCommandHandler : IRequestHandler<SimpleTestCommand>
{
    public Task Handle(SimpleTestCommand request, CancellationToken cancellationToken)
    {
        // Simulate command execution
        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for ReturningTestCommand.
/// </summary>
public class ReturningTestCommandHandler : IRequestHandler<ReturningTestCommand, int>
{
    public Task<int> Handle(ReturningTestCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Value.Length);
    }
}

#endregion