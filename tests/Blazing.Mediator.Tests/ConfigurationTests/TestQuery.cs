namespace Blazing.Mediator.Tests.ConfigurationTests;

public record TestQuery(string Value) : IRequest<string>;