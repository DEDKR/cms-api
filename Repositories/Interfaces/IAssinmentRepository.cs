using CmsApi.DTOs.AssignmentDtos;
namespace CmsApi.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<bool> SetROToUserAsync(long userId, string? roId);
        Task<List<MeetingAssignmentUserDto>> GetEligibleUsersAsync(
            string? officeKeyCode, string? search, long? onlyUserId = null);
    }
}
