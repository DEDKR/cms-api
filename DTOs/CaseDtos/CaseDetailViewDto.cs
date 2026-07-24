namespace CmsApi.DTOs.CaseDtos
{
    public class CaseDetailViewDto
    {
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
    }
}
