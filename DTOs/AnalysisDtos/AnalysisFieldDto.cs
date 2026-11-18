namespace CmsApi.DTOs.AnalysisDtos
{
    public class AnalysisFieldDto<T>
    {
        public T? Value { get; set; }
        public string? Context { get; set; }
        public string? DocumentName { get; set; }
        public long? DocumentId { get; set; }
    }
}
