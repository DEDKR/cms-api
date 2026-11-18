namespace CmsApi.DTOs.AnalysisDtos
{
    public class CaseAnalysisClaimDto
    {
        public long Id { get; set; }
        public long CarId { get; set; }
        public long CaseId { get; set; }

        public AnalysisFieldDto<string>? ClaimType { get; set; }
        public AnalysisFieldDto<string>? ClaimText { get; set; }
        public AnalysisFieldDto<string>? DecisionResult { get; set; }
        public AnalysisFieldDto<string>? DecisionFavor { get; set; }
        public AnalysisFieldDto<string>? DecisionText { get; set; }
    }
}
