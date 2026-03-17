namespace Talleres360.Dtos.Responses
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }

        public static ServiceResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public static ServiceResult<T> Fail(string errorCode, string message) =>
            new() { Success = false, ErrorCode = errorCode, Message = message };
    }
}