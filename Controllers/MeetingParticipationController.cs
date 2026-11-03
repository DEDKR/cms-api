using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.AssignmentDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

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
        private readonly IAssignmentRepository _eligibleUsers;
        public MeetingParticipationController(IUserRepository users, IMeetingRepository meetings,
            IMeetingParticipationRepository participation, IAssignmentRepository eligibleUsers)
        {
            _users = users; _meetings = meetings; _participation = participation;
            _eligibleUsers = eligibleUsers;
        }

        private async Task<MeetingParticipationPermission?> GetPermissionAsync()
        {
            var perm = await MeetingParticipationPermission.ResolveAsync(User, _users);
            return perm.Allowed ? perm : null;
        }

        // Resolve the logged-in user's actual roles/permissions on the server.
        // Profile values are display-only and do NOT grant regional access.
        [HttpGet("participation/capabilities")]
        public async Task<IActionResult> Capabilities()
        {
            var permission = await GetPermissionAsync();
            if (permission is null) return Forbid();

            // Read values already present in JWT when available. The stored procedure
            // falls back to current TBL_USERS values for missing/older claims.
            var officeClaim = GetClaimValue("officeKeyCode", "OfficeKeyCode",
                "OFFICE_KEY_CODE", "regionalOfficeId", "REGIONAL_OFFICE_ID");
            var positionClaim = GetClaimInt("positionId", "PositionId", "POSITION_ID", "position_id");
            var departmentClaim = GetClaimInt("departmentId", "DepartmentId", "DEPARTMENT_ID", "department_id");

            var profile = await _participation.GetCapabilityLabelsAsync(
                permission.UserId, officeClaim, positionClaim, departmentClaim);

            return Ok(ApiResponse<object>.Ok(new
            {
                permission.CanManageOthers,
                permission.CanSeeAll,
                profile.OfficeKeyCode,
                profile.Position,
                profile.Department,
                permission.Role,
                permission.UserId
            }));
        }

        private string? GetClaimValue(params string[] names)
        {
            foreach (var name in names)
            {
                var value = User.FindFirst(name)?.Value;
                if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            }
            return null;
        }

        private int? GetClaimInt(params string[] names) =>
            int.TryParse(GetClaimValue(names), out var value) && value > 0 ? value : null;

        [HttpGet("participation/eligible-users")]
        public async Task<IActionResult> EligibleUsers([FromQuery] string? search = null)
        {
            var permission = await GetPermissionAsync();
            if (permission is null) return Forbid();
            // Participant sees only themselves; managers see all eligible users in scope.
            var users = await _eligibleUsers.GetEligibleUsersAsync(permission.OfficeKeyCode, search,
                permission.CanManageOthers ? null : permission.UserId);
            if (!permission.CanManageOthers)
                users = users.Where(x => x.UserId == permission.UserId).ToList();
            return Ok(ApiResponse<object>.Ok(users));
        }

        [HttpGet("{meetingId:long}/participation")]
        public async Task<IActionResult> Get(long meetingId)
        {
            var permission = await GetPermissionAsync();
            if (permission is null) return Forbid();
            if (!await _meetings.CanAccessMeetingAsync(meetingId, permission.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("İclas tapılmadı.", 404));
            return Ok(ApiResponse<object>.Ok(await _participation.GetAsync(
                meetingId, permission.UserId, permission.OfficeKeyCode, permission.CanManageOthers)));
        }

        [HttpPut("{meetingId:long}/participation")]
        public Task<IActionResult> Update(long meetingId, [FromBody] UpdateMeetingParticipationDto input) =>
            Save(new[] { meetingId }, new[] { input?.UserId ?? 0 },
                input?.Status ?? 0, input?.Note, true);

        [HttpPost("participation/bulk")]
        public Task<IActionResult> Bulk([FromBody] SetMeetingParticipationsRequestDto input) =>
            Save(input?.MeetIds, input?.UserIds, input?.Status ?? 0, input?.Note, false);

        private async Task<IActionResult> Save(IReadOnlyCollection<long>? meetIds,
            IReadOnlyCollection<long>? targetIds, int status, string? note, bool defaultToSelf)
        {
            var permission = await GetPermissionAsync();
            if (permission is null) return Forbid();
            var meetings = meetIds?.Distinct().ToArray() ?? Array.Empty<long>();
            var users = targetIds?.Distinct().ToArray() ?? Array.Empty<long>();
            if (defaultToSelf && users.Length == 1 && users[0] == 0)
                users[0] = permission.UserId;
            var normalizedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (status is not (1 or 2) || (status == 2 && normalizedNote is null) ||
                normalizedNote?.Length > 1500 || meetings.Length == 0 || users.Length == 0 ||
                meetings.Any(x => x <= 0) || users.Any(x => x <= 0) ||
                meetings.Length > 200 || users.Length != 1)
                return BadRequest(ApiResponse<object>.Fail(
                    "Hər sorğuda yalnız bir istifadəçi seçilə bilər. İştirak etmədikdə qeyd məcburidir."));

            if (!permission.CanManageOthers && users.Any(x => x != permission.UserId))
                return Forbid();
            try
            {
                var result = await _participation.SetAsync(meetings, users,
                    permission.UserId, permission.OfficeKeyCode, permission.CanManageOthers,
                    status, normalizedNote);
                return Ok(ApiResponse<object>.Ok(result, "İştirak məlumatı saxlanıldı."));
            }
            catch (SqlException ex) when (ex.Number == 51001)
            {
                return NotFound(ApiResponse<object>.Fail("İclas tapılmadı və ya bu region üzrə icazə yoxdur.", 404));
            }
            catch (SqlException ex) when (ex.Number == 51002)
            {
                return BadRequest(ApiResponse<object>.Fail("İstifadəçi tapılmadı və ya icazəniz yoxdur."));
            }
            catch (SqlException ex) when (ex.Number == 51003)
            {
                return BadRequest(ApiResponse<object>.Fail("İştirak statusu və ya qeyd düzgün deyil."));
            }
            catch (SqlException ex) when (ex.Number is 51004 or 2601 or 2627)
            {
                return Conflict(ApiResponse<object>.Fail(
                    "Bu iclas üzrə artıq başqa istifadəçinin iştirakı qeydə alınıb. İştirakçını dəyişmək olmaz.", 409));
            }
        }
    }
}
