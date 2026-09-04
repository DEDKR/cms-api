using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;

namespace CmsApi.Services.Interfaces
{
    public interface IComplaintService
    {
        Task<PagedResult<ComplaintListDto?>> GetComplaintsAsync(ComplaintListRequestDto request);
        Task<ComplaintDetailsDto?> GetComplaintDetailsAsync(long complaintId);
    }
}
