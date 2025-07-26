namespace Blazing.Mediator.Examples.Streams;

/// <summary>
/// Response object for streaming Song results.
/// This demonstrates a simple response object for streaming scenarios.
/// This class is identical to the MediatR version - no changes needed for DTOs.
/// </summary>
public class Song
{
    /// <summary>
    /// Gets or sets the song message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
