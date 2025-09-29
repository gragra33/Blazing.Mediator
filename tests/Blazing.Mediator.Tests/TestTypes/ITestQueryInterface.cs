namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Interface that should be excluded from analysis.
/// </summary>
public interface ITestQueryInterface : IQuery<string>
{
    string Data { get; set; }
}