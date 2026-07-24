namespace CmsApi.Common
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = null!;
        public int ExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDay { get; set; }
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
    }
}
