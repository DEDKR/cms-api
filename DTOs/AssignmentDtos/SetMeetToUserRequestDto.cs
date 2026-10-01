using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.AssignmentDtos
{
    public class SetMeetToUserRequestDto
    {
        [Range(1, long.MaxValue)]
        public long UserId { get; set; }

        [Range(1, long.MaxValue)]
        public long MeetId { get; set; }
    }
}
