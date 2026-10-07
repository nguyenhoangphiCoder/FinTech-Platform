using FinTech.SharedKernel;

namespace FinTech.Modules.Identity.Domain.Users;

public sealed class UserRefreshToken : Entity<Guid>
{
    private UserRefreshToken() { }

    public UserRefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        string deviceFingerprint,
        string createdByIp,
        DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        DeviceFingerprint = deviceFingerprint;
        CreatedByIp = createdByIp;
        CreatedAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public static UserRefreshToken Create(
        Guid userId,
        string tokenHash,
        string deviceFingerprint,
        string createdByIp,
        TimeSpan lifetime)
    {
        return new UserRefreshToken(
            Guid.CreateVersion7(),
            userId,
            tokenHash,
            deviceFingerprint,
            createdByIp,
            DateTimeOffset.UtcNow.Add(lifetime));
    }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;
    public string DeviceFingerprint { get; private set; } = string.Empty;
    public string CreatedByIp { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive(DateTimeOffset now) => !IsRevoked && ExpiresAt > now;

    public void Revoke(string? replacedByTokenHash = null)
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
