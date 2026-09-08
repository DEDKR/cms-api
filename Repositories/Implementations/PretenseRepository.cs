using CmsApi.DB;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.ComplaintDtos;
using CmsApi.DTOs.PretenseDtos;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class PretenseRepository : IPretenseRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PretenseRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        public async Task<PagedResult<PretenseListDto>> GetPretenseListAsync(PretenseListRequestDto payload)
        {
            try
            {
                var result = new PagedResult<PretenseListDto>
                {
                    PageNumber = payload.PageNumber <= 0 ? 1 : payload.PageNumber,
                    PageSize = payload.PageSize <= 0 || payload.PageSize > 10000 ? 10 : payload.PageSize
                };

                using var connection = _connectionFactory.CreateMsSqlConnection();
                await connection.OpenAsync();

                using var cmd = new SqlCommand("P_GET_PRETENSES", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                cmd.Parameters.Add("@ID_VIEW", SqlDbType.NVarChar, 50).Value = (object?)payload.IdView ?? DBNull.Value;
                cmd.Parameters.Add("@VOEN", SqlDbType.NVarChar, 100).Value = (object?)payload.Voen ?? DBNull.Value;
                cmd.Parameters.Add("@START_DATE", SqlDbType.Date).Value = (object?)payload.StartDate ?? DBNull.Value;
                cmd.Parameters.Add("@END_DATE", SqlDbType.Date).Value = (object?)payload.EndDate ?? DBNull.Value;
                cmd.Parameters.Add("@STATE_ID", SqlDbType.Int).Value = (object?)payload.StateId ?? DBNull.Value;
                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = result.PageNumber;
                cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = result.PageSize;

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (result.TotalCount == 0)
                    {
                        result.TotalCount = reader.SafeGet<int>("TOTAL_COUNT");
                    }

                    result.Items.Add(new PretenseListDto
                    {
                        ApplicationHeader = reader.SafeGet<string>("APPLICATION_HEADER"),
                        IdView = reader.SafeGet<string>("ID_VIEW"),
                        InsertedDate = reader.SafeGet<DateTime?>("INSERT_DATE"),
                        ProjectKind = reader.SafeGet<int?>("PROJECT_KIND"),
                        ArchiveStatus = reader.SafeGet<int?>("ARCHIVE_STATUS"),
                        PartySignActionType = reader.SafeGet<int?>("PARTY_SIGN_ACTION_TYPE"),
                        PretenseType = reader.SafeGet<string>("PRETENSE_TYPE"),
                        ContractType = reader.SafeGet<string>("CONTRACT_TYPE"),
                        ContractNo = reader.SafeGet<string>("CONTRACT_NO"),
                        SendDate = reader.SafeGet<DateTime?>("SEND_DATE"),
                        ExecutionDateLimit = reader.SafeGet<int?>("EXECUTION_DATE_LIMIT"),
                        BrokenRuleInfo = reader.SafeGet<string>("BROKEN_RULE_INFO"),
                        LegalName = reader.SafeGet<string>("LEGAL_NAME"),
                        IsPretenseViewedStatus = reader.SafeGet<string>("IS_PRETENSE_VIEWED_STATUS"),
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


        public async Task<PretenseDetailsDto> GetPretenseDetailsAsync(long pretenseId)
        {
            try
            {
                var result = new PretenseDetailsDto();
                using var connection = _connectionFactory.CreateMsSqlConnection();
                await connection.OpenAsync();
                using var cmd = new SqlCommand("P_GET_PRETENSE_DETAILS", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };
                cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = pretenseId;
                using var reader = await cmd.ExecuteReaderAsync();


                // 1st Table

                if (await reader.ReadAsync())
                {
                    result.CaseView = new PretenseCaseViewDto
                    {
                        Id = reader.SafeGet<long>("ID"),  
                        IdView = reader.SafeGet<string>("ID_VIEW"),
                        PretenseType = reader.SafeGet<string>("PRETENSE_TYPE"),
                        ContractType = reader.SafeGet<string>("CONTRACT_TYPE"),
                        ContractNo = reader.SafeGet<string>("CONTRACT_NO"),
                        ContractDate = reader.SafeGet<DateTime?>("CONTRACT_DATE"),
                        BrokenRuleInfo = reader.SafeGet<string>("BROKEN_RULE_INFO"),
                        ExecutionDateLimit = reader.SafeGet<int?>("EXECUTION_DATE_LIMIT"),
                        IsPretenseViewedStatus = reader.SafeGet<string>("IS_PRETENSE_VIEWED_STATUS"),
                        InsertDate = reader.SafeGet<DateTime?>("INSERT_DATE"),
                        SendDate = reader.SafeGet<DateTime?>("SEND_DATE"),
                        StateName = reader.SafeGet<string>("STATE_NAME")
                    };
                }


                // 2nd Table

                await reader.NextResultAsync();

                while (await reader.ReadAsync())
                {
                    result.Parties.Add(new PretensePartiesDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        PartyTypeName = reader.SafeGet<string>("PARTY_TYPE_NAME"),
                        PersonTypeName = reader.SafeGet<string>("PERSON_TYPE_NAME"),
                        LegalPerson = reader.SafeGet<string>("FULL_NAME") != null ? new PretenseLegalPersonDto
                        {
                            Id = reader.SafeGet<long>("LEGAL_PERSON_ID"),
                            FullName = reader.SafeGet<string>("FULL_NAME"),
                            FullAddress = reader.SafeGet<string>("FULL_ADDRESS"),
                            ZipCode = reader.SafeGet<string>("ZIP_CODE"),
                            Phone = reader.SafeGet<string>("PHONE"),
                            Email = reader.SafeGet<string>("EMAIL")
                        } : null
                    });
                }


                // 3rd Table

                await reader.NextResultAsync();

                while (await reader.ReadAsync())
                {
                    result.Documents.Add(new PretenseDocumentsDto
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
                    result.ApplicationContext = new PretenseApplicationContextDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        Header = reader.SafeGet<string>("HEADER"),
                        Body = reader.SafeGet<string>("BODY")
                    };
                }


                // 4th Table

                await reader.NextResultAsync();


                if (await reader.ReadAsync())
                {
                    result.Signers = new PretenseSignerDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        SignerCertId = reader.SafeGet<int>("SIGNER_CERT_ID"),
                        SignerKey = reader.SafeGet<string>("SIGNER_KEY"),
                        SignerName = reader.SafeGet<string>("SIGNER_NAME"),
                        SignerPosition = reader.SafeGet<string>("SIGNER_POSITION")
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<PretenseStatisticDto?> GetPretenseStatisticsAsync()
        {
            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            var result = new PretenseStatisticDto();

            // Birinci resultset
            using var cmd = new SqlCommand("P_GET_PRETENSE_STATISTICS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.TotalPretenses = reader.SafeGet<long>("TotalPretenses");
                result.CompletedPretenses = reader.SafeGet<long>("CompletedPretenses");
                result.InProgressPretenses = reader.SafeGet<long>("InProgressPretenses");
                result.NewPretensesThisMonth = reader.SafeGet<long>("NewPretensesThisMonth");
            }

            // İkinci resultset
            if (await reader.NextResultAsync())
            {
                var years = new Dictionary<int, PretenseYearDto>();

                while (await reader.ReadAsync())
                {
                    var year = reader.SafeGet<int>("YEAR");

                    if (!years.TryGetValue(year, out var yearDto))
                    {
                        yearDto = new PretenseYearDto
                        {
                            Year = year,
                            TotalCount = reader.SafeGet<int>("YEAR_COUNT"),
                            Months = new List<PretenseMonthDto>()
                        };

                        years.Add(year, yearDto);
                    }

                    yearDto.Months!.Add(new PretenseMonthDto
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
