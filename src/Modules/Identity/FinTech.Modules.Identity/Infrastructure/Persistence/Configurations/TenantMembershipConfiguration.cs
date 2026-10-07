using FinTech.Modules.Identity.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>
{
    public void Configure(EntityTypeBuilder<TenantMembership> builder)
    {
        builder.ToTable("TenantMemberships", "iam");

        builder.HasKey(m => new { m.TenantId, m.UserId })
            .HasName("PK_TenantMemberships");

        builder.Property(m => m.Role)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.CustomPermissions)
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.DepartmentId);

        builder.Property(m => m.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(m => m.JoinedAt)
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(m => m.Tenant)
            .WithMany(t => t.Memberships)
            .HasForeignKey(m => m.TenantId)
            .HasConstraintName("FK_TM_Tenant")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(m => m.UserId)
            .HasConstraintName("FK_TM_User")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
