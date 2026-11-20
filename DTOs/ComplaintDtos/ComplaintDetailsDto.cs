namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintDetailsDto
    {
        public ComplaintCaseViewDto? CaseView { get; set; }
        public List<ComplaintPartiesDto?> Parties { get; set; } = [];
        public List<ComplaintDocumentsDto?> Documents { get; set; } = [];
        public ComplaintDecisionInfoDto? DecisionInfo { get; set; } = new();
        public ComplaintFreePaidReasonDto? FreePaidReason { get; set; } = new();
        public ComplaintApplicationContextDto? ApplicationContext { get; set; } = new();
        public List<ComplaintSignerDto?> Signers { get; set; } = [];
        public List<ComplaintLegalNorms?> LegalNorms { get; set; } = [];
        public List<ComplaintApplicationDetails?> ApplicationDetails { get; set; } = [];
    }
}
