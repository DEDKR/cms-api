using CmsApi.DTOs.AssignmentDtos;
namespace CmsApi.Repositories.Interfaces
{
    public interface IMeetingParticipationRepository
    {
        Task<MeetingCapabilityProfileDto> GetCapabilityLabelsAsync(long userId,
            string? officeClaim, int? positionClaim, int? departmentClaim);
        Task<List<MeetingParticipationDto>> GetAsync(long meetingId, long requestingUserId,
            string? officeKeyCode, bool canManageOthers);
        Task<MeetingParticipationSaveResultDto> SetAsync(IReadOnlyCollection<long> meetingIds,
            IReadOnlyCollection<long> userIds, long actorId, string? officeKeyCode,
            bool canManageOthers, int status, string? note);
    }
}
