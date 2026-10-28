using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.Dashboard;
using CmsApi.DTOs.Meeting;
using CmsApi.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CmsApi.Repositories.Interfaces
{
    public interface ICaseRepository
    {
        Task<PagedResult<CaseListDto>> GetCasesAsync(
            CaseListRequestDto request,
            string? officeKeyCode);
        Task<bool> CanAccessCaseAsync(long caseId, string? officeKeyCode);
        Task<HashSet<long>> GetAccessibleCaseIdsAsync(IEnumerable<long> caseIds, string officeKeyCode);
        Task<CaseDetailDto?> GetCaseAsync(long caseId);
        Task<List<CaseNotificationListItem>?> GetCaseNewNotificationsAsync(long caseId);

        Task<CaseStatisticDto?> CaseStatisticAsync(string? officeKeyCode = null);
        Task<List<CaseDocuments>>? CaseDocumentsAsync(long reId);
        Task<List<CaseStatusStatisticDto>> GetCaseStatusStatisticsAsync(string? officeKeyCode = null);
        Task<List<CourtLevelStatisticDto>> GetCourtLevelCaseStatisticsAsync(CourtLevelStatisticRequestDto request, string? officeKeyCode = null);

        Task<List<GetCaseStatisticsByCourtDto>> GetCaseStatisticsByCourtAsync(GetCaseStatisticsByCourtRequestDto request, string? officeKeyCode = null);

        Task<List<CaseTotalByYearStatisticDto>> GetCaseTotalByYearStatisticsAsync(CaseTotalByYearStatisticRequestDto  caseTotalByYearStatisticRequestDto, string? officeKeyCode = null);

        Task<CaseOrBaseCaseGetDto> GetCaseOrStarterCaseAsync(long caseId);

        Task<long> InsertCaseDocumentRawContentAsync(InsertCaseDocumentRawContentRequestDto request);

        Task<List<CaseForAnalizeDto>> CaseForAnalizesAsync();

        Task<string?> GetKeyCodeByTextAsync(string text, int groupId);


        Task<long> InsertCaseAnalysisFindingAsync(
    CaseAnalysisFindingDto caseAnalysisFindingDto);

        Task<long> InsertCaseAnalysisResultAsync(
    CaseAnalysisResultDto dto);


        Task UpdateCaseWarnings(long caseId, int messageId);

        Task<long> UpsertCaseAnalysisResultAsync(CaseAnalysisResultRequestDto payload);
    }
}
