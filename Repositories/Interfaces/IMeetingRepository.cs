using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.Meeting;

namespace CmsApi.Repositories.Interfaces
{
    public interface IMeetingRepository
    {
        Task<PagedResult<MeetingListItemDto>> GetMeetingsAsync(
            MeetingRequestDto meetingRequestDto, string? officeKeyCode);

        Task<bool> CanAccessMeetingAsync(long meetingId, string? officeKeyCode);

        Task<MeetingDetailDto> MeetingDetail(long meetingId);

        Task<MeetingStatisticDto?> MeetingStatisticAsync(string? officeKeyCode);
    }
}
