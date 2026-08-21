using CmsApi.Entities;
using CmsApi.Entities.CmsApi.Entities;

namespace CmsApi.Services.Interfaces
{
    public interface ICmsLoginService
    {
        Task<Token> GetTokenAsync();
        Task<Token> RefreshTokenAsync(Token token);
    }
}
