namespace ECommerce.Api.Application.DTOs;

public class OperationResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T Data { get; set; } = default(T)!;
    public List<string> Errors { get; set; } = [];

    public static OperationResult<T> SuccessResult(T data, string message = "")
    {
        return new OperationResult<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static OperationResult<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new OperationResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? []
        };
    }
}