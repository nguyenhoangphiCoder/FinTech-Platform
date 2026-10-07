using FinTech.SharedKernel;

namespace FinTech.Modules.Identity.Domain.Users;

public sealed class UserDevice : Entity<Guid>
{
    private UserDevice() { }

    public UserDevice(
        Guid id,
        Guid userId,
        string deviceFingerprint,
        string deviceModel,
        string operatingSystem,
        string appVersion,
        string? pushNotificationToken = null,
        bool isTrusted = false)
    {
        Id = id;
        UserId = userId;
        DeviceFingerprint = deviceFingerprint;
        DeviceModel = deviceModel;
        OperatingSystem = operatingSystem;
        AppVersion = appVersion;
        PushNotificationToken = pushNotificationToken;
        IsTrusted = isTrusted;
        FirstSeenAt = DateTimeOffset.UtcNow;
        LastActiveAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string DeviceFingerprint { get; private set; } = string.Empty;
    public string DeviceModel { get; private set; } = string.Empty;
    public string OperatingSystem { get; private set; } = string.Empty;
    public string AppVersion { get; private set; } = string.Empty;
    public string? PushNotificationToken { get; private set; }
    public bool IsTrusted { get; private set; }
    public DateTimeOffset FirstSeenAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastActiveAt { get; private set; } = DateTimeOffset.UtcNow;

    public void UpdateActivity(string? pushToken = null)
    {
        LastActiveAt = DateTimeOffset.UtcNow;
        if (!string.IsNullOrWhiteSpace(pushToken))
        {
            PushNotificationToken = pushToken;
        }
    }

    public void Trust() => IsTrusted = true;
}
