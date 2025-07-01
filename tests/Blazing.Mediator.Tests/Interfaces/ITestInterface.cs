namespace Blazing.Mediator.Tests;

/// <summary>
/// Interface used for testing that interface types are skipped during registration.
/// </summary>
public interface ITestInterface : IRequestHandler<TestCommand>
{
    // Interface should be skipped during registration
}