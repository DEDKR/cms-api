using CmsApi.DTOs.CaseDtos;

namespace CmsApi.Services.Interfaces
{
    public interface ICaseService 
    {
        Task AnalizeCase(long caseId);
        Task<long> UpsertCaseAnalysisResultAsync(CaseAnalysisResultRequestDto payload);
    }
}
