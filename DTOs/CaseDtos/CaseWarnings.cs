namespace CmsApi.DTOs.CaseDtos
{
    public class CaseWarnings
    {
        public long Id { get; set; }

        public long CaseId { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TypeId { get; set; }

        public string Type { get; set; } = string.Empty;

        public bool IsResolved { get; set; }

        public DateTime? ResolvedDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
