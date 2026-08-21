using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.Meeting;
using CmsApi.Repositories.Implementations;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class MeetingController : ControllerBase
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IUserRepository _userRepository;


        public MeetingController(IMeetingRepository meetingRepository, IUserRepository userRepository)
        {
            _meetingRepository = meetingRepository;
            _userRepository = userRepository;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList([FromBody] MeetingRequestDto meetingRequestDto)
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
            var result = await _meetingRepository.GetMeetingsAsync(meetingRequestDto);

            return Ok(result);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetCaseDetail([FromQuery] long meetingId)
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
            var result = await _meetingRepository.MeetingDetail(meetingId);

            if (result?.MeetingView is null)
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        "Case not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<MeetingDetailDto>.Ok(result));

        }
    }
}
