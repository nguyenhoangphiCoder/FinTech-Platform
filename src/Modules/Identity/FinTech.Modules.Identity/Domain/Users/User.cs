using FinTech.Modules.Identity.Domain.Tenants;
using FinTech.SharedKernel;

namespace FinTech.Modules.Identity.Domain.Users;

public sealed class User : AggregateRoot<Guid>
{
    private User() { }

    public User(
        Guid id,
        string email,
        string fullName,
        string? passwordHash = null,
        string preferredLanguage = "vi-VN")
    {
        Id = id;
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        FullName = fullName;
        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
        PreferredLanguage = preferredLanguage;
        CreatedAt = DateTimeOffset.UtcNow;
        IsEmailConfirmed = false;
        IsPhoneConfirmed = false;
        TwoFactorEnabled = false;
        AccessFailedCount = 0;
    }

    public static User Create(string email, string fullName, string? passwordHash = null)
    {
        return new User(
            Guid.CreateVersion7(),
            email.Trim(),
            fullName.Trim(),
            passwordHash);
    }

    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public string SecurityStamp { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsPhoneConfirmed { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecretKey { get; private set; }
    public DateTimeOffset? LockoutEnd { get; private set; }
    public int AccessFailedCount { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string PreferredLanguage { get; private set; } = "vi-VN";
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public byte[]? RowVer { get; private set; }

    private readonly List<TenantMembership> _memberships = [];
    public IReadOnlyCollection<TenantMembership> Memberships => _memberships.AsReadOnly();

    private readonly List<UserRefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<UserRefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private readonly List<UserDevice> _devices = [];
    public IReadOnlyCollection<UserDevice> Devices => _devices.AsReadOnly();

    private readonly List<PasskeyCredential> _passkeys = [];
    public IReadOnlyCollection<PasskeyCredential> Passkeys => _passkeys.AsReadOnly();

    public void RecordFailedLogin(int maxFailedAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxFailedAttempts)
        {
            LockoutEnd = DateTimeOffset.UtcNow.Add(lockoutDuration ?? TimeSpan.FromMinutes(15));
        }
    }

    public void RecordSuccessfulLogin()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEnd.HasValue && LockoutEnd.Value > now;

    public void SetPassword(string newHash)
    {
        PasswordHash = newHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public void AddMembership(TenantMembership membership)
    {
        _memberships.Add(membership);
    }

    public void AddRefreshToken(UserRefreshToken token)
    {
        _refreshTokens.Add(token);
    }
}
