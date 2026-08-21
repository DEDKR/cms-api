using System.ComponentModel.DataAnnotations;

namespace CmsApi.DTOs.AuthDtos
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
