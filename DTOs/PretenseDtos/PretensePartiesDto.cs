

namespace CmsApi.DTOs.PretenseDtos
{
    public class PretensePartiesDto
    {
        public long? Id { get; set; }
        public string? PartyTypeName { get; set; }
        public string? PersonTypeName { get; set; }
        public PretenseLegalPersonDto? LegalPerson { get; set; }
    }
}
