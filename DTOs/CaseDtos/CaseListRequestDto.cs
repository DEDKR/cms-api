namespace CmsApi.DTOs.CaseDtos
{
    public class CaseListRequestDto
    {
        public string? CaseNo { get; set; }

        public int? CaseTypeId { get; set; }

        public int? CourtId { get; set; }

        public int? JudgeId { get; set; }

        public int? CaseStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
