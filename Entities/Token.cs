namespace CmsApi.Entities
{
    namespace CmsApi.Entities
    {
        public class Token
        {
            public string Id { get; set; } = string.Empty;

            public int UserId { get; set; }

            public string AccessToken { get; set; } = string.Empty;

            public string RefreshToken { get; set; } = string.Empty;

            public DateTimeOffset AccessTokenExpire { get; set; }

            public DateTimeOffset RefreshTokenExpire { get; set; }

            public DateTime UpdatedAt { get; set; }
        }
    }
}
