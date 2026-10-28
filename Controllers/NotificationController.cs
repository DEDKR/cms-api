using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.NotificationDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using CmsApi.Security;
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
        private readonly ICaseRepository _caseRepository;
        private readonly INotificationRepository _notificationRepository;


        public NotificationController(INotificationService notificationService, IUserRepository userRepository, ICaseRepository caseRepository, INotificationRepository notificationRepository)
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _caseRepository = caseRepository;
            _notificationRepository = notificationRepository;
        }


        [HttpGet("detail")]
        public async Task<IActionResult> GetNotificationDetail([FromQuery] long notificationId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            // Check access before loading a notification's content.
            var notificationCaseId = await _notificationRepository.GetNotificationCaseIdAsync(notificationId);
            if (!notificationCaseId.HasValue ||
                !await _caseRepository.CanAccessCaseAsync(notificationCaseId.Value, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Notification not found"));

            var result = await _notificationService.GetNotificationDetailDtoAsync(notificationId);

            if (result is null )
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        " notificaton not found",
                        StatusCodes.Status404NotFound));
            }

            if (scope.OfficeKeyCode is not null && result.RelatedNotifications is not null)
            {
                // The detail SP may return other related cases: hide those not in the user's RO.
                var allowedIds = await _caseRepository.GetAccessibleCaseIdsAsync(
                    result.RelatedNotifications.Select(x => x.CaseId), scope.OfficeKeyCode);
                result.RelatedNotifications = result.RelatedNotifications
                    .Where(x => allowedIds.Contains(x.CaseId)).ToList();
            }

            return Ok(ApiResponse<NotificationDetailDto>.Ok(result));

        }


        [HttpPost("list")]
        public async Task<IActionResult> GetNotifications([FromBody] NoitifcationRequestDto noitifcationRequestDto)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            var result = await _notificationService.GetNotifications(noitifcationRequestDto, scope.OfficeKeyCode);

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

            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            var newNotificationsCount = await _notificationService.GetNewNotificationsCount(scope.OfficeKeyCode);
            return Ok(newNotificationsCount);
        }


        [HttpPost("read-notify")]
        public async Task<IActionResult> ReadAsync(long notificationId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            var readCaseId = await _notificationRepository.GetNotificationCaseIdAsync(notificationId);
            if (!readCaseId.HasValue ||
                !await _caseRepository.CanAccessCaseAsync(readCaseId.Value, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Notification not found"));

            await _notificationService.MakeReadNotification(notificationId);

            return Ok(ApiResponse<object>.Ok("Ugurlu"));
        }

        [HttpPost("statistics")]
        public async Task<IActionResult> GetNotificationStatistics(NotificationStatisticRequestDto notificationStatisticRequestDto)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            var result = await _notificationService.GetNotificationStatistics(notificationStatisticRequestDto, scope.OfficeKeyCode);

            return Ok(ApiResponse<object>.Ok(result));
        }

    }
}
