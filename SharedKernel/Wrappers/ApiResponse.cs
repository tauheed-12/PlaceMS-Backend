using System.Net;

namespace SharedKernel.Wrappers;

/// <summary>
/// Standard API response envelope used by every endpoint across all services.
/// All controllers return this — never raw objects.
/// 
/// Success: { success: true, data: {...}, message: "...", errors: [] }
/// Failure: { success: false, data: null, message: "...", errors: ["err1", "err2"] }
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private ApiResponse() { }

    public static ApiResponse<T> Ok(T data, string message = "Request successful")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Created(T data, string message = "Resource created successfully")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> NoContent(string message = "Request successful, no content to return")
        => new() { Success = true, Data = default, Message = message };

    public static ApiResponse<T> Updated(T data, string message = "Resource updated successfully")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };

    public static ApiResponse<T> Fail(string message, string error)
        => new() { Success = false, Message = message, Errors = new() { error } };
}

/// <summary>
/// Non-generic version for responses that return no data body.
/// Used for delete operations, status updates, etc.
/// </summary>
public class ApiResponse
{
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private ApiResponse() { }

    public static ApiResponse Ok(string message = "Request successful")
        => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };

    public static ApiResponse Fail(string message, string error)
        => new() { Success = false, Message = message, Errors = new() { error } };
}