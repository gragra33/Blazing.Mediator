using Blazing.Mediator;

namespace Blazing.Mediator.Examples.Streams;

/// <summary>
/// Example streaming request using Blazing.Mediator.
/// This demonstrates streaming responses that return IAsyncEnumerable.
/// Compare with MediatR version: uses Blazing.Mediator.IStreamRequest&lt;T&gt; instead of MediatR.IStreamRequest&lt;T&gt;.
/// </summary>
public class Sing : IStreamRequest<Song>
{
    /// <summary>
    /// Gets or sets the message to sing.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
