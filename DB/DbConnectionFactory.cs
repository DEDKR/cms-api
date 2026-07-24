using Microsoft.Data.SqlClient;

namespace CmsApi.DB
{
    public sealed class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _config;

        public DbConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection CreateMsSqlConnection()
            => new SqlConnection(_config.GetConnectionString("MsSqlDb"));

    }
}
