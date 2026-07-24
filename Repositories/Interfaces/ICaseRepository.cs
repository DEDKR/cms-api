using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CmsApi.Repositories.Interfaces
{
    public interface ICaseRepository
    {
        Task<PagedResult<CaseListDto>> GetCasesAsync(CaseListRequestDto request);
        Task<CaseDetailDto?> GetCaseAsync(long caseId);
        Task<List<CaseNotificationListItem>?> GetCaseNewNotificationsAsync(long caseId);

        Task<CaseStatisticDto?> CaseStatisticAsync();
    }
}
