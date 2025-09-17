namespace OpenTelemetryExample.Exceptions;

/// <summary>
/// Custom exception for not found scenarios.
/// </summary>
public sealed class NotFoundException(string message) : Exception(message);