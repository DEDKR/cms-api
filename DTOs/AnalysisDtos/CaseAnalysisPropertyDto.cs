namespace CmsApi.DTOs.AnalysisDtos
{
    public class CaseAnalysisPropertyDto
    {
        public long Id { get; set; }
        public long CarId { get; set; }
        public long CaseId { get; set; }

        public AnalysisFieldDto<string>? PropertyType { get; set; }
        public AnalysisFieldDto<string>? PropertyAddress { get; set; }
        public AnalysisFieldDto<decimal?>? PropertyArea { get; set; }
        public AnalysisFieldDto<string>? AreaUnit { get; set; }
        public AnalysisFieldDto<string>? RegistryNumber { get; set; }
        public AnalysisFieldDto<string>? RegistrationNumber { get; set; }
        public AnalysisFieldDto<string>? CadastralNumber { get; set; }
        public AnalysisFieldDto<string>? Description { get; set; }
    }
}
