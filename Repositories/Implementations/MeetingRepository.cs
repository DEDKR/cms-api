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

        public async Task<PagedResult<MeetingListItemDto>> GetMeetingsAsync(MeetingRequestDto meetingRequestDto)
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
            command.Parameters.Add("@JsonData", SqlDbType.NVarChar).Value = json;


            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Items.Add(new MeetingListItemDto
                {
                    Id = reader.SafeGet<long>("ID"),
                    Ids = reader.SafeGet<string>("ID"),
                    CaseNo = reader.SafeGet<string>("CASE_NO"),
                    MeetingDate = reader.SafeGet<DateTime?>("MEETING_DATE"),
                    MeetingType = reader.SafeGet<string>("MEETING_TYPE"),
                    Court = reader.SafeGet<string>("COURT_NAME"),
                    Hall = reader.SafeGet<string>("HALL"),
                    Judge = reader.SafeGet<string>("JUDGE_NAME"),
                    MeetingStatus = reader.SafeGet<string>("MEETING_STATUS"),
                    ParticipationRole = reader.SafeGet<string>("PARTY_TYPE"),

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

       
    }
}
