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

    }
}
