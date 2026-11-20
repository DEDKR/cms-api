namespace CmsApi.DTOs.UserDtos
{
    public class UserItemDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FatherName { get; set; }
        public string? Pin { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? InsertDate { get; set; }
        public string? PassChangeAt { get; set; }
        public string? RegionalOfficeId { get; set; }
        public string? RegionalOfficeName { get; set; }
    }
}
