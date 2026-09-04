using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;

namespace CmsApi.Repositories.Interfaces
{
    public interface IComplaintRepository
    {
        Task<PagedResult<ComplaintListDto?>> GetComplaintsAsync(ComplaintListRequestDto request);
        Task<ComplaintDetailsDto?> GetComplaintDetailsAsync(long complaintId);
    }
}
