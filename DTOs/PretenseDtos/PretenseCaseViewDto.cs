namespace CmsApi.DTOs.PretenseDtos
{
    public class PretenseCaseViewDto
    {
        public long Id { get; set; }
        public string? IdView { get; set; }
        public string? PretenseType { get; set; }
        public string? ContractType { get; set; }
        public string? ContractNo { get; set; }
        public DateTime? ContractDate { get; set; }
        public string? BrokenRuleInfo { get; set; }
        public int? ExecutionDateLimit { get; set; }
        public string? IsPretenseViewedStatus { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? SendDate { get; set; }
        public string? StateName { get; set; }
    }
}
