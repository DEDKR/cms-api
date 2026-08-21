namespace CmsApi.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public byte? RoleId { get; set; }

        public string Role {  get; set; }


        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? FatherName { get; set; }

        public string? Pin { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? PassHash { get; set; }

        public bool IsPassChangeRequired { get; set; }

        public bool IsActive { get; set; }

        public DateTime? InsertDate { get; set; }

        public DateTime? PassChangeAt { get; set; }

        public int FailedLoginCount { get; set; }

        public DateTime? LockoutUntil { get; set; }
    }
}
