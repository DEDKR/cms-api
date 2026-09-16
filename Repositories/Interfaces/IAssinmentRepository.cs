namespace CmsApi.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<object> SetROToUserAsync(long userId, int roId);
        Task<object> SetMeetToUserAsync(long userId, long meetId, long attendedBy);
    }
}
