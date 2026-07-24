namespace CmsApi.DTOs.Meeting
{
    public class MeetingDetailViewDto
    {
        public long Id { get; set; }
        public long CaseId { get; set; }
        public string CaseNo { get; set; }
        public string CaseType { get; set; }
        public string MeetingType { get; set; }
        public string MeetingStatus { get; set; }
        public string PartipationRole { get; set; }
        public string Court { get; set; }
        public string Hall { get; set; }
        public DateTime? MeetingDate { get; set; }
    }
}
