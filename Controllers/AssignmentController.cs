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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IUserRepository _userRepository;

        public AssignmentController(
            IAssignmentRepository assignmentRepository,
            IUserRepository userRepository)
        {
            _assignmentRepository = assignmentRepository;
            _userRepository = userRepository;
        }

        [HttpPost("ro-to-user")]
        public async Task<IActionResult> SetROToUser(
            [FromBody] SetROToUserRequestDto request)
        {
            var permission = await MeetingAssignmentPermission.ResolveAsync(User, _userRepository);
            if (!permission.CanAssign || permission.Role != "Admin")
                return Forbid();
            var updated = await _assignmentRepository.SetROToUserAsync(
                request.UserId, request.RoId);

            if (!updated)
                return NotFound(ApiResponse<object>.Fail(
                    "İstifadəçi tapılmadı.", StatusCodes.Status404NotFound));

            return Ok(ApiResponse<object>.Ok(
                new { request.UserId, request.RoId },
                "Ərazi idarəsi uğurla yeniləndi."));
        }

        // UI permission is obtained from server DB, not from a potentially stale JWT role.
        [HttpGet("capabilities")]
        public async Task<IActionResult> GetCapabilities()
        {
            var permission = await MeetingAssignmentPermission.ResolveAsync(User, _userRepository);
            return Ok(ApiResponse<object>.Ok(new
            {
                permission.CanAssign,
                permission.CanSeeAll,
                permission.OfficeKeyCode,
                permission.Role
            }));
        }

        // GET /api/Assignment/eligible-users?search=...
        // Admin/RO_00: all active users; Kurator: only users of their office.
        [HttpGet("eligible-users")]
        public async Task<IActionResult> EligibleUsers([FromQuery] string? search = null)
        {
            var permission = await MeetingAssignmentPermission.ResolveAsync(User, _userRepository);
            if (!permission.CanAssign) return Forbid();
            var items = await _assignmentRepository.GetEligibleUsersAsync(permission.OfficeKeyCode, search);
            return Ok(ApiResponse<object>.Ok(items));
        }

        // POST /api/Assignment/meet-to-user
        // { "meetIds": [1, 2], "userIds": [3, 4] }
        // Every selected meeting is assigned to every selected user.
        [HttpPost("meet-to-user")]
        public async Task<IActionResult> SetMeetToUser(
            [FromBody] SetMeetToUserRequestDto request)
        {
            if (request is null)
                return BadRequest(ApiResponse<object>.Fail("Sorğu boş ola bilməz."));

            var meetIds = request.MeetIds?.Distinct().ToArray() ?? Array.Empty<long>();
            var userIds = request.UserIds?.Distinct().ToArray() ?? Array.Empty<long>();

            if (meetIds.Length == 0 || userIds.Length == 0 ||
                meetIds.Any(id => id <= 0) || userIds.Any(id => id <= 0))
                return BadRequest(ApiResponse<object>.Fail(
                    "İclas və istifadəçi ID-ləri müsbət və boş olmayan siyahılar olmalıdır."));

            // Avoid accidentally massive Cartesian-product updates.
            if (meetIds.Length > 200 || userIds.Length > 200 ||
                (long)meetIds.Length * userIds.Length > 10000)
                return BadRequest(ApiResponse<object>.Fail(
                    "Limit: hər siyahıda maksimum 200 ID və ümumilikdə 10000 təyinat."));

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdClaim, out var attendedBy))
                return Unauthorized(ApiResponse<object>.Fail(
                    "User not authenticated", StatusCodes.Status401Unauthorized));

            var permission = await MeetingAssignmentPermission.ResolveAsync(User, _userRepository);
            if (!permission.CanAssign) return Forbid();

            try
            {
                // The SQL procedure validates ALL meeting IDs, ALL active users and
                // regional access before writing anything (all-or-nothing).
                var result = await _assignmentRepository.SetMeetsToUsersAsync(
                    meetIds, userIds, attendedBy, permission.OfficeKeyCode, request.AssignmentNote);

                return Ok(ApiResponse<object>.Ok(new
                {
                    MeetIds = meetIds,
                    UserIds = userIds,
                    result.TotalPairs,
                    result.CreatedAssignments,
                    result.UpdatedAssignments
                }, "İclaslar istifadəçilərə uğurla təyin olundu."));
            }
            catch (SqlException ex) when (ex.Number == 51001)
            {
                // Nonexistent and foreign-region meetings deliberately look the same.
                return NotFound(ApiResponse<object>.Fail(
                    "İclas tapılmadı və ya bu region üzrə icazəniz yoxdur.",
                    StatusCodes.Status404NotFound));
            }
            catch (SqlException ex) when (ex.Number == 51002)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "İstifadəçi tapılmadı və ya aktiv deyil."));
            }
            catch (SqlException ex) when (ex.Number == 51003)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "Təyinat üçün göndərilən siyahılar düzgün deyil."));
            }
        }
    }
}
