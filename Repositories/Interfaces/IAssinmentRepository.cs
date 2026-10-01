namespace CmsApi.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<bool> SetROToUserAsync(long userId, string? roId);
        Task<bool> SetMeetToUserAsync(long userId, long meetId, long attendedBy);
    }
}
