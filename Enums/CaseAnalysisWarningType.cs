namespace CmsApi.Enums
{
    public enum CaseAnalysisWarningType
    {
        EnforcementAuthorityNotFound = 1,
        CaseSubjectNotFound = 2,
        StatementOfClaimNotUploaded = 3,
        FinalCourtActNotUploaded = 4,
        NoAnalyzableTextFound = 5,
        NoSuitableDocumentsForAnalysisFound = 6,
        DocumentCouldNotBeRetrievedFromSource = 7
    }
}
