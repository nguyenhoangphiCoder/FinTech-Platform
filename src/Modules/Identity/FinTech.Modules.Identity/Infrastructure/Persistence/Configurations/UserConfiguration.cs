using FinTech.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "iam");

        builder.HasKey(u => u.Id)
            .IsClustered(false);

        builder.HasIndex(u => new { u.CreatedAt, u.Id })
            .HasDatabaseName("CIX_Users_CreatedAt")
            .IsClustered();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.NormalizedEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("UQ_Users_Email")
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        builder.Property(u => u.SecurityStamp)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.IsEmailConfirmed)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.IsPhoneConfirmed)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.TwoFactorEnabled)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(u => u.TwoFactorSecretKey)
            .HasMaxLength(256);

        builder.Property(u => u.LockoutEnd)
            .HasPrecision(3);

        builder.Property(u => u.AccessFailedCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.PreferredLanguage)
            .HasColumnType("varchar(10)")
            .HasDefaultValue("vi-VN")
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(u => u.RowVer)
            .IsRowVersion();

        builder.HasMany(u => u.Memberships)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Devices)
            .WithOne(d => d.User)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Passkeys)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
