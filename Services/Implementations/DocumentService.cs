using CmsApi.Helpers;
using CmsApi.Http.Handlers.Interfaces;
using CmsApi.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CmsApi.Services.Implementations
{
    public class DocumentService : IDocumentService
    {
        private readonly ICmsHttpHandler _cmsHttpHandler;
        private static readonly MemoryCache _attachments = new(new MemoryCacheOptions());



        public DocumentService(ICmsHttpHandler cmsHttpHandler)
        {
            _cmsHttpHandler = cmsHttpHandler;
        }

        public async Task<byte[]> GetDocument(string attachmentId)
        {
            if (_attachments.TryGetValue(attachmentId, out byte[] bytes))
                return bytes;

            var result = await _cmsHttpHandler.GetDocumentAsBase64Async(attachmentId);

            var resultConvert = Base64FileTypeDetector.DetectContentType(result.Result.Content);

            bytes = resultConvert.Item2;

            _attachments.Set(
                attachmentId,
                bytes,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
                });

            return bytes;
        }
    }
}
