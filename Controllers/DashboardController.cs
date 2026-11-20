using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.Dashboard;
using CmsApi.DTOs.Meeting;
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
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));

            var result = await _dashboardService.GetCaseStatusStatisticsAsync(scope.OfficeKeyCode);

            return Ok(result);
        }

        [HttpPost("court-level-case-statistics")]
        public async Task<IActionResult> GetCourtLevelStatistics([FromBody] CourtLevelStatisticRequestDto request)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));

            var result = await _dashboardService.GetCourtLevelCaseStatisticsAsync(request, scope.OfficeKeyCode);

            return Ok(result);
        }

        [HttpPost("case-statistics-by-court")]
        public async Task<IActionResult> GetCaseStatisticsByCourt([FromBody] GetCaseStatisticsByCourtRequestDto request)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));

            var result = await _dashboardService.GetCaseStatisticsByCourtAsync(request, scope.OfficeKeyCode);

            return Ok(result);
        }


        [HttpPost("case-total-by-year-statistics")]
        public async Task<IActionResult> GetTotalByYearStatistics([FromBody] CaseTotalByYearStatisticRequestDto caseTotalByYearStatisticRequestDto)
        {
            var scope = await RegionalOfficeScope.ResolveAsync(User, _userRepository);
            if (!scope.Allowed)
                return StatusCode(scope.ErrorStatus, ApiResponse<object>.Fail(scope.ErrorMessage ?? "Access denied"));

            var result = await _dashboardService.GetCaseTotalByYearStatisticsAsync(caseTotalByYearStatisticRequestDto, scope.OfficeKeyCode);

            return Ok(result);
        }
    }
}
