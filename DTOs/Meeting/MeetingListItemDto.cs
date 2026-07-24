namespace CmsApi.DTOs.Meeting
{
    public class MeetingListItemDto
    {
        public long Id { get; set; }
        public string Ids { get; set; }
        public string CaseNo { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string MeetingType { get; set; }
        public string Court { get; set; }
        public string? Hall { get; set; }
        public string Judge { get; set; }
        public string MeetingStatus { get; set; }
        public string ParticipationRole { get; set; }
    }
}
