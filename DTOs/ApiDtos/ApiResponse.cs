namespace CmsApi.DTOs.ApiDtos
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<Error>? Errors { get; set; }

        public int? Status { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(
          string message,
          int status = StatusCodes.Status400BadRequest,
          List<Error>? errors = null)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = message,
                    Errors = errors,
                    Status = status
                };
            }

        public class Error
        {
            public string Key { get; set; }
            public string Message { get; set; }
        }
    }
}
