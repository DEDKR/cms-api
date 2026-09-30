using CmsApi.DB;
using CmsApi.DTOs.UserDtos;
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


        public async Task<UserResponseDto> GetUsers(int pageSize, int pageNumber)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_GET_USERS",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@PAGE_NUMBER", SqlDbType.Int)
                .Value = pageNumber;

            cmd.Parameters.Add("@PAGE_SIZE", SqlDbType.Int)
                .Value = pageSize;

            using var reader = await cmd.ExecuteReaderAsync();

            var result = new UserResponseDto();

            // 1. Pagination
            if (await reader.ReadAsync())
            {
                result.PageNumber = reader.SafeGet<int>("PAGE_NUMBER");
                result.PageSize = reader.SafeGet<int>("PAGE_SIZE");
                result.TotalCount = reader.SafeGet<int>("TOTAL_COUNT");
                result.TotalPages = reader.SafeGet<int>("TOTAL_PAGES");
                result.HasPrev = reader.SafeGet<bool>("HAS_PREV");
                result.HasNext = reader.SafeGet<bool>("HAS_NEXT");
            }

            // Keçirik 2-ci result set-ə
            if (await reader.NextResultAsync())
            {
                // 2. Users
                while (await reader.ReadAsync())
                {
                    result.Items.Add(new UserItemDto
                    {
                        UserId = reader.SafeGet<int>("USER_ID"),
                        RoleId = reader.SafeGet<int>("ROLE_ID"),
                        RoleName = reader.SafeGet<string>("ROLE_NAME"),
                        FirstName = reader.SafeGet<string>("FIRST_NAME"),
                        LastName = reader.SafeGet<string>("LAST_NAME"),
                        FatherName = reader.SafeGet<string>("FATHER_NAME"),
                        Pin = reader.SafeGet<string>("PIN"),
                        Username = reader.SafeGet<string>("USERNAME"),
                        IsActive = reader.SafeGet<bool>("IS_ACTIVE"),
                        InsertDate = reader.SafeGet<string>("INSERT_DATE"),
                        PassChangeAt = reader.SafeGet<string>("PASS_CHANGE_AT"),
                        RegionalOfficeId = reader.SafeGet<string>("REGIONAL_OFFICE_ID"),
                        RegionalOfficeName = reader.SafeGet<string>("OFFICE_NAME")
                    });
                }
            }

            return result;
        }


        public async Task<List<UserRolesResponse>> GetUserRoles()
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(
                "P_GET_ROLES",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            using var reader = await cmd.ExecuteReaderAsync();
            var result = new List<UserRolesResponse>();
            while (await reader.ReadAsync())
            {
                result.Add(new UserRolesResponse
                {
                    RoleId = reader.SafeGet<int>("ROLE_ID"),
                    Name = reader.SafeGet<string>("ROLE_NAME")
                });
            }
            return result;
        }

    }
}