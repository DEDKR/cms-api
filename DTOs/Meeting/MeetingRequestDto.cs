namespace CmsApi.DTOs.Meeting
{
    public class MeetingRequestDto
    {
        public int? CourtId { get; set; }
        public int? Status { get; set; }

        public List<int>? CaseTypes { get; set; }
        public List<int>? MeetingTypes { get; set; }
        public List<int>? Judges { get; set; }

        public DateTime? FirstDate { get; set; }
        public DateTime? LastDate { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

    }
}
