using CmsApi.DTOs.Dashboard;

namespace CmsApi.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<CaseStatusStatisticDto>> GetCaseStatusStatisticsAsync();

        Task<List<CourtLevelStatisticDto>> GetCourtLevelCaseStatisticsAsync(CourtLevelStatisticRequestDto request);

        Task<List<GetCaseStatisticsByCourtDto>> GetCaseStatisticsByCourtAsync(GetCaseStatisticsByCourtRequestDto request);

        Task<List<CaseTotalByYearStatisticDto>> GetCaseTotalByYearStatisticsAsync(CaseTotalByYearStatisticRequestDto caseTotalByYearStatisticRequestDto);
    }
}
