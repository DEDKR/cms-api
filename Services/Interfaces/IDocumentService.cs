namespace CmsApi.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<byte[]> GetDocument(string attachmentId);

        string ExtractText(byte[] pdfBytes);
    }
}
