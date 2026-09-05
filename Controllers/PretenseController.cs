using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.DTOs.PretenseDtos;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class PretenseController : ControllerBase
    {
        private readonly IPretenseService _pretenseService;
        public PretenseController(IPretenseService pretenseService)
        {
            _pretenseService = pretenseService;
        }


        [HttpPost]
        public async Task<IActionResult> GetPretenses([FromQuery] PretenseListRequestDto request)
        {
            var pretenses = await _pretenseService.GetPretenseListAsync(request);

            return Ok(ApiResponse<PagedResult<PretenseListDto?>>.Ok(pretenses));
        }

        [HttpGet("{pretenseId}")]
        public async Task<IActionResult> GetPretenseDetails(long pretenseId)
        {
            var pretenseDetails = await _pretenseService.GetPretenseDetailsAsync(pretenseId);

            if (pretenseDetails?.CaseView is null)
            {
                return NotFound(
                    ApiResponse<PretenseDetailsDto>.Fail(
                        "Pretense not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<PretenseDetailsDto>.Ok(pretenseDetails));
        }



    }
}
