namespace CmsApi.DTOs.Reports.Cassasion
{
    public class CassasionResponse
    {
        public string CourtName { get; set; } = string.Empty;
        public string RegionalOfficeName { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int PartiallyApproved { get; set; }
    }
}
