namespace CmsApi.DTOs.Meeting
{
    public class MeetingStatisticDto
    {

        public long? TotalMeetings { get; set; }
        public long? CompletedMeetings { get; set; }
        public long? InProgressMeetings { get; set; }
        public long? NewMeetingsThisMonth { get; set; }

        public List<MeetingYearDto>? Years { get; set; }
        
    }


    public class MeetingYearDto
    {
        public int Year { get; set; }
        public int TotalCount { get; set; }
        public List<MeetingMonthDto>? Months { get; set; }
    }

    public class MeetingMonthDto
    {

        public string Month { get; set; }
        public int Count { get; set; }
    }
    
}
