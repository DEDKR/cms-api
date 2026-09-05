using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.PretenseDtos;

namespace CmsApi.Repositories.Interfaces
{
    public interface IPretenseRepository
    {
        Task<PagedResult<PretenseListDto>> GetPretenseListAsync(PretenseListRequestDto payload);
        Task<PretenseDetailsDto> GetPretenseDetailsAsync(long pretenseId);
    }
}
