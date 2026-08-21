namespace CmsApi.DTOs.CaseDtos
{
    public class CaseForAnalizeDto
    {
        public long Id { get; set; }
        public string CaseNo { get; set; } = string.Empty;
        public int CourtLevelId {  get; set; }

        public bool? CompletedCaseHasRequiredDocuments { get; set; }
        public bool CaseHasAnalysisDocuments { get; set; }

    }
}
