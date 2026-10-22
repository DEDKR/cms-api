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

        [HttpGet("first-instances")]
        public async Task<IActionResult> GetFirstInstanceReport([FromQuery] FirstInstanceRequest request)
        {
            var result = await _reportsRepository.GetFirstInstanceReportAsync(request);
            return Ok(result);
        }

        [HttpGet("appeals")]
        public async Task<IActionResult> GetAppealReport([FromQuery] AppealRequest request)
        {
            var result = await _reportsRepository.GetAppealReportAsync(request);
            return Ok(result);
        }

        [HttpGet("cassasions")]
        public async Task<IActionResult> GetCassasionReport([FromQuery] CassasionRequest request)
        {
            var result = await _reportsRepository.GetCassasionReportAsync(request);
            return Ok(result);
        }

        [HttpGet("meetings")]
        public async Task<IActionResult> GetMeetingReport([FromQuery] MeetingRequest request)
        {
            var result = await _reportsRepository.GetMeetingReportAsync(request);
            return Ok(result);
        }
    }
}
