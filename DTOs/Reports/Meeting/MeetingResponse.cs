namespace CmsApi.DTOs.Reports.Meeting
{
    public class MeetingResponse
    {
        public string CourtName { get; set; } = string.Empty;
        public string RegionalOfficeName { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
