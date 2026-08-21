namespace CmsApi.DTOs.CaseDtos
{
    public class CaseNotificationDto
    {
        public int Id { get; set; }

        public string? CaseNo { get; set; }

        public string? Content { get; set; }

        public string? Court { get; set; }

        public int Status { get; set; }

        public string? StatusName { get; set; }

        public DateTime InsertDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public int? TypeId { get; set; }

        public string? Result { get; set; }
    }
}
