namespace CmsApi.DTOs.Reports.Meeting
{
    public class MeetingRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? OfficeKeyCode { get; set; }
        public bool ByCourt { get; set; }
        public int? CourtTypeId { get; set; }
    }
}
