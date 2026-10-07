using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.UserDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordPolicyService _passwordPolicyService;

        public UserController(
            IUserRepository userRepository,
            IPasswordPolicyService passwordPolicyService)
        {
            _userRepository = userRepository;
            _passwordPolicyService = passwordPolicyService;
        }

        [HttpPost]
        public async Task<IActionResult> GetUsers([FromBody] UserRequestDto request)
        {
            var users = await _userRepository.GetUsers(request);
            return Ok(ApiResponse<UserResponseDto>.Ok(users));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetUserRoles()
        {
            var roles = await _userRepository.GetUserRoles();
            return Ok(ApiResponse<List<UserRolesResponse>>.Ok(roles));
        }

        [HttpGet("admin-dashboard")]
        public async Task<IActionResult> GetUserAdminDashboard()
        {
            var dashboard = await _userRepository.GetUserAdminDashboard();
            return Ok(ApiResponse<UserAdminDashboardResponseDto>.Ok(dashboard));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            // Content(1)-dəki parol qaydaları:
            // min 8, böyük hərf, kiçik hərf, rəqəm, xüsusi simvol.
            var passwordPolicyError =
                _passwordPolicyService.Validate(
                    request.InitialPassword,
                    passwordRequired: true);

            if (passwordPolicyError is not null)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        passwordPolicyError));
            }

            var userId = await _userRepository.CreateUserAsync(request);

            if (userId == -1)
            {
                return Conflict(
                    ApiResponse<object>.Fail(
                        "Bu istifadəçi adı artıq mövcuddur.",
                        StatusCodes.Status409Conflict));
            }

            if (userId == -2)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "Seçilmiş rol mövcud deyil."));
            }

            if (userId <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "İstifadəçi yaradıla bilmədi."));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new { UserId = userId },
                    "İstifadəçi uğurla yaradıldı."));
        }

        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser(
            int userId,
            [FromBody] UpdateUserRequestDto request)
        {
            var updated = await _userRepository.UpdateUserAsync(userId, request);

            if (!updated)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "İstifadəçi tapılmadı.",
                        StatusCodes.Status404NotFound));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new { UserId = userId },
                    "İstifadəçi uğurla yeniləndi."));
        }

        [HttpPut("{userId:int}/active")]
        public async Task<IActionResult> SetUserActive(
            int userId,
            [FromBody] SetUserActiveRequestDto request)
        {
            var updated =
                await _userRepository.SetUserActiveAsync(
                    userId,
                    request.IsActive);

            if (!updated)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "İstifadəçi tapılmadı.",
                        StatusCodes.Status404NotFound));
            }

            var message = request.IsActive
                ? "İstifadəçi aktiv edildi."
                : "İstifadəçi deaktiv edildi.";

            return Ok(
                ApiResponse<object>.Ok(
                    new { UserId = userId, IsActive = request.IsActive },
                    message));
        }

        [HttpPost("{userId:int}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(int userId)
        {
            var updated =
                await _userRepository.ResetUserPasswordAsync(userId);

            if (!updated)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "İstifadəçi tapılmadı.",
                        StatusCodes.Status404NotFound));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new { UserId = userId },
                    "Parol sıfırlandı."));
        }

        // Fiziki DELETE etmirik. Sil əməliyyatı IS_ACTIVE = 0 edir.
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var updated =
                await _userRepository.SoftDeleteUserAsync(userId);

            if (!updated)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "İstifadəçi tapılmadı.",
                        StatusCodes.Status404NotFound));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new { UserId = userId },
                    "İstifadəçi deaktiv edildi."));
        }
    }
}
