using CmsApi.DTOs.CaseDtos;

namespace CmsApi.DTOs.AnalysisDtos
{
    public class CaseAnalysisDetailDto
    {
        public long CarId { get; set; }
        public long CaseId { get; set; }

        public AnalysisFieldDto<string>? Office { get; set; }

        public List<CasePartyDto> Parties { get; set; } = new();

        public List<CaseAnalysisPropertyDto> Properties { get; set; } = new();

        public List<CaseAnalysisPropertyProblemDto> PropertyProblems { get; set; } = new();

        public List<CaseAnalysisClaimDto> Claims { get; set; } = new();

        public string? AnalyzeBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
