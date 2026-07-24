namespace CmsApi.DTOs.NotificationDtos
{
    public class NotificationListItemDto
    {
        public long Id { get; set; }
        public string Ids { get; set; }

        public string Content { get; set; }
        public DateTime? InsertDate {  get; set; }
        public int Status  { get; set; }
        public string StatusName  { get; set; }
    }
}
