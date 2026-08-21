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

        public async Task<List<CaseStatusStatisticDto>> GetCaseStatusStatisticsAsync()
        {
            var res = await _caseRepository.GetCaseStatusStatisticsAsync();
            return res;
        }

        public async Task<List<CourtLevelStatisticDto>> GetCourtLevelCaseStatisticsAsync(CourtLevelStatisticRequestDto request)
        {
            var res = await _caseRepository.GetCourtLevelCaseStatisticsAsync(request);

            return res;

        }

        public async Task<List<GetCaseStatisticsByCourtDto>> GetCaseStatisticsByCourtAsync(GetCaseStatisticsByCourtRequestDto request)
        {
            return await _caseRepository.GetCaseStatisticsByCourtAsync(request);
        }

        public async Task<List<CaseTotalByYearStatisticDto>> GetCaseTotalByYearStatisticsAsync(CaseTotalByYearStatisticRequestDto caseTotalByYearStatisticRequestDto )
        {
            return await _caseRepository.GetCaseTotalByYearStatisticsAsync(caseTotalByYearStatisticRequestDto);
        }
    }
}
