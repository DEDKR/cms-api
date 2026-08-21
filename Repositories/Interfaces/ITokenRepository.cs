using CmsApi.Entities;

namespace CmsApi.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        Task UpsertRefreshTokenAsync(
            int userId,
            string tokenHash,
            DateTime expiresAt);

        Task<UserToken?> GetRefreshTokenAsync(
            string tokenHash);

        Task RevokeRefreshTokenAsync(int userId);
    }
}
