using CmsApi.DB;
using CmsApi.DTOs.AssignmentDtos;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace CmsApi.Repositories.Implementations
{
    public class MeetingParticipationRepository : IMeetingParticipationRepository
    {
        private readonly IDbConnectionFactory _factory;
        public MeetingParticipationRepository(IDbConnectionFactory factory) => _factory = factory;

        // Get display labels in one stored-procedure call. These values do not
        // influence CanManageOthers, CanSeeAll or meeting authorization.
        public async Task<MeetingCapabilityProfileDto> GetCapabilityLabelsAsync(
            long userId, string? officeClaim, int? positionClaim, int? departmentClaim)
        {
            using var connection = _factory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand("dbo.P_GET_MEETING_CAPABILITY_LABELS", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 30
            };
            cmd.Parameters.Add("@USER_ID", SqlDbType.Int).Value = checked((int)userId);
            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.VarChar, 10).Value =
                (object?)officeClaim ?? DBNull.Value;
            cmd.Parameters.Add("@POSITION_ID", SqlDbType.Int).Value =
                (object?)positionClaim ?? DBNull.Value;
            cmd.Parameters.Add("@DEPARTMENT_ID", SqlDbType.Int).Value =
                (object?)departmentClaim ?? DBNull.Value;

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new InvalidOperationException("Capability profile not found for authenticated user.");

            string? ReadString(string column) => reader[column] == DBNull.Value
                ? null : Convert.ToString(reader[column]);
            int? ReadInt(string column) => reader[column] == DBNull.Value
                ? null : Convert.ToInt32(reader[column]);

            var officeValue = ReadString("OFFICE_VALUE");
            var positionValue = ReadInt("POSITION_VALUE");
            var departmentValue = ReadInt("DEPARTMENT_VALUE");
            return new MeetingCapabilityProfileDto
            {
                OfficeKeyCode = string.IsNullOrWhiteSpace(officeValue) ? null :
                    new ParticipationOptionDto<string>
                    {
                        Value = officeValue,
                        Label = ReadString("OFFICE_LABEL")
                    },
                Position = positionValue.HasValue ?
                    new ParticipationOptionDto<int>
                    {
                        Value = positionValue.Value,
                        Label = ReadString("POSITION_LABEL")
                    } : null,
                Department = departmentValue.HasValue ?
                    new ParticipationOptionDto<int>
                    {
                        Value = departmentValue.Value,
                        Label = ReadString("DEPARTMENT_LABEL")
                    } : null
            };
        }

        public async Task<List<MeetingParticipationDto>> GetAsync(
            long meetingId, long requestingUserId, string? officeKeyCode, bool canManageOthers)
        {
            var result = new List<MeetingParticipationDto>();
            using var connection = _factory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT MA.MEETING_ID, MA.USER_ID,
                       LTRIM(RTRIM(CONCAT(ISNULL(U.FIRST_NAME,N''),N' ',ISNULL(U.LAST_NAME,N''),N' ',ISNULL(U.FATHER_NAME,N'')))) AS USER_NAME,
                       MA.PARTICIPATION_STATUS, MA.PARTICIPATION_NOTE, MA.RESPONDED_AT,
                       MA.PARTICIPATION_RECORDED_BY AS RECORDED_BY,
                       LTRIM(RTRIM(CONCAT(ISNULL(A.FIRST_NAME,N''),N' ',ISNULL(A.LAST_NAME,N'')))) AS RECORDED_BY_NAME
                FROM dbo.TBL_MEETING_ATTENDS MA
                JOIN dbo.TBL_USERS U ON U.USER_ID = MA.USER_ID
                LEFT JOIN dbo.TBL_USERS A ON A.USER_ID = MA.PARTICIPATION_RECORDED_BY
                WHERE MA.MEETING_ID = @MeetingId
                  AND MA.PARTICIPATION_STATUS IN (1,2)
                  AND (
                      MA.USER_ID = @UserId
                      OR (@CanManage = 1 AND
                          (@Office IS NULL OR U.REGIONAL_OFFICE_ID = @Office))
                  )
                ORDER BY MA.USER_ID", connection);
            cmd.Parameters.Add("@MeetingId", SqlDbType.BigInt).Value = meetingId;
            cmd.Parameters.Add("@UserId", SqlDbType.BigInt).Value = requestingUserId;
            cmd.Parameters.Add("@Office", SqlDbType.VarChar, 10).Value = (object?)officeKeyCode ?? DBNull.Value;
            cmd.Parameters.Add("@CanManage", SqlDbType.Bit).Value = canManageOthers;
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(new MeetingParticipationDto
                {
                    MeetingId = Convert.ToInt64(reader["MEETING_ID"]),
                    UserId = Convert.ToInt64(reader["USER_ID"]),
                    UserName = reader["USER_NAME"]?.ToString() ?? "",
                    ParticipationStatus = Convert.ToInt32(reader["PARTICIPATION_STATUS"]),
                    ParticipationNote = reader["PARTICIPATION_NOTE"] == DBNull.Value ? null : reader["PARTICIPATION_NOTE"].ToString(),
                    RespondedAt = reader["RESPONDED_AT"] == DBNull.Value ? null : Convert.ToDateTime(reader["RESPONDED_AT"]),
                    RecordedBy = reader["RECORDED_BY"] == DBNull.Value ? null : Convert.ToInt64(reader["RECORDED_BY"]),
                    RecordedByName = reader["RECORDED_BY_NAME"] == DBNull.Value ? null : reader["RECORDED_BY_NAME"].ToString()
                });
            return result;
        }

        public async Task<MeetingParticipationSaveResultDto> SetAsync(
            IReadOnlyCollection<long> meetingIds, IReadOnlyCollection<long> userIds, long actorId,
            string? officeKeyCode, bool canManageOthers, int status, string? note)
        {
            using var connection = _factory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand("dbo.P_SET_MEETING_PARTICIPATIONS", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 60
            };
            cmd.Parameters.Add("@MEET_IDS_JSON", SqlDbType.NVarChar, -1).Value = JsonSerializer.Serialize(meetingIds);
            cmd.Parameters.Add("@USER_IDS_JSON", SqlDbType.NVarChar, -1).Value = JsonSerializer.Serialize(userIds);
            cmd.Parameters.Add("@ACTOR_ID", SqlDbType.BigInt).Value = actorId;
            cmd.Parameters.Add("@OFFICE_KEY_CODE", SqlDbType.VarChar, 10).Value = (object?)officeKeyCode ?? DBNull.Value;
            cmd.Parameters.Add("@CAN_MANAGE_OTHERS", SqlDbType.Bit).Value = canManageOthers;
            cmd.Parameters.Add("@STATUS", SqlDbType.TinyInt).Value = status;
            cmd.Parameters.Add("@NOTE", SqlDbType.NVarChar, 1500).Value = (object?)note?.Trim() ?? DBNull.Value;
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) throw new InvalidOperationException("Missing participation result");
            return new MeetingParticipationSaveResultDto
            {
                TotalPairs = Convert.ToInt32(reader["TOTAL_PAIRS"]),
                Created = Convert.ToInt32(reader["CREATED"]),
                Updated = Convert.ToInt32(reader["UPDATED"])
            };
        }
    }
}
