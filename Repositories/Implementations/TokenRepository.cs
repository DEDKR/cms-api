using CmsApi.DB;
using CmsApi.Entities;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class TokenRepository : ITokenRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public TokenRepository(
            IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task UpsertRefreshTokenAsync(
            int userId,
            string tokenHash,
            DateTime expiresAt)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_UPSERT_REFRESH_TOKEN",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add(
                "@USER_ID",
                SqlDbType.Int).Value = userId;

            cmd.Parameters.Add(
                "@TOKEN_HASH",
                SqlDbType.NVarChar,
                500).Value = tokenHash;

            cmd.Parameters.Add(
                "@EXPIRES_AT",
                SqlDbType.DateTime2).Value = expiresAt;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<UserToken?> GetRefreshTokenAsync(
         
            string tokenHash)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_GET_REFRESH_TOKEN",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add(
                "@TOKEN_HASH",
                SqlDbType.NVarChar,
                500).Value = tokenHash;
           

            using var reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new UserToken
            {
                TokenId = reader.SafeGet<long>("ID"),

                UserId = reader.SafeGet<int>("USER_ID"),

                TokenHash =
                    reader.SafeGet<string>("TOKEN_HASH")
                    ?? string.Empty,

                ExpiresAt =
                    reader.SafeGet<DateTime>("EXPIRES_AT"),

                IsRevoked =
                    reader.SafeGet<bool>("IS_REVOKED"),

                CreatedAt =
                    reader.SafeGet<DateTime>("CREATED_AT")
            };
        }

        public async Task RevokeRefreshTokenAsync(
            int userId)
        {
            using var connection =
                _dbConnectionFactory.CreateMsSqlConnection();

            await connection.OpenAsync();

            using var cmd = new SqlCommand(
                "P_AUTH_REVOKE_REFRESH_TOKEN",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add(
                "@USER_ID",
                SqlDbType.Int).Value = userId;

            await cmd.ExecuteNonQueryAsync();
        }
    }
}