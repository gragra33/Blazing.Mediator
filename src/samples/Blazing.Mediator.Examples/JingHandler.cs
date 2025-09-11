namespace Blazing.Mediator.Examples;

/// <summary>
/// Handler for the Jing void request using Blazing.Mediator.
/// This demonstrates a command handler that doesn't return a value.
/// Compare with MediatR version: uses Blazing.Mediator.IRequestHandler&lt;T&gt; instead of MediatR.IRequestHandler&lt;T&gt;.
/// </summary>
public class JingHandler : IRequestHandler<Jing>
{
    private readonly TextWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="JingHandler"/> class.
    /// </summary>
    /// <param name="writer">The text writer for output.</param>
    public JingHandler(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Handles the Jing command.
    /// </summary>
    /// <param name="request">The jing request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(Jing request, CancellationToken cancellationToken)
    {
        await _writer.WriteLineAsync($"--- Handled Jing: {request.Message}");
    }
}
