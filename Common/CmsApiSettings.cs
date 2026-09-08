namespace CmsApi.Common
{
    public class CmsApiSettings
    {
        public string AttachmentApi {  get; set; }
        public string LoginByAsanCertificateApi { get; set; }

        public string FileReaderApi {  get; set; }

        public string NotificationReadApi { get; set; }

        public string BorderTokenApi { get; set; } = null!;
        public string BordersApi { get; set; } = null!;
        public string BorderClientId { get; set; } = null!;
        public string BorderClientSecret { get; set; } = null!;

    }
}
