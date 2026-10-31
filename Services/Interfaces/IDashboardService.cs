using CmsApi.DTOs.Dashboard;

namespace CmsApi.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<List<CaseStatusStatisticDto>> GetCaseStatusStatisticsAsync(string? officeKeyCode = null);

        Task<List<CourtLevelStatisticDto>> GetCourtLevelCaseStatisticsAsync(CourtLevelStatisticRequestDto request, string? officeKeyCode = null);

        Task<List<GetCaseStatisticsByCourtDto>> GetCaseStatisticsByCourtAsync(GetCaseStatisticsByCourtRequestDto request, string? officeKeyCode = null);

        Task<List<CaseTotalByYearStatisticDto>> GetCaseTotalByYearStatisticsAsync(CaseTotalByYearStatisticRequestDto caseTotalByYearStatisticRequestDto, string? officeKeyCode = null);
    }
}
