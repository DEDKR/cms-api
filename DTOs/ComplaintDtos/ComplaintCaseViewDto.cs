namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintCaseViewDto
    {
        public long? Id { get; set; }
        public string? ApplicationHeader { get; set; }
        public string? IdView { get; set; }
        public string? CaseNo { get; set; }
        public string? CourtName { get; set; }
        public string? DocNo { get; set; }
        public string? DecisionTypeName { get; set; }
        public DateTime? DecisionDate { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? SendDate { get; set; }
        public string? StateName { get; set; }
    }
}
