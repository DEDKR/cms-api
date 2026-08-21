using CmsApi.Common;
using CmsApi.DTOs.CmsLoginDtos;
using CmsApi.Entities;
using CmsApi.Entities.CmsApi.Entities;
using CmsApi.Http.Handlers.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CmsApi.Services.Implementations
{
    public class CmsLoginService : ICmsLoginService
    {
        private readonly ICmsLoginHttpHandler _cmsHttpHandler;
        private readonly VerificatedAsanCertificates _verificatedAsanCertificates;

        public CmsLoginService(ICmsLoginHttpHandler cmsHttpHandler,
           IOptions<VerificatedAsanCertificates> verificatedAsanCertificatesOptions)
        {
            _cmsHttpHandler = cmsHttpHandler;
            _verificatedAsanCertificates = verificatedAsanCertificatesOptions.Value;
        }

        public async Task<Token> GetTokenAsync()
        {
            var loginBody = new LoginRequestBody
            {
                DedkrCertDetail = _verificatedAsanCertificates.DedkrCertDetail
            };
            var tokenFromApi = await _cmsHttpHandler.LoginByAsanCertificateAsync(loginBody);
            var result = tokenFromApi.Result;
            var token = new Token
            {
                Id = result.Id,
                UserId = result.UserId,
                AccessToken = result.Token,
                RefreshToken = result.RefreshToken,
                AccessTokenExpire = result.Expire,
                RefreshTokenExpire = result.RefreshTokenExpire,
            };

            return token;
        }

        public async Task<Token> RefreshTokenAsync(Token token)
        {
            var refreshTokenRequestDto = new RefreshRequestDto
            {
                RefreshToken = token.RefreshToken,
                SignType = 1
            };
            Token tokenee = null;
            var tokenFromApi = await _cmsHttpHandler.RefreshTokenAsync(refreshTokenRequestDto, token);

            if (tokenFromApi.IsSuccess == false)
            {
                tokenee = await GetTokenAsync();
                return tokenee;
            }

            var result = tokenFromApi.Result;

            var tokene = new Token
            {
                Id = result.Id,
                UserId = result.UserId,
                AccessToken = result.Token,
                RefreshToken = result.RefreshToken,
                AccessTokenExpire = result.Expire,
                RefreshTokenExpire = result.RefreshTokenExpire,
            };

            return tokene;
        }
    }
}
