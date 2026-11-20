using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.DocumentDtos;
using CmsApi.DTOs.HttpApiDtos;
using System.Text.Json;

namespace CmsApi.Http.Handlers.Interfaces
{
    public interface ICmsHttpHandler
    {
        Task<bool> SetAsReadAsync(string notificationId);

        Task<CmsApiResponse<DocumentDto>> GetDocumentAsBase64Async(string attachmentId);
        Task<JsonElement?> GetBordersLegacyAsync();
        Task<byte[]?> GetBorderTileAsync(int z, int x, int y);
    }
}
