namespace AnalyzerExample.Common.Domain;

/// <summary>
/// Base class for operation results
/// </summary>
public class OperationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static OperationResult Success() => new() { IsSuccess = true };
    public static OperationResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}