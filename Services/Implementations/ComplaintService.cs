using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;

namespace CmsApi.Services.Implementations
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _complaintRepository;

        public ComplaintService(IComplaintRepository complaintRepository) {
            _complaintRepository = complaintRepository;
        }

        public async Task<PagedResult<ComplaintListDto?>> GetComplaintsAsync(ComplaintListRequestDto request)
        {
            return await _complaintRepository.GetComplaintsAsync(request);
        }

        public async Task<ComplaintDetailsDto?> GetComplaintDetailsAsync(long complaintId)
        {
            return await _complaintRepository.GetComplaintDetailsAsync(complaintId);
        }
    }
}
