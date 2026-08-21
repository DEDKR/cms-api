namespace CmsApi.DTOs.CaseDtos
{
    public class CaseDetailViewDto
    {
        public long CaseId { get; set; }
        public string? Ids { get; set; }

        public string? CaseNo { get; set; }

        public string? CaseType { get; set; }

        public string? ExecType { get; set; }

        public string? CaseStatus { get; set; }

        public string? Court { get; set; }
        public int? CourtTypeId { get; set; }

        public string? Judge { get; set; }

        public DateTime? EnterDate { get; set; }

        public int? Year { get; set; }

        public int? CategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        public int? CourtLevelId { get; set; }
        public string? CourtLevelName { get; set; }

        public string? TerritorialOffice { get; set; }

        public string? CaseSubject { get; set; }

        public string? Result { get; set; }
    }
}
