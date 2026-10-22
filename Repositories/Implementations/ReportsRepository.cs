using CmsApi.DB;
using CmsApi.DTOs.Reports.Appeals;
using CmsApi.DTOs.Reports.Cassasion;
using CmsApi.DTOs.Reports.FirstInstance;
using CmsApi.DTOs.Reports.Meeting;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public ReportsRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<List<FirstInstanceResponse>> GetFirstInstanceReportAsync(
    FirstInstanceRequest payload, string? officeKeyCode)
        {
            var result = new List<FirstInstanceResponse>();

            using var connection = _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_GET_1ST_INSTANCE_STATS",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@FROM", SqlDbType.Date).Value =
                (object?)payload.From ?? DBNull.Value;

            cmd.Parameters.Add("@TO", SqlDbType.Date).Value =
                (object?)payload.To ?? DBNull.Value;

            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.NVarChar, 50).Value =
                (object?)officeKeyCode ?? DBNull.Value;

            cmd.Parameters.Add("@BY_COURT", SqlDbType.Bit).Value = payload.ByCourt;

            cmd.Parameters.Add("@COURT_TYPE_ID", SqlDbType.Int).Value =
                (object?)payload.CourtTypeId ?? DBNull.Value;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = new FirstInstanceResponse
                {
                    RegionalOfficeName = reader["REGIONAL_OFFICE_NAME"].ToString() ?? string.Empty,

                    CourtName = reader["COURT_NAME"].ToString()
                                ?? string.Empty,

                    TotalCourts = Convert.ToInt32(
                        reader["TOTAL_COURTS"]),

                    TotalByResults = Convert.ToInt32(
                        reader["TOTAL_BY_RESULTS"]),

                    ApprovedClaims = Convert.ToInt32(
                        reader["APPROVED_CLAIMS"]),

                    RejectedClaims = Convert.ToInt32(
                        reader["REJECTED_CLAIMS"]),

                    PartialClaims = Convert.ToInt32(
                        reader["PARTIAL_CLAIMS"]),

                    RejectedByCaseTermination = Convert.ToInt32(
                        reader["REJECTED_BY_CASE_TERMINATION"]),

                    NotConsideredClaims = Convert.ToInt32(
                        reader["NOT_CONSIDERED_CLAIMS"]),

                    RespondedAppeals = Convert.ToInt32(
                        reader["RESPONDENT_APPEALS"])
                };

                result.Add(item);
            }

            return result;
        }

        public async Task<List<AppealResponse>> GetAppealReportAsync(
    AppealRequest payload, string? officeKeyCode)
        {
            var result = new List<AppealResponse>();

            using var connection = _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_GET_APPEAL_STATS",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@FROM", SqlDbType.Date).Value =
                (object?)payload.From ?? DBNull.Value;

            cmd.Parameters.Add("@TO", SqlDbType.Date).Value =
                (object?)payload.To ?? DBNull.Value;

            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.NVarChar, 50).Value =
                (object?)officeKeyCode ?? DBNull.Value;

            cmd.Parameters.Add("@BY_COURT", SqlDbType.Bit).Value = payload.ByCourt;

            cmd.Parameters.Add("@COURT_TYPE_ID", SqlDbType.Int).Value =
                (object?)payload.CourtTypeId ?? DBNull.Value;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = new AppealResponse
                {
                    RegionalOfficeName = reader["REGIONAL_OFFICE_NAME"].ToString() ?? string.Empty,

                    CourtName = reader["COURT_NAME"].ToString()
                                ?? string.Empty,

                    Total = Convert.ToInt32(
                        reader["TOTAL"]),

                    Approved = Convert.ToInt32(
                        reader["APPROVED"]),

                    Rejected = Convert.ToInt32(
                        reader["REJECTED"]),

                    PartiallyApproved = Convert.ToInt32(
                        reader["PARTIALLY_APPROVED"])
                };

                result.Add(item);
            }

            return result;
        }

        public async Task<List<CassasionResponse>> GetCassasionReportAsync(
    CassasionRequest payload, string? officeKeyCode)
        {
            var result = new List<CassasionResponse>();

            using var connection = _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_GET_CASSASION_STATS",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@FROM", SqlDbType.Date).Value =
                (object?)payload.From ?? DBNull.Value;

            cmd.Parameters.Add("@TO", SqlDbType.Date).Value =
                (object?)payload.To ?? DBNull.Value;

            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.NVarChar, 50).Value =
                (object?)officeKeyCode ?? DBNull.Value;

            cmd.Parameters.Add("@BY_COURT", SqlDbType.Bit).Value = payload.ByCourt;

            cmd.Parameters.Add("@COURT_TYPE_ID", SqlDbType.Int).Value =
                (object?)payload.CourtTypeId ?? DBNull.Value;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = new CassasionResponse
                {
                    RegionalOfficeName = reader["REGIONAL_OFFICE_NAME"].ToString() ?? string.Empty,

                    CourtName = reader["COURT_NAME"].ToString()
                                ?? string.Empty,

                    Total = Convert.ToInt32(
                        reader["TOTAL"]),

                    Approved = Convert.ToInt32(
                        reader["APPROVED"]),

                    Rejected = Convert.ToInt32(
                        reader["REJECTED"]),

                    PartiallyApproved = Convert.ToInt32(
                        reader["PARTIALLY_APPROVED"])
                };

                result.Add(item);
            }

            return result;
        }

        public async Task<List<MeetingResponse>> GetMeetingReportAsync(
    MeetingRequest payload, string? officeKeyCode)
        {
            var result = new List<MeetingResponse>();

            using var connection = _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_GET_COURT_MEETINGS_COUNT",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@FROM", SqlDbType.Date).Value =
                (object?)payload.From ?? DBNull.Value;

            cmd.Parameters.Add("@TO", SqlDbType.Date).Value =
                (object?)payload.To ?? DBNull.Value;

            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.NVarChar, 50).Value =
                (object?)officeKeyCode ?? DBNull.Value;

            cmd.Parameters.Add("@BY_COURT", SqlDbType.Bit).Value = payload.ByCourt;

            cmd.Parameters.Add("@COURT_TYPE_ID", SqlDbType.Int).Value =
                (object?)payload.CourtTypeId ?? DBNull.Value;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var item = new MeetingResponse
                {
                    RegionalOfficeName = reader["REGIONAL_OFFICE_NAME"].ToString() ?? string.Empty,

                    CourtName = reader["COURT_NAME"].ToString()
                                ?? string.Empty,

                    Total = Convert.ToInt32(
                        reader["TOTAL"])
                };

                result.Add(item);
            }

            return result;
        }


    }
}
