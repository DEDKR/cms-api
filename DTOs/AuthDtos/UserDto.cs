namespace CmsApi.DTOs.AuthDtos
{
    public class UserDto
    {
        public int UserId { get; set; }

        public byte? RoleId { get; set; }
        public string RoleName { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsPassChangeRequired { get; set; }
    }
}
