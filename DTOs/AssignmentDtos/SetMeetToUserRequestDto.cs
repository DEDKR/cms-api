using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.AssignmentDtos
{
    // Cross-product: each meeting receives each selected user.
    public class SetMeetToUserRequestDto
    {
        [Required, MinLength(1)]
        public List<long> MeetIds { get; set; } = new();

        [Required, MinLength(1)]
        public List<long> UserIds { get; set; } = new();

        [StringLength(1500)]
        public string? AssignmentNote { get; set; }
    }
}
