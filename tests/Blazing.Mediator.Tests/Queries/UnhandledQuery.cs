using Blazing.Mediator;

/// <summary>
/// Unhandled query for testing error scenarios.
/// </summary>
public record UnhandledQuery : IRequest<string>;