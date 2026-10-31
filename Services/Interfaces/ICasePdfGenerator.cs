using CmsApi.DTOs.CaseDtos;

namespace CmsApi.Services.Interfaces
{
    public interface ICasePdfGenerator
    {
        byte[] Generate(CaseDetailDto caseDetail);
    }
}
