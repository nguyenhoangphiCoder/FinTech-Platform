using FinTech.Modules.Identity.Domain.Users;

namespace FinTech.Modules.Identity.Domain.Tenants;

public sealed class TenantMembership
{
    private TenantMembership() { }

    public TenantMembership(
        Guid tenantId,
        Guid userId,
        string role,
        string? customPermissions = null,
        Guid? departmentId = null)
    {
        TenantId = tenantId;
        UserId = userId;
        Role = role;
        CustomPermissions = customPermissions;
        DepartmentId = departmentId;
        IsActive = true;
        JoinedAt = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public string Role { get; private set; } = string.Empty; // Owner, Admin, Accountant, Approver, Employee, Viewer
    public string? CustomPermissions { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset JoinedAt { get; private set; } = DateTimeOffset.UtcNow;
}
