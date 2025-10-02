namespace AnalyzerExample.Common.Domain;

/// <summary>
/// Generic operation result with data
/// </summary>
/// <typeparam name="T">Type of data returned</typeparam>
public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }

    public static OperationResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public new static OperationResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}