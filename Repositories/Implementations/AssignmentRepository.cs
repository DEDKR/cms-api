using CmsApi.DB;
using CmsApi.DTOs.AssignmentDtos;
using System.Text.Json;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public AssignmentRepository(
            IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> SetROToUserAsync(
            long userId,
            string? roId)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_SET_RO_TO_USER",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.BigInt)
                .Value = userId;

            cmd.Parameters.Add("@RO_ID", SqlDbType.VarChar, 10)
                .Value = string.IsNullOrWhiteSpace(roId)
                    ? DBNull.Value
                    : roId.Trim();

            var affectedRows = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(affectedRows ?? 0) > 0;
        }

        public async Task<bool> SetMeetToUserAsync(
            long userId,
            long meetId,
            long attendedBy)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_SET_MEET_TO_USER",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.BigInt)
                .Value = userId;

            cmd.Parameters.Add("@MEETING_ID", SqlDbType.BigInt)
                .Value = meetId;

            cmd.Parameters.Add("@ATTENDED_BY", SqlDbType.BigInt)
                .Value = attendedBy;

            var result = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(result ?? 0) > 0;
        }

        // This endpoint is for assignment dropdown only. Never trust RO from request.
        public async Task<List<MeetingAssignmentUserDto>> GetEligibleUsersAsync(
            string? officeKeyCode, string? search)
        {
            var users = new List<MeetingAssignmentUserDto>();
            using var connection = _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT TOP (100)
                    U.USER_ID,
                    U.USERNAME,
                    U.REGIONAL_OFFICE_ID AS OFFICE_KEY_CODE,
                    LTRIM(RTRIM(CONCAT(
                        COALESCE(U.FIRST_NAME, N''), N' ',
                        COALESCE(U.LAST_NAME, N''), N' ',
                        COALESCE(U.FATHER_NAME, N'')))) AS FULL_NAME
                FROM dbo.TBL_USERS U
                WHERE U.IS_ACTIVE = 1
                  AND (@Office IS NULL OR U.REGIONAL_OFFICE_ID = @Office)
                  AND (@Search IS NULL OR
                       CONCAT(U.FIRST_NAME, N' ', U.LAST_NAME, N' ', U.FATHER_NAME) LIKE N'%' + @Search + N'%'
                       OR U.USERNAME LIKE N'%' + @Search + N'%')
                ORDER BY U.FIRST_NAME, U.LAST_NAME, U.USER_ID", connection);
            cmd.Parameters.Add("@Office", SqlDbType.VarChar, 10).Value =
                (object?)officeKeyCode ?? DBNull.Value;
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value =
                string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim().Substring(0, Math.Min(100, search.Trim().Length));
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                users.Add(new MeetingAssignmentUserDto
                {
                    UserId = Convert.ToInt64(reader["USER_ID"]),
                    Username = reader["USERNAME"]?.ToString() ?? "",
                    OfficeKeyCode = reader["OFFICE_KEY_CODE"] == DBNull.Value ? null : reader["OFFICE_KEY_CODE"].ToString(),
                    FullName = reader["FULL_NAME"]?.ToString() ?? ""
                });
            return users;
        }

        // One SQL round-trip, one transaction, no partial assignment if an ID is invalid.
        public async Task<MeetAssignmentResultDto> SetMeetsToUsersAsync(
            IReadOnlyCollection<long> meetIds,
            IReadOnlyCollection<long> userIds,
            long attendedBy,
            string? officeKeyCode,
            string? assignmentNote)
        {
            using var connection = _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            using var cmd = new SqlCommand("P_SET_MEETS_TO_USERS", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.Add("@MEET_IDS_JSON", SqlDbType.NVarChar, -1)
                .Value = JsonSerializer.Serialize(meetIds);
            cmd.Parameters.Add("@USER_IDS_JSON", SqlDbType.NVarChar, -1)
                .Value = JsonSerializer.Serialize(userIds);
            cmd.Parameters.Add("@ATTENDED_BY", SqlDbType.BigInt).Value = attendedBy;
            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.VarChar, 10)
                .Value = (object?)officeKeyCode ?? DBNull.Value;

            cmd.Parameters.Add("@ASSIGNMENT_NOTE", SqlDbType.NVarChar, 1500).Value =
                (object?)assignmentNote?.Trim() ?? DBNull.Value;

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new InvalidOperationException(
                    "P_SET_MEETS_TO_USERS returned no result.");

            return new MeetAssignmentResultDto
            {
                TotalPairs = Convert.ToInt32(reader["TOTAL_PAIRS"]),
                CreatedAssignments = Convert.ToInt32(reader["CREATED_ASSIGNMENTS"]),
                UpdatedAssignments = Convert.ToInt32(reader["UPDATED_ASSIGNMENTS"])
            };
        }
    }
}
