namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Abstract query type that should be excluded from analysis.
/// </summary>
public abstract class AbstractTestQuery : IQuery<string>
{
    public abstract string GetData();
}