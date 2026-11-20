using CmsApi.DTOs.AnalysisDtos;

namespace CmsApi.Repositories.Interfaces;

public interface ICaseAnalysisStatisticsRepository
{
    Task<CaseAnalysisStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
