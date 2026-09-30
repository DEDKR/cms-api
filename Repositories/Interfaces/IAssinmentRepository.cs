namespace CmsApi.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<object> SetROToUserAsync(long userId, string roId);
        Task<object> SetMeetToUserAsync(long userId, long meetId, long attendedBy);
    }
}
