using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.DocumentDtos;
using CmsApi.DTOs.HttpApiDtos;

namespace CmsApi.Http.Handlers.Interfaces
{
    public interface ICmsHttpHandler
    {
        Task<bool> SetAsReadAsync(string notificationId);

        Task<CmsApiResponse<DocumentDto>> GetDocumentAsBase64Async(string attachmentId);
    }
}
