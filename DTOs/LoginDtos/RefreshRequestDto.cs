using System.Text.Json.Serialization;

namespace CmsApi.DTOs.LoginDtos
{
    public class RefreshRequestDto
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("signType")]
        public int SignType { get; set; }
    }
}
