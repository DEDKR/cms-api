using CmsApi.DB;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.DTOs.Dashboard;
using CmsApi.Entities;
using CmsApi.Enums;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;

namespace CmsApi.Repositories.Implementations
{
    public class CaseRepository : ICaseRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CaseRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<CaseListDto>> GetCasesAsync(CaseListRequestDto request)
        {
            try
            {


                var result = new PagedResult<CaseListDto>
                {
                    PageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber,
                    PageSize = request.PageSize <= 0 || request.PageSize > 10000 ? 10 : request.PageSize
                };

                using var connection = _connectionFactory.CreateMsSqlConnection();
                await connection.OpenAsync();

               

                using var cmd = new SqlCommand("[dbo].[P_GET_CASES]", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                cmd.Parameters.Add("@CaseNo", SqlDbType.NVarChar, 100).Value =
                    (object?)request.CaseNo ?? DBNull.Value;

                    cmd.Parameters.Add("@JudgeIds", SqlDbType.NVarChar).Value =
                    request.JudgeIds is { Count: > 0 }
                        ? string.Join(",", request.JudgeIds)
                        : DBNull.Value;

                    cmd.Parameters.Add("@CaseTypeIds", SqlDbType.NVarChar).Value =
                    request.CaseTypeIds is { Count: > 0 }
                        ? string.Join(",", request.CaseTypeIds)
                        : DBNull.Value;


                cmd.Parameters.Add("@CourtId", SqlDbType.Int).Value =
                    (object?)request.CourtId ?? DBNull.Value;


                cmd.Parameters.Add("@CaseStatus", SqlDbType.Int).Value =
                    (object?)request.CaseStatus ?? DBNull.Value;

                cmd.Parameters.Add("@OnlyWarningsCases", SqlDbType.Bit).Value =
                  (object?)request.OnlyWarningsCases ?? DBNull.Value;

                cmd.Parameters.Add("@StartDate", SqlDbType.Date).Value =
                    (object?)request.StartDate ?? DBNull.Value;

                cmd.Parameters.Add("@EndDate", SqlDbType.Date).Value =
                    (object?)request.EndDate ?? DBNull.Value;

                cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = result.PageNumber;
                cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = result.PageSize;

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (result.TotalCount == 0)
                    {
                        result.TotalCount = reader.SafeGet<int>("TOTAL_COUNT");
                    }

                    result.Items.Add(new CaseListDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        CaseNo = reader.SafeGet<string?>("CASE_NO"),
                        Type = reader.SafeGet<string?>("TYPE"),
                        CourtName = reader.SafeGet<string?>("COURT_NAME"),
                        JudgeName = reader.SafeGet<string?>("JUDGE_NAME"),
                        CaseStatus = reader.SafeGet<string?>("CASE_STATUS"),
                        EnterDate = reader.SafeGet<DateTime?>("ENTER_DATE"),
                        CategoryId = reader.SafeGet<int?>("CATEGORY_ID"),
                        SubCategoryId = reader.SafeGet<int?>("SUB_CATEGORY_ID"),
                        Year = reader.SafeGet<int?>("YEAR"),
                        Result = reader.SafeGet<string?>("RESULT"),
                        HasNewNotification = reader.SafeGet<bool>("HAS_NEW_NOTIFICATION"),
                        CourtLevelId = reader.SafeGet<int>("COURT_LEVEL_ID"),
                        CourtLevelName = reader.SafeGet<string>("COURT_LEVEL_NAME"),
                        IsAnalizeSuccess = reader.SafeGet<bool?>("IS_ANALIZE_SUCCESS")
                    });
                }


                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<CaseDetailDto?> GetCaseAsync(long caseId)
        {
            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            using var cmd = new SqlCommand("P_GET_CASE_DETAIL", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };

            cmd.Parameters.Add("@CASE_ID", SqlDbType.BigInt).Value = caseId;

            using var reader = await cmd.ExecuteReaderAsync();

            var result = new CaseDetailDto
            {
                Judges = new(),
                Parties = new(),
                RelatedMeetingDtos = new(),
                Documents = new(),
                Appeals = new(),
                CaseHistories = new(),
                RelatedCases = new(),
                Notifications = new(),
                Warnings = new(),
                CaseCodes = new(),
            };

            #region Case

            if (await reader.ReadAsync())
            {
                result.CaseView = new CaseDetailViewDto
                {
                    CaseId = reader.SafeGet<long>("ID"),
                    Ids = reader["IDS"]?.ToString(),
                    CaseNo = reader["CASE_NO"]?.ToString(),
                    ExecType = reader["EXEC_TYPE"]?.ToString(),
                    CaseType = reader["CASE_TYPE"]?.ToString(),
                    Court = reader["COURT_NAME"]?.ToString(),
                    CourtTypeId = reader["COURT_TYPE_ID"] as int?,
                    Judge = reader["JUDGE_NAME"]?.ToString(),
                    CaseStatus = reader["NAME"]?.ToString(),
                    EnterDate = reader["ENTER_DATE"] as DateTime?,
                    CategoryId = reader["CATEGORY_ID"] as int?,
                    SubCategoryId = reader["SUB_CATEGORY_ID"] as int?,
                    Year = reader["YEAR"] as int?,
                    CourtLevelId = reader.SafeGet<int>("COURT_LEVEL_ID"),
                    CourtLevelName = reader.SafeGet<string>("COURT_LEVEL_NAME"),
                    CaseSubject = reader.SafeGet<string>("CASE_SUBJECT"),
                    TerritorialOffice = reader.SafeGet<string>("TERRITORIAL_OFFICE"),
                    Result = reader.SafeGet<string>("RESULT")
                };
            }

            #endregion

            #region Judges

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Judges.Add(new CaseJudgeDto
                {
                    Name = reader["NAME"]?.ToString(),
                    Type = reader["TYPE"]?.ToString()
                });
            }

