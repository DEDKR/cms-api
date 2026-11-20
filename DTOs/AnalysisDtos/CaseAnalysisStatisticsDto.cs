namespace CmsApi.DTOs.AnalysisDtos;

public sealed class CaseAnalysisStatisticsDto
{
    public int TotalCases{ get; set; }
    public int TotalAnalyzed { get; set; }
    public int TotalAnalyzeHasResult { get; set; }
    public int BothResult { get; set; }
    public int CaseSubjectIsNotFindings { get; set; }
    public int CaseOfficeIsNotFindings { get; set; }
    public int TotalErrorCount { get; set; }
    public List<CaseAnalysisWarningStatisticDto> Warnings { get; set; } = new();
}

public sealed class CaseAnalysisWarningStatisticDto
{
    public int WarningMessageId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int WarningCount { get; set; }
}
