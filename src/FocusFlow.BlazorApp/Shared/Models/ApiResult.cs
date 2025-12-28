namespace FocusFlow.BlazorApp.Shared.Models;

public record ApiResult(bool Succeeded, string? Error = null)
{
    public static ApiResult Success() => new(true);
    public static ApiResult Failure(string error) => new(false, error);
}

public record ApiResult<T>(bool Succeeded, T? Data = default, string? Error = null) : ApiResult(Succeeded, Error)
{
    public static ApiResult<T> Success(T data) => new(true, data);
    public static new ApiResult<T> Failure(string error) => new(false, default, error);
}
