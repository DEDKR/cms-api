namespace CmsApi.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public int? PersonnelId { get; set; }
        public byte? RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime? InsertDate { get; set; }
        public byte? StatusId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? PassHash { get; set; }
        public DateTime? PassChangeAt { get; set; }
        public bool IsPassChangeRequired { get; set; }
    }
}
