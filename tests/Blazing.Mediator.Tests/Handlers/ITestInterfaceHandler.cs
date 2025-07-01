namespace Blazing.Mediator.Tests;

/// <summary>
/// Test interface handler for testing interface registration scenarios.
/// Used to verify that interface handlers are not registered in the dependency injection container.
/// </summary>
public interface ITestInterfaceHandler : IRequestHandler<TestInterfaceCommand>
{
}