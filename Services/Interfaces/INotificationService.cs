using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.NotificationDtos;

namespace CmsApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDetailDto> GetNotificationDetailDtoAsync(long id);
        Task<PagedResult<NotificationListItemDto>> GetNotifications(NoitifcationRequestDto noitifcationRequestDto, string? officeKeyCode = null);

        Task<int> MakeReadNotification(long id);

        Task<int> GetNewNotificationsCount(string? officeKeyCode = null);

        Task<NotificationStatisticDto> GetNotificationStatistics(NotificationStatisticRequestDto notificationStatisticRequestDto, string? officeKeyCode = null);

    }
}
