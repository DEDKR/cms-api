using System.Threading.Tasks;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.Meeting;
using CmsApi.Repositories.Interfaces;
using CmsApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // POST /api/Meeting/list: RO is resolved on the server; the client must not send it.
        [HttpPost("list")]
        public async Task<IActionResult> GetList([FromBody] MeetingRequestDto meetingRequestDto)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus,
                    ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied", scope.ErrorStatus));

            var result = await _meetingRepository.GetMeetingsAsync(
                meetingRequestDto, scope.OfficeKeyCode);
            return Ok(result); // Keep the existing unwrapped response for this endpoint.
        }

        // Meetings for /cases/:id; the client cannot forge a CASE_NO or RO.
        [HttpGet("by-case/{caseId:long}")]
        public async Task<IActionResult> GetMeetingsByCase(long caseId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus,
                    ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied", scope.ErrorStatus));
            // Distinguish neither unknown nor foreign-region case.
            // The repository checks both source case and each matched meeting.
            var items = await _meetingRepository.GetByCaseIdAsync(caseId, scope.OfficeKeyCode);
            return Ok(ApiResponse<object>.Ok(items));
        }

        // GET /api/Meeting/detail?meetingId=...: protect against direct-ID access.
        [HttpGet("detail")]
        public async Task<IActionResult> GetCaseDetail([FromQuery] long meetingId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus,
                    ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied", scope.ErrorStatus));

            // Distinguish neither non-existent nor another RO's meeting: both are 404.
            // The SQL check enforces meeting.CASE_ID -> case regional ownership.
            if (!await _meetingRepository.CanAccessMeetingAsync(meetingId, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Meeting not found", 404));

            var result = await _meetingRepository.MeetingDetail(meetingId);
            if (result?.MeetingView is null)
                return NotFound(ApiResponse<MeetingDetailDto>.Fail("Meeting not found", 404));

            return Ok(ApiResponse<MeetingDetailDto>.Ok(result));
        }

        // GET /api/Meeting/statistics: region must apply BEFORE SQL aggregation.
        [HttpGet("statistics")]
        public async Task<IActionResult> GetMeetingStatistics()
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus,
                    ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied", scope.ErrorStatus));

            var result = await _meetingRepository.MeetingStatisticAsync(scope.OfficeKeyCode);
            return Ok(ApiResponse<MeetingStatisticDto>.Ok(result));
        }
    }
}
