

namespace CmsApi.DTOs.PretenseDtos
{
    public class PretenseDetailsDto
    {
        public PretenseCaseViewDto? CaseView { get; set; }
        public List<PretensePartiesDto?> Parties { get; set; } = [];
        public List<PretenseDocumentsDto?> Documents { get; set; } = [];
        public PretenseApplicationContextDto ApplicationContext { get; set; } = new();
        public PretenseSignerDto? Signers { get; set; } = new();
    }
}
