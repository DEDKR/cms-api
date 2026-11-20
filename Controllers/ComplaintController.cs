using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.DTOs.Meeting;
using CmsApi.Repositories.Implementations;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly IComplaintRepository _complaintRepository;
        private readonly IUserRepository _userRepository;

        public ComplaintController(IComplaintService complaintService, IComplaintRepository complaintRepository, IUserRepository userRepository)
        {
            _complaintService = complaintService;
            _complaintRepository = complaintRepository;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> GetComplaints([FromBody] ComplaintListRequestDto request)
        {
            var complaints = await _complaintService.GetComplaintsAsync(request);

            return Ok(ApiResponse<PagedResult<ComplaintListDto?>>.Ok(complaints));
        }

        [HttpGet("{complaintId}")]
        public async Task<IActionResult> GetComplaint(long complaintId)
        {
            var complaint = await _complaintService.GetComplaintDetailsAsync(complaintId);

            if (complaint?.CaseView is null)
            {
                return NotFound(
                    ApiResponse<ComplaintDetailsDto>.Fail(
                        "Complaint not found",
                        StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<ComplaintDetailsDto>.Ok(complaint));
        }


        [HttpGet("statistics")]
        public async Task<IActionResult> GetComplaintStatistics()
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

            var result = await _complaintRepository.ComplaintStatisticAsync();

            return Ok(ApiResponse<ComplaintStatisticDto>.Ok(result));
        }



    }
}
