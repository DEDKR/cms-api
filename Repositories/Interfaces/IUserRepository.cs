using CmsApi.DTOs.UserDtos;
using CmsApi.Entities;

namespace CmsApi.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int userId);

        Task<bool> UpdatePasswordAsync(
            int userId,
            string passHash,
            string passOrg,
            bool isPassChangeRequired);

        Task<bool> ResetLoginAttemptsAsync(int userId);

        Task<bool> RegisterFailedLoginAsync(
            int userId,
            int maxAttempts,
            int lockoutMinutes);

        Task<UserResponseDto> GetUsers(UserRequestDto request);
        Task<List<UserRolesResponse>> GetUserRoles();
        Task<UserAdminDashboardResponseDto> GetUserAdminDashboard();

        Task<int> CreateUserAsync(CreateUserRequestDto request);
        Task<bool> UpdateUserAsync(int userId, UpdateUserRequestDto request);
        Task<bool> SetUserActiveAsync(int userId, bool isActive);
        Task<bool> ResetUserPasswordAsync(int userId);
        Task<bool> SoftDeleteUserAsync(int userId);
    }
}
