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
    string? officeKeyCode,
    string? search,
    long? onlyUserId = null)
        {
            var users = new List<MeetingAssignmentUserDto>();

            using var connection = _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "dbo.P_GET_MEETING_ELIGIBLE_USERS", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.VarChar, 10).Value =
                (object?)officeKeyCode ?? DBNull.Value;

            cmd.Parameters.Add("@SEARCH", SqlDbType.NVarChar, 100).Value =
                string.IsNullOrWhiteSpace(search)
                    ? DBNull.Value
                    : search.Trim()[..Math.Min(100, search.Trim().Length)];

            cmd.Parameters.Add("@ONLY_USER_ID", SqlDbType.BigInt).Value =
                (object?)onlyUserId ?? DBNull.Value;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string? GetString(string name) =>
                    reader[name] == DBNull.Value
                        ? null
                        : Convert.ToString(reader[name]);

                int? GetInt(string name) =>
                    reader[name] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader[name]);

                var office = GetString("OFFICE_VALUE");
                var position = GetInt("POSITION_VALUE");
                var department = GetInt("DEPARTMENT_VALUE");

                users.Add(new MeetingAssignmentUserDto
                {
                    UserId = Convert.ToInt64(reader["USER_ID"]),
                    FullName = GetString("FULL_NAME") ?? "",
                    Username = GetString("USERNAME") ?? "",

                    OfficeKeyCode = office == null ? null :
                        new ParticipationOptionDto<string>
                        {
                            Value = office,
                            Label = GetString("OFFICE_LABEL")
                        },

                    Position = !position.HasValue ? null :
                        new ParticipationOptionDto<int>
                        {
                            Value = position.Value,
                            Label = GetString("POSITION_LABEL")
                        },

                    Department = !department.HasValue ? null :
                        new ParticipationOptionDto<int>
                        {
                            Value = department.Value,
                            Label = GetString("DEPARTMENT_LABEL")
                        }
                });
            }

            return users;
        }

    }
}
