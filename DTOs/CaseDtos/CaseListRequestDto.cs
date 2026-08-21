namespace CmsApi.DTOs.CaseDtos
{
    public class CaseListRequestDto
    {
        public string? CaseNo { get; set; }

        public int? CourtId { get; set; }

        public List<int>? JudgeIds { get; set; }
        public List<int>? CaseTypeIds { get; set; }

        public int? CaseStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int AnalizeStatus {  get; set; }
        public bool? OnlyWarningsCases { get; set; }
    }
}
