using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTech.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "iam");

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LegalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BaseCurrency = table.Column<string>(type: "char(3)", nullable: false, defaultValue: "VND"),
                    TimeZoneId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "SE Asia Standard Time"),
                    FiscalYearStartMonth = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    FiscalMonthStartDay = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsPhoneConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TwoFactorSecretKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "varchar(10)", nullable: false, defaultValue: "vi-VN"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVer = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PasskeyCredentials",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialId = table.Column<byte[]>(type: "varbinary(128)", maxLength: 128, nullable: false),
                    PublicKey = table.Column<byte[]>(type: "varbinary(512)", maxLength: 512, nullable: false),
                    SignCount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AAGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    LastUsedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passkeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Passkeys_User",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantMemberships",
                schema: "iam",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMemberships", x => new { x.TenantId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TM_Tenant",
                        column: x => x.TenantId,
                        principalSchema: "iam",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TM_User",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "varchar(128)", nullable: false),
                    DeviceModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OperatingSystem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AppVersion = table.Column<string>(type: "varchar(20)", nullable: false),
                    PushNotificationToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsTrusted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FirstSeenAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    LastActiveAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UD_User",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRefreshTokens",
                schema: "iam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "char(64)", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "varchar(128)", nullable: false),
                    CreatedByIp = table.Column<string>(type: "varchar(45)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "char(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RT_User",
                        column: x => x.UserId,
                        principalSchema: "iam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasskeyCredentials_UserId",
                schema: "iam",
                table: "PasskeyCredentials",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Passkey_CredId",
                schema: "iam",
                table: "PasskeyCredentials",
                column: "CredentialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantMemberships_UserId",
                schema: "iam",
                table: "TenantMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "CIX_Tenants_CreatedAt",
                schema: "iam",
                table: "Tenants",
                columns: new[] { "CreatedAt", "Id" })
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "UQ_User_Device",
                schema: "iam",
                table: "UserDevices",
                columns: new[] { "UserId", "DeviceFingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RT_TokenHash",
                schema: "iam",
                table: "UserRefreshTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_RT_UserExpires",
                schema: "iam",
                table: "UserRefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" },
                filter: "[IsRevoked] = 0");

            migrationBuilder.CreateIndex(
                name: "CIX_Users_CreatedAt",
                schema: "iam",
                table: "Users",
                columns: new[] { "CreatedAt", "Id" })
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Email",
                schema: "iam",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasskeyCredentials",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "TenantMemberships",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "UserDevices",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "UserRefreshTokens",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "iam");
        }
    }
}
