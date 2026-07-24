using CmsApi.DB;
using CmsApi.DTOs;
using CmsApi.Enums;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class ReferenceDataRepository : IReferenceDataRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReferenceDataRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ReferenceData>> GetDataAsync(
             ReferenceDataType referenceDataQueryType,
             int? courtId = null,
             string? parametr = null)
        {
            var result = new List<ReferenceData>();

            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            if (!ReferenceDataQueries.Procedures.TryGetValue(referenceDataQueryType, out var procedureName))
            {
                throw new ArgumentException($"Invalid reference data type: {referenceDataQueryType}");
            }

            using var cmd = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (courtId.HasValue)
            {
                cmd.Parameters.Add("@COURT_ID", SqlDbType.Int).Value = courtId.Value;
            }

            if (!string.IsNullOrWhiteSpace(parametr))
            {
                cmd.Parameters.Add("@PARAMETR", SqlDbType.NVarChar, 200).Value = parametr;
            }

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new ReferenceData(
                    reader["value"],
                    reader.SafeGet<string>("label")));
            }

            return result;
        }
    }
}
