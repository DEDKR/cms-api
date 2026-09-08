namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintListRequestDto
    {
        public string? CaseNo { get; set; }

        public int? CourtId { get; set; }
        public int? ApplicationTypeId { get; set; }
        public string? IdView { get; set; }
        public string? Voen { get; set; }
        public string? DocNumber { get; set; }
        public string? PhysicalName { get; set; }
        public string? PhysicalSurname { get; set; }
        public string? PhysicalLastName { get; set; }

        public int? StateId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

    }
}
