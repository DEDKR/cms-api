using CmsApi.DTOs.AssignmentDtos;
namespace CmsApi.Repositories.Interfaces
{
    public interface IMeetingParticipationRepository
    {
        Task<List<MeetingParticipationDto>> GetAsync(long meetingId, long requestingUserId, bool showAll);
        Task<bool> UpdateOwnAsync(long meetingId, long userId, int status, string note);
    }
}
