using CmsApi.DTOs.ApiDtos;
using CmsApi.DTOs.NotificationDtos;
using CmsApi.Http.Handlers.Interfaces;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;

namespace CmsApi.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICmsHttpHandler _httpHandler;
        public NotificationService(INotificationRepository notificationRepository, ICmsHttpHandler httpHandler)
        {
            _notificationRepository = notificationRepository;
            _httpHandler = httpHandler;
        }

        public async Task<int> GetNewNotificationsCount(string? officeKeyCode = null)
        {
            return await _notificationRepository.GetNewNotificationsCount(officeKeyCode);
        }

        public async Task<NotificationDetailDto> GetNotificationDetailDtoAsync(long id)
        {
            var notification = await _notificationRepository.GetNotificationDetailAsync(id);

            //if(notification != null)
            //{
            //    var readResult = _httpHandler.SetAsReadAsync(notification.Ids);
            //    if(readResult != null)
            //    {

            //    }
            //}

            return notification;
        }

        public Task<PagedResult<NotificationListItemDto>> GetNotifications(NoitifcationRequestDto noitifcationRequestDto, string? officeKeyCode = null)
        {
            return _notificationRepository.GetNotifications(noitifcationRequestDto, officeKeyCode);
        }

        public async Task<NotificationStatisticDto> GetNotificationStatistics(NotificationStatisticRequestDto notificationStatisticRequestDto, string? officeKeyCode = null)
        {
            return await _notificationRepository.GetNotificationStatistics(notificationStatisticRequestDto, officeKeyCode);
        }

        public async Task<int> MakeReadNotification(long id)
        {
            await _notificationRepository.SetAsRead(id);

            return 1;
        }
    }
}
