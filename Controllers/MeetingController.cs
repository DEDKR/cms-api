using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.Meeting;
using CmsApi.Repositories.Implementations;
using CmsApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeetingController : ControllerBase
    {
        private readonly IMeetingRepository _meetingRepository;

        public MeetingController(IMeetingRepository meetingRepository)
        {
            _meetingRepository = meetingRepository;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetList([FromBody] MeetingRequestDto meetingRequestDto)
        {
            var result = await _meetingRepository.GetMeetingsAsync(meetingRequestDto);

            return Ok(result);
        }

        [HttpGet("detail")]
        public async Task<IActionResult> GetCaseDetail([FromQuery] long meetingId)
        {
            var result = await _meetingRepository.MeetingDetail(meetingId);

            if (result?.MeetingView is null)
            {
                return NotFound(
                    ApiResponse<CaseDetailDto>.Fail(
                        "Case not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<MeetingDetailDto>.Ok(result));

        }
    }
}
