using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.NotificationDtos;

namespace CmsApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDetailDto> GetNotificationDetailDtoAsync(long id);
        Task<PagedResult<NotificationListItemDto>> GetNotifications(NoitifcationRequestDto noitifcationRequestDto);

        Task<int> MakeReadNotification(long id);

        Task<int> GetNewNotificationsCount(); 

    }
}
