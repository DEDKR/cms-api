using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.NotificationDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;


        public NotificationController(INotificationService notificationService, IUserRepository userRepository)
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
        }


        [HttpGet("detail")]
        public async Task<IActionResult> GetNotificationDetail([FromQuery] long notificationId)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) ||
                !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "User not authenticated"));
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "User not found"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }
            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }
            var result = await _notificationService.GetNotificationDetailDtoAsync(notificationId);

            if (result is null )
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        " notificaton not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<NotificationDetailDto>.Ok(result));

        }


        [HttpPost("list")]
        public async Task<IActionResult> GetNotifications([FromBody] NoitifcationRequestDto noitifcationRequestDto)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) ||
                !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "User not authenticated"));
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "User not found"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }
            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }
            var result = await _notificationService.GetNotifications(noitifcationRequestDto);

            if (result is null)
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        "notificaton not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<PagedResult<NotificationListItemDto>>.Ok(result));

        }

        [HttpGet("new-count")]

        public async Task<IActionResult> GetNewCount()
        {

            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) ||
                !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "User not authenticated"));
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "User not found"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }
            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }
            var newNotificationsCount = await _notificationService.GetNewNotificationsCount();
            return Ok(newNotificationsCount);
        }


        [HttpPost("read-notify")]
        public async Task<IActionResult> ReadAsync(long notificationId)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) ||
                !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "User not authenticated"));
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "User not found"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }
            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }
            await _notificationService.MakeReadNotification(notificationId);

            return Ok(ApiResponse<object>.Ok("Ugurlu"));
        }

        [HttpPost("statistics")]
        public async Task<IActionResult> GetNotificationStatistics(NotificationStatisticRequestDto notificationStatisticRequestDto)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) ||
                !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "User not authenticated"));
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "User not found"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }
            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }
            var result = await _notificationService.GetNotificationStatistics(notificationStatisticRequestDto);

            return Ok(ApiResponse<object>.Ok(result));
        }

    }
}
