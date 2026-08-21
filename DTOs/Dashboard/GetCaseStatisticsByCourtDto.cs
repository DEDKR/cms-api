namespace CmsApi.DTOs.Dashboard
{
    public class GetCaseStatisticsByCourtDto
    {
        public int CourtId { get; set; }

        public string? CourtName { get; set; }

        public int CaseCount { get; set; }
        public int CaseInProgressCount { get; set; }
        public int CaseStoppedCount { get; set; }
        public int CaseComletedCount { get; set; }
    }
}
