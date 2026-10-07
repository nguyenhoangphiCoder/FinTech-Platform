using FinTech.SharedKernel;

namespace FinTech.Modules.Identity.Domain.Users;

public sealed class PasskeyCredential : Entity<Guid>
{
    private PasskeyCredential() { }

    public PasskeyCredential(
        Guid id,
        Guid userId,
        byte[] credentialId,
        byte[] publicKey,
        string deviceName,
        Guid aaguid)
    {
        Id = id;
        UserId = userId;
        CredentialId = credentialId;
        PublicKey = publicKey;
        DeviceName = deviceName;
        AAGUID = aaguid;
        SignCount = 0;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public byte[] CredentialId { get; private set; } = [];
    public byte[] PublicKey { get; private set; } = [];
    public long SignCount { get; private set; }
    public string DeviceName { get; private set; } = string.Empty;
    public Guid AAGUID { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastUsedAt { get; private set; }

    public void UpdateSignCount(long newCount)
    {
        if (newCount > SignCount)
        {
            SignCount = newCount;
        }
        LastUsedAt = DateTimeOffset.UtcNow;
    }
}
