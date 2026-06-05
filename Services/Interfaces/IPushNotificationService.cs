namespace StreetSignalApi.Services.Interfaces;

public interface IPushNotificationService
{
    /// <summary>
    /// Sends a push notification to all registered devices for the given user.
    /// Silently does nothing if Firebase is not configured or the user has no tokens.
    /// </summary>
    Task SendAsync(Guid userId, string title, string body, CancellationToken ct = default);
}
