using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.DTOs.PretenseDtos;
using CmsApi.Repositories.Implementations;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PretenseController : ControllerBase
    {
        private readonly IPretenseService _pretenseService;
        private readonly IPretenseRepository _pretenseRepository;
        private readonly IUserRepository _userRepository;
        public PretenseController(IPretenseService pretenseService, IPretenseRepository pretenseRepository, IUserRepository userRepository)
        {
            _pretenseService = pretenseService;
            _pretenseRepository = pretenseRepository;
            _userRepository = userRepository;
        }


        [HttpPost]
        public async Task<IActionResult> GetPretenses([FromBody] PretenseListRequestDto request)
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


        [HttpGet("statistics")]
        public async Task<IActionResult> GetPretenseStatistics()
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

            var result = await _pretenseRepository.GetPretenseStatisticsAsync();

            return Ok(ApiResponse<PretenseStatisticDto>.Ok(result));
        }



    }
}
