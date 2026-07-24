using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.NotificationDtos;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpGet("detail")]
        public async Task<IActionResult> GetNotificationDetail([FromQuery] long notificationId)
        {
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
            var newNotificationsCount = await _notificationService.GetNewNotificationsCount();
            return Ok(newNotificationsCount);
        }


        [HttpPost("read-notify")]

        public async Task<IActionResult> ReadAsync(long notificationId)
        {
            await _notificationService.MakeReadNotification(notificationId);

            return Ok(ApiResponse<object>.Ok("Ugurlu"));
        }

    }
}
