using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CaseController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ICaseRepository _caseRepository;
        private readonly ICaseService _caseService;
        private readonly IUserRepository _userRepository;

        public CaseController(ICaseRepository caseRepository, IDocumentService documentService, ICaseService caseService, IUserRepository userRepository)
        {
            _caseRepository = caseRepository;
            _documentService = documentService;
            _caseService = caseService;
            _userRepository = userRepository;
        }

        [HttpPost("list")]
        [Authorize]
        public async Task<IActionResult> GetCases([FromBody] CaseListRequestDto request)
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

            if (user.LockoutUntil.HasValue &&
                user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }

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

            var result =
                await _caseRepository.GetCasesAsync(request);

            return Ok(
                ApiResponse<object>.Ok(result));
        }


        [HttpGet("statistics")]
        public async Task<IActionResult> GetCaseStatistics()
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

            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }


            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }

            var result = await _caseRepository.CaseStatisticAsync();

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetCaseDetail([FromQuery] long caseId)
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

            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }

            var result = await _caseRepository.GetCaseAsync(caseId);

            if (result?.CaseView is null)
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        "Case not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<CaseDetailDto>.Ok(result));

        }

        [HttpGet("case-new-notifications")]
        public async Task<IActionResult> GetCaseNewNotifications([FromQuery] long caseId)
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

            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }

            var result = await _caseRepository.GetCaseNewNotificationsAsync(caseId);

            if (result is null || !result.Any())
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        "New notificaton not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<List<CaseNotificationListItem>>.Ok(result));

        }

        [HttpGet("attachment")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMqsDocFile(string attachmentId)
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

            if (user.LockoutUntil.HasValue &&
              user.LockoutUntil.Value > DateTime.Now)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is temporarily locked"));
            }

            if (user.IsPassChangeRequired)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "Password change is required"));
            }

            var result = await _documentService.GetDocument(attachmentId);

            return File(result, "application/pdf", "document.pdf");
        }

        //[HttpGet("analize-case")]
        //[AllowAnonymous]

        //public async Task<IActionResult> AnalizeCase([FromQuery] long caseId)
        //{
        //    await _caseService.AnalizeCase(caseId);

        //    return Ok(ApiResponse<object>.Ok("ok"));

        //}
    }
}
