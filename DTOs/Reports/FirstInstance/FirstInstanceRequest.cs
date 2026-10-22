namespace CmsApi.DTOs.Reports.FirstInstance
{
    public class FirstInstanceRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? OfficeKeyCode { get; set; }
        public bool ByCourt { get; set; }
    }
}
