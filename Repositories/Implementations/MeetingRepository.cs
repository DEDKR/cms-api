using CmsApi.DB;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.Meeting;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace CmsApi.Repositories.Implementations
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MeetingRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<PagedResult<MeetingListItemDto>> GetMeetingsAsync(MeetingRequestDto meetingRequestDto, string? officeKeyCode)
        {
            var result = new PagedResult<MeetingListItemDto>
            {
                Items = new List<MeetingListItemDto>()
            };

            var json = JsonSerializer.Serialize(meetingRequestDto);

            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand("P_GET_MEETINGS", connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@JsonData", SqlDbType.NVarChar, -1).Value = json;
            // NULL is valid ONLY after RegionalOfficeScope has approved Admin / RO_00.
            command.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.VarChar, 10).Value =
                (object?)officeKeyCode ?? DBNull.Value;


            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Items.Add(new MeetingListItemDto
                {
                    Id = reader.SafeGet<long>("ID"),
                    Ids = reader.SafeGet<string?>("MEETING_IDS"),
                    CaseNo = reader.SafeGet<string>("CASE_NO"),
                    MeetingDate = reader.SafeGet<DateTime?>("MEETING_DATE"),
                    MeetingType = reader.SafeGet<string>("MEETING_TYPE"),
                    Court = reader.SafeGet<string>("COURT_NAME"),
                    Hall = reader.SafeGet<string>("HALL"),
                    Judge = reader.SafeGet<string>("JUDGE_NAME"),
                    MeetingStatus = reader.SafeGet<string>("MEETING_STATUS"),
                    ParticipationRole = reader.SafeGet<string?>("PARTY_TYPE"),
                    AssignedUsers = reader.SafeGet<string?>("ASSIGNED_USERS"),
                    AssignedUserIds = reader.SafeGet<string?>("ASSIGNED_USER_IDS"),

                });

                // COUNT(*) OVER() AS TotalCount procedure-də varsa
                if (result.TotalCount == 0)
                {
                    result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
                }
            }

            result.PageNumber = meetingRequestDto.PageIndex;
            result.PageSize = meetingRequestDto.PageSize;

            return result;
        }

        // Query is bound to the REAL case ID. Meeting CASE_NO is matched to the
        // case's CASE_NO; region is checked on the linked meeting CASE_ID as well.
        public async Task<List<MeetingListItemDto>> GetByCaseIdAsync(long caseId, string? officeKeyCode)
        {
            var result = new List<MeetingListItemDto>();
            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT M.ID, M.MEETING_IDS, M.CASE_NO, M.MEETING_DATE,
                       MT.NAME AS MEETING_TYPE, DC.COURT_NAME, M.HALL,
                       J.JUDGE_NAME, MS.NAME AS MEETING_STATUS, M.PARTY_TYPE,
                       A.ASSIGNED_USERS, A.ASSIGNED_USER_IDS
                FROM dbo.TBL_CASES C
                JOIN dbo.TBL_MEETINGS M ON M.CASE_NO = C.CASE_NO
                LEFT JOIN dbo.DIC_COURTS DC ON DC.COURT_ID = M.COURT_ID
                LEFT JOIN dbo.DIC_MEETING_TYPE MT ON MT.ID = M.MEETING_TYPE_ID
                LEFT JOIN dbo.DIC_MEETING_STATUSES MS ON MS.ID = M.MEETING_STATUS_ID
                OUTER APPLY (
                    SELECT TOP(1) TJ.JUDGE_NAME
                    FROM dbo.TBL_JUDGES TJ WHERE TJ.JUDGE_ID = M.JUDGE_ID
                    ORDER BY CASE WHEN TJ.COURT_ID = M.COURT_ID THEN 0 ELSE 1 END,
                             TJ.COURT_ID, TJ.JUDGE_NAME
                ) J
                OUTER APPLY (
                    SELECT STRING_AGG(X.FULL_NAME, N', ') AS ASSIGNED_USERS,
                           STRING_AGG(CONVERT(NVARCHAR(MAX), X.USER_ID), N',') AS ASSIGNED_USER_IDS
                    FROM (
                        SELECT DISTINCT MA.USER_ID,
                               LTRIM(RTRIM(CONCAT(ISNULL(U.FIRST_NAME,N''),N' ',ISNULL(U.LAST_NAME,N'')))) AS FULL_NAME
                        FROM dbo.TBL_MEETING_ATTENDS MA
                        LEFT JOIN dbo.TBL_USERS U ON U.USER_ID = MA.USER_ID
                        WHERE MA.MEETING_ID = M.ID
                    ) X
                ) A
                WHERE C.ID = @CaseId
                  AND (@Office IS NULL OR
                       EXISTS (SELECT 1 FROM dbo.TBL_CASE_ANALYSIS_RESULT CAR
                               WHERE CAR.CASE_ID = C.ID AND CAR.OFFICE_KEY_CODE = @Office))
                  AND (@Office IS NULL OR
                       EXISTS (SELECT 1 FROM dbo.TBL_CASE_ANALYSIS_RESULT CAR
                               WHERE CAR.CASE_ID = M.CASE_ID AND CAR.OFFICE_KEY_CODE = @Office)
                       OR (
                         NOT EXISTS (SELECT 1 FROM dbo.TBL_CASES MC WHERE MC.ID = M.CASE_ID)
                         AND C.COURT_ID = M.COURT_ID
                       ))
                ORDER BY M.MEETING_DATE DESC, M.ID DESC", connection);
            cmd.Parameters.Add("@CaseId", SqlDbType.BigInt).Value = caseId;
            cmd.Parameters.Add("@Office", SqlDbType.VarChar, 10).Value =
                (object?)officeKeyCode ?? DBNull.Value;
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new MeetingListItemDto
                {
                    Id = reader.SafeGet<long>("ID"),
                    Ids = reader.SafeGet<string?>("MEETING_IDS"),
                    CaseNo = reader.SafeGet<string>("CASE_NO"),
                    MeetingDate = reader.SafeGet<DateTime?>("MEETING_DATE"),
                    MeetingType = reader.SafeGet<string>("MEETING_TYPE"),
                    Court = reader.SafeGet<string>("COURT_NAME"),
                    Hall = reader.SafeGet<string>("HALL"),
                    Judge = reader.SafeGet<string>("JUDGE_NAME"),
                    MeetingStatus = reader.SafeGet<string>("MEETING_STATUS"),
                    ParticipationRole = reader.SafeGet<string?>("PARTY_TYPE"),
                    AssignedUsers = reader.SafeGet<string?>("ASSIGNED_USERS"),
                    AssignedUserIds = reader.SafeGet<string?>("ASSIGNED_USER_IDS")
                });
            }
            return result;
        }

        // Does not expose meeting details until related case is authorized.
        // Requires dbo.TBL_MEETINGS.CASE_ID (see SQL preflight).
        public async Task<bool> CanAccessMeetingAsync(long meetingId, string? officeKeyCode)
        {
            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.TBL_MEETINGS M
                    WHERE M.ID = @MEETING_ID
                      AND (
                          @OFFICE_KEY_CODE IS NULL
                          OR EXISTS (
                              SELECT 1
                              FROM dbo.TBL_CASE_ANALYSIS_RESULT CAR
                              WHERE CAR.CASE_ID = M.CASE_ID
                                AND CAR.OFFICE_KEY_CODE = @OFFICE_KEY_CODE
                          )
                          OR (
                              NOT EXISTS (SELECT 1 FROM dbo.TBL_CASES MC WHERE MC.ID = M.CASE_ID)
                              AND EXISTS (
                                  SELECT 1
                                  FROM dbo.TBL_CASES C
                                  JOIN dbo.TBL_CASE_ANALYSIS_RESULT CAR ON CAR.CASE_ID = C.ID
                                  WHERE C.CASE_NO = M.CASE_NO
                                    AND C.COURT_ID = M.COURT_ID
                                    AND CAR.OFFICE_KEY_CODE = @OFFICE_KEY_CODE
                              )
                          )
                      )
                ) THEN 1 ELSE 0 END", connection);

            command.Parameters.Add("@MEETING_ID", SqlDbType.BigInt).Value = meetingId;
            command.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.VarChar, 10).Value =
                (object?)officeKeyCode ?? DBNull.Value;

            return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
        }

        public async Task<MeetingDetailDto> MeetingDetail(long meetingId)
        {
            var result = new MeetingDetailDto
            {
                Judges = new List<MeetingJudgesDto>(),
                Parties = new List<MeetingPartyDto>(),
                RelatedMeetingDtos = new List<MeetingRelatedMeetingDto>()
            };

            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand("P_GET_MEETING_DETAIL", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@MEETING_ID", meetingId);


            using var reader = await command.ExecuteReaderAsync();

            #region 1. Meeting

            if (await reader.ReadAsync())
            {
                result.MeetingView = new MeetingDetailViewDto
                {
                    Id = reader["ID"] != DBNull.Value ? Convert.ToInt64(reader["ID"]) : 0,
                    CaseId = reader["CASE_ID"] != DBNull.Value ? Convert.ToInt64(reader["CASE_ID"]) : 0,
                    CaseNo = reader["CASE_NO"]?.ToString(),
                    CaseType = reader["CASE_TYPE"]?.ToString(),
                    MeetingType = reader["MEETING_TYPE"]?.ToString(),
                    MeetingStatus = reader["MEETING_STATUS"]?.ToString(),
                    PartipationRole = reader["PARTY_TYPE"]?.ToString(),
                    Court = reader["COURT_NAME"]?.ToString(),
                    Hall = reader["HALL"]?.ToString(),
                    MeetingDate = reader["MEETING_DATE"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["MEETING_DATE"])
                };
            }

            #endregion

            #region 2. Judges

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Judges.Add(new MeetingJudgesDto
                {
                    Name = reader["NAME"]?.ToString(),
                    Type = reader["TYPE"]?.ToString()
                });
            }

            #endregion

            #region 3. Parties

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.Parties.Add(new MeetingPartyDto
                {
                    Type = reader["TYPE"]?.ToString(),
                    Name = reader["NAME"]?.ToString()
                });
            }

            #endregion

            #region 4. Related Meetings

            await reader.NextResultAsync();

            while (await reader.ReadAsync())
            {
                result.RelatedMeetingDtos.Add(new MeetingRelatedMeetingDto
                {
                    Court = reader["COURT_NAME"]?.ToString(),
                    Judge = reader["JUDGE_NAME"]?.ToString(),
                    MeetingType = reader["MEETING_TYPE"]?.ToString(),
                    MeetingViewResult = reader["MEETING_STATUS"]?.ToString()
                });
            }

            #endregion

            return result;
        }


        public async Task<MeetingStatisticDto?> MeetingStatisticAsync(string? officeKeyCode)
        {
            using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            var result = new MeetingStatisticDto();

            // Birinci resultset
            using var cmd = new SqlCommand("P_GET_MEET_STATISTICS", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Old procedure stays unchanged for Admin / RO_00.
            // For limited users P_GET_MEET_STATISTICS MUST be upgraded to accept
            // @OFFICE_KEY_CODE and apply the same EXISTS filter to all result sets.
            // Otherwise SQL throws rather than leaking global statistics.
            if (officeKeyCode is not null)
                cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.VarChar, 10).Value = officeKeyCode;

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.TotalMeetings = reader.SafeGet<long>("TotalMeetings");
                result.CompletedMeetings = reader.SafeGet<long>("CompletedMeetings");
                result.InProgressMeetings = reader.SafeGet<long>("InProgressMeetings");
                result.NewMeetingsThisMonth = reader.SafeGet<long>("NewMeetingsThisMonth");
            }

            // İkinci resultset
            if (await reader.NextResultAsync())
            {
                var years = new Dictionary<int, MeetingYearDto>();

                while (await reader.ReadAsync())
                {
                    var year = reader.SafeGet<int>("YEAR");

                    if (!years.TryGetValue(year, out var yearDto))
                    {
                        yearDto = new MeetingYearDto
                        {
                            Year = year,
                            TotalCount = reader.SafeGet<int>("YEAR_COUNT"),
                            Months = new List<MeetingMonthDto>()
                        };

                        years.Add(year, yearDto);
                    }

                    yearDto.Months!.Add(new MeetingMonthDto
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