            #endregion

            #region Parties

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Parties.Add(new CasePartyDto
                {
                    Type = reader["TYPE"]?.ToString(),
                    Name = reader["NAME"]?.ToString()
                });
            }

            #endregion

            #region Meetings

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.RelatedMeetingDtos.Add(new CaseRelatedMeetingDto
                {
                    Court = reader["COURT"]?.ToString(),
                    Judge = reader["JUDGE"]?.ToString(),
                    MeetingType = reader["MEETING_TYPE"]?.ToString(),
                    MeetingDate = reader["MEETING_DATE"] as DateTime?,
                    Status = reader.SafeGet<string>("STATUS")
                });
            }

            #endregion

            #region Documents

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Documents.Add(new CaseDocuments
                {
                    Id = reader.SafeGet<long>("ID"),
                    CaseId = Convert.ToInt64(reader["CASE_ID"]),
                    CaseIds = reader["CASE_IDS"]?.ToString(),
                    DocTypeId = reader["DOC_TYPE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DOC_TYPE_ID"]),
                    OtherDocTypeId = reader["OTHER_DOC_TYPE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["OTHER_DOC_TYPE_ID"]),
                    DocTypeName = reader["DOC_TYPE_NAME"]?.ToString(),
                    Status = reader["STATUS"]?.ToString(),
                    InsertDate = Convert.ToDateTime(reader["INSERT_DATE"]),
                    Attachment = new CaseDocAttachment(
                    reader["ATTACHMENT_IDS"]?.ToString(),
                    reader["FILE_NAME"]?.ToString()),
                    IsImportant= reader.SafeGet<bool>("IS_IMPORTANT")

                });
            }

            #endregion

            #region Appeals

            await reader.NextResultAsync();

            var appeals = new Dictionary<int, CaseAppealDto>();

            while (await reader.ReadAsync())
            {
                int id = Convert.ToInt32(reader["CASE_ID"]);

                if (!appeals.TryGetValue(id, out var appeal))
                {
                    appeal = new CaseAppealDto
                    {
                        CaseId = id,
                        CaseIds = reader["CASE_IDS"]?.ToString(),
                        Status = reader["STATUS"]?.ToString(),
                        OtherDocumentNumber = reader["OTHER_DOCUMENT_NUMBER"]?.ToString(),
                        OtherDocumentTypeName = reader["OTHER_DOCUMENT_TYPE_NAME"]?.ToString(),
                        OtherDocumentEnterDate = reader["OTHER_DOCUMENT_ENTER_DATE"] as DateTime?,
                        DecisionTypeName = reader["DECISION_TYPE_NAME"]?.ToString(),
                        DecisitonDocumentNumber = reader["DECISITON_DOCUMENT_NUMBER"]?.ToString(),
                        DecisionEnterDate = reader["DECISION_ENTER_DATE"] as DateTime?,
                        SendedOrgan = reader["SENDED_ORGAN"]?.ToString(),
                        AppealParties = new List<string>()
                    };

                    appeals.Add(id, appeal);
                }

                if (reader["APPEAL_PARTY"] != DBNull.Value)
                    appeal.AppealParties!.Add(reader["APPEAL_PARTY"].ToString()!);
            }

            result.Appeals = appeals.Values.ToList();

            #endregion

            #region History

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.CaseHistories.Add(new CaseHistoryDto
                {
                    CaseId = Convert.ToInt64(reader["CASE_ID"]),
                    CaseIds = reader["CASE_IDS"]?.ToString(),
                    CaseNo = reader["CASE_NO"]?.ToString(),
                    Court = reader["COURT_NAME"]?.ToString(),
                    Judge = reader["JUDGE"]?.ToString(),
                    Status = reader["STATUS"]?.ToString(),
                    Result = reader["RESULT"]?.ToString(),
                    ResultDate = reader["RESULT_DATE"] as DateTime?,
                    DecisionDate = reader["DECISION_DATE"] as DateTime?,
                    EnterDate = reader["ENTER_DATE"] as DateTime?
                });
            }

            #endregion 

            #region RelatedCases

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.RelatedCases.Add(new CaseListDto
                {
                    Id = reader.SafeGet<long>("ID"),
                    CaseNo = reader.SafeGet<string?>("CASE_NO"),
                    Type = reader.SafeGet<string?>("TYPE"),
                    CourtName = reader.SafeGet<string?>("COURT_NAME"),
                    JudgeName = reader.SafeGet<string?>("JUDGE_NAME"),
                    CaseStatus = reader.SafeGet<string?>("CASE_STATUS"),
                    EnterDate = reader.SafeGet<DateTime?>("ENTER_DATE"),
                    CategoryId = reader.SafeGet<int?>("CATEGORY_ID"),
                    SubCategoryId = reader.SafeGet<int?>("SUB_CATEGORY_ID"),
                    Year = reader.SafeGet<int?>("YEAR"),
                    Result = reader.SafeGet<string?>("RESULT"),
                    HasNewNotification = reader.SafeGet<bool>("HAS_NEW_NOTIFICATION"),
                    CourtLevelId = reader.SafeGet<int>("COURT_LEVEL_ID"),
                    CourtLevelName = reader.SafeGet<string>("COURT_LEVEL_NAME")
                });
            }
            #endregion

            #region CaseNotifications

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Notifications.Add(new CaseNotificationDto
                {
                    Id = reader.SafeGet<int>("ID"),
                    CaseNo = reader.SafeGet<string?>("CASE_NO"),
                    Content = reader.SafeGet<string?>("CONTENT"),
                    Court = reader.SafeGet<string?>("COURT"),
                    Status = reader.SafeGet<int>("STATUS"),
                    StatusName = reader.SafeGet<string?>("STATUS_NAME"),
                    InsertDate = reader.SafeGet<DateTime>("INSERT_DATE"),
                    ReadDate = reader.SafeGet<DateTime?>("READ_DATE"),
                    TypeId = reader.SafeGet<int?>("TYPE_ID"),
                    Result = reader.SafeGet<string?>("RESULT")
                });
            }

            #endregion

            #region Warnings

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Warnings.Add(new CaseWarnings
                {
                    Id = reader.SafeGet<long>("ID"),
                    CaseId = reader.SafeGet<long>("CASE_ID"),
                    Message = reader.SafeGet<string?>("MESSAGE"),
                    TypeId = reader.SafeGet<int>("TYPE_ID"),
                    Type = reader.SafeGet<string?>("TYPE"),
                    IsResolved = reader.SafeGet<bool>("IS_RESOLVED"),
                    ResolvedDate = reader.SafeGet<DateTime?>("RESOLVED_DATE"),
                    CreatedDate = reader.SafeGet<DateTime>("CREATED_DATE")
                 
                });
            }

            #endregion

            #region CaseCodes

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.CaseCodes.Add(new CaseCode
                {
                    Chapter = reader.SafeGet<string>("CHAPTER"),
                    ArticleNo = reader.SafeGet<string>("ARTICLE_NO")
                });
            }

            #endregion

            return result;
        }

        public async Task<List<CaseNotificationListItem>?> GetCaseNewNotificationsAsync(long caseId)
        {
            try
            {


                using var connection = _connectionFactory.CreateMsSqlConnection();
                await connection.OpenAsync();

                using var cmd = new SqlCommand("P_GET_CASE_NEW_NOTIFICATIONS", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                cmd.Parameters.Add("@CASE_ID", SqlDbType.BigInt).Value = caseId;

                using var reader = await cmd.ExecuteReaderAsync();

                var notifications = new List<CaseNotificationListItem>();

                while (await reader.ReadAsync())
                {
                    notifications.Add(new CaseNotificationListItem
                    {
                        Id = reader.SafeGet<long>("ID"),
                        CaseNo = reader.SafeGet<string>("CASE_NO"),
                        Content = reader.SafeGet<string>("CONTENT"),
                        Court = reader.SafeGet<string>("COURT"),
                        InsertDate = reader.SafeGet<DateTime?>("INSERT_DATE"),
                        TypeId = reader.SafeGet<int?>("TYPE_ID"),
                        NotificationTypeName = reader.SafeGet<string?>("NOTIFICATION_TYPE_NAME"),
                        Color = reader.SafeGet<string?>("COLOR")
                    });
                }

                return notifications;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<CaseStatisticDto?> CaseStatisticAsync()
        {
            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            var result = new CaseStatisticDto();

            // Birinci resultset
            using var cmd = new SqlCommand("P_GET_CASE_STATISTICS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.TotalCases = reader.SafeGet<long>("TotalCases");
                result.CompletedCases = reader.SafeGet<long>("CompletedCases");
                result.InProgressCases = reader.SafeGet<long>("InProgressCases");
                result.NewCasesThisMonth = reader.SafeGet<long>("NewCasesThisMonth");
            }

            // İkinci resultset
            if (await reader.NextResultAsync())
            {
                var years = new Dictionary<int, YearDto>();

                while (await reader.ReadAsync())
                {
                    var year = reader.SafeGet<int>("YEAR");

                    if (!years.TryGetValue(year, out var yearDto))
                    {
                        yearDto = new YearDto
                        {
                            Year = year,
                            TotalCount = reader.SafeGet<int>("YEAR_COUNT"),
                            Months = new List<MonthDto>()
                        };

                        years.Add(year, yearDto);
                    }

                    yearDto.Months!.Add(new MonthDto
                    {
                        Month = reader.SafeGet<string>("MONTH"),
                        Count = reader.SafeGet<int>("MONTH_COUNT")
                    });
                }

                result.Years = years.Values.ToList();
            }

            return result;
        }

        public async Task<List<CaseDocuments>> CaseDocumentsAsync(long caseId)
        {
            var result = new List<CaseDocuments>();

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_CASE_DOCUMENTS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CASE_ID", caseId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var attachmentIds = reader.SafeGet<string>("ATTACHMENT_IDS");
                var fileName = reader.SafeGet<string>("FILE_NAME");

                result.Add(new CaseDocuments
                {
                    Id = reader.SafeGet<long>("ID"),
                    CaseId = reader.SafeGet<long>("CASE_ID"),
                    CaseIds = reader.SafeGet<string>("CASE_IDS"),
                    DocTypeId = reader.SafeGet<int>("DOC_TYPE_ID"),
                    OtherDocTypeId = reader.SafeGet<int>("OTHER_DOC_TYPE_ID"),
                    DocTypeName = reader.SafeGet<string>("DOC_TYPE_NAME"),
                    Status = reader.SafeGet<string>("STATUS"),
                    InsertDate = reader.SafeGet<DateTime>("INSERT_DATE"),
                    Attachment = new CaseDocAttachment(attachmentIds, fileName)
                });
            }

            return result;
        }

        public async Task<List<CaseStatusStatisticDto>> GetCaseStatusStatisticsAsync()
        {
          
            var result = new List<CaseStatusStatisticDto>();

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_CASE_STATUS_STATISTICS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };


            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CaseStatusStatisticDto
                {
                    CaseStatusId = reader.SafeGet<int>("CASE_STATUS_ID"),
                    CaseStatus = reader.SafeGet<string>("CASE_STATUS"),
                    CaseCount = reader.SafeGet<int>("CASE_COUNT")
                });
            }

            return result;
        }

        public async Task<List<CourtLevelStatisticDto>> GetCourtLevelCaseStatisticsAsync(CourtLevelStatisticRequestDto request)
        {
            var result = new List<CourtLevelStatisticDto>();

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_COURT_LEVEL_STATISTICS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(
                 "@CASE_STATUS_ID",
                 request.CaseStatusId ?? (object)DBNull.Value);


            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CourtLevelStatisticDto
                {
                    CourtLevelId = reader.SafeGet<int>("COURT_LEVEL_ID"),
                    CourtLevelName = reader.SafeGet<string>("COURT_LEVEL_NAME"),
                    CaseCount = reader.SafeGet<int?>("CASE_COUNT")
                   
                });
            }

            return result;
        }

        public async Task<List<GetCaseStatisticsByCourtDto>> GetCaseStatisticsByCourtAsync(GetCaseStatisticsByCourtRequestDto request)
        {

            var result = new List<GetCaseStatisticsByCourtDto>();

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_COURT_STATISTICS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(
                 "@CASE_STATUS_ID",
                 request.CaseStatusId ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@COURT_TYPE_ID",
                request.CourtTypeId ?? (object)DBNull.Value);


            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new GetCaseStatisticsByCourtDto
                {
                    CourtId = reader.SafeGet<int>("COURT_ID"),
                    CourtName = reader.SafeGet<string>("COURT_NAME"),
                    CaseCount = reader.SafeGet<int>("CASE_COUNT"),
                    CaseComletedCount = reader.SafeGet<int>("COMPLETED_CASE"),
                    CaseStoppedCount = reader.SafeGet<int>("STOPPED_CASE"),
                    CaseInProgressCount = reader.SafeGet<int>("CASE_IN_PROGRESS")
                });
            }

            return result;
        }


        public async Task<List<CaseTotalByYearStatisticDto>> GetCaseTotalByYearStatisticsAsync(
    CaseTotalByYearStatisticRequestDto request)
        {
            var result = new List<CaseTotalByYearStatisticDto>();

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_STAT_TOTAL_BY_YEAR", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Year",
                request.Year == 0 ? DBNull.Value : request.Year);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CaseTotalByYearStatisticDto
                {
                    Year = reader.SafeGet<int>("YEAR"),
                    Month = reader.SafeGet<int>("MONTH"),
                    FirstInstance = reader.SafeGet<int>("Birinci instansiya"),
                    Appeal = reader.SafeGet<int>("Appelyasiya"),
                    Cassation = reader.SafeGet<int>("Kassasiya"),

                    TotalMonths = reader.SafeGet<int>("TotalMonths"),
                    TotalFirstInstance = reader.SafeGet<int>("TotalBirinciInstansiya"),
                    TotalAppeal = reader.SafeGet<int>("TotalAppelyasiya"),
                    TotalCassation = reader.SafeGet<int>("TotalKassasiya")
                });
            }

            return result;
        }

        public async Task<CaseOrBaseCaseGetDto> GetCaseOrStarterCaseAsync(long caseId)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_INITIAL_CASE", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@CASE_ID", caseId);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new CaseOrBaseCaseGetDto
                {
                    Id = reader.SafeGet<long?>("Id"),
                    CaseNo = reader.SafeGet<string>("CaseNo"),
                    CourtLevelId = reader.SafeGet<int?>("CourtLevelId"),
                    SearchedCaseNo = reader.SafeGet<string?>("SearchedCaseNo"),
                    SearchedCourtLevelId = reader.SafeGet<int?>("SearchedCourtLevelId")
                };
            }

            return null!;
        }

        public async Task<long> InsertCaseDocumentRawContentAsync(
                 InsertCaseDocumentRawContentRequestDto request)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand("P_INSERT_CASE_DOCUMENT_RAW_CONTENT", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@CASE_NO", request.CaseNo);
            command.Parameters.AddWithValue("@CONTENT", request.Content);
            command.Parameters.AddWithValue("@CONTENT_TYPE", request.ContentType);
            command.Parameters.AddWithValue("@COURT_LEVEL_ID", (object?)request.CourtLevelId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DOCUMENT_ID", (object?)request.DocumentId ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }

        public async Task<List<CaseForAnalizeDto>> CaseForAnalizesAsync()
        {
            var result = new List<CaseForAnalizeDto>();

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.P_GET_CASES_FOR_ANALIZE",
                connection);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new CaseForAnalizeDto
                {
                    Id = reader.SafeGet<long>("ID"),
                    CaseNo = reader.SafeGet<string>("CASE_NO"),
                    CourtLevelId = reader.SafeGet<int>("COURT_LEVEL_ID"),
                    CompletedCaseHasRequiredDocuments = reader.SafeGet<bool?>("COMPLETED_CASE_HAS_REQUIRED_DOCUMENTS"),
                    CaseHasAnalysisDocuments = reader.SafeGet<bool>("CASE_HAS_REQUIRED_ANALYSIS_DOCUMENTS")
                });
            }

            return result;
        }


        public async Task<string?> GetKeyCodeByTextAsync(
    string text,
    int groupId)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            const string query = @"
        SELECT [config].[FN_GET_KEY_CODE_BY_TEXT](@TEXT, @GROUP_ID);
    ";

            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@TEXT", SqlDbType.NVarChar, -1).Value =
                (object?)text ?? DBNull.Value;

            command.Parameters.Add("@GROUP_ID", SqlDbType.TinyInt).Value = groupId;

            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
                return null;

            var keyCode = result.ToString();

            return string.IsNullOrWhiteSpace(keyCode)
                ? null
                : keyCode.Trim();
        }

        public async Task<long> InsertCaseAnalysisFindingAsync(
     CaseAnalysisFindingDto caseAnalysisFindingDto)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "P_INSERT_CASE_ANALYSIS_FINDING",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue(
                "@CASE_ID",
                caseAnalysisFindingDto.CaseId);

            command.Parameters.AddWithValue(
                "@WARNING_MESSAGE_ID",
                caseAnalysisFindingDto.WarningMessageId);

            command.Parameters.AddWithValue(
                "@TYPE",
                caseAnalysisFindingDto.Type);

            command.Parameters.AddWithValue(
                "@IS_RESOLVED",
                caseAnalysisFindingDto.IsResolved);

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }

        public async Task<long> InsertCaseAnalysisResultAsync(
            CaseAnalysisResultDto dto)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "dbo.P_INSERT_CASE_ANALYSIS_RESULT",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@CASE_ID", SqlDbType.BigInt)
                .Value = dto.CaseId;

            command.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.NVarChar, 100)
                .Value = (object?)dto.OfficeKeyCode ?? DBNull.Value;

            command.Parameters.Add("@CASE_SUBJECT_KEY_CODE", SqlDbType.NVarChar, 100)
                .Value = (object?)dto.CaseSubjectKeyCode ?? DBNull.Value;

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }

        public async Task UpdateCaseWarnings(long caseId, int messageId)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();


            using var command = new SqlCommand(
                "P_UPDATE_CASE_ANALYSIS_FINDING",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@CASE_ID", SqlDbType.BigInt).Value = caseId;
            command.Parameters.Add("@WARNING_MESSAGE_ID", SqlDbType.Int).Value = messageId;

            await command.ExecuteNonQueryAsync();
        }
    }
}
