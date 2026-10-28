using CmsApi.DTOs.Reports.Appeals;
using CmsApi.DTOs.Reports.Cassasion;
using CmsApi.DTOs.Reports.FirstInstance;
using CmsApi.DTOs.Reports.Meeting;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CmsApi.Services.Implementations;
using System;

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
        private bool TryGetEffectiveOfficeKeyCode(string? requestedOfficeKeyCode, out string? officeKeyCode)
        {
            officeKeyCode = null;
            var requestedOffice = requestedOfficeKeyCode?.Trim();
            if (string.IsNullOrWhiteSpace(requestedOffice))
                requestedOffice = null;

            var roleId = User.FindFirst("roleId")?.Value;
            if (roleId == "1")
            {
                // Admin: no filter returns every office; a supplied filter returns one office.
                officeKeyCode = requestedOffice;
                return true;
            }

            var claimOffice = User.FindFirst("officeKeyCode")?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(claimOffice))
                return false;

            // Non-admin cannot request another regional office, even through query parameters.
            if (requestedOffice != null &&
                !string.Equals(requestedOffice, claimOffice, StringComparison.OrdinalIgnoreCase))
                return false;

            officeKeyCode = claimOffice;
            return true;
        }

        [HttpGet("first-instances")]
        public async Task<IActionResult> GetFirstInstances([FromQuery] FirstInstanceRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(request.OfficeKeyCode, out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetFirstInstanceReportAsync(request, officeKeyCode);
            return Ok(result);
        }

        [HttpGet("appeals")]
        public async Task<IActionResult> GetAppealReport([FromQuery] AppealRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(request.OfficeKeyCode, out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetAppealReportAsync(request, officeKeyCode);
            return Ok(result);
        }

        [HttpGet("cassasions")]
        public async Task<IActionResult> GetCassasionReport([FromQuery] CassasionRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(request.OfficeKeyCode, out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetCassasionReportAsync(request, officeKeyCode);
            return Ok(result);
        }

        /// <summary>Returns the white/black, merged-header Excel report matching the official table layout.</summary>
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] string kind = "first-instance",
            [FromQuery] bool combineAll = false,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? officeKeyCode = null,
            [FromQuery] int? courtTypeId = null,
            [FromQuery] string? courtTypeLabel = null)
        {
            if (!TryGetEffectiveOfficeKeyCode(officeKeyCode, out var effectiveOffice))
                return Forbid();
            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                return BadRequest("Başlanğıc tarixi son tarixdən böyük ola bilməz.");
            if (from.HasValue && from.Value.Date > DateTime.Today || to.HasValue && to.Value.Date > DateTime.Today)
                return BadRequest("Gələcək tarix seçilə bilməz.");
            if (courtTypeId.HasValue && courtTypeId.Value <= 0)
                return BadRequest("Seçilmiş məhkəmə növü düzgün deyil.");
            if (!combineAll && kind != "first-instance" && kind != "appeal" && kind != "cassation" && kind != "meetings")
                return BadRequest("Naməlum hesabat növü.");

            var reportKind = combineAll ? "all" : kind;
            var commonDates = new { From = from, To = to, CourtTypeId = courtTypeId };
            var first = reportKind == "all" || reportKind == "first-instance"
                ? await _reportsRepository.GetFirstInstanceReportAsync(new FirstInstanceRequest
                  { From = commonDates.From, To = commonDates.To, OfficeKeyCode = effectiveOffice, CourtTypeId = courtTypeId, ByCourt = false }, effectiveOffice)
                : null;
            var appeal = reportKind == "all" || reportKind == "appeal"
                ? await _reportsRepository.GetAppealReportAsync(new AppealRequest
                  { From = commonDates.From, To = commonDates.To, OfficeKeyCode = effectiveOffice, CourtTypeId = courtTypeId, ByCourt = false }, effectiveOffice)
                : null;
            var cassation = reportKind == "all" || reportKind == "cassation"
                ? await _reportsRepository.GetCassasionReportAsync(new CassasionRequest
                  { From = commonDates.From, To = commonDates.To, OfficeKeyCode = effectiveOffice, CourtTypeId = courtTypeId, ByCourt = false }, effectiveOffice)
                : null;
            var meetings = reportKind == "all" || reportKind == "meetings"
                ? await _reportsRepository.GetMeetingReportAsync(new MeetingRequest
                  { From = commonDates.From, To = commonDates.To, OfficeKeyCode = effectiveOffice, CourtTypeId = courtTypeId, ByCourt = false }, effectiveOffice)
                : null;

            var data = ReportExcelGenerator.Build(reportKind, from, to, courtTypeId, courtTypeLabel, first, appeal, cassation, meetings);
            var fileName = $"CMS_{reportKind}_{(from.HasValue ? from.Value.ToString("yyyyMMdd") : "all")}_{(to.HasValue ? to.Value.ToString("yyyyMMdd") : "all")}.xlsx";
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet("meetings")]
        public async Task<IActionResult> GetMeetingReport([FromQuery] MeetingRequest request)
        {
            if (!TryGetEffectiveOfficeKeyCode(request.OfficeKeyCode, out var officeKeyCode))
                return Forbid();

            var result = await _reportsRepository.GetMeetingReportAsync(request, officeKeyCode);
            return Ok(result);
        }
    }
}
