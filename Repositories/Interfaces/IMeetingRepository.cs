using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.Meeting;

namespace CmsApi.Repositories.Interfaces
{
    public interface IMeetingRepository
    {
        Task<PagedResult<MeetingListItemDto>> GetMeetingsAsync(MeetingRequestDto meetingRequestDto);
        Task<MeetingDetailDto> MeetingDetail(long meetingId);
    }
}
