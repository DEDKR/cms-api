using CmsApi.Common;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.BorderDtos;
using CmsApi.DTOs.DocumentDtos;
using CmsApi.DTOs.HttpApiDtos;
using CmsApi.Helpers;
using CmsApi.Http.Handlers.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CmsApi.Http.Handlers.Implementations
{
    public class CmsHttpHandler : ICmsHttpHandler
    {
        private readonly HttpClient _httpClient;
        private readonly CmsApiSettings _apiSettings;
        private readonly ILogger<CmsHttpHandler> _logger;
        private readonly IECmsAuthService _ecmsAuthService;
        private readonly IMemoryCache _memoryCache;

        private const string BorderTokenCacheKey = "BorderAccessToken";

        public CmsHttpHandler(
         HttpClient httpClient,
         IOptions<CmsApiSettings> options,
         ILogger<CmsHttpHandler> logger,
         IMemoryCache memoryCache,
         IECmsAuthService ecmsAuthService)
        {
            _httpClient = httpClient;
            _apiSettings = options.Value;
            _logger = logger;
            _memoryCache = memoryCache;
            _ecmsAuthService = ecmsAuthService;
        }

        public async  Task<CmsApiResponse<DocumentDto>> GetDocumentAsBase64Async(string attachmentId)
        {
            try
            {
                var token = TokenCache.Get();

                if (token is null || token.AccessTokenExpire <= DateTime.UtcNow.AddMinutes(1))
                {
                    await _ecmsAuthService.RefreshTokenAsync();
                    token = TokenCache.Get();
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var url = $"{_apiSettings.FileReaderApi}?attachmentId={Uri.EscapeDataString(attachmentId)}";


                var response = await _httpClient.GetAsync(url);

                var jsonString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "GetNotificationDetail request failed. StatusCode: {StatusCode}, Response: {Response}",
                        response.StatusCode,
                        jsonString);

                    return null;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Case Detail API token expired. Refresh olunur.");

                    await _ecmsAuthService.RefreshTokenAsync();

                    token = TokenCache.Get();

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.AccessToken);

                    response = await _httpClient.GetAsync(url);
                    jsonString = await response.Content.ReadAsStringAsync();
                }

                var result = JsonSerializer.Deserialize<CmsApiResponse<DocumentDto>>(
                    jsonString,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    _logger.LogWarning("Case Detail API token expired. Refresh olunur.");

                    await _ecmsAuthService.RefreshTokenAsync();

                    token = TokenCache.Get();

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token.AccessToken);

                    response = await _httpClient.GetAsync(url);
                    jsonString = await response.Content.ReadAsStringAsync();

                    result = JsonSerializer.Deserialize<CmsApiResponse<DocumentDto>>(
                        jsonString,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }


                if (result is null)
                {
                    _logger.LogError("Notification detail response deserialize olunmadı.");
                    return null;
                }

                if (result.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    _logger.LogWarning(
                        "Notification Detail API token expired. Message: {Message}",
                        result.ResponseException?.ExceptionMessage);
                }

                if (!result.IsSuccess)
                {
                    _logger.LogWarning(
                        "Notification Detail API business error. StatusCode: {StatusCode}, Message: {Message}",
                        result.StatusCode,
                        result.ResponseException?.ExceptionMessage ?? result.Message);

                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting notification detail.");

                return null;
            }
        }

        public async Task<bool> SetAsReadAsync(string notificationId)
        {
            try
            {
                var token = TokenCache.Get();

                if (token is null || token.AccessTokenExpire <= DateTime.UtcNow)
                {
                    await _ecmsAuthService.RefreshTokenAsync();
                    token = TokenCache.Get();
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.AccessToken);

                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        value = notificationId
                    }),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(
                    _apiSettings.NotificationReadApi,
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "SetAsRead request failed. StatusCode: {StatusCode}, Response: {Response}",
                        response.StatusCode,
                        responseContent);

                    return false;
                }

                _logger.LogInformation(
                    "Notification {NotificationId} marked as read successfully.",
                    notificationId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception occurred while setting notification as read.");

                return false;
            }
        }


        public async Task<JsonElement?> GetBordersLegacyAsync()
        {
            try
            {
                // 1. Get token
                var tokenRequest = new
                {
                    client_id = _apiSettings.BorderClientId,
                    client_secret = _apiSettings.BorderClientSecret
                };

                var tokenJson = JsonSerializer.Serialize(tokenRequest);

                using var tokenContent = new StringContent(
                    tokenJson,
                    Encoding.UTF8,
                    "application/json");

                var tokenResponse = await _httpClient.PostAsync(
                    _apiSettings.BorderTokenApi,
                    tokenContent);

                var tokenResponseJson =
                    await tokenResponse.Content.ReadAsStringAsync();

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Token request failed. Status: {Status}, Response: {Response}",
                        tokenResponse.StatusCode,
                        tokenResponseJson);

                    return null;
                }

                var token = JsonSerializer.Deserialize<BorderTokenResponseDto>(
                    tokenResponseJson);

                if (token is null ||
                    string.IsNullOrWhiteSpace(token.AccessToken))
                {
                    _logger.LogError("Access token alınmadı.");
                    return null;
                }

                // 2. Get coords
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    _apiSettings.BordersApiLegacy);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token.AccessToken);

                var bordersResponse =
                    await _httpClient.SendAsync(request);

                var bordersJson =
                    await bordersResponse.Content.ReadAsStringAsync();

                if (!bordersResponse.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Borders request failed. Status: {Status}, Response: {Response}",
                        bordersResponse.StatusCode,
                        bordersJson);

                    return null;
                }

                // 3. Do not create a strict DTO for coordinates
                using var document =
                    JsonDocument.Parse(bordersJson);

                return document.RootElement.Clone();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting borders");

                return null;
            }
        }

        // Getting or caching token
        private async Task<string?> GetBorderTokenAsync()
        {
            // Checking for is there a valid cached token
            if (_memoryCache.TryGetValue(
                BorderTokenCacheKey,
                out string? cachedToken))
            {
                return cachedToken;
            }

            // If not — get a new one
            var tokenRequest = new
            {
                client_id = _apiSettings.BorderClientId,
                client_secret = _apiSettings.BorderClientSecret
            };

            var tokenJson = JsonSerializer.Serialize(tokenRequest);

            using var tokenContent = new StringContent(
                tokenJson,
                Encoding.UTF8,
                "application/json");

            using var tokenResponse = await _httpClient.PostAsync(
                _apiSettings.BorderTokenApi,
                tokenContent);

            var tokenResponseJson =
                await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var token =
                JsonSerializer.Deserialize<BorderTokenResponseDto>(
                    tokenResponseJson);

            if (token is null ||
                string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return null;
            }

            // Saving until expiration
            _memoryCache.Set(
                BorderTokenCacheKey,
                token.AccessToken,
                TimeSpan.FromSeconds(
                    Math.Max(1, token.ExpiresIn - 30)));

            return token.AccessToken;
        }


        // 2. Getting a specific PBF
        public async Task<byte[]?> GetBorderTileAsync(int z, int x, int y)
        {
            try
            {
                // Getting token from Cache.
                // If not — the method will get a new one.
                var token = await GetBorderTokenAsync();

                if (string.IsNullOrWhiteSpace(token))
                {
                    return null;
                }

                var url =
                    $"{_apiSettings.BordersApi}/{z}/{x}/{y}.pbf";

                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Get,
                        url);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);

                using var bordersResponse =
                    await _httpClient.SendAsync(request);

                if (!bordersResponse.IsSuccessStatusCode)
                {
                    var error =
                        await bordersResponse.Content.ReadAsStringAsync();

                    _logger.LogError(
                        "Border PBF request failed. Status: {Status}, Response: {Response}",
                        bordersResponse.StatusCode,
                        error);

                    return null;
                }

                return await bordersResponse.Content
                    .ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception occurred while getting Border PBF.");

                return null;
            }
        }


       



    }
}
