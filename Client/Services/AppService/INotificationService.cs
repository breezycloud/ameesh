using Shared.Models.Notifications;

public interface INotificationService
{
    Task<bool> AreNewNotificationsAvailable();
    Task MarkNotificationsAsRead();
    Task MarkNotificationsAsRead(Guid id);

    Task<NotificationMessage> GetMessageById(Guid id);
    Task<NotificationMessage[]> GetNotifications();
    Task<NotificationMessage[]> GetUnReadNotifications();
    Task AddNotification(NotificationMessage message);
}