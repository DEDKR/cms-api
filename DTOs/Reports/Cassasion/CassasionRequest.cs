namespace CmsApi.DTOs.Reports.Cassasion
{
    public class CassasionRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? OfficeKeyCode { get; set; }
        public bool ByCourt { get; set; }
        public int? CourtTypeId { get; set; }
    }
}
