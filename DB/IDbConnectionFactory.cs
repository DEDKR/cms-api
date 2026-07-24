using Microsoft.Data.SqlClient;

namespace CmsApi.DB
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateMsSqlConnection();
    }
}
