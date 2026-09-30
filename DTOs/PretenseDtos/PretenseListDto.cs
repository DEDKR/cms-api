namespace CmsApi.DTOs.PretenseDtos
{
    public class PretenseListDto
    {
        public long Id { get; set; }
        public string? ApplicationHeader { get; set; }
        public string? IdView { get; set; }
        public DateTime? InsertedDate { get; set; }
        public int? ProjectKind { get; set; }
        public int? ArchiveStatus { get; set; }
        public int? PartySignActionType { get; set; }
        public string? PretenseType { get; set; }
        public string? ContractType { get; set; }
        public string? ContractNo { get; set; }
        public DateTime? SendDate { get; set; }
        public int? ExecutionDateLimit { get; set; }
        public string? BrokenRuleInfo { get; set; }
        public string? LegalName { get; set; }
        public string? IsPretenseViewedStatus { get; set; }
        public string? StateName { get; set; }
    }
}
