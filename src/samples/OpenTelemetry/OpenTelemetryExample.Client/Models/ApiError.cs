namespace OpenTelemetryExample.Client.Models;

public sealed class ApiError
{
    public string Message { get; set; } = string.Empty;
    public List<ValidationError> Errors { get; set; } = new();
}