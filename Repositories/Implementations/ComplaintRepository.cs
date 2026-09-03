using CmsApi.DB;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ComplaintRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<ComplaintListDto?>> GetComplaintsAsync(ComplaintListRequestDto request) {
            try
            {
                var result = new PagedResult<ComplaintListDto>
                {
                    PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber,
                    PageSize = request.PageSize <= 0 || request.PageSize > 10000 ? 10 : request.PageSize
                };

                using var connection = _connectionFactory.CreateMsSqlConnection();
                await connection.OpenAsync();

                using var cmd = new SqlCommand("P_GET_COMPLAINTS", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                cmd.Parameters.Add("@CASE_NO", SqlDbType.NVarChar, 100).Value = (object?)request.CaseNo ?? DBNull.Value;    
                cmd.Parameters.Add("@START_DATE", SqlDbType.Date).Value = (object?)request.StartDate ?? DBNull.Value;
                cmd.Parameters.Add("@END_DATE", SqlDbType.Date).Value = (object?)request.EndDate ?? DBNull.Value;
                cmd.Parameters.Add("@COURT_ID", SqlDbType.Int).Value = (object?)request.CourtId ?? DBNull.Value;
                cmd.Parameters.Add("@APPLICATION_TYPE_ID", SqlDbType.Int).Value = (object?)request.ApplicationTypeId ?? DBNull.Value;
                cmd.Parameters.Add("@STATE_ID", SqlDbType.Int).Value = (object?)request.CaseStatus ?? DBNull.Value;
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = result.PageNumber;
                cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = result.PageSize;

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (result.TotalCount == 0)
                    {
                        result.TotalCount = reader.SafeGet<int>("TOTAL_COUNT");
                    }

                    result.Items.Add(new ComplaintListDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        IdView = reader.SafeGet<string>("ID_VIEW"),
                        CaseNo = reader.SafeGet<string>("CASE_NO"),
                        CourtName = reader.SafeGet<string>("COURT_NAME"),
                        ApplicationName = reader.SafeGet<string>("APPLICATION_NAME"),
                        InsertDate = reader.SafeGet<DateTime?>("INSERT_DATE"),
                        SendDate = reader.SafeGet<DateTime?>("SEND_DATE"),
                        StateName = reader.SafeGet<string>("STATE_NAME")
                    });
                }


                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        


    }
}
