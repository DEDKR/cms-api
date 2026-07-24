namespace CmsApi.DTOs.NotificationDtos
{
    public class NoitifcationRequestDto
    {
        public int? CourtId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CaseNo { get; set; }
        public int? Status { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
