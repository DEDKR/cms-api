namespace CmsApi.Entities
{
    public class Token
    {
        public string Id { get; set; }

        public int UserId { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTimeOffset AccessTokenExpire { get; set; }
        public DateTimeOffset RefreshTokenExpire { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
