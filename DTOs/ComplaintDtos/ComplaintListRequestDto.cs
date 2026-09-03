namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintListRequestDto
    {
        public string? CaseNo { get; set; }

        public int? CourtId { get; set; }
        public int? ApplicationTypeId { get; set; }

        public int? CaseStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

    }
}
