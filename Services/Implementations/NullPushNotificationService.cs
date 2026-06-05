using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Services.Implementations;

/// <summary>
/// No-op push service used when Firebase is not configured.
/// Allows the app to run without a service account file.
/// </summary>
public sealed class NullPushNotificationService : IPushNotificationService
{
    public Task SendAsync(Guid userId, string title, string body, CancellationToken ct = default)
        => Task.CompletedTask;
}
