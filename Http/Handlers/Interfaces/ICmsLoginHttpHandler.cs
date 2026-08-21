using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CmsLoginDtos;
using CmsApi.DTOs.HttpApiDtos;
using CmsApi.Entities.CmsApi.Entities;

namespace CmsApi.Http.Handlers.Interfaces
{
    public interface ICmsLoginHttpHandler
    {
        Task<CmsApiResponse<LoginResultDto>> LoginByAsanCertificateAsync(LoginRequestBody loginRequestBody);
        Task<CmsApiResponse<LoginResultDto>> RefreshTokenAsync(RefreshRequestDto refreshRequestDto, Token token);

    }
}
