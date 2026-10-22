using CmsApi.DTOs.Reports.Appeals;
using CmsApi.DTOs.Reports.Cassasion;
using CmsApi.DTOs.Reports.FirstInstance;
using CmsApi.DTOs.Reports.Meeting;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportsRepository _reportsRepository;

        public ReportController(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }

        // Admins can see all offices. Other users are restricted to their JWT office.
        // A missing office claim for a non-admin must not turn into an unfiltered query.
        private bool TryGetEffectiveOfficeKeyCode(out string? officeKeyCode)
        {
            officeKeyCode = null;

            var roleId = User.FindFirst("roleId")?.Value;
            if (roleId == "1")
                return true;

            var claimOffice = User.FindFirst("officeKeyCode")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(claimOffice))
                return false;

            officeKeyCode = claimOffice;
            return true;
        }

        [HttpGet("first-instances")]
        public async Task<IActionResult> GetFirstInstances([FromQuery] FirstInstanceRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetFirstInstanceReportAsync(request, officeKeyCode);
            return Ok(result);
        }

        [HttpGet("appeals")]
        public async Task<IActionResult> GetAppealReport([FromQuery] AppealRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetAppealReportAsync(request, officeKeyCode);
            return Ok(result);
        }

        [HttpGet("cassasions")]
        public async Task<IActionResult> GetCassasionReport([FromQuery] CassasionRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetCassasionReportAsync(request, officeKeyCode);
            return Ok(result);
        }

        [HttpGet("meetings")]
        public async Task<IActionResult> GetMeetingReport([FromQuery] MeetingRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetMeetingReportAsync(request, officeKeyCode);
            return Ok(result);
        }
    }
}
