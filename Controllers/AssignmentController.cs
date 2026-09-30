using CmsApi.DTOs.ApiDtos;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepository;

        public AssignmentController(IAssignmentRepository assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }

        [HttpPost("ro-to-user")]
        public async Task<IActionResult> SetROToUser([Required] long userId, [Required] string roId)
        {
            var result = await _assignmentRepository.SetROToUserAsync(userId, roId);
            return Ok(result);
        }

        [HttpPost("meet-to-user")]
        public async Task<IActionResult> SetMeetToUser([Required] long userId, [Required] long meetId)
        {
            var userIdClaim =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) ||
                !long.TryParse(userIdClaim, out var attendedBy))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "User not authenticated"));
            }

            var result = await _assignmentRepository.SetMeetToUserAsync(userId, meetId, attendedBy);
            return Ok(result);
        }



    }
}
