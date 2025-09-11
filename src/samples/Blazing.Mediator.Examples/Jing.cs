namespace Blazing.Mediator.Examples;

/// <summary>
/// Example void request that doesn't return a value.
/// This demonstrates a command pattern using Blazing.Mediator.
/// Compare with MediatR version: uses Blazing.Mediator.IRequest instead of MediatR.IRequest.
/// </summary>
public class Jing : IRequest
{
    /// <summary>
    /// Gets or sets the message for the Jing command.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
