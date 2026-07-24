namespace CmsApi.DTOs.HttpApiDtos
{
    public sealed class ResponseExceptionDto
    {
        public string? ExceptionMessage { get; set; }
        public string? Details { get; set; }
        public object? ValidationErrors { get; set; }
    }
}
