using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.Dashboard;
using CmsApi.DTOs.Meeting;
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

    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IUserRepository _userRepository;

        public DashboardController(IDashboardService dashboardService, IUserRepository userRepository)
        {
            _dashboardService = dashboardService;
            _userRepository = userRepository;
        }

        [HttpGet("case-status-statistics")]
        public async Task<IActionResult> GetCaseStatusStatistics()
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


            var result = await _dashboardService.GetCaseStatusStatisticsAsync();

            return Ok(result);
        }

        [HttpPost("court-level-case-statistics")]
        public async Task<IActionResult> GetCourtLevelStatistics([FromBody] CourtLevelStatisticRequestDto request)
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

            var result = await _dashboardService.GetCourtLevelCaseStatisticsAsync(request);

            return Ok(result);
        }

        [HttpPost("case-statistics-by-court")]
        public async Task<IActionResult> GetCaseStatisticsByCourt([FromBody] GetCaseStatisticsByCourtRequestDto request)
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

            var result = await _dashboardService.GetCaseStatisticsByCourtAsync(request);

            return Ok(result);
        }


        [HttpPost("case-total-by-year-statistics")]
        public async Task<IActionResult> GetTotalByYearStatistics([FromBody] CaseTotalByYearStatisticRequestDto caseTotalByYearStatisticRequestDto)
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

            var result = await _dashboardService.GetCaseTotalByYearStatisticsAsync(caseTotalByYearStatisticRequestDto);

            return Ok(result);
        }
    }
}
