using CmsApi.Common;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.DocumentDtos;
using CmsApi.DTOs.HttpApiDtos;
using CmsApi.Helpers;
using CmsApi.Http.Handlers.Interfaces;
using CmsApi.Services.Interfaces;
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

        public CmsHttpHandler(
         HttpClient httpClient,
         IOptions<CmsApiSettings> options,
         ILogger<CmsHttpHandler> logger
,
         IECmsAuthService ecmsAuthService)
        {
            _httpClient = httpClient;
            _apiSettings = options.Value;
            _logger = logger;
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

        
    }
}
