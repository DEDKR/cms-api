namespace CmsApi.DTOs.UserDtos
{
    public class UserAdminDashboardResponseDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalRoles { get; set; }
    }
}
