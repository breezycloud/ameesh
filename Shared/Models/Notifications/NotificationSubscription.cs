namespace Shared.Models.Notifications;

public class NotificationSubscription
{
    public Guid NotificationSubscriptionId { get; set; }

    public Guid UserId { get; set; }

    public string? Url { get; set; }

    public string? P256dh { get; set; }

    public string? Auth { get; set; }
}

