using CmsApi.DB;
using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.NotificationDtos;
using CmsApi.Entities;
using CmsApi.ExtensionMethods;
using CmsApi.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CmsApi.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public NotificationRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> GetNewNotificationsCount()
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            using var command = new SqlCommand("P_GET_UNREAD_NOTIFICATION_COUNT", connection);

            command.CommandType = CommandType.StoredProcedure;

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            return result != null && result != DBNull.Value
                ? Convert.ToInt32(result)
                : 0;
        }

        public async Task<NotificationDetailDto?> GetNotificationDetailAsync(long notificationId)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_NOTIFICATION_BY_ID", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = notificationId;

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var result = new NotificationDetailDto
            {
                Id = reader.SafeGet<long>("ID"),
                Ids = reader.SafeGet<string>("IDS"),
                CaseId = reader.SafeGet<long>("CASE_ID"),
                CaseNo = reader.SafeGet<string>("CASE_NO"),
                Court = reader.SafeGet<string>("COURT"),
                EnterDate = reader.SafeGet<DateTime>("INSERT_DATE"),
                ReadDate = reader.SafeGet<DateTime?>("READ_DATE"),
                Content = reader.SafeGet<string>("CONTENT"),
                Status = reader.SafeGet<string>("STATUS")
            };

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.RelatedNotifications.Add(new RelatedNotificationDto
                    {
                        Id = reader.SafeGet<long>("ID"),
                        CaseId = reader.SafeGet<long>("CASE_ID"),
                        CaseNo = reader.SafeGet<string>("CASE_NO"),
                        InsertDate = reader.SafeGet<DateTime>("INSERT_DATE"),
                        Status = reader.SafeGet<int>("STATUS")
                    });
                }
            }

            return result;
        }

        public async Task<PagedResult<NotificationListItemDto>> GetNotifications(NoitifcationRequestDto request)
        {
            var result = new PagedResult<NotificationListItemDto>
            {
                Items = new List<NotificationListItemDto>()
            };

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_NOTIFICATIONS", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CourtId", (object?)request.CourtId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", (object?)request.StartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", (object?)request.EndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CaseNo", string.IsNullOrWhiteSpace(request.CaseNo)
                ? DBNull.Value
                : request.CaseNo);
            cmd.Parameters.AddWithValue("@Status", (object?)request.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PageIndex", request.PageIndex);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Items.Add(new NotificationListItemDto
                {
                    Id = reader.SafeGet<long>("ID"),
                    Ids = reader.SafeGet<string>("IDS"),
                    Content = reader.SafeGet<string>("CONTENT"),
                    InsertDate = reader.SafeGet<DateTime?>("INSERT_DATE"),
                    Status = reader.SafeGet<int>("STATUS"),
                    StatusName = reader.SafeGet<string>("STATUS_NAME")
                });

                if (result.TotalCount == 0)
                    result.TotalCount = reader.SafeGet<int>("TotalCount");
            }

            result.PageNumber = request.PageIndex;
            result.PageSize = request.PageSize;

            return result;
        }

        public async Task<NotificationStatisticDto> GetNotificationStatistics(
             NotificationStatisticRequestDto request)
        {
            var result = new NotificationStatisticDto();

            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var cmd = new SqlCommand("dbo.P_GET_NOTIFICATION_STATISTICS", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@InsertDate", (object?)request.InsertDate ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result.Total = reader.SafeGet<int>("TOTAL");
                result.ReadCount = reader.SafeGet<int>("READ_COUNT");
                result.UnreadCount = reader.SafeGet<int>("UNREAD_COUNT");
                result.LastInsertDate = reader.SafeGet<DateTime?>("LAST_INSERT_DATE");
                result.InsertDateCount = reader.SafeGet<int>("INSERT_DATE_COUNT");
            }

            return result;
        }

        public async Task SetAsRead(long notificationId)
        {
            await using var connection = _connectionFactory.CreateMsSqlConnection();
            await connection.OpenAsync();

            await using var command = new SqlCommand("P_SET_NOTIFICATION_STATUS", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Id", notificationId);

            await command.ExecuteNonQueryAsync();
        }
    }
}
