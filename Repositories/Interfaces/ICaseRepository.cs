using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.Dashboard;
using CmsApi.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CmsApi.Repositories.Interfaces
{
    public interface ICaseRepository
    {
        Task<PagedResult<CaseListDto>> GetCasesAsync(CaseListRequestDto request);
        Task<CaseDetailDto?> GetCaseAsync(long caseId);
        Task<List<CaseNotificationListItem>?> GetCaseNewNotificationsAsync(long caseId);

        Task<CaseStatisticDto?> CaseStatisticAsync();
        Task<List<CaseDocuments>>? CaseDocumentsAsync(long reId);
        Task<List<CaseStatusStatisticDto>> GetCaseStatusStatisticsAsync();
        Task<List<CourtLevelStatisticDto>> GetCourtLevelCaseStatisticsAsync(CourtLevelStatisticRequestDto request);

        Task<List<GetCaseStatisticsByCourtDto>> GetCaseStatisticsByCourtAsync(GetCaseStatisticsByCourtRequestDto request);

        Task<List<CaseTotalByYearStatisticDto>> GetCaseTotalByYearStatisticsAsync(CaseTotalByYearStatisticRequestDto  caseTotalByYearStatisticRequestDto);

        Task<CaseOrBaseCaseGetDto> GetCaseOrStarterCaseAsync(long caseId);

        Task<long> InsertCaseDocumentRawContentAsync(
    InsertCaseDocumentRawContentRequestDto request);

        Task<List<CaseForAnalizeDto>> CaseForAnalizesAsync();

        Task<string?> GetKeyCodeByTextAsync(string text, int groupId);


        Task<long> InsertCaseAnalysisFindingAsync(
    CaseAnalysisFindingDto caseAnalysisFindingDto);

        Task<long> InsertCaseAnalysisResultAsync(
    CaseAnalysisResultDto dto);


        Task UpdateCaseWarnings(long caseId, int messageId);
    }
}
