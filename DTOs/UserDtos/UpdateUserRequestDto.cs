using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.UserDtos
{
    public class UpdateUserRequestDto
    {
        [Required]
        [MaxLength(250)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? FatherName { get; set; }

        [MaxLength(7)]
        public string? Pin { get; set; }

        [Range(1, byte.MaxValue)]
        public int RoleId { get; set; }

        // Ərazi idarəsi ayrıca Assignment API ilə yenilənir.
        [MaxLength(10)]
        public string? RegionalOfficeId { get; set; }
    }
}
