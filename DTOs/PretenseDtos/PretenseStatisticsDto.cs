namespace CmsApi.DTOs.PretenseDtos
{
    public class PretenseStatisticDto
    {

        public long? TotalPretenses { get; set; }
        public long? CompletedPretenses { get; set; }
        public long? InProgressPretenses { get; set; }
        public long? NewPretensesThisMonth { get; set; }

        public List<PretenseYearDto>? Years { get; set; }

    }


    public class PretenseYearDto
    {
        public int Year { get; set; }
        public int TotalCount { get; set; }
        public List<PretenseMonthDto>? Months { get; set; }
    }

    public class PretenseMonthDto
    {

        public string Month { get; set; }
        public int Count { get; set; }
    }
}
