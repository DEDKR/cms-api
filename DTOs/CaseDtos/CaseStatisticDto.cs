namespace CmsApi.DTOs.CaseDtos
{
    public class CaseStatisticDto
    {

        public long? TotalCases { get; set; }
        public long? CompletedCases { get; set; }
        public long? InProgressCases { get; set; }
        public long? NewCasesThisMonth { get; set; }
    }
}
