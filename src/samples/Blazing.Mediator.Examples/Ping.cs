using Blazing.Mediator;

namespace Blazing.Mediator.Examples;

/// <summary>
/// Example request that expects a response.
/// This demonstrates a basic command/query with a return value using Blazing.Mediator.
/// Compare with MediatR version: uses Blazing.Mediator.IRequest&lt;T&gt; instead of MediatR.IRequest&lt;T&gt;.
/// </summary>
public class Ping : IRequest<Pong>
{
    /// <summary>
    /// Gets or sets the message to ping with.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
