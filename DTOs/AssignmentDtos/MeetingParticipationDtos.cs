using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.AssignmentDtos
{
    public class MeetingParticipationDto
    {
        public long MeetingId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public int ParticipationStatus { get; set; } // 1 = attended, 2 = absent
        public string? ParticipationNote { get; set; }
        public DateTime? RespondedAt { get; set; }
        public long? RecordedBy { get; set; }
        public string? RecordedByName { get; set; }
    }
    public class UpdateMeetingParticipationDto
    {
        public long? UserId { get; set; } // optional: authenticated self by default
        [Range(1, 2)] public int Status { get; set; }
        [MaxLength(1500)] public string? Note { get; set; }
    }
    public class SetMeetingParticipationsRequestDto
    {
        [Required, MinLength(1)] public List<long> MeetIds { get; set; } = new();
        [Required, MinLength(1)] public List<long> UserIds { get; set; } = new();
        [Range(1, 2)] public int Status { get; set; }
        [MaxLength(1500)] public string? Note { get; set; }
    }
    public class MeetingParticipationSaveResultDto
    {
        public int TotalPairs { get; set; }
        public int Created { get; set; }
        public int Updated { get; set; }
    }
}
