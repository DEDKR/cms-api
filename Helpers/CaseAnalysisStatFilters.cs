namespace CmsApi.Helpers
{
    public static class CaseAnalysisStatFilters
    {
        public const string TotalCases = "TOTAL_CASES";
        public const string TotalAnalyzed = "TOTAL_ANALIZED";
        public const string TotalHasResult = "TOTAL_ANALIZE_HAS_RESULT";
        public const string BothResult = "BOTH_RESULT";
        public const string SubjectNotFound = "CASE_SUBJECT_IS_NOT_FINDINGS";
        public const string OfficeNotFound = "CASE_OFFICE_IS_NOT_FINDINGS";
        public const string TotalError = "TOTAL_ERROR_COUNT";

        private static readonly HashSet<string> Allowed =
            new(StringComparer.Ordinal)
            {
                TotalCases,
                TotalAnalyzed,
                TotalHasResult,
                BothResult,
                SubjectNotFound,
                OfficeNotFound,
                TotalError
            };

        public static bool IsValid(string? value)
        {
            return value is null || Allowed.Contains(value);
        }
    }
}
