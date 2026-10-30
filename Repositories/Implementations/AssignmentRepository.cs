using CmsApi.DB;
using CmsApi.DTOs.AssignmentDtos;
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

        // User lookup for participation. Never trust RO from request.
        public async Task<List<MeetingAssignmentUserDto>> GetEligibleUsersAsync(
            string? officeKeyCode, string? search, long? onlyUserId = null)
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
                  AND (@OnlyUserId IS NULL OR U.USER_ID = @OnlyUserId)
                  AND (@Search IS NULL OR
                       CONCAT(U.FIRST_NAME, N' ', U.LAST_NAME, N' ', U.FATHER_NAME) LIKE N'%' + @Search + N'%'
                       OR U.USERNAME LIKE N'%' + @Search + N'%')
                ORDER BY U.FIRST_NAME, U.LAST_NAME, U.USER_ID", connection);
            cmd.Parameters.Add("@Office", SqlDbType.VarChar, 10).Value =
                (object?)officeKeyCode ?? DBNull.Value;
            cmd.Parameters.Add("@OnlyUserId", SqlDbType.BigInt).Value =
                (object?)onlyUserId ?? DBNull.Value;
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

    }
}
