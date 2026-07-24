using CmsApi.Entities;

namespace CmsApi.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
    }
}
