namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintStatisticDto
    {

        public long? TotalComplaints { get; set; }
        public long? CompletedComplaints { get; set; }
        public long? InProgressComplaints { get; set; }
        public long? NewComplaintsThisMonth { get; set; }

        public List<ComplaintYearDto>? Years { get; set; }

    }


    public class ComplaintYearDto
    {
        public int Year { get; set; }
        public int TotalCount { get; set; }
        public List<ComplaintMonthDto>? Months { get; set; }
    }

    public class ComplaintMonthDto
    {

        public string Month { get; set; }
        public int Count { get; set; }
    }

}
