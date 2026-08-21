using CmsApi.DTOs;
using CmsApi.DTOs.ApiDtos;
using CmsApi.Enums;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class ReferenceDataController : ControllerBase
    {
        private readonly IReferenceDataRepository _referenceDataRepository;

        public ReferenceDataController(IReferenceDataRepository referenceDataRepository)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        [HttpGet("courts")]
        public async Task<IActionResult> GetCourts(string? parametr)
            => await HandleReferenceDataAsync(ReferenceDataType.Courts,null,parametr);

        [HttpGet("judges")]
        public async Task<IActionResult> GetJudges([FromQuery] int courtId, string? parametr)
            => await HandleReferenceDataAsync(
                ReferenceDataType.Judges,
                courtId,
                parametr);

        [HttpGet("case-types")]
        public async Task<IActionResult> GetCaseTypes([FromQuery]  string? parametr)
            => await HandleReferenceDataAsync(
                ReferenceDataType.CaseTypes,
                null,
                parametr);


        [HttpGet("meeting-types")]
        public async Task<IActionResult> GetMeetingTypes([FromQuery] string? parametr)
            => await HandleReferenceDataAsync(
         ReferenceDataType.MeetTypes,
            null,
         parametr);

        [HttpGet("case-statuses")]
        public async Task<IActionResult> GetCaseStatuses([FromQuery] string? parametr)
            => await HandleReferenceDataAsync(
         ReferenceDataType.CaseStatuses,
            null,
         parametr);

        [HttpGet("meeting-statuses")]
        public async Task<IActionResult> GetMeetingStatuses([FromQuery] string? parametr)
           => await HandleReferenceDataAsync(
        ReferenceDataType.MeetingStatuses,
           null,
        parametr);


        [HttpGet("court-types")]
        public async Task<IActionResult> GetCourtTypes()
         => await HandleReferenceDataAsync(
             ReferenceDataType.CourtTypes
         );

        private async Task<IActionResult> HandleReferenceDataAsync(
         ReferenceDataType type,
         int? courtId = null,
         string? parameter = null)
            {
                parameter = string.IsNullOrWhiteSpace(parameter)
                    ? null
                    : parameter;

                var data = await _referenceDataRepository.GetDataAsync(
                    type,
                    courtId,
                    parameter);

                return Ok(ApiResponse<List<ReferenceData>>.Ok(data));
            }
    }
}
