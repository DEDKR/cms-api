namespace CmsApi.DTOs.ComplaintDtos
{
    public class ComplaintPartiesDto
    {
        public long? Id { get; set; }
        public string? PartyTypeName { get; set; }
        public ComplaintLegalPersonDto? LegalPerson { get; set; }
        public ComplaintPhysicalPersonDto? PhysicalPerson { get; set; }
    }
}
