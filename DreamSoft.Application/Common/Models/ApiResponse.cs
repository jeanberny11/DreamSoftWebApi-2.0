namespace DreamSoft.Application.Common.Models;

/// <summary>
/// Base response for all API operations
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
}

/// <summary>
/// Generic API response with data
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailureResult(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default
        };
    }
}