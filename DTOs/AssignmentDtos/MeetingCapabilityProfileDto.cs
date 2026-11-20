namespace CmsApi.DTOs.AssignmentDtos
{
    public sealed class ParticipationOptionDto<T>
    {
        public T Value { get; set; } = default!;
        public string? Label { get; set; }
    }

    public sealed class MeetingCapabilityProfileDto
    {
        public ParticipationOptionDto<string>? OfficeKeyCode { get; set; }
        public ParticipationOptionDto<int>? Position { get; set; }
        public ParticipationOptionDto<int>? Department { get; set; }
    }
}
