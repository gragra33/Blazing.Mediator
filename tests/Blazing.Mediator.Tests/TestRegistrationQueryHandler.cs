namespace Blazing.Mediator.Tests;

public class TestRegistrationQueryHandler : IRequestHandler<TestRegistrationQuery, string>
{
    public Task<string> Handle(TestRegistrationQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult("test");
}