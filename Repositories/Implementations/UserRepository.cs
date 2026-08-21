using CmsApi.DB;
using CmsApi.Entities;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_GET_USER_BY_ID",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = userId;

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapUser(reader);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_GET_USER_BY_USERNAME",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USERNAME", SqlDbType.NVarChar, 100)
                .Value = username;

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return MapUser(reader);
        }

        public async Task<bool> RegisterFailedLoginAsync(
            int userId,
            int maxAttempts,
            int lockoutMinutes)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_REGISTER_FAILED_LOGIN",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.Int)
                .Value = userId;

            cmd.Parameters.Add("@MAX_ATTEMPTS", SqlDbType.Int)
                .Value = maxAttempts;

            cmd.Parameters.Add("@LOCKOUT_MINUTES", SqlDbType.Int)
                .Value = lockoutMinutes;

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return false;

            var lockoutUntilOrdinal =
                reader.GetOrdinal("LOCKOUT_UNTIL");

            return !reader.IsDBNull(lockoutUntilOrdinal);
        }

        public async Task<bool> ResetLoginAttemptsAsync(int userId)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_RESET_LOGIN_ATTEMPTS",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.Int)
                .Value = userId;

            var affectedRows = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(affectedRows) > 0;
        }

        public async Task<bool> UpdatePasswordAsync(
            int userId,
            string passHash,
            string passOrg,
            bool isPassChangeRequired)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_UPDATE_PASSWORD",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.Int)
                .Value = userId;

            cmd.Parameters.Add("@PASS_HASH", SqlDbType.NVarChar, 500)
                .Value = passHash;

            cmd.Parameters.Add("@PASS_ORG", SqlDbType.NVarChar, 500)
              .Value = passOrg;

            var affectedRows = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(affectedRows) > 0;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.SafeGet<int>("USER_ID"),

                RoleId = reader.SafeGet<byte?>("ROLE_ID"),

                Role = reader.SafeGet<string?>("ROLE"),

                FirstName = reader.SafeGet<string?>("FIRST_NAME"),

                LastName = reader.SafeGet<string?>("LAST_NAME"),

                FatherName = reader.SafeGet<string?>("FATHER_NAME"),

                Pin = reader.SafeGet<string?>("PIN"),

                Username = reader.SafeGet<string?>("USERNAME"),

                PassHash = reader.SafeGet<string?>("PASS_HASH"),
                Password = reader.SafeGet<string?>("PASSWORD"),

                IsPassChangeRequired =
                    reader.SafeGet<bool>("IS_PASS_CHANGE_REQUIRED"),

                IsActive =
                    reader.SafeGet<bool>("IS_ACTIVE"),

                InsertDate =
                    reader.SafeGet<DateTime?>("INSERT_DATE"),

                PassChangeAt =
                    reader.SafeGet<DateTime?>("PASS_CHANGE_AT"),

                FailedLoginCount =
                    reader.SafeGet<int>("FAILED_LOGIN_COUNT"),

                LockoutUntil =
                    reader.SafeGet<DateTime?>("LOCKOUT_UNTIL")
            };
        }
    }
}