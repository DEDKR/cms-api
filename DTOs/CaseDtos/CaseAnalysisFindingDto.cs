namespace CmsApi.DTOs.CaseDtos
{
    public class CaseAnalysisFindingDto
    {
        public long CaseId { get; set; }
        public int WarningMessageId { get; set; }
        public int Type { get; set; }
        public bool IsResolved { get; set; }
    }
}
