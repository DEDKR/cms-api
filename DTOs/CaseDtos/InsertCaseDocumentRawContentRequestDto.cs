namespace CmsApi.DTOs.CaseDtos
{
    public class InsertCaseDocumentRawContentRequestDto
    {
        public string CaseNo { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int ContentType { get; set; }
        public int? CourtLevelId { get; set; }
        public long? DocumentId { get; set; }
    }
}
