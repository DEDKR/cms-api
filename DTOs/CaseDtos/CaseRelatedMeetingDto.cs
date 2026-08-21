namespace CmsApi.DTOs.CaseDtos
{
    public class CaseRelatedMeetingDto
    {
        public string Court { get; set; }
        public string Judge { get; set; }
        public string MeetingType { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string Status { get; set; }
    }
}
