using CmsApi.DB;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public AssignmentRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<object> SetROToUserAsync(long userId, int roId)
        {
            using var connection = _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(
                "P_SET_RO_TO_USER",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@RO_ID", SqlDbType.Int).Value = roId;

            var affectedRows = await cmd.ExecuteScalarAsync();

            return new
            {
                message = "Əİ uğurla təyin olundu"
            };
        }

        public async Task<object> SetMeetToUserAsync(long userId, long meetId, long attendedBy)
        {
            using var connection = _dbConnectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(
                "P_SET_MEET_TO_USER",
                connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@USER_ID", SqlDbType.BigInt).Value = userId;
            cmd.Parameters.Add("@MEETING_ID", SqlDbType.BigInt).Value = meetId;
            cmd.Parameters.Add("@ATTENDED_BY", SqlDbType.BigInt).Value = attendedBy;

            var affectedRows = await cmd.ExecuteScalarAsync();

            return new
            {
                message = "İclas uğurla təyin olundu"
            };
        }


    }
}
