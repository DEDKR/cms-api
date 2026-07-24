namespace CmsApi.DTOs.Meeting
{
    public class MeetingDetailDto
    {
        public MeetingDetailViewDto MeetingView { get; set; }

        public List<MeetingJudgesDto>? Judges { get; set; }

        public List<MeetingPartyDto>? Parties { get; set; }  

        public List<MeetingRelatedMeetingDto>? RelatedMeetingDtos { get; set; }
    }
}
