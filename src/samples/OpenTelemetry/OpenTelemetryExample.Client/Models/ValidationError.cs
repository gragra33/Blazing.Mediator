namespace OpenTelemetryExample.Client.Models;

public sealed class ValidationError
{
    public string Property { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}