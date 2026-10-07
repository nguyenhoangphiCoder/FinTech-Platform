using FinTech.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("UserDevices", "iam");

        builder.HasKey(d => d.Id)
            .HasName("PK_UserDevices");

        builder.Property(d => d.DeviceFingerprint)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.Property(d => d.DeviceModel)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.OperatingSystem)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.AppVersion)
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(d => d.PushNotificationToken)
            .HasMaxLength(500);

        builder.Property(d => d.IsTrusted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(d => d.FirstSeenAt)
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(d => d.LastActiveAt)
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasIndex(d => new { d.UserId, d.DeviceFingerprint })
            .HasDatabaseName("UQ_User_Device")
            .IsUnique();

        builder.HasOne(d => d.User)
            .WithMany(u => u.Devices)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK_UD_User")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
