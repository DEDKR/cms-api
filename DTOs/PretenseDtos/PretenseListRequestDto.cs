namespace CmsApi.DTOs.PretenseDtos
{
    public class PretenseListRequestDto
    {
        public string? IdView { get; set; }
        public string? Voen { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? StateId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
