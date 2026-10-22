using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.AssignmentDtos
{
    public class SetROToUserRequestDto
    {
        [Range(1, long.MaxValue)]
        public long UserId { get; set; }

        // NULL göndərilərsə ərazi idarəsi istifadəçidən silinir.
        [MaxLength(10)]
        public string? RoId { get; set; }
    }
}
