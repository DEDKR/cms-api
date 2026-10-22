using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.AssignmentDtos;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepository;

        public AssignmentController(
            IAssignmentRepository assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }

        [HttpPost("ro-to-user")]
        public async Task<IActionResult> SetROToUser(
            [FromBody] SetROToUserRequestDto request)
        {
            var updated =
                await _assignmentRepository.SetROToUserAsync(
                    request.UserId,
                    request.RoId);

            if (!updated)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "İstifadəçi tapılmadı.",
                        StatusCodes.Status404NotFound));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new
                    {
                        request.UserId,
                        request.RoId
                    },
                    "Ərazi idarəsi uğurla yeniləndi."));
        }

        [HttpPost("meet-to-user")]
        public async Task<IActionResult> SetMeetToUser(
            [FromBody] SetMeetToUserRequestDto request)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) ||
                !long.TryParse(userIdClaim, out var attendedBy))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "User not authenticated",
                        StatusCodes.Status401Unauthorized));
            }

            var assigned =
                await _assignmentRepository.SetMeetToUserAsync(
                    request.UserId,
                    request.MeetId,
                    attendedBy);

            if (!assigned)
            {
                return BadRequest(
                    ApiResponse<object>.Fail(
                        "İclas və ya istifadəçi tapılmadı."));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    new
                    {
                        request.UserId,
                        request.MeetId
                    },
                    "İclas uğurla təyin olundu."));
        }
    }
}
