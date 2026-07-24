namespace CmsApi.DTOs.CaseDtos
{
    public class CaseListDto
    {
        public long Id { get; set; }

        public string? CaseNo { get; set; }

        public string? Type { get; set; }

        public string? CourtName { get; set; }

        public string? JudgeName { get; set; }

        public string? CaseStatus { get; set; }

        public DateTime? EnterDate { get; set; }

        public int? CategoryId { get; set; }

        public int? SubCategoryId { get; set; }

        public int? Year { get; set; }

        public string? Result { get; set; }

        public bool HasNewNotification {  get; set; }

        public int? CourtLevelId { get; set; }
        public string? CourtLevelName { get; set; }

    }
}
