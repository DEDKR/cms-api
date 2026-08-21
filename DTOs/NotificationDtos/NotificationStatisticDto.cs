namespace CmsApi.DTOs.NotificationDtos
{
    public class NotificationStatisticDto
    {
        public int Total { get; set; }

        public int ReadCount { get; set; }

        public int UnreadCount { get; set; }

        public DateTime? LastInsertDate { get; set; }

        public int InsertDateCount { get; set; }
    }
}
