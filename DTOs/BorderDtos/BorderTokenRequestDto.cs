using System.Text.Json.Serialization;

namespace CmsApi.DTOs.BorderDtos
{
    public class BorderTokenRequestDto
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = null!;

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = null!;
    }
}
