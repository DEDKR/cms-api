using CmsApi.Entities;

namespace CmsApi.Repositories.Interfaces
{
    public interface IECmsTokenRepository
    {
        public Task<int> UpsertAsync(Token token);

        public Task<Token> GetTokenAsync();
    }
}
