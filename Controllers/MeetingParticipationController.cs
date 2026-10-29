using System.Security.Claims;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.AssignmentDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [ApiController]
    [Route("api/Meeting")]
    [Authorize]
    public class MeetingParticipationController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly IMeetingRepository _meetings;
        private readonly IMeetingParticipationRepository _participation;
        public MeetingParticipationController(
            IUserRepository users, IMeetingRepository meetings,
            IMeetingParticipationRepository participation)
        {
            _users = users; _meetings = meetings; _participation = participation;
        }

        [HttpGet("{meetingId:long}/participation")]
        public async Task<IActionResult> Get(long meetingId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _users);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus,
                    ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied", scope.ErrorStatus));
            if (!await _meetings.CanAccessMeetingAsync(meetingId, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Meeting not found", 404));
            if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();
            // Non-managers see ONLY their own assignment and notes.
            var canManage = (await MeetingAssignmentPermission.ResolveAsync(User, _users)).CanAssign;
            return Ok(ApiResponse<object>.Ok(await _participation.GetAsync(meetingId, userId, canManage)));
        }

        [HttpPut("{meetingId:long}/participation")]
        public async Task<IActionResult> Update(long meetingId, [FromBody] UpdateMeetingParticipationDto input)
        {
            if (input is null || input.Status is not (1 or 2) ||
                string.IsNullOrWhiteSpace(input.Note) || input.Note.Trim().Length > 1500)
                return BadRequest(ApiResponse<object>.Fail("Status və iştirak barədə qeyd tələb olunur."));
            var scope = await RegionalOfficeScope.ResolveAsync(User, _users);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus,
                    ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied", scope.ErrorStatus));
            if (!await _meetings.CanAccessMeetingAsync(meetingId, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Meeting not found", 404));
            if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();
            var updated = await _participation.UpdateOwnAsync(meetingId, userId, input.Status, input.Note);
            if (!updated)
                return NotFound(ApiResponse<object>.Fail("You are not assigned to this meeting", 404));
            return Ok(ApiResponse<object>.Ok(new { MeetingId = meetingId, UserId = userId }, "İştirak məlumatı saxlanıldı."));
        }
    }
}
