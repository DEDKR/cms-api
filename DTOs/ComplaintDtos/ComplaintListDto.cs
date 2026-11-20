namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintListDto
    {
        public long Id { get; set; }
        public string? IdView { get; set; }
        public string? CaseNo { get; set; }
        public string? CourtName { get; set; }
        public string? ApplicationName { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? SendDate { get; set; }
        public string? StateName { get; set; }
    }
}
