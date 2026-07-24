using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.NotificationDtos;

namespace CmsApi.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<NotificationDetailDto> GetNotificationDetailAsync(long notificationId);
        Task SetAsRead(long notificationId);
        Task<PagedResult<NotificationListItemDto>> GetNotifications(NoitifcationRequestDto noitifcationRequestDto);

        Task<int> GetNewNotificationsCount();
    }
}
