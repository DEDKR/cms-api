using CmsApi.DB;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.CaseDtos;
using CmsApi.Entities;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System.Data;
using System.Globalization;
using System.Net.NetworkInformation;

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

                cmd.Parameters.Add("@CaseTypeId", SqlDbType.Int).Value =
                    (object?)request.CaseTypeId ?? DBNull.Value;

                cmd.Parameters.Add("@CourtId", SqlDbType.Int).Value =
                    (object?)request.CourtId ?? DBNull.Value;

                cmd.Parameters.Add("@JudgeId", SqlDbType.Int).Value =
                    (object?)request.JudgeId ?? DBNull.Value;

                cmd.Parameters.Add("@CaseStatus", SqlDbType.Int).Value =
                    (object?)request.CaseStatus ?? DBNull.Value;

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
                        CourtLevelName = reader.SafeGet<string>("COURT_LEVEL_NAME")
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
                CaseHistories = new()
            };

            #region Case

            if (await reader.ReadAsync())
            {
                result.CaseView = new CaseDetailViewDto
                {
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
                    Year = reader["YEAR"] as int?
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
                    MeetingDate = reader["MEETING_DATE"] as DateTime?
                });
            }

            #endregion

            #region Documents

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Documents.Add(new CaseDocuments
                {
                    CaseId = Convert.ToInt64(reader["CASE_ID"]),
                    CaseIds = reader["CASE_IDS"]?.ToString(),
                    DocTypeId = reader["DOC_TYPE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DOC_TYPE_ID"]),
                    OtherDocTypeId = reader["OTHER_DOC_TYPE_ID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["OTHER_DOC_TYPE_ID"]),
                    DocTypeName = reader["DOC_TYPE_NAME"]?.ToString(),
                    Status = reader["STATUS"]?.ToString(),
                    InsertDate = Convert.ToDateTime(reader["INSERT_DATE"]),
                    Attachment = new CaseDocAttachment(
                    reader["ATTACHMENT_IDS"]?.ToString(),
                    reader["FILE_NAME"]?.ToString())

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
            try
            {
                using var connection = _connectionFactory.CreateMsSqlConnection();
                await connection.OpenAsync();

                using var cmd = new SqlCommand("P_GET_CASE_STATISTICS", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new CaseStatisticDto
                    {
                        TotalCases = reader.SafeGet<long>("TotalCases"),
                        InProgressCases = reader.SafeGet<long>("InProgressCases"),
                        CompletedCases = reader.SafeGet<long>("CompletedCases"),
                        NewCasesThisMonth = reader.SafeGet<long>("NewCasesThisMonth")
                    };
                }

                return null;
            }
            catch
            {
                throw;
            }
        }
    }
}
