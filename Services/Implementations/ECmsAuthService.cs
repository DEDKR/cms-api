using CmsApi.Helpers;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;

namespace CmsApi.Services.Implementations
{
    public class ECmsAuthService : IECmsAuthService
    {
        private readonly IECmsTokenRepository _tokenRepository;
        private readonly ICmsLoginService _cmsService;

        public ECmsAuthService(IECmsTokenRepository tokenRepository, ICmsLoginService cmsService)
        {
            _tokenRepository = tokenRepository;

            _cmsService = cmsService;
        }

        public async Task RefreshTokenAsync()
        {

            var tokenFromDb = await _tokenRepository.GetTokenAsync();
            if (tokenFromDb == null)
            {
                var tokenFromApi = await _cmsService.GetTokenAsync();

                if (tokenFromApi != null)
                {
                    await _tokenRepository.UpsertAsync(tokenFromApi);
                    TokenCache.Set(tokenFromApi);
                    return;
                }
            }
            if (tokenFromDb.RefreshTokenExpire < DateTime.UtcNow)
            {
                var tokenFromApi = await _cmsService.GetTokenAsync();

                if (tokenFromApi != null)
                {
                    await _tokenRepository.UpsertAsync(tokenFromApi);
                    TokenCache.Set(tokenFromApi);
                    return;

                }
            }
            if (tokenFromDb.AccessTokenExpire < DateTime.UtcNow)
            {
                var resfreshToken = await _cmsService.RefreshTokenAsync(tokenFromDb);
                await _tokenRepository.UpsertAsync(resfreshToken);
                TokenCache.Set(resfreshToken);
                return;

            }

            TokenCache.Set(tokenFromDb);

        }
    }
}
