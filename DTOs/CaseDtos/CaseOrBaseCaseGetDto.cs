namespace CmsApi.DTOs.CaseDtos
{
    public class CaseOrBaseCaseGetDto
    {
        public long ? Id { get; set; }  
        public string? CaseNo { get; set; }
        public int? CourtLevelId { get; set; }
        public string? SearchedCaseNo { get; set; }
        public int? SearchedCourtLevelId { get; set; }
    }
}
