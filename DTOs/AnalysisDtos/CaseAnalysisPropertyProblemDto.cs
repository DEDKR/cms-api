namespace CmsApi.DTOs.AnalysisDtos
{
    public class CaseAnalysisPropertyProblemDto
    {
        public long Id { get; set; }
        public long CarId { get; set; }
        public long CaseId { get; set; }

        public AnalysisFieldDto<string>? PropertyProblem { get; set; }
    }
}
