using CmsApi.DTOs.Dashboard;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;

namespace CmsApi.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly ICaseRepository _caseRepository;

        public DashboardService(ICaseRepository caseRepository)
        {
            _caseRepository = caseRepository;
        }

        public async Task<List<CaseStatusStatisticDto>> GetCaseStatusStatisticsAsync(string? officeKeyCode = null)
        {
            var res = await _caseRepository.GetCaseStatusStatisticsAsync(officeKeyCode);
            return res;
        }

        public async Task<List<CourtLevelStatisticDto>> GetCourtLevelCaseStatisticsAsync(CourtLevelStatisticRequestDto request, string? officeKeyCode = null)
        {
            var res = await _caseRepository.GetCourtLevelCaseStatisticsAsync(request, officeKeyCode);

            return res;

        }

        public async Task<List<GetCaseStatisticsByCourtDto>> GetCaseStatisticsByCourtAsync(GetCaseStatisticsByCourtRequestDto request, string? officeKeyCode = null)
        {
            return await _caseRepository.GetCaseStatisticsByCourtAsync(request, officeKeyCode);
        }

        public async Task<List<CaseTotalByYearStatisticDto>> GetCaseTotalByYearStatisticsAsync(CaseTotalByYearStatisticRequestDto caseTotalByYearStatisticRequestDto, string? officeKeyCode = null)
        {
            return await _caseRepository.GetCaseTotalByYearStatisticsAsync(caseTotalByYearStatisticRequestDto, officeKeyCode);
        }
    }
}
