using CmsApi.Entities;

namespace CmsApi.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        string HashRefreshToken(string refreshToken);

        DateTime GetAccessTokenExpiration();
        DateTime GetRefreshTokenExpiration();

    }
}
