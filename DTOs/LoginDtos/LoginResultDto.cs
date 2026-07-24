namespace CmsApi.DTOs.LoginDtos
{
    public class LoginResultDto
    {
        public string Id { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; } = default!;

        public string RefreshToken { get; set; } = default!;

        public DateTimeOffset Expire { get; set; }

        public DateTimeOffset RefreshTokenExpire { get; set; }
    }
}
