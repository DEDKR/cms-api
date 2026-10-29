using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.AssignmentDtos
{
    public class MeetingParticipationDto
    {
        public long MeetingId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public long? AssignedBy { get; set; }
        public string? AssignedByName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string? AssignmentNote { get; set; }
        public int ParticipationStatus { get; set; } // 0 pending, 1 attended, 2 absent
        public string? ParticipationNote { get; set; }
        public DateTime? RespondedAt { get; set; }
    }

    public class UpdateMeetingParticipationDto
    {
        [Range(1, 2)]
        public int Status { get; set; }

        [Required, MinLength(1), MaxLength(1500)]
        public string Note { get; set; } = "";
    }
}
