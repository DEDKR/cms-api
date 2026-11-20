namespace CmsApi.Enums
{
    public static class ReferenceDataQueries
    {
        public static readonly Dictionary<ReferenceDataType, string> Procedures = new()
    {
        { ReferenceDataType.CaseTypes, "P_GET_CASE_TYPES" },
        { ReferenceDataType.MeetTypes, "P_GET_MEETING_TYPES" },
        { ReferenceDataType.CaseStatuses, "P_GET_CASE_STATUSES" },
        { ReferenceDataType.ComplaintStatuses, "P_GET_COMPLAINT_STATUSES" },
        { ReferenceDataType.PretenseStatuses, "P_GET_PRETENSE_STATUSES" },
        { ReferenceDataType.MeetingStatuses, "P_GET_MEETING_STATUSES" },
        { ReferenceDataType.Courts, "P_GET_COURTS" },
        { ReferenceDataType.Judges, "P_GET_JUDGES" },
        { ReferenceDataType.CourtTypes, "P_GET_COURT_TYPES" },
        { ReferenceDataType.RealEstateTypes, "P_GET_REAL_ESTATE_TYPES" },
        { ReferenceDataType.RegionalOffices, "P_GET_REGIONAL_OFFICES" },

    };

        public static readonly Dictionary<ReferenceDataType, string> SuccessMessages = new()
        {
            { ReferenceDataType.CaseTypes, "İş növləri uğurla alındı" },
            { ReferenceDataType.Courts, "Məhkəmələr uğurla alındı" },
            { ReferenceDataType.Judges, "Hakimlər uğurla alındı" },
            { ReferenceDataType.MeetTypes, "Iclas novleri uğurla alındı" }
        };
    }
}
