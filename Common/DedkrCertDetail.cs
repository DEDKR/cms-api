using System.Text.Json.Serialization;

namespace CmsApi.Common
{
    public class DedkrCertDetail
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        [JsonPropertyName("personalCode")]
        public string PersonalCode { get; set; }

        [JsonPropertyName("organizationName")]
        public string OrganizationName { get; set; }

        [JsonPropertyName("organizationCode")]
        public string OrganizationCode { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("certType")]
        public int CertType { get; set; }

        [JsonPropertyName("informationSystemName")]
        public string InformationSystemName { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("certificateKey")]
        public string CertificateKey { get; set; }

        [JsonPropertyName("certificateValue")]
        public string CertificateValue { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("asanUserId")]
        public string AsanUserId { get; set; }

    }
}
