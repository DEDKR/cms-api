namespace CmsApi.DTOs.CaseDtos
{
    public class CaseDocuments
    {
        public long Id { get; set; }
        public long CaseId { get; set; }
        public string? CaseIds { get; set; }

        public int DocTypeId { get; set; }
        public int OtherDocTypeId { get; set; }
        public string? DocTypeName { get; set; }
        public string? Status { get; set; }
        public DateTime InsertDate { get; set; }
        public CaseDocAttachment? Attachment { get; set; }
        public bool? IsImportant { get; set; }
    }


    public class CaseDocAttachment
    {
        private const string BaseUrl = "https://e-mehkeme.gov.az/signed/ShowPdf";

        public string? Ids { get; set; }
        public string? FileName { get; set; }
        public string? Url { get; set; }

        public CaseDocAttachment()
        {
        }

        public CaseDocAttachment(string? ids, string? fileName)
        {
            Ids = ids;
            FileName = fileName;
            Url = string.IsNullOrWhiteSpace(fileName)
                ? null
                : $"{BaseUrl}?guid={fileName}";
        }
    }


}
