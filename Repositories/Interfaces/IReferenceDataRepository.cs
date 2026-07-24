using CmsApi.DTOs;
using CmsApi.Enums;

namespace CmsApi.Repositories.Interfaces
{
    public interface IReferenceDataRepository
    {
        Task<List<ReferenceData>> GetDataAsync(
            ReferenceDataType referenceDataQueryType,
            int? courtId = null,
            string? parametr = null);
    }
}
