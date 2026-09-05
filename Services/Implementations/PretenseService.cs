using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.PretenseDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;

namespace CmsApi.Services.Implementations
{
    public class PretenseService : IPretenseService
    {
        private readonly IPretenseRepository _pretenseRepository;

        public PretenseService(IPretenseRepository pretenseRepository)
        {
            _pretenseRepository = pretenseRepository;
        }


        public async Task<PagedResult<PretenseListDto?>> GetPretenseListAsync(PretenseListRequestDto request)
        {
            return await _pretenseRepository.GetPretenseListAsync(request);
        }

        public async Task<PretenseDetailsDto> GetPretenseDetailsAsync(long pretenseId)
        {
            return await _pretenseRepository.GetPretenseDetailsAsync(pretenseId);
        }


    }
}
