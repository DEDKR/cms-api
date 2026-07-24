namespace CmsApi.DTOs.HttpApiDtos
{
    public class CmsApiResponse<T>
    {
        public string Version { get; set; } = default!;
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = default!;
        public T? Result { get; set; }
        public ResponseExceptionDto? ResponseException { get; set; }
    }
}
