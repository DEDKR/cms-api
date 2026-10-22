namespace CmsApi.DTOs.PretenseDtos
{
    public class PretenseSignerDto
    {
        public long? Id { get; set; }
        public string? SignerCertId { get; set; }
        public string? SignerKey { get; set; }
        public string? SignerName { get; set; }
        public string? SignerPosition{ get; set; }
    }
}
