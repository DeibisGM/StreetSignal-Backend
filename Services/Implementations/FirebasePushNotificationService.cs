using FirebaseAdmin.Messaging;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

public sealed class FirebasePushNotificationService : IPushNotificationService
{
    private readonly IDeviceTokenRepository _deviceTokens;
    private readonly ILogger<FirebasePushNotificationService> _logger;

    public FirebasePushNotificationService(
        IDeviceTokenRepository deviceTokens,
        ILogger<FirebasePushNotificationService> logger)
    {
        _deviceTokens = deviceTokens;
        _logger = logger;
    }

    public async Task SendAsync(Guid userId, string title, string body, CancellationToken ct = default)
    {
        var tokens = await _deviceTokens.GetByUserAsync(userId, ct);
        if (tokens.Count == 0) return;

        var staleTokens = new List<string>();

        foreach (var deviceToken in tokens)
        {
            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(new Message
                {
                    Token = deviceToken.Token,
                    Notification = new Notification { Title = title, Body = body },
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            Title = title,
                            Body = body,
                            ChannelId = "streetsignal_default",
                        },
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps { Sound = "default" },
                    },
                }, ct);
            }
            catch (FirebaseMessagingException ex)
                when (ex.MessagingErrorCode is MessagingErrorCode.Unregistered
                                            or MessagingErrorCode.InvalidArgument)
            {
                // Token is no longer valid — schedule removal
                staleTokens.Add(deviceToken.Token);
                _logger.LogInformation("Removing stale device token for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send push notification to user {UserId}", userId);
            }
        }

        if (staleTokens.Count > 0)
        {
            foreach (var token in staleTokens)
                await _deviceTokens.RemoveAsync(token, ct);
            await _deviceTokens.SaveChangesAsync(ct);
        }
    }
}
