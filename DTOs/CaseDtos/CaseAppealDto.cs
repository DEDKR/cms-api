namespace CmsApi.DTOs.CaseDtos
{
    public class CaseAppealDto
    {

        public int? CaseId { get; set; }

        public string? CaseIds { get; set; }

        public string? Status { get; set; }

        public string? OtherDocumentNumber { get; set; }

        public string? OtherDocumentTypeName { get; set; }

        public DateTime? OtherDocumentEnterDate { get; set; }

        public string? DecisionTypeName { get; set; }

        public string? DecisitonDocumentNumber { get; set; }

        public DateTime? DecisionEnterDate { get; set; }

        public string? SendedOrgan { get; set; }

        public List<string>? AppealParties { get; set; }

    }
}
