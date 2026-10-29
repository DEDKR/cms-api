namespace CmsApi.DTOs.AssignmentDtos
{
    public class MeetingAssignmentUserDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public string? OfficeKeyCode { get; set; }
    }
}
