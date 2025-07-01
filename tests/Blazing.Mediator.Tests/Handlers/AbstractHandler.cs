namespace Blazing.Mediator.Tests;

/// <summary>
/// Test types for edge cases
/// </summary>

/// <summary>
/// Abstract handler implementation used for testing that abstract types are skipped during registration.
/// </summary>
public abstract class AbstractHandler : IRequestHandler<TestCommand>
{
    public abstract Task Handle(TestCommand request, CancellationToken cancellationToken = default);
}