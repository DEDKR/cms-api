namespace CmsApi.DTOs.Reports.Appeals
{
    public class AppealResponse
    {
        public string CourtName { get; set; } = string.Empty;
        public string RegionalOfficeName { get; set; } = string.Empty;
        public string RegionalOfficeKeyCode { get; set; } = string.Empty;
        public int? CourtId { get; set; }
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int PartiallyApproved { get; set; }
    }
}
