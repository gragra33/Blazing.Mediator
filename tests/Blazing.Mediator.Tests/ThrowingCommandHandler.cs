namespace Blazing.Mediator.Tests;

public class ThrowingCommandHandler : IRequestHandler<ThrowingCommand>
{
    public Task Handle(ThrowingCommand request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Handler threw an exception");
    }
}