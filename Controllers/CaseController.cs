using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaseController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ICaseRepository _caseRepository;
        public CaseController(ICaseRepository caseRepository, IDocumentService documentService)
        {
            _caseRepository = caseRepository;
            _documentService = documentService;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetCases([FromBody] CaseListRequestDto request)
        {
            var result = await _caseRepository.GetCasesAsync(request);

            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpGet("statistics")]
        public async Task<IActionResult> GetCaseStatistics()
        {
            var result = await _caseRepository.CaseStatisticAsync();

            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetCaseDetail([FromQuery] long caseId)
        {
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
            var result = await _documentService.GetDocument(attachmentId);


            return File(result, "application/pdf", "document.pdf");
        }
    }
}
