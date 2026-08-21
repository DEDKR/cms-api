namespace CmsApi.DTOs.CaseDtos
{
    public class CaseStatisticDto
    {

        public long? TotalCases { get; set; }
        public long? CompletedCases { get; set; }
        public long? InProgressCases { get; set; }
        public long? NewCasesThisMonth { get; set; }

        public List<YearDto>? Years { get; set; }
        
    }


    public class YearDto
    {
        public int Year { get; set; }
        public int TotalCount { get; set; }
        public List<MonthDto>? Months { get; set; }
    }

    public class MonthDto
    {

        public string Month { get; set; }
        public int Count { get; set; }
    }
    
}
