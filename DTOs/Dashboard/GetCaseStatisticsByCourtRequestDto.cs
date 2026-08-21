namespace CmsApi.DTOs.Dashboard
{
    public class GetCaseStatisticsByCourtRequestDto
    {
        public int? CaseStatusId { get; set; }
        public int? CourtTypeId {  get; set; }
    }
}
