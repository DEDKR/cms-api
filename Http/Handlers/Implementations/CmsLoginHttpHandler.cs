using CmsApi.Common;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CmsLoginDtos;
using CmsApi.DTOs.HttpApiDtos;
using CmsApi.Entities;
using CmsApi.Entities.CmsApi.Entities;
using CmsApi.Http.Handlers.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CmsApi.Http.Handlers.Implementations
{
    public class CmsLoginHttpHandler : ICmsLoginHttpHandler
    {
        private readonly HttpClient _httpClient;
        private readonly CmsApiSettings _apiSettings;
        private readonly ILogger<CmsLoginHttpHandler> _logger;

        public CmsLoginHttpHandler(
         HttpClient httpClient,
         IOptions<CmsApiSettings> options,
         ILogger<CmsLoginHttpHandler> logger
         )
        {
            _httpClient = httpClient;
            _apiSettings = options.Value;
            _logger = logger;

        }

        public async Task<CmsApiResponse<LoginResultDto>> LoginByAsanCertificateAsync(
           LoginRequestBody loginRequestBody)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(loginRequestBody.DedkrCertDetail),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                _apiSettings.LoginByAsanCertificateApi,
                content);

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CmsApiResponse<LoginResultDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }

        public async Task<CmsApiResponse<LoginResultDto>> RefreshTokenAsync(RefreshRequestDto refreshRequestDto, Token token)
        {
            var jso = JsonSerializer.Serialize(refreshRequestDto);


            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            var content = new StringContent(
                jso,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                _apiSettings.LoginByAsanCertificateApi,
                content);

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CmsApiResponse<LoginResultDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }
    }
}
