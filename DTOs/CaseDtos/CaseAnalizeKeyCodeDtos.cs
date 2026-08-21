namespace CmsApi.DTOs.CaseDtos
{
    public class CaseAnalizeKeyCodeDto
    {
        public string? KeyCode { get; set; }
        public List<CaseAnalizeKeywordDto> Keywords { get; set; } = new();
    }

    public class CaseAnalizeKeywordDto
    {
        public string? Keyword { get; set; }
    }
}
