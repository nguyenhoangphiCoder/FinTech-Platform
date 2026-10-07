namespace FinTech.Modules.Identity.Domain.Tenants;

public enum TenantType : byte
{
    Personal = 1,
    Household = 2,
    SmeOrganization = 3
}

public sealed class Tenant : FinTech.SharedKernel.AggregateRoot<Guid>
{
    private Tenant() { }

    public Tenant(
        Guid id,
        TenantType type,
        string name,
        string baseCurrency = "VND",
        string timeZoneId = "SE Asia Standard Time")
    {
        Id = id;
        Type = type;
        Name = name;
        BaseCurrency = baseCurrency;
        TimeZoneId = timeZoneId;
        FiscalYearStartMonth = 1;
        FiscalMonthStartDay = 1;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Tenant CreatePersonal(string ownerFullName, string currency = "VND")
    {
        return new Tenant(
            Guid.CreateVersion7(),
            TenantType.Personal,
            $"Ví cá nhân - {ownerFullName}",
            currency);
    }

    public static Tenant CreateSme(string organizationName, string? taxCode, string? legalAddress, string currency = "VND")
    {
        var tenant = new Tenant(
            Guid.CreateVersion7(),
            TenantType.SmeOrganization,
            organizationName,
            currency)
        {
            TaxCode = taxCode,
            LegalAddress = legalAddress
        };
        return tenant;
    }

    public TenantType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? TaxCode { get; private set; }
    public string? LegalAddress { get; private set; }
    public string BaseCurrency { get; private set; } = "VND";
    public string TimeZoneId { get; private set; } = "SE Asia Standard Time";
    public byte FiscalYearStartMonth { get; private set; } = 1;
    public byte FiscalMonthStartDay { get; private set; } = 1;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public byte[]? RowVer { get; private set; }

    private readonly List<TenantMembership> _memberships = [];
    public IReadOnlyCollection<TenantMembership> Memberships => _memberships.AsReadOnly();
}
