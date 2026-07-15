using System.Collections.Generic;

namespace BuildingBlocks.Presentation.Models
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? TraceId { get; set; }
        public bool IsSuccess { get; set; }

        public static ApiResponse<T> Success(T data, string? traceId = null)
        {
            return new ApiResponse<T> { Data = data, IsSuccess = true, TraceId = traceId };
        }

        public static ApiResponse<T> Failure(List<string> errors, string? traceId = null)
        {
            return new ApiResponse<T> { Errors = errors, IsSuccess = false, TraceId = traceId };
        }

        public static ApiResponse<T> Failure(string error, string? traceId = null)
        {
            return new ApiResponse<T> { Errors = new List<string> { error }, IsSuccess = false, TraceId = traceId };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResult(string? traceId = null)
        {
            return new ApiResponse { IsSuccess = true, TraceId = traceId };
        }

        public static new ApiResponse Failure(List<string> errors, string? traceId = null)
        {
            return new ApiResponse { Errors = errors, IsSuccess = false, TraceId = traceId };
        }

        public static new ApiResponse Failure(string error, string? traceId = null)
        {
            return new ApiResponse { Errors = new List<string> { error }, IsSuccess = false, TraceId = traceId };
        }
    }
}
