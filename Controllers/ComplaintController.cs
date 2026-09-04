using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;

        public ComplaintController(IComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        [HttpPost]
        public async Task<IActionResult> GetComplaints([FromBody] ComplaintListRequestDto request)
        {
            var complaints = await _complaintService.GetComplaintsAsync(request);

            return Ok(ApiResponse<PagedResult<ComplaintListDto?>>.Ok(complaints));
        }

        [HttpGet]
        public async Task<IActionResult> GetComplaint([FromQuery, Required] long complaintId)
        {
            var complaint = await _complaintService.GetComplaintDetailsAsync(complaintId);

            return Ok(ApiResponse<ComplaintDetailsDto?>.Ok(complaint));
        }
    }
}
