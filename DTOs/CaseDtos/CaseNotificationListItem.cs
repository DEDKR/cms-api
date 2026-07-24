namespace CmsApi.DTOs.CaseDtos
{
    public class CaseNotificationListItem
    {
        public long Id { get; set; }

        public string? CaseNo { get; set; }

        public string? Content { get; set; }

        public string? Court { get; set; }

        public DateTime? InsertDate { get; set; }

        public int? TypeId { get; set; }

        public string? NotificationTypeName { get; set; }

        public string? Color { get; set; }
    }
}
