using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.AuthDtos;
using CmsApi.Services.Interfaces;
using CmsApi.Services.Interfaces.CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// İstifadəçini sistemə daxil edir və access + refresh token qaytarır.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Login attempt started for username: {Username}",
                    request.Username);

                var response =
                    await _authService.LoginAsync(request);

                _logger.LogInformation(
                    "Login successful for username: {Username}",
                    request.Username);

                return Ok(
                    ApiResponse<object>.Ok(
                        response,
                        "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                // Password və username haqqında ətraflı məlumat
                // log-a da yazmırıq.
                _logger.LogWarning(
                    ex,
                    "Login failed for username: {Username}",
                    request.Username);

                return Unauthorized(
                    ApiResponse<object>.Fail(
                        ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred during login");

                return StatusCode(
                    500,
                    ApiResponse<object>.Fail(
                        "Internal server error"));
            }
        }

        /// <summary>
        /// Cari istifadəçini sistemdən çıxarır və
        /// refresh token-i deaktiv edir.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(
                        ApiResponse<object>.Fail(
                            "Invalid user identity"));
                }

                _logger.LogInformation(
                    "Logout attempt started for userId: {UserId}",
                    userId);

                await _authService.LogoutAsync(userId);

                _logger.LogInformation(
                    "Logout successful for userId: {UserId}",
                    userId);

                return Ok(
                    ApiResponse<object>.Ok(
                        null,
                        "Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred during logout");

                return StatusCode(
                    500,
                    ApiResponse<object>.Fail(
                        "Internal server error"));
            }
        }

        /// <summary>
        /// Refresh token vasitəsilə yeni access və refresh token yaradır.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    request.RefreshToken))
                {
                    return BadRequest(
                        ApiResponse<object>.Fail(
                            "Refresh token is required"));
                }

                _logger.LogInformation(
                    "Refresh token attempt started");

                var response =
                    await _authService.RefreshAsync(request);

                _logger.LogInformation(
                    "Refresh token successful");

                return Ok(
                    ApiResponse<object>.Ok(
                        response,
                        "Token refreshed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Refresh token failed");

                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Invalid refresh token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred during token refresh");

                return StatusCode(
                    500,
                    ApiResponse<object>.Fail(
                        "Internal server error"));
            }
        }

        /// <summary>
        /// İstifadəçinin password-unu dəyişir.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userIdClaim =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(
                        ApiResponse<object>.Fail(
                            "Invalid user identity"));
                }

                await _authService.ChangePasswordAsync(
                    userId,
                    request);

                return Ok(
                    ApiResponse<object>.Ok(
                        null,
                        "Password changed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Password change failed for userId: {UserId}",
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier));

                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Current password is incorrect"));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred during password change");

                return StatusCode(
                    500,
                    ApiResponse<object>.Fail(
                        "Internal server error"));
            }
        }
    }
}