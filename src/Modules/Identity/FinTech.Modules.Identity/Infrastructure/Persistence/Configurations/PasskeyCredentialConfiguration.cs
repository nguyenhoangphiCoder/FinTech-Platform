using FinTech.Modules.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class PasskeyCredentialConfiguration : IEntityTypeConfiguration<PasskeyCredential>
{
    public void Configure(EntityTypeBuilder<PasskeyCredential> builder)
    {
        builder.ToTable("PasskeyCredentials", "iam");

        builder.HasKey(p => p.Id)
            .HasName("PK_Passkeys");

        builder.Property(p => p.CredentialId)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(p => p.CredentialId)
            .HasDatabaseName("UQ_Passkey_CredId")
            .IsUnique();

        builder.Property(p => p.PublicKey)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(p => p.SignCount)
            .HasDefaultValue(0L)
            .IsRequired();

        builder.Property(p => p.DeviceName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.AAGUID)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasPrecision(3)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(p => p.LastUsedAt)
            .HasPrecision(3);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Passkeys)
            .HasForeignKey(p => p.UserId)
            .HasConstraintName("FK_Passkeys_User")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
