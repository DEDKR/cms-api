using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using CmsApi.Security;
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

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "User not found"));
            }

            if (!user.IsActive)
            {
                return Unauthorized(
                    ApiResponse<object>.Fail(
                        "Account is inactive"));
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

            // Apply RO access on the server, never from the request body.
            // RoleId = 1 (Admin) and RO_00 can see all cases.
            // Every other user must have an assigned office.
            var isAdmin = user.RoleId == 1;
            var userOffice = user.OfficeKeyCode?.Trim();
            var isHeadOffice = string.Equals(
                userOffice,
                "RO_00",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isHeadOffice &&
                string.IsNullOrWhiteSpace(userOffice))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(
                        "No regional office assigned"));
            }

            string? effectiveOfficeKeyCode =
                isAdmin || isHeadOffice ? null : userOffice;

            var result = await _caseRepository.GetCasesAsync(
                request,
                effectiveOfficeKeyCode);

            return Ok(
                ApiResponse<object>.Ok(result));
        }


        [HttpGet("statistics")]
        public async Task<IActionResult> GetCaseStatistics()
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));

            var result = await _caseRepository.CaseStatisticAsync(scope.OfficeKeyCode);

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetCaseDetail([FromQuery] long caseId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            // Return 404 rather than reveal whether another office's case exists.
            if (!await _caseRepository.CanAccessCaseAsync(caseId, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Case not found"));

            var result = await _caseRepository.GetCaseAsync(caseId);

            if (result?.CaseView is null)
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        "Case not found",
                        StatusCodes.Status404NotFound));
            }

            if (scope.OfficeKeyCode is not null)
            {
                // Prevent cross-RO case references in the nested detail response.
                var relatedIds = (result.RelatedCases ?? new List<CaseListDto>()).Select(x => x.Id)
                    .Concat((result.CaseHistories ?? new List<CaseHistoryDto>()).Select(x => x.CaseId))
                    .Concat((result.Appeals ?? new List<CaseAppealDto>()).Where(x => x.CaseId.HasValue)
                        .Select(x => (long)x.CaseId!.Value));
                var allowedIds = await _caseRepository.GetAccessibleCaseIdsAsync(
                    relatedIds, scope.OfficeKeyCode);
                if (result.RelatedCases is not null)
                    result.RelatedCases = result.RelatedCases.Where(x => allowedIds.Contains(x.Id)).ToList();
                if (result.CaseHistories is not null)
                    result.CaseHistories = result.CaseHistories.Where(x => allowedIds.Contains(x.CaseId)).ToList();
                if (result.Appeals is not null)
                    result.Appeals = result.Appeals.Where(x => x.CaseId.HasValue &&
                        allowedIds.Contains(x.CaseId.Value)).ToList();
            }

            return Ok(ApiResponse<CaseDetailDto>.Ok(result));

        }

        [HttpGet("case-new-notifications")]
        public async Task<IActionResult> GetCaseNewNotifications([FromQuery] long caseId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            if (!await _caseRepository.CanAccessCaseAsync(caseId, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Case not found"));

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

            // The source archives do not expose a verified attachmentId -> CASE_ID relation.
            // Fail closed for region-limited users rather than allow arbitrary attachment IDs.
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            if (scope.OfficeKeyCode is not null)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail("Attachment access needs case ownership verification"));

            var result = await _documentService.GetDocument(attachmentId);

            return File(result, "application/pdf", "document.pdf");
        }



        [HttpGet("analize-case")]
        [Authorize]

        public async Task<IActionResult> AnalizeCase([FromQuery] long caseId)
        {
            await _caseService.AnalizeCase(caseId);

            return Ok(ApiResponse<object>.Ok("ok"));

        }

        [Authorize]
        [HttpPost("analize-case")]
        public async Task<IActionResult> UpsertCaseAnalysisResult([FromBody] CaseAnalysisResultRequestDto payload)
        {
            if (!payload.CaseId.HasValue || payload.CaseId.Value <= 0)
            {
                return BadRequest(
                    ApiResponse<object>.Fail("Case ID düzgün deyil."));
            }

            if (string.IsNullOrWhiteSpace(payload.OfficeKeyCode) &&
                string.IsNullOrWhiteSpace(payload.CaseSubjectKeyCode))
            {
                return BadRequest(
                    ApiResponse<object>.Fail("İcra orqanı və ya işin predmeti seçilməlidir."));
            }

            // EXECUTER_USER_ID frontend-dən qəbul edilmir, token-dən götürülür.
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(
                    ApiResponse<object>.Fail("User not authenticated"));
            }

            payload.ExecuterUserId = userId;

            var result = await _caseService.UpsertCaseAnalysisResultAsync(payload);

            return Ok(ApiResponse<long>.Ok(result));
        }

        [HttpGet("{caseId:long}/pdf")]
        public async Task<IActionResult> GetCasePdf(long caseId)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));
            if (!await _caseRepository.CanAccessCaseAsync(caseId, scope.OfficeKeyCode))
                return NotFound(ApiResponse<object>.Fail("Case not found"));

            var pdf = await _caseService.GetCasePdfAsync(caseId);

            if (pdf is null)
            {
                return NotFound(
                    ApiResponse<object>.Fail(
                        "Case not found"));
            }

            return File(
                pdf,
                "application/pdf",
                $"case-{caseId}.pdf");
        }
    }
}
