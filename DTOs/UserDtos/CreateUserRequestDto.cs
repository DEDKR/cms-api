using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.UserDtos
{
    public class CreateUserRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

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

        // İlk parol əvvəlcə TBL_USERS.PASSWORD sütununda saxlanılır.
        // İlk uğurlu dəyişiklikdən sonra PASS_HASH doldurulur.
        [Required]
        [MinLength(8)]
        [MaxLength(50)]
        public string InitialPassword { get; set; } = string.Empty;

        // Yeni istifadəçi yaradılarkən ilkin Ərazi idarəsi də yazıla bilər.
        // Redaktə zamanı isə /api/Assignment/ro-to-user istifadə olunur.
        [MaxLength(10)]
        public string? RegionalOfficeId { get; set; }
    }
}
