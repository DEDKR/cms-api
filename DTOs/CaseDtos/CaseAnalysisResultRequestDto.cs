namespace CmsApi.DTOs.CaseDtos
{
    public class CaseAnalysisResultRequestDto
    {
        public long? CaseId { get; set; }
        public string? OfficeKeyCode { get; set; }
        public string? CaseSubjectKeyCode { get; set; }
        public int? ExecuterUserId { get; set; }
    }
}
