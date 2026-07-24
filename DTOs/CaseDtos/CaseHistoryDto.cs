namespace CmsApi.DTOs.CaseDtos
{
    public class CaseHistoryDto
    {
        public long CaseId {  get; set; }
        public string CaseIds {  get; set; }
        public string CaseNo { get; set; }
        public string? Court { get; set; }
        public string? Judge { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public DateTime? ResultDate { get; set; }
        public DateTime? DecisionDate { get; set; }
        public DateTime? EnterDate { get; set; }
    }
}
