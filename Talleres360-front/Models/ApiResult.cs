using Talleres360.Dtos.Responses;

namespace Talleres360_front.Models;

public class ApiResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiErrorResponse? Error { get; init; }
    public int StatusCode { get; init; }

    public bool IsUnauthorized    => StatusCode == 401;
    public bool IsSubscriptionError => StatusCode == 402;
    public bool IsForbidden       => StatusCode == 403;

    public static ApiResult<T> Ok(T data) =>
        new() { Success = true, Data = data, StatusCode = 200 };

    public static ApiResult<T> Fail(int statusCode, ApiErrorResponse? error = null) =>
        new() { Success = false, StatusCode = statusCode, Error = error };
}
