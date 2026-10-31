namespace CmsApi.DTOs.AssignmentDtos
{
    public class MeetingAssignmentUserDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";

        public ParticipationOptionDto<string>? OfficeKeyCode { get; set; }
        public ParticipationOptionDto<int>? Position { get; set; }
        public ParticipationOptionDto<int>? Department { get; set; }
    }
}