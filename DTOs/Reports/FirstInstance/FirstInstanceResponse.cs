namespace CmsApi.DTOs.Reports.FirstInstance
{
    public class FirstInstanceResponse
    {
        public int TotalCourts { get; set; }
        public int TotalByResults { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int PartialClaims { get; set; }
        public int RejectedByCaseTermination { get; set; }
        public int NotConsideredClaims { get; set; }
        public int RespondedAppeals { get; set; }
        public string RegionalOfficeName { get; set; } = string.Empty;
        public string CourtName { get; set; } = string.Empty;
        // Stable identifiers for correct parent/child court grouping.
        // Added without changing the names of the existing JSON properties.
        public string RegionalOfficeKeyCode { get; set; } = string.Empty;
        public int? CourtId { get; set; }
    }
}
