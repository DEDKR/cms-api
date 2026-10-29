using CmsApi.DB;
using CmsApi.DTOs.AssignmentDtos;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class MeetingParticipationRepository : IMeetingParticipationRepository
    {
        private readonly IDbConnectionFactory _factory;
        public MeetingParticipationRepository(IDbConnectionFactory factory) { _factory = factory; }

        public async Task<List<MeetingParticipationDto>> GetAsync(
            long meetingId, long requestingUserId, bool showAll)
        {
            var result = new List<MeetingParticipationDto>();
            using var connection = _factory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(@"
                SELECT MA.MEETING_ID, MA.USER_ID,
                       LTRIM(RTRIM(CONCAT(ISNULL(U.FIRST_NAME,N''),N' ',ISNULL(U.LAST_NAME,N''),N' ',ISNULL(U.FATHER_NAME,N'')))) AS USER_NAME,
                       MA.ATTENDED_BY,
                       LTRIM(RTRIM(CONCAT(ISNULL(A.FIRST_NAME,N''),N' ',ISNULL(A.LAST_NAME,N''),N' ',ISNULL(A.FATHER_NAME,N'')))) AS ASSIGNED_BY_NAME,
                       MA.CREATED_AT, MA.ASSIGNMENT_NOTE,
                       MA.PARTICIPATION_STATUS, MA.PARTICIPATION_NOTE, MA.RESPONDED_AT
                FROM dbo.TBL_MEETING_ATTENDS MA
                LEFT JOIN dbo.TBL_USERS U ON U.USER_ID = MA.USER_ID
                LEFT JOIN dbo.TBL_USERS A ON A.USER_ID = MA.ATTENDED_BY
                WHERE MA.MEETING_ID = @MeetingId
                  AND (@ShowAll = 1 OR MA.USER_ID = @UserId)
                ORDER BY MA.USER_ID", connection);
            cmd.Parameters.Add("@MeetingId", SqlDbType.BigInt).Value = meetingId;
            cmd.Parameters.Add("@UserId", SqlDbType.BigInt).Value = requestingUserId;
            cmd.Parameters.Add("@ShowAll", SqlDbType.Bit).Value = showAll;
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(new MeetingParticipationDto
                {
                    MeetingId = Convert.ToInt64(reader["MEETING_ID"]),
                    UserId = Convert.ToInt64(reader["USER_ID"]),
                    UserName = reader["USER_NAME"]?.ToString() ?? "",
                    AssignedBy = reader["ATTENDED_BY"] == DBNull.Value ? null : Convert.ToInt64(reader["ATTENDED_BY"]),
                    AssignedByName = reader["ASSIGNED_BY_NAME"] == DBNull.Value ? null : reader["ASSIGNED_BY_NAME"].ToString(),
                    AssignedAt = reader["CREATED_AT"] == DBNull.Value ? null : Convert.ToDateTime(reader["CREATED_AT"]),
                    AssignmentNote = reader["ASSIGNMENT_NOTE"] == DBNull.Value ? null : reader["ASSIGNMENT_NOTE"].ToString(),
                    ParticipationStatus = Convert.ToInt32(reader["PARTICIPATION_STATUS"]),
                    ParticipationNote = reader["PARTICIPATION_NOTE"] == DBNull.Value ? null : reader["PARTICIPATION_NOTE"].ToString(),
                    RespondedAt = reader["RESPONDED_AT"] == DBNull.Value ? null : Convert.ToDateTime(reader["RESPONDED_AT"])
                });
            return result;
        }

        // The user ID ALWAYS comes from JWT, never from the request body.
        public async Task<bool> UpdateOwnAsync(long meetingId, long userId, int status, string note)
        {
            using var connection = _factory.CreateMsSqlConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.TBL_MEETING_ATTENDS
                   SET PARTICIPATION_STATUS = @Status,
                       PARTICIPATION_NOTE = @Note,
                       IS_ATTENDED = CASE WHEN @Status = 1 THEN 1 ELSE 0 END,
                       RESPONDED_AT = SYSDATETIME()
                WHERE MEETING_ID = @MeetingId AND USER_ID = @UserId", connection);
            cmd.Parameters.Add("@MeetingId", SqlDbType.BigInt).Value = meetingId;
            cmd.Parameters.Add("@UserId", SqlDbType.BigInt).Value = userId;
            cmd.Parameters.Add("@Status", SqlDbType.TinyInt).Value = status;
            cmd.Parameters.Add("@Note", SqlDbType.NVarChar, 1500).Value = note.Trim();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
