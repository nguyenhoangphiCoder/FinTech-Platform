using FinTech.Modules.Identity.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "iam");

        builder.HasKey(t => t.Id)
            .IsClustered(false);

        builder.HasIndex(t => new { t.CreatedAt, t.Id })
            .HasDatabaseName("CIX_Tenants_CreatedAt")
            .IsClustered();

        builder.Property(t => t.Type)
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.TaxCode)
            .HasMaxLength(20);

        builder.Property(t => t.LegalAddress)
            .HasMaxLength(500);

        builder.Property(t => t.BaseCurrency)
            .HasColumnType("char(3)")
            .HasDefaultValue("VND")
            .IsRequired();

        builder.Property(t => t.TimeZoneId)
            .HasMaxLength(50)
            .HasDefaultValue("SE Asia Standard Time")
            .IsRequired();

        builder.Property(t => t.FiscalYearStartMonth)
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)1)
            .IsRequired();

        builder.Property(t => t.FiscalMonthStartDay)
            .HasColumnType("tinyint")
            .HasDefaultValue((byte)1)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(t => t.RowVer)
            .IsRowVersion();

        builder.HasMany(t => t.Memberships)
            .WithOne(m => m.Tenant)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
