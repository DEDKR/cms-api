using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.NotificationDtos;

namespace CmsApi.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<NotificationDetailDto> GetNotificationDetailAsync(long notificationId);
        Task<long?> GetNotificationCaseIdAsync(long notificationId);
        Task SetAsRead(long notificationId);
        Task<PagedResult<NotificationListItemDto>> GetNotifications(NoitifcationRequestDto noitifcationRequestDto, string? officeKeyCode = null);

        Task<int> GetNewNotificationsCount(string? officeKeyCode = null);

        Task<NotificationStatisticDto> GetNotificationStatistics(NotificationStatisticRequestDto notificationStatisticRequestDto, string? officeKeyCode = null);
    }
}
