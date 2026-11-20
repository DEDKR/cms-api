namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintDecisionInfoDto
    {
        public long? Id { get; set; }
        public string? Court { get; set; }
        public string? DocNo { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? Name { get; set; }
    }
}
