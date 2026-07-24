using CmsApi.DB;
using CmsApi.Entities;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class ECmsTokenRepository : IECmsTokenRepository
    {
        public readonly IDbConnectionFactory _dbConnectionFactory;

        public ECmsTokenRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Token?> GetTokenAsync()
        {
            await using var connection = _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_CMS_TOKEN_GET", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Token
            {
                Id = reader.SafeGet<string>("ID"),
                UserId = reader.SafeGet<int>("USER_ID"),
                AccessToken = reader.SafeGet<string>("ACCESS_TOKEN"),
                RefreshToken = reader.SafeGet<string>("REFRESH_TOKEN"),
                AccessTokenExpire = reader.SafeGet<DateTime>("ACCESS_TOKEN_EXPIRE"),
                RefreshTokenExpire = reader.SafeGet<DateTime>("REFRESH_TOKEN_EXPIRE"),
                UpdatedAt = reader.SafeGet<DateTime>("UPDATED_AT")
            };
        }
        public async Task<int> UpsertAsync(Token token)
        {
            using var connection = _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand("[dbo].[P_CMS_TOKEN_UPSERT]", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@USER_ID", token.UserId);
            command.Parameters.AddWithValue("@ACCESS_TOKEN", token.AccessToken);
            command.Parameters.AddWithValue("@REFRESH_TOKEN", token.RefreshToken);
            command.Parameters.AddWithValue("@ACCESS_TOKEN_EXPIRE", token.AccessTokenExpire);
            command.Parameters.AddWithValue("@REFRESH_TOKEN_EXPIRE", token.RefreshTokenExpire);

            return await command.ExecuteNonQueryAsync();
        }
    }
}
