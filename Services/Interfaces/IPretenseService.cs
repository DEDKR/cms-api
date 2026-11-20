using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.PretenseDtos;

namespace CmsApi.Services.Interfaces
{
    public interface IPretenseService
    {
        Task<PagedResult<PretenseListDto>> GetPretenseListAsync(PretenseListRequestDto payload);
        Task<PretenseDetailsDto> GetPretenseDetailsAsync(long pretenseId);
    }
}
