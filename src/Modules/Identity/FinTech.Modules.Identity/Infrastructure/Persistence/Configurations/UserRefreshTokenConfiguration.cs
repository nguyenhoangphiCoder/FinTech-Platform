using FinTech.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens", "iam");

        builder.HasKey(r => r.Id)
            .HasName("PK_RefreshTokens");

        builder.Property(r => r.TokenHash)
            .HasColumnType("char(64)")
            .IsRequired();

        builder.HasIndex(r => r.TokenHash)
            .HasDatabaseName("IX_RT_TokenHash");

        builder.Property(r => r.DeviceFingerprint)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.Property(r => r.CreatedByIp)
            .HasColumnType("varchar(45)")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(r => r.ExpiresAt)
            .HasPrecision(3)
            .IsRequired();

        builder.Property(r => r.IsRevoked)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(r => r.RevokedAt)
            .HasPrecision(3);

        builder.Property(r => r.ReplacedByTokenHash)
            .HasColumnType("char(64)");

        builder.HasIndex(r => new { r.UserId, r.ExpiresAt })
            .HasDatabaseName("IX_RT_UserExpires")
            .HasFilter("[IsRevoked] = 0");

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .HasConstraintName("FK_RT_User")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
