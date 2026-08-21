namespace CmsApi.DTOs.NotificationDtos
{
    public class NotificationDetailDto
    {
        public long Id { get; set; }    
        public string Ids {  get; set; }
        public long CaseId {  get; set; }
        public string CaseNo {  get; set; }
        public string Court {  get; set; }
        public DateTime EnterDate {  get; set; }
        public DateTime? ReadDate {  get; set; }
        public string? Content {  get; set; }
        public string? Status {  get; set; }

        public List<RelatedNotificationDto>? RelatedNotifications { get; set; }= [];
    }


    public class RelatedNotificationDto
    {
        public long Id { get; set; }
        public long CaseId { get; set; }
        public string CaseNo { get; set; }
        public DateTime InsertDate { get; set; }
        public int Status { get; set; }

    }
}
