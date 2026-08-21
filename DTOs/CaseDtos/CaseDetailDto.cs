namespace CmsApi.DTOs.CaseDtos
{
    public class CaseDetailDto
    {
        public CaseDetailViewDto CaseView { get; set; }
        public List<CaseJudgeDto>? Judges { get; set;  }
        public List<CasePartyDto>? Parties { get; set; }
        public List<CaseRelatedMeetingDto>? RelatedMeetingDtos { get; set; }
        public List<CaseDocuments>? Documents { get; set; }
        public List<CaseAppealDto>? Appeals { get; set; }
        public List<CaseHistoryDto>? CaseHistories { get; set; }
        public List<CaseListDto>? RelatedCases { get; set; }
        public List<CaseNotificationDto>? Notifications { get; set; }
        public List<CaseWarnings>? Warnings { get; set; }
        public List<CaseCode>? CaseCodes { get; set; }
    }
}
