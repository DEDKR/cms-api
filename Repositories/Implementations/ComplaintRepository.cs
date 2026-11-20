using CmsApi.DB;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.DTOs.Meeting;
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
                cmd.Parameters.Add("@STATE_ID", SqlDbType.Int).Value = (object?)request.Status ?? DBNull.Value;
                cmd.Parameters.Add("@ID_VIEW", SqlDbType.NVarChar, 100).Value = (object?)request.IdView ?? DBNull.Value;
                cmd.Parameters.Add("@VOEN", SqlDbType.NVarChar, 100).Value = (object?)request.Voen ?? DBNull.Value;
                cmd.Parameters.Add("@DOC_NUMBER", SqlDbType.NVarChar, 50).Value = (object?)request.DocNumber ?? DBNull.Value;
                cmd.Parameters.Add("@PHYSICAL_NAME", SqlDbType.NVarChar, 50).Value = (object?)request.PhysicalName ?? DBNull.Value;
                cmd.Parameters.Add("@PHYSICAL_SURNAME", SqlDbType.NVarChar, 50).Value = (object?)request.PhysicalSurname ?? DBNull.Value;
                cmd.Parameters.Add("@PHYSICAL_LAST_NAME", SqlDbType.NVarChar, 50).Value = (object?)request.PhysicalLastName ?? DBNull.Value;
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
        

        public async Task<ComplaintDetailsDto?> GetComplaintDetailsAsync(long complaintId) {
            try
            {
                var result = new ComplaintDetailsDto();

                using var connection = _connectionFactory.CreateMsSqlConnection();
                await connection.OpenAsync();

                using var cmd = new SqlCommand("P_GET_COMPLAINT_DETAILS", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = complaintId;

                using var reader = await cmd.ExecuteReaderAsync();


                // 1st Table

                if (await reader.ReadAsync())
                {
                    result.CaseView = new ComplaintCaseViewDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        ApplicationHeader = reader.SafeGet<string>("APPLICATION_HEADER"),
                        IdView = reader.SafeGet<string>("ID_VIEW"),
                        CaseNo = reader.SafeGet<string>("CASE_NO"),
                        CourtName = reader.SafeGet<string>("COURT_NAME"),
                        DocNo = reader.SafeGet<string>("DOC_NO"),
                        DecisionTypeName = reader.SafeGet<string>("DECISION_TYPE_NAME"),
                        DecisionDate = reader.SafeGet<DateTime?>("DECISION_DATE"),
                        InsertDate = reader.SafeGet<DateTime?>("INSERT_DATE"),
                        SendDate = reader.SafeGet<DateTime?>("SEND_DATE"),
                        StateName = reader.SafeGet<string>("STATE_NAME")
                    };
                }


                // 2nd Table

                await reader.NextResultAsync();

                while (await reader.ReadAsync())
                {
                    result.Parties.Add(new ComplaintPartiesDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        PartyTypeName = reader.SafeGet<string>("PARTY_TYPE_NAME"),
                        LegalPerson = reader.SafeGet<string>("LEGAL_FULL_NAME") != null ? new ComplaintLegalPersonDto
                        {
                            Id = reader.SafeGet<long>("LEGAL_PERSON_ID"),
                            FullName = reader.SafeGet<string>("LEGAL_FULL_NAME"),
                            Voen = reader.SafeGet<string>("VOEN"),
                            Email = reader.SafeGet<string>("LEGAL_EMAIL"),
                            Phone = reader.SafeGet<string>("LEGAL_PHONE")
                        } : null,
                        PhysicalPerson = reader.SafeGet<string>("PHYSICAL_NAME") != null ? new ComplaintPhysicalPersonDto
                        {
                            Id = reader.SafeGet<long>("PHYSICAL_PERSON_ID"),
                            Name = reader.SafeGet<string>("PHYSICAL_NAME"),
                            SurName = reader.SafeGet<string>("PHYSICAL_SURNAME"),
                            LastName = reader.SafeGet<string>("PHYSICAL_LAST_NAME"),
                            DocSerial = reader.SafeGet<string>("DOC_SERIAL"),
                            DocNumber = reader.SafeGet<string>("DOC_NUMBER"),
                            Email = reader.SafeGet<string>("PHYSICAL_EMAIL"),
                            Phone = reader.SafeGet<string>("PHYSICAL_PHONE")
                        } : null
                    });
                }


                // 3rd Table

                await reader.NextResultAsync();

                while (await reader.ReadAsync())
                {
                    result.Documents.Add(new ComplaintDocumentsDto
                    {
                        Id = reader.SafeGet<long>("ATT_ID"),
                        OtherDocsTypeName = reader.SafeGet<string>("OTHER_DOC_TYPE_NAME"),
                        FileName = reader.SafeGet<string>("FILE_NAME")
                    });
                }


                // 4th Table

                await reader.NextResultAsync();


                if (await reader.ReadAsync())
                {
                    result.DecisionInfo = new ComplaintDecisionInfoDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        Court = reader.SafeGet<string>("COURT"),
                        DocNo = reader.SafeGet<string>("DOC_NO"),
                        DecisionDate = reader.SafeGet<DateTime?>("DECISION_DATE"),
                        Name = reader.SafeGet<string>("NAME")
                    };
                }


                // 5th Table

                await reader.NextResultAsync();


                if (await reader.ReadAsync())
                {
                    result.FreePaidReason = new ComplaintFreePaidReasonDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        ReasonName = reader.SafeGet<string>("REASON_NAME")
                    };
                }


                // 6th Table

                await reader.NextResultAsync();


                if (await reader.ReadAsync())
                {
                    result.ApplicationContext = new ComplaintApplicationContextDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        Header = reader.SafeGet<string>("HEADER"),
                        Body = reader.SafeGet<string>("BODY")
                    };
                }


                // 7th Table

                await reader.NextResultAsync();


                while (await reader.ReadAsync())
                {
                    result.Signers.Add(new ComplaintSignerDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        PartyTypeName = reader.SafeGet<string>("PARTY_TYPE_NAME"),
                        FullName = reader.SafeGet<string>("FULL_NAME")
                    });
                }


                // 8th Table

                await reader.NextResultAsync();


                while (await reader.ReadAsync())
                {
                    result.LegalNorms.Add(new ComplaintLegalNorms
                    {
                        Id = reader.SafeGet<long>("ID"),
                        Norm = reader.SafeGet<string>("NORM"),
                        Name = reader.SafeGet<string>("NAME")
                    });
                }


                // 9th Table

                await reader.NextResultAsync();


                while (await reader.ReadAsync())
                {
                    result.ApplicationDetails.Add(new ComplaintApplicationDetails
                    {
                        Id = reader.SafeGet<long>("ID"),
                        Name = reader.SafeGet<string>("NAME")
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<ComplaintStatisticDto?> ComplaintStatisticAsync()
        {
            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            var result = new ComplaintStatisticDto();

            // Birinci resultset
            using var cmd = new SqlCommand("P_GET_COMPLAINT_STATISTICS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.TotalComplaints = reader.SafeGet<long>("TotalComplaints");
                result.CompletedComplaints = reader.SafeGet<long>("CompletedComplaints");
                result.InProgressComplaints = reader.SafeGet<long>("InProgressComplaints");
                result.NewComplaintsThisMonth = reader.SafeGet<long>("NewComplaintsThisMonth");
            }

            // İkinci resultset
            if (await reader.NextResultAsync())
            {
                var years = new Dictionary<int, ComplaintYearDto>();

                while (await reader.ReadAsync())
                {
                    var year = reader.SafeGet<int>("YEAR");

                    if (!years.TryGetValue(year, out var yearDto))
                    {
                        yearDto = new ComplaintYearDto
                        {
                            Year = year,
                            TotalCount = reader.SafeGet<int>("YEAR_COUNT"),
                            Months = new List<ComplaintMonthDto>()
                        };

                        years.Add(year, yearDto);
                    }

                    yearDto.Months!.Add(new ComplaintMonthDto
                    {
                        Month = reader.SafeGet<string>("MONTH"),
                        Count = reader.SafeGet<int>("MONTH_COUNT")
                    });
                }

                result.Years = years.Values.ToList();
            }

            return result;
        }


    }
}
