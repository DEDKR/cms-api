using CmsApi.DTOs.AssignmentDtos;

namespace CmsApi.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<bool> SetROToUserAsync(long userId, string? roId);
        // Retained for callers of the original single-assignment procedure.
        Task<bool> SetMeetToUserAsync(long userId, long meetId, long attendedBy);
        Task<List<MeetingAssignmentUserDto>> GetEligibleUsersAsync(string? officeKeyCode, string? search);
        Task<MeetAssignmentResultDto> SetMeetsToUsersAsync(
            IReadOnlyCollection<long> meetIds,
            IReadOnlyCollection<long> userIds,
            long attendedBy,
            string? officeKeyCode,
            string? assignmentNote);
    }
}
