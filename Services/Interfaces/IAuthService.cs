using CmsApi.DTOs.AuthDtos;

namespace CmsApi.Services.Interfaces
{

    namespace CmsApi.Services.Interfaces
    {

        public interface IAuthService
        {
            Task<LoginResponse> LoginAsync(
                LoginRequest request);

            Task<RefreshTokenResponse> RefreshAsync(

                RefreshTokenRequest request);

            Task LogoutAsync(
                int userId);

            Task ChangePasswordAsync(
                int userId,
                ChangePasswordRequest request);
        }
    }
}
