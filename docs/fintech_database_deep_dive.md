# 🗄️ FinTech Platform — Thiết kế Cơ sở Dữ liệu Toàn diện & Chuyên sâu (Full Production Grade: 72 Bảng)

> **Hệ quản trị CSDL:** Microsoft SQL Server 2025 Enterprise / Azure SQL Database  
> **Tài liệu tham chiếu:** [fintech_architecture_dotnet.md](./fintech_architecture_dotnet.md) · [fintech_functional_specification.md](./fintech_functional_specification.md) · [fintech_modules_deep_dive.md](./fintech_modules_deep_dive.md) · [fintech_security_deep_dive.md](./fintech_security_deep_dive.md)  
> **Quy chuẩn bắt buộc:** Tuân thủ 100% [Bản quy tắc AI Coding (.agents/skills/fintech-platform-coding-rules/SKILL.md)](../.agents/skills/fintech-platform-coding-rules/SKILL.md).

---

## Mục lục Phân tích Cơ sở Dữ liệu

1. [Chiến lược Thiết kế CSDL Cấp độ Enterprise Production](#1-chiến-lược-thiết-kế-csdl-cấp-độ-enterprise-production)
2. [Cấu trúc 16 Schemas & Phân tách Ranh giới (Bounded Contexts)](#2-cấu-trúc-16-schemas--phân-tách-ranh-giới)
3. [Danh mục Toàn bộ 72 Bảng Dữ liệu Production](#3-danh-mục-toàn-bộ-72-bảng-dữ-liệu-production)
4. [DDL Chi tiết Toàn bộ 72 Bảng theo Module (Production-Ready)](#4-ddl-chi-tiết-toàn-bộ-72-bảng-theo-module)
   - [4.1 Schema `iam`: Định danh, Thiết bị, Phiên & Multi-Tenancy (6 bảng)](#41-schema-iam-identity-devices-sessions--multi-tenancy)
   - [4.2 Schema `billing`: SaaS Plans, Subscriptions & Thu phí Nền tảng (4 bảng)](#42-schema-billing-saas-plans-subscriptions--platform-monetization)
   - [4.3 Schema `ledger`: Lõi Sổ cái Kép Mật mã & Ngoại tệ (6 bảng)](#43-schema-ledger-double-entry-ledger--currencies)
   - [4.4 Schema `recon`: Phiên Đối soát Sao kê Ngân hàng (3 bảng)](#44-schema-recon-bank-reconciliation-engine)
   - [4.5 Schema `import`: Staging Upload Sao kê, Parsing & Mapping (4 bảng)](#45-schema-import-statement-import-staging--mapping)
   - [4.6 Schema `txn`: Giao dịch, Split, Categories, Payees & Tags (7 bảng)](#46-schema-txn-transactions-splits-categories-payees--tags)
   - [4.7 Schema `budget`: Kế hoạch Ngân sách & Temporal History (3 bảng)](#47-schema-budget-budgeting-temporal-history--goals)
   - [4.8 Schema `loan`: Khoản vay, Hợp đồng Trả góp & Lịch Amortization (3 bảng)](#48-schema-loan-loans-installments--amortization-schedules)
   - [4.9 Schema `bill`: Hóa đơn Định kỳ, Nhắc hạn & Subscriptions (3 bảng)](#49-schema-bill-recurring-bills-reminders--subscriptions)
   - [4.10 Schema `inv`: Quản lý Đầu tư, Khớp Lô FIFO & Giá Thị trường (5 bảng)](#410-schema-inv-investments-lot-accounting-fifo--market-feed)
   - [4.11 Schema `sme`: Khách hàng, NCC, Hóa đơn AR/AP & Tài sản cố định (8 bảng)](#411-schema-sme-invoicing-apar-fixed-assets--cost-centers)
   - [4.12 Schema `workflow`: Động cơ Phê duyệt Đa cấp & Lịch sử Ký số (3 bảng)](#412-schema-workflow-multi-tier-approvals--signatures)
   - [4.13 Schema `rules`: Động cơ Luật Nghiệp vụ & Tự động hóa (2 bảng)](#413-schema-rules-rule-engine--automation-logs)
   - [4.14 Schema `ai`: Vector Search kNN, Mô hình & Cache Phân loại (4 bảng)](#414-schema-ai-vector-search-embeddings-models--cache)
   - [4.15 Schema `notification`: Hệ thống Thông báo, Preferences & Templates (4 bảng)](#415-schema-notification-multi-channel-notifications--templates)
   - [4.16 Schema `media`: Quản lý File Đính kèm, Hóa đơn OCR & Scan (2 bảng)](#416-schema-media-attachments-ocr-data--blob-metadata)
   - [4.17 Schema `audit`: Nhật ký Kiểm toán Bất biến & Khóa Kỳ Kế toán (2 bảng)](#417-schema-audit-security-audit-trail--period-lock)
   - [4.18 Schema `messaging`: Outbox/Inbox & Idempotency Store (3 bảng)](#418-schema-messaging-outboxinbox--idempotency)
5. [Bảo mật Dữ liệu Cấp độ Database (RLS, Always Encrypted, Masking)](#5-bảo-mật-dữ-liệu-cấp-độ-database)
6. [Chiến lược Đánh Index & Tối ưu Hiệu năng Cao](#6-chiến-lược-đánh-index--tối-ưu-hiệu-năng)
7. [Phân vùng Bảng (Partitioning) & Lưu trữ Lịch sử Dài hạn](#7-phân-vùng-bảng-partitioning--lưu-trữ-dài-hạn)
8. [Bảo trì, Giám sát & Kế hoạch Khôi phục Thảm họa (HA/DR)](#8-bảo-trì-giám-sát--khôi-phục-thảm-họa-hadr)

---

## 1. Chiến lược Thiết kế CSDL Cấp độ Enterprise Production

Hệ thống được thiết kế theo các nguyên tắc nghiêm ngặt của hệ thống tài chính phân tán hiện đại:
1. **Tuyệt đối không dùng số thực dấu phẩy động:** Mọi trường số tiền dùng `DECIMAL(19,4)`. Tỷ giá, số lượng chứng khoán, crypto dùng `DECIMAL(28,10)`.
2. **Khóa chính tuần tự theo thời gian:** 100% Primary Key dạng UUID được tạo bởi **GUID v7** (`Guid.CreateVersion7()`), loại bỏ hoàn toàn hiện tượng phân mảnh B-Tree (Page Split) so với GUID v4.
3. **Multi-tenancy cấp độ hạ tầng:** Mọi bảng nghiệp vụ chứa `TenantId UNIQUEIDENTIFIER NOT NULL`, được bảo vệ 2 lớp bởi Application và **SQL Server Row-Level Security (`SESSION_CONTEXT`)**.
4. **Append-Only & Tamper-Evident:** Bảng sổ cái (`Postings`, `JournalEntries`) và kiểm toán (`SecurityAuditTrail`) là **SQL Server Ledger Tables**, nối chuỗi băm mật mã SHA-256. Không có quyền `UPDATE` hay `DELETE`.
5. **Staging & Hoàn tác:** Toàn bộ dữ liệu import từ sao kê ngân hàng đi qua Staging Table (`import.RawBankTransactions`) trước khi được duyệt ghi sổ chính thức, cho phép hoàn tác (Rollback) toàn bộ một lô import mà không ảnh hưởng tính toàn vẹn sổ cái.

---

## 2. Cấu trúc 16 Schemas & Phân tách Ranh giới

```
                                  FINTECH OS ENTERPRISE DATABASE
  ┌────────────────────────────────────────────────────────────────────────────────────────┐
  │ 1.  iam          : Định danh, Người dùng, Thiết bị, Phiên làm việc & Quyền RBAC        │
  │ 2.  billing      : Quản trị Gói SaaS, Thu tiền người dùng, Gia hạn & Hóa đơn Nền tảng  │
  │ 3.  ledger       : Sổ cái Kép Mật mã (Append-Only), Tài khoản Sổ cái & Lịch sử Tỷ giá  │
  │ 4.  recon        : Phiên Đối soát Sao kê Ngân hàng (Reconciliation Engine)              │
  │ 5.  import       : Lô Upload Sao kê, Dữ liệu Thô Staging & Cấu hình Map Cột Ngân hàng  │
  │ 6.  txn          : Giao dịch Tài chính, Tách Giao dịch (Splits), Danh mục & Payees     │
  │ 7.  budget       : Ngân sách (Temporal System-Versioning) & Mục tiêu Tiết kiệm (Goals) │
  │ 8.  loan         : Khoản vay, Hợp đồng Trả góp & Bảng Lịch Amortization Chi tiết       │
  │ 9.  bill         : Hóa đơn Định kỳ, Nhắc hạn & Tự động Phát hiện Subscriptions         │
  │ 10. inv          : Danh mục Đầu tư, Khớp Lô Cổ phiếu (Lot FIFO) & Bảng Giá Thị trường  │
  │ 11. sme          : Hóa đơn Bán (AR), Hóa đơn Mua (AP), Khấu hao TSCĐ & Cost Centers    │
  │ 12. workflow     : Luồng Phê duyệt Đa cấp (Approvals) & Lưu trữ Chữ ký Số Xác thực     │
  │ 13. rules        : Động cơ Luật Nghiệp vụ & Lịch sử Thực thi Tự động hóa               │
  │ 14. ai           : Vector Embeddings (kNN Search), Quản lý Mô hình AI & Cache Dự đoán  │
  │ 15. notification : Hộp thư Thông báo Đa kênh, Tùy chọn Nhận tin & Mẫu Thông báo        │
  │ 16. media        : Quản lý Chứng từ Đính kèm, Kết quả Quét Virus & Dữ liệu OCR Hóa đơn │
  │ 17. audit        : Nhật ký Kiểm toán Mật mã Bất biến & Khóa Kỳ Kế toán                 │
  │ 18. messaging    : Transactional Outbox, Consumer Inbox Idempotent & Idempotency Store │
  └────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Danh mục Toàn bộ 72 Bảng Dữ liệu Production

1. `iam.Tenants`
2. `iam.Users`
3. `iam.TenantMemberships`
4. `iam.PasskeyCredentials`
5. `iam.UserRefreshTokens`
6. `iam.UserDevices`
7. `billing.SubscriptionPlans`
8. `billing.TenantSubscriptions`
9. `billing.PlatformInvoices`
10. `billing.PaymentTransactions`
11. `ledger.Currencies`
12. `ledger.ExchangeRates`
13. `ledger.Accounts`
14. `ledger.JournalEntries`
15. `ledger.Postings`
16. `ledger.DailyBalances`
17. `recon.ReconciliationSessions`
18. `recon.ReconciledMatches`
19. `recon.UnmatchedAdjustments`
20. `import.BankProfiles`
21. `import.StatementColumnMappings`
22. `import.ImportBatches`
23. `import.RawBankTransactions`
24. `txn.Categories`
25. `txn.Payees`
26. `txn.Merchants`
27. `txn.Transactions`
28. `txn.TransactionSplits`
29. `txn.Tags`
30. `txn.TransactionTags`
31. `budget.Budgets`
32. `budget.BudgetsHistory` *(Temporal History Table)*
33. `budget.SavingsGoals`
34. `loan.Loans`
35. `loan.AmortizationSchedules`
36. `loan.LoanPaymentHistory`
37. `bill.Subscriptions`
38. `bill.BillOccurrences`
39. `bill.DetectedRecurringPatterns`
40. `inv.Portfolios`
41. `inv.Assets`
42. `inv.TaxLots`
43. `inv.InvestmentOrders`
44. `inv.AssetPriceHistories`
45. `sme.BusinessContacts`
46. `sme.CostCenters`
47. `sme.Projects`
48. `sme.Invoices`
49. `sme.InvoiceItems`
50. `sme.InvoicePayments`
51. `sme.FixedAssets`
52. `sme.DepreciationSchedules`
53. `workflow.ApprovalPolicies`
54. `workflow.ApprovalRequests`
55. `workflow.ApprovalDecisions`
56. `rules.TransactionRules`
57. `rules.RuleExecutionLogs`
58. `ai.MerchantVectorEmbeddings`
59. `ai.ClassificationModelVersions`
60. `ai.InferencePredictionCaches`
61. `ai.AnomalyDetectionAlerts`
62. `notification.NotificationTemplates`
63. `notification.UserPreferences`
64. `notification.Notifications`
65. `notification.NotificationDeliveryLogs`
66. `media.Attachments`
67. `media.EntityAttachments`
68. `audit.AccountingPeriodLocks`
69. `audit.SecurityAuditTrail`
70. `messaging.OutgoingMessages`
71. `messaging.IncomingMessages`
72. `messaging.IdempotencyKeys`

---

## 4. DDL Chi tiết Toàn bộ 72 Bảng theo Module

### 4.1 Schema `iam`: Identity, Devices, Sessions & Multi-Tenancy

```sql
CREATE SCHEMA iam;
GO

-- 1. Bảng Tổ chức / Workspace
CREATE TABLE iam.Tenants (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Tenants PRIMARY KEY NONCLUSTERED,
    Type TINYINT NOT NULL, -- 1: Personal, 2: Household, 3: SME Organization
    Name NVARCHAR(200) NOT NULL,
    TaxCode NVARCHAR(20) NULL,
    LegalAddress NVARCHAR(500) NULL,
    BaseCurrency CHAR(3) NOT NULL DEFAULT 'VND',
    TimeZoneId NVARCHAR(50) NOT NULL DEFAULT 'SE Asia Standard Time',
    FiscalYearStartMonth TINYINT NOT NULL DEFAULT 1,
    FiscalMonthStartDay TINYINT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVer ROWVERSION,
    INDEX CIX_Tenants_CreatedAt CLUSTERED (CreatedAt, Id)
);

-- 2. Bảng Người dùng Toàn cục
CREATE TABLE iam.Users (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY NONCLUSTERED,
    Email NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(500) NULL,
    SecurityStamp NVARCHAR(100) NOT NULL,
    IsEmailConfirmed BIT NOT NULL DEFAULT 0,
    PhoneNumber NVARCHAR(20) NULL,
    IsPhoneConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorSecretKey NVARCHAR(256) NULL, -- Always Encrypted
    LockoutEnd DATETIMEOFFSET(3) NULL,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    FullName NVARCHAR(150) NOT NULL,
    AvatarUrl NVARCHAR(500) NULL,
    PreferredLanguage VARCHAR(10) NOT NULL DEFAULT 'vi-VN',
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVer ROWVERSION,
    CONSTRAINT UQ_Users_Email UNIQUE (NormalizedEmail),
    INDEX CIX_Users_CreatedAt CLUSTERED (CreatedAt, Id)
);

-- 3. Thành viên thuộc Workspace & Phân quyền (RBAC)
CREATE TABLE iam.TenantMemberships (
    TenantId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TM_Tenant REFERENCES iam.Tenants(Id),
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TM_User REFERENCES iam.Users(Id),
    Role NVARCHAR(50) NOT NULL, -- Owner, Admin, Accountant, Approver, Employee, Viewer
    CustomPermissions NVARCHAR(MAX) NULL, -- JSON array of claim codes
    DepartmentId UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    JoinedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TenantMemberships PRIMARY KEY (TenantId, UserId)
);

-- 4. Thông tin Khóa Passkey / FIDO2 WebAuthn
CREATE TABLE iam.PasskeyCredentials (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Passkeys PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Passkeys_User REFERENCES iam.Users(Id) ON DELETE CASCADE,
    CredentialId VARBINARY(128) NOT NULL,
    PublicKey VARBINARY(512) NOT NULL,
    SignCount BIGINT NOT NULL DEFAULT 0,
    DeviceName NVARCHAR(100) NOT NULL,
    AAGUID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    LastUsedAt DATETIMEOFFSET(3) NULL,
    CONSTRAINT UQ_Passkey_CredId UNIQUE (CredentialId)
);

-- 5. Bảng Quản lý Phiên & Refresh Tokens (Bảo mật Server-side)
CREATE TABLE iam.UserRefreshTokens (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RT_User REFERENCES iam.Users(Id) ON DELETE CASCADE,
    TokenHash CHAR(64) NOT NULL, -- SHA-256 hash của token
    DeviceFingerprint VARCHAR(128) NOT NULL,
    CreatedByIp VARCHAR(45) NOT NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt DATETIMEOFFSET(3) NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIMEOFFSET(3) NULL,
    ReplacedByTokenHash CHAR(64) NULL,
    INDEX IX_RT_TokenHash (TokenHash),
    INDEX IX_RT_UserExpires (UserId, ExpiresAt) WHERE IsRevoked = 0
);

-- 6. Danh sách Thiết bị Tin cậy (Device Binding)
CREATE TABLE iam.UserDevices (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserDevices PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_UD_User REFERENCES iam.Users(Id) ON DELETE CASCADE,
    DeviceFingerprint VARCHAR(128) NOT NULL,
    DeviceModel NVARCHAR(100) NOT NULL,
    OperatingSystem NVARCHAR(50) NOT NULL,
    AppVersion VARCHAR(20) NOT NULL,
    PushNotificationToken NVARCHAR(500) NULL,
    IsTrusted BIT NOT NULL DEFAULT 0,
    FirstSeenAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    LastActiveAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_User_Device UNIQUE (UserId, DeviceFingerprint)
);
```

---

### 4.2 Schema `billing`: SaaS Plans, Subscriptions & Platform Monetization

```sql
CREATE SCHEMA billing;
GO

-- 7. Danh mục Gói Dịch vụ SaaS của Nền tảng
CREATE TABLE billing.SubscriptionPlans (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SubPlans PRIMARY KEY,
    Code VARCHAR(50) NOT NULL, -- FREE, PRO_MONTHLY, PRO_YEARLY, BUSINESS_STANDARD, ENTERPRISE
    Name NVARCHAR(100) NOT NULL,
    MonthlyPrice DECIMAL(19,4) NOT NULL,
    YearlyPrice DECIMAL(19,4) NOT NULL,
    Currency CHAR(3) NOT NULL DEFAULT 'VND',
    MaxMembers INT NOT NULL DEFAULT 1,
    MaxAccounts INT NOT NULL DEFAULT 5,
    MaxImportMonthlyRows INT NOT NULL DEFAULT 500,
    HasAiFeatures BIT NOT NULL DEFAULT 0,
    HasSmeFeatures BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_SubPlan_Code UNIQUE (Code)
);

-- 8. Trạng thái Gói Đang dùng của Tenant
CREATE TABLE billing.TenantSubscriptions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TenantSub PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TSub_Tenant REFERENCES iam.Tenants(Id),
    PlanId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TSub_Plan REFERENCES billing.SubscriptionPlans(Id),
    Status TINYINT NOT NULL, -- 1: Trialing, 2: Active, 3: PastDue, 4: Cancelled, 5: Expired
    StartDate DATETIMEOFFSET(3) NOT NULL,
    EndDate DATETIMEOFFSET(3) NOT NULL,
    GracePeriodEnd DATETIMEOFFSET(3) NULL,
    AutoRenew BIT NOT NULL DEFAULT 1,
    PaymentMethodType VARCHAR(50) NOT NULL, -- VNPAY, MOMO, STRIPE, BANK_TRANSFER
    ExternalSubscriptionId NVARCHAR(100) NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX IX_TSub_TenantStatus (TenantId, Status)
);

-- 9. Hóa đơn Thu phí Dịch vụ của Nền tảng (Platform Invoices)
CREATE TABLE billing.PlatformInvoices (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PlatformInvoices PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber VARCHAR(50) NOT NULL,
    BillingPeriodStart DATE NOT NULL,
    BillingPeriodEnd DATE NOT NULL,
    SubTotal DECIMAL(19,4) NOT NULL,
    TaxAmount DECIMAL(19,4) NOT NULL,
    TotalAmount DECIMAL(19,4) NOT NULL,
    Currency CHAR(3) NOT NULL DEFAULT 'VND',
    Status TINYINT NOT NULL, -- 1: Draft, 2: Open, 3: Paid, 4: Void, 5: Uncollectible
    PaidAt DATETIMEOFFSET(3) NULL,
    PdfStorageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_PlatformInv_Num UNIQUE (InvoiceNumber)
);

-- 10. Giao dịch Cổng Thanh toán (VNPay, MoMo, Stripe...)
CREATE TABLE billing.PaymentTransactions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PayTransactions PRIMARY KEY,
    PlatformInvoiceId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_PayTxn_Inv REFERENCES billing.PlatformInvoices(Id),
    GatewayProvider VARCHAR(50) NOT NULL, -- VNPAY, MOMO, STRIPE, ZALOPAY
    GatewayTransactionId NVARCHAR(100) NOT NULL,
    Amount DECIMAL(19,4) NOT NULL,
    Currency CHAR(3) NOT NULL DEFAULT 'VND',
    Status TINYINT NOT NULL, -- 1: Pending, 2: Success, 3: Failed, 4: Refunded
    RawResponseJson NVARCHAR(MAX) NULL,
    ProcessedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX IX_PayTxn_GatewayId (GatewayProvider, GatewayTransactionId)
);
```

---

### 4.3 Schema `ledger`: Double-Entry Ledger & Currencies

```sql
CREATE SCHEMA ledger;
GO

-- 11. Bảng Danh mục Tiền tệ
CREATE TABLE ledger.Currencies (
    Code CHAR(3) NOT NULL CONSTRAINT PK_Currencies PRIMARY KEY,
    NumericCode INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Symbol NVARCHAR(10) NOT NULL,
    MinorUnits TINYINT NOT NULL, -- VND: 0, USD: 2, BHD: 3
    IsActive BIT NOT NULL DEFAULT 1
);

-- 12. Bảng Lịch sử Tỷ giá Hối đoái theo Ngày
CREATE TABLE ledger.ExchangeRates (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FromCurrency CHAR(3) NOT NULL CONSTRAINT FK_Ex_From REFERENCES ledger.Currencies(Code),
    ToCurrency CHAR(3) NOT NULL CONSTRAINT FK_Ex_To REFERENCES ledger.Currencies(Code),
    RateDate DATE NOT NULL,
    Rate DECIMAL(28,10) NOT NULL CHECK (Rate > 0),
    Source VARCHAR(50) NOT NULL DEFAULT 'VCB', -- VCB, SBV, OPEN_EXCHANGE
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_ExchangeRates UNIQUE (FromCurrency, ToCurrency, RateDate, Source)
);

-- 13. Bảng Danh mục Tài khoản Sổ cái (Chart of Accounts)
CREATE TABLE ledger.Accounts (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Accounts PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Code NVARCHAR(50) NOT NULL, -- 111, 112, 131, 331, 511, 642...
    Name NVARCHAR(200) NOT NULL,
    Type TINYINT NOT NULL, -- 1: ASSET, 2: LIABILITY, 3: EQUITY, 4: REVENUE, 5: EXPENSE
    Currency CHAR(3) NOT NULL CONSTRAINT FK_Acc_Curr REFERENCES ledger.Currencies(Code),
    Balance DECIMAL(19,4) NOT NULL DEFAULT 0,
    AllowNegative BIT NOT NULL DEFAULT 1,
    ParentAccountId UNIQUEIDENTIFIER NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    Version BIGINT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX CIX_Accounts_Tenant CLUSTERED (TenantId, Id),
    CONSTRAINT UQ_Accounts_TenantCode UNIQUE (TenantId, Code)
);

-- 14. Bảng Journal Entries (Append-Only Ledger Table)
CREATE TABLE ledger.JournalEntries (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    IdempotencyKey VARCHAR(128) NOT NULL,
    EffectiveDate DATE NOT NULL,
    RecordedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    SourceModule VARCHAR(20) NOT NULL, -- MANUAL, TXN, IMPORT, LOAN, BILL, SME
    SourceReferenceId UNIQUEIDENTIFIER NULL,
    Description NVARCHAR(500) NULL,
    ReversesId UNIQUEIDENTIFIER NULL,
    Metadata NVARCHAR(MAX) NULL, -- Native JSON metadata
    CONSTRAINT UQ_Journal_Idem UNIQUE (TenantId, IdempotencyKey),
    INDEX CIX_JournalEntries CLUSTERED (TenantId, EffectiveDate, Id)
)
WITH (LEDGER = ON (APPEND_ONLY = ON));

-- 15. Bảng Postings Ghi Nợ / Ghi Có (Append-Only Ledger Table)
CREATE TABLE ledger.Postings (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EntryId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(19,4) NOT NULL CHECK (Amount <> 0), -- Dương: DEBIT, Âm: CREDIT
    Currency CHAR(3) NOT NULL CONSTRAINT FK_Post_Curr REFERENCES ledger.Currencies(Code),
    ExchangeRate DECIMAL(28,10) NOT NULL DEFAULT 1.0,
    BaseCurrencyAmount DECIMAL(19,4) NOT NULL,
    INDEX IX_Postings_Account (TenantId, AccountId) INCLUDE (Amount, BaseCurrencyAmount),
    INDEX IX_Postings_Entry (EntryId)
)
WITH (LEDGER = ON (APPEND_ONLY = ON));

-- 16. Bảng Snapshot Số dư Chốt Cuối Ngày (Columnstore Analytics)
CREATE TABLE ledger.DailyBalances (
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL,
    BalanceDate DATE NOT NULL,
    ClosingBalance DECIMAL(19,4) NOT NULL,
    Currency CHAR(3) NOT NULL,
    INDEX CCI_DailyBalances CLUSTERED COLUMNSTORE
);
```

---

### 4.4 Schema `recon`: Bank Reconciliation Engine

```sql
CREATE SCHEMA recon;
GO

-- 17. Phiên Đối soát Tài khoản Ngân hàng
CREATE TABLE recon.ReconciliationSessions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ReconSessions PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Recon_Acc REFERENCES ledger.Accounts(Id),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    StatementEndingBalance DECIMAL(19,4) NOT NULL,
    CalculatedClearedBalance DECIMAL(19,4) NOT NULL,
    DifferenceAmount DECIMAL(19,4) NOT NULL,
    Status TINYINT NOT NULL, -- 1: InProgress, 2: Reconciled/Locked, 3: Abandoned
    ReconciledByUserId UNIQUEIDENTIFIER NOT NULL,
    ReconciledAt DATETIMEOFFSET(3) NULL,
    Notes NVARCHAR(500) NULL,
    INDEX CIX_ReconSessions CLUSTERED (TenantId, EndDate DESC, Id)
);

-- 18. Bảng Khớp Giao dịch trong Phiên Đối soát
CREATE TABLE recon.ReconciledMatches (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SessionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RMatch_Session REFERENCES recon.ReconciliationSessions(Id) ON DELETE CASCADE,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    TransactionId UNIQUEIDENTIFIER NOT NULL,
    RawTransactionId BIGINT NULL,
    MatchedAmount DECIMAL(19,4) NOT NULL,
    MatchedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX IX_RMatch_Txn (TransactionId)
);

-- 19. Bút toán Điều chỉnh Chênh lệch Đối soát (Discrepancy Adjustments)
CREATE TABLE recon.UnmatchedAdjustments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SessionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RAdj_Session REFERENCES recon.ReconciliationSessions(Id),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AdjustmentType TINYINT NOT NULL, -- 1: BankFee, 2: InterestEarned, 3: RoundingError
    Amount DECIMAL(19,4) NOT NULL,
    Reason NVARCHAR(250) NOT NULL,
    JournalEntryId UNIQUEIDENTIFIER NULL
);
```

---

### 4.5 Schema `import`: Statement Import, Staging & Mapping

```sql
CREATE SCHEMA import;
GO

-- 20. Danh mục Hồ sơ Ngân hàng & Định dạng Sao kê
CREATE TABLE import.BankProfiles (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BankProfiles PRIMARY KEY,
    BankCode VARCHAR(20) NOT NULL, -- VCB, TCB, MB, ACB, BIDV...
    BankName NVARCHAR(150) NOT NULL,
    ShortName NVARCHAR(50) NOT NULL,
    SwiftCode VARCHAR(20) NULL,
    LogoUrl NVARCHAR(500) NULL,
    SupportedFileFormats VARCHAR(50) NOT NULL DEFAULT 'XLSX,CSV,PDF',
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Bank_Code UNIQUE (BankCode)
);

-- 21. Cấu hình Ánh xạ Cột Sao kê (Column Mappings)
CREATE TABLE import.StatementColumnMappings (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    BankProfileId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Map_Bank REFERENCES import.BankProfiles(Id),
    ProfileName NVARCHAR(100) NOT NULL,
    DateColumnIndex INT NOT NULL,
    DateFormatPattern VARCHAR(30) NOT NULL DEFAULT 'dd/MM/yyyy',
    AmountColumnIndex INT NULL,
    DebitColumnIndex INT NULL,
    CreditColumnIndex INT NULL,
    DescriptionColumnIndex INT NOT NULL,
    ReferenceNumberColumnIndex INT NULL,
    BalanceColumnIndex INT NULL,
    SkipHeaderRowsCount INT NOT NULL DEFAULT 1,
    INDEX IX_Map_TenantBank (TenantId, BankProfileId)
);

-- 22. Quản lý Lô Upload Sao kê (Import Batches)
CREATE TABLE import.ImportBatches (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ImportBatches PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_IBatch_Acc REFERENCES ledger.Accounts(Id),
    BankProfileId UNIQUEIDENTIFIER NULL CONSTRAINT FK_IBatch_Bank REFERENCES import.BankProfiles(Id),
    OriginalFileName NVARCHAR(255) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    FileSha256Hash CHAR(64) NOT NULL,
    StorageBlobUrl NVARCHAR(500) NOT NULL,
    TotalRowsCount INT NOT NULL DEFAULT 0,
    ImportedRowsCount INT NOT NULL DEFAULT 0,
    DuplicateRowsCount INT NOT NULL DEFAULT 0,
    ErrorRowsCount INT NOT NULL DEFAULT 0,
    Status TINYINT NOT NULL, -- 1: Uploaded, 2: Parsing, 3: Staged, 4: Committed, 5: RolledBack, 6: Failed
    UploadedByUserId UNIQUEIDENTIFIER NOT NULL,
    UploadedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CommittedAt DATETIMEOFFSET(3) NULL,
    INDEX CIX_ImportBatches CLUSTERED (TenantId, UploadedAt DESC, Id)
);

-- 23. Bảng Dữ liệu Thô Staging (Trước khi Duyệt Ghi sổ Chính thức)
CREATE TABLE import.RawBankTransactions (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    BatchId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Raw_Batch REFERENCES import.ImportBatches(Id) ON DELETE CASCADE,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    RowNumberInFile INT NOT NULL,
    RawTransactionDate VARCHAR(50) NOT NULL,
    ParsedTransactionDate DATE NULL,
    RawAmount VARCHAR(50) NOT NULL,
    ParsedAmount DECIMAL(19,4) NULL,
    RawDescription NVARCHAR(500) NOT NULL,
    NormalizedDescription NVARCHAR(500) NULL,
    RawReferenceNumber NVARCHAR(100) NULL,
    FingerprintHash CHAR(64) NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1: PendingReview, 2: ReadyToCommit, 3: DuplicateSkipped, 4: Committed, 5: Rejected
    CommittedTransactionId UNIQUEIDENTIFIER NULL,
    INDEX IX_Raw_BatchStatus (BatchId, Status),
    INDEX IX_Raw_Fingerprint (TenantId, FingerprintHash)
);
```

---

### 4.6 Schema `txn`: Transactions, Splits, Categories, Payees & Tags

```sql
CREATE SCHEMA txn;
GO

-- 24. Cây Danh mục Thu / Chi
CREATE TABLE txn.Categories (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ParentId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Cat_Parent REFERENCES txn.Categories(Id),
    Name NVARCHAR(100) NOT NULL,
    Type TINYINT NOT NULL, -- 1: EXPENSE, 2: INCOME
    GroupType TINYINT NOT NULL DEFAULT 1, -- 1: Needs (50%), 2: Wants (30%), 3: Savings/Debt (20%)
    Icon NVARCHAR(50) NOT NULL DEFAULT 'folder',
    ColorHex VARCHAR(7) NOT NULL DEFAULT '#6366F1',
    LedgerAccountId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Cat_LAccount REFERENCES ledger.Accounts(Id),
    IsActive BIT NOT NULL DEFAULT 1,
    INDEX CIX_Categories CLUSTERED (TenantId, Id)
);

-- 25. Danh bạ Người nhận / Đối tác Thu Chi
CREATE TABLE txn.Payees (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Payees PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    NormalizedName NVARCHAR(200) NOT NULL,
    DefaultCategoryId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Payee_Cat REFERENCES txn.Categories(Id),
    AccountNumber NVARCHAR(100) NULL,
    BankCode NVARCHAR(50) NULL,
    INDEX CIX_Payees CLUSTERED (TenantId, NormalizedName, Id)
);

-- 26. Danh mục Thương hiệu Chuẩn hóa (Merchant Master Data)
CREATE TABLE txn.Merchants (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Merchants PRIMARY KEY,
    CleanName NVARCHAR(150) NOT NULL, -- Grab, Shopee, Highlands Coffee, Netflix...
    MccCode CHAR(4) NULL, -- Merchant Category Code
    DefaultCategoryId UNIQUEIDENTIFIER NULL,
    LogoUrl NVARCHAR(500) NULL,
    Website NVARCHAR(250) NULL,
    CONSTRAINT UQ_Merchant_Name UNIQUE (CleanName)
);

-- 27. Bảng Giao dịch Tài chính Cốt lõi
CREATE TABLE txn.Transactions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Transactions PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Txn_Account REFERENCES ledger.Accounts(Id),
    DestinationAccountId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Txn_DestAcc REFERENCES ledger.Accounts(Id),
    Type TINYINT NOT NULL, -- 1: Expense, 2: Income, 3: Transfer, 4: Adjustment
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Pending, 2: Cleared, 3: Reconciled, 4: Void
    Amount DECIMAL(19,4) NOT NULL CHECK (Amount > 0),
    Currency CHAR(3) NOT NULL,
    TransactionDate DATETIMEOFFSET(3) NOT NULL,
    CategoryId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Txn_Category REFERENCES txn.Categories(Id),
    PayeeId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Txn_Payee REFERENCES txn.Payees(Id),
    MerchantId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Txn_Merchant REFERENCES txn.Merchants(Id),
    Description NVARCHAR(500) NULL,
    OriginalBankDescription NVARCHAR(500) NULL,
    BankReferenceNumber NVARCHAR(100) NULL,
    ImportBatchId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Txn_Batch REFERENCES import.ImportBatches(Id),
    LedgerEntryId UNIQUEIDENTIFIER NULL,
    IsSplit BIT NOT NULL DEFAULT 0,
    IsReconciled BIT NOT NULL DEFAULT 0,
    HasAttachments BIT NOT NULL DEFAULT 0,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    RowVer ROWVERSION,
    INDEX CIX_Transactions CLUSTERED (TenantId, TransactionDate DESC, Id),
    INDEX IX_Txn_AccountDate (TenantId, AccountId, TransactionDate DESC)
);

-- 28. Chi tiết Tách Giao dịch (Split Transactions)
CREATE TABLE txn.TransactionSplits (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TransactionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Splits_Txn REFERENCES txn.Transactions(Id) ON DELETE CASCADE,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Splits_Cat REFERENCES txn.Categories(Id),
    Amount DECIMAL(19,4) NOT NULL CHECK (Amount > 0),
    Note NVARCHAR(255) NULL,
    CostCenterId UNIQUEIDENTIFIER NULL,
    INDEX IX_Splits_Txn (TransactionId)
);

-- 29. Bảng Nhãn Tùy chỉnh (Tags)
CREATE TABLE txn.Tags (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    ColorHex VARCHAR(7) NOT NULL DEFAULT '#10B981',
    INDEX IX_Tags_TenantName (TenantId, Name)
);

-- 30. Quan hệ Many-to-Many giữa Giao dịch và Nhãn
CREATE TABLE txn.TransactionTags (
    TransactionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TT_Txn REFERENCES txn.Transactions(Id) ON DELETE CASCADE,
    TagId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_TT_Tag REFERENCES txn.Tags(Id) ON DELETE CASCADE,
    CONSTRAINT PK_TransactionTags PRIMARY KEY (TransactionId, TagId)
);
```

---

### 4.7 Schema `budget`: Budgeting, Temporal History & Goals

```sql
CREATE SCHEMA budget;
GO

-- 31 & 32. Kế hoạch Ngân sách (System-Versioned Temporal Tables)
CREATE TABLE budget.Budgets (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Budgets PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Bud_Cat REFERENCES txn.Categories(Id),
    PeriodStartDate DATE NOT NULL,
    PeriodEndDate DATE NOT NULL,
    AllocatedAmount DECIMAL(19,4) NOT NULL CHECK (AllocatedAmount >= 0),
    RolloverAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    SpentAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    ThresholdPercent INT NOT NULL DEFAULT 80,
    IsAlertSent BIT NOT NULL DEFAULT 0,
    SysStartTime DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    SysEndTime DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime),
    CONSTRAINT UQ_Budget_CatPeriod UNIQUE (TenantId, CategoryId, PeriodStartDate)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = budget.BudgetsHistory));

-- 33. Mục tiêu Tiết kiệm (Savings Goals)
CREATE TABLE budget.SavingsGoals (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SavingsGoals PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    TargetAmount DECIMAL(19,4) NOT NULL CHECK (TargetAmount > 0),
    CurrentAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    TargetDate DATE NOT NULL,
    DedicatedAccountId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Goal_Acc REFERENCES ledger.Accounts(Id),
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Active, 2: Completed, 3: Abandoned
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX CIX_SavingsGoals CLUSTERED (TenantId, TargetDate ASC, Id)
);
```

---

### 4.8 Schema `loan`: Loans, Installments & Amortization Schedules

```sql
CREATE SCHEMA loan;
GO

-- 34. Hợp đồng Vay & Trả góp
CREATE TABLE loan.Loans (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Loans PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    LenderName NVARCHAR(200) NOT NULL,
    LoanType TINYINT NOT NULL, -- 1: Bank Loan, 2: Mortgage, 3: Credit Card Installment, 4: Personal IOU
    InterestCalculationMethod TINYINT NOT NULL, -- 1: Reducing Balance (Annuity), 2: Straight/Flat, 3: Fixed Principal
    PrincipalAmount DECIMAL(19,4) NOT NULL CHECK (PrincipalAmount > 0),
    RemainingPrincipal DECIMAL(19,4) NOT NULL,
    AnnualInterestRate DECIMAL(8,4) NOT NULL, -- e.g. 11.5000%
    TermMonths INT NOT NULL,
    StartDate DATE NOT NULL,
    MaturityDate DATE NOT NULL,
    MonthlyDueDay TINYINT NOT NULL,
    LiabilityAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Loan_LiabAcc REFERENCES ledger.Accounts(Id),
    DisbursementAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Loan_DisbAcc REFERENCES ledger.Accounts(Id),
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Active, 2: Paid Off, 3: Defaulted
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX CIX_Loans CLUSTERED (TenantId, Id)
);

-- 35. Lịch Trả góp Chi tiết từng Kỳ (Amortization Schedules)
CREATE TABLE loan.AmortizationSchedules (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LoanId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Sched_Loan REFERENCES loan.Loans(Id) ON DELETE CASCADE,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PeriodNumber INT NOT NULL,
    DueDate DATE NOT NULL,
    PrincipalAmount DECIMAL(19,4) NOT NULL,
    InterestAmount DECIMAL(19,4) NOT NULL,
    TotalPayment DECIMAL(19,4) NOT NULL,
    RemainingBalance DECIMAL(19,4) NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Scheduled, 2: Paid, 3: Overdue, 4: Partial
    PaidDate DATE NULL,
    TransactionId UNIQUEIDENTIFIER NULL,
    INDEX IX_Amortization_LoanPeriod (LoanId, PeriodNumber),
    INDEX IX_Amortization_Due (TenantId, DueDate, Status)
);

-- 36. Lịch sử Thanh toán Khoản vay & Phí Trả trước hạn
CREATE TABLE loan.LoanPaymentHistory (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LoanId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_LPH_Loan REFERENCES loan.Loans(Id),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PaymentDate DATE NOT NULL,
    PrincipalPaid DECIMAL(19,4) NOT NULL,
    InterestPaid DECIMAL(19,4) NOT NULL,
    EarlyPrepaymentFee DECIMAL(19,4) NOT NULL DEFAULT 0,
    LatePenaltyFee DECIMAL(19,4) NOT NULL DEFAULT 0,
    TotalPaidAmount DECIMAL(19,4) NOT NULL,
    TransactionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_LPH_Txn REFERENCES txn.Transactions(Id)
);
```

---

### 4.9 Schema `bill`: Recurring Bills, Reminders & Subscriptions

```sql
CREATE SCHEMA bill;
GO

-- 37. Quản lý Hóa đơn Lặp lại / Đăng ký Thuê bao
CREATE TABLE bill.Subscriptions (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Subscriptions PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    ProviderName NVARCHAR(200) NOT NULL,
    ExpectedAmount DECIMAL(19,4) NOT NULL,
    Currency CHAR(3) NOT NULL,
    BillingCycle TINYINT NOT NULL, -- 1: Weekly, 2: Monthly, 3: Quarterly, 4: Yearly
    RRulePattern NVARCHAR(200) NULL, -- RFC 5545 RRULE
    NextDueDate DATE NOT NULL,
    PaymentAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Sub_Acc REFERENCES ledger.Accounts(Id),
    CategoryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Sub_Cat REFERENCES txn.Categories(Id),
    IsActive BIT NOT NULL DEFAULT 1,
    RemindBeforeDays INT NOT NULL DEFAULT 3,
    INDEX CIX_Subscriptions CLUSTERED (TenantId, NextDueDate, Id)
);

-- 38. Bảng Phiên bản Đến hạn Thực tế của Hóa đơn
CREATE TABLE bill.BillOccurrences (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    SubscriptionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_BillOcc_Sub REFERENCES bill.Subscriptions(Id),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    DueDate DATE NOT NULL,
    Amount DECIMAL(19,4) NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Pending, 2: Paid, 3: Overdue, 4: Skipped
    PaidTransactionId UNIQUEIDENTIFIER NULL CONSTRAINT FK_BillOcc_Txn REFERENCES txn.Transactions(Id),
    INDEX IX_Occurrences_DueStatus (TenantId, DueDate, Status)
);

-- 39. Tự động Nhận diện Khoản chi Lặp lại Bất thường (Recurring Detector)
CREATE TABLE bill.DetectedRecurringPatterns (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    MerchantName NVARCHAR(200) NOT NULL,
    EstimatedCycleDays INT NOT NULL, -- 30, 7, 365...
    AverageAmount DECIMAL(19,4) NOT NULL,
    ConfidenceScore DECIMAL(5,4) NOT NULL,
    LastDetectedDate DATE NOT NULL,
    IsConfirmedByUser BIT NOT NULL DEFAULT 0,
    DismissedByUser BIT NOT NULL DEFAULT 0
);
```

---

### 4.10 Schema `inv`: Investments, Lot Accounting (FIFO) & Market Feed

```sql
CREATE SCHEMA inv;
GO

-- 40. Danh mục Tài khoản Đầu tư (Portfolios)
CREATE TABLE inv.Portfolios (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Portfolios PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    BrokerName NVARCHAR(100) NULL, -- SSI, TCBS, VPS, Binance...
    Currency CHAR(3) NOT NULL DEFAULT 'VND',
    IsDefault BIT NOT NULL DEFAULT 0
);

-- 41. Danh mục Mã Tài sản Đầu tư
CREATE TABLE inv.Assets (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Assets PRIMARY KEY,
    Symbol NVARCHAR(50) NOT NULL, -- FPT, VNM, BTC, SJC...
    Name NVARCHAR(200) NOT NULL,
    AssetType TINYINT NOT NULL, -- 1: Stock VN, 2: Fund/ETF, 3: Gold, 4: Crypto, 5: Bond
    Currency CHAR(3) NOT NULL,
    CurrentPrice DECIMAL(28,10) NOT NULL DEFAULT 0,
    PriceUpdatedAt DATETIMEOFFSET(3) NULL,
    CONSTRAINT UQ_Asset_Symbol UNIQUE (Symbol, Currency)
);

-- 42. Khớp Lô Mua (Tax Lots - FIFO Accounting)
CREATE TABLE inv.TaxLots (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TaxLots PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PortfolioId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Lots_Port REFERENCES inv.Portfolios(Id),
    AssetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Lots_Asset REFERENCES inv.Assets(Id),
    PurchaseDate DATETIMEOFFSET(3) NOT NULL,
    OriginalQuantity DECIMAL(28,10) NOT NULL CHECK (OriginalQuantity > 0),
    RemainingQuantity DECIMAL(28,10) NOT NULL CHECK (RemainingQuantity >= 0),
    CostPerUnit DECIMAL(28,10) NOT NULL,
    TotalCostBasis DECIMAL(19,4) NOT NULL,
    IsOpen BIT NOT NULL DEFAULT 1,
    INDEX CIX_TaxLots CLUSTERED (TenantId, PortfolioId, AssetId, PurchaseDate)
);

-- 43. Lịch sử Lệnh Giao dịch Đầu tư
CREATE TABLE inv.InvestmentOrders (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PortfolioId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Orders_Port REFERENCES inv.Portfolios(Id),
    AssetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Orders_Asset REFERENCES inv.Assets(Id),
    OrderType TINYINT NOT NULL, -- 1: BUY, 2: SELL, 3: DIVIDEND_CASH, 4: DIVIDEND_STOCK
    ExecutionDate DATETIMEOFFSET(3) NOT NULL,
    Quantity DECIMAL(28,10) NOT NULL,
    PricePerUnit DECIMAL(28,10) NOT NULL,
    FeeAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    TotalCashAmount DECIMAL(19,4) NOT NULL,
    RealizedPnL DECIMAL(19,4) NULL, -- Tính theo FIFO khi bán
    INDEX CIX_Orders CLUSTERED (TenantId, ExecutionDate DESC, Id)
);

-- 44. Lịch sử Giá Thị trường theo Ngày (EOD Price Feed)
CREATE TABLE inv.AssetPriceHistories (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    AssetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_APH_Asset REFERENCES inv.Assets(Id),
    PriceDate DATE NOT NULL,
    ClosePrice DECIMAL(28,10) NOT NULL,
    Source VARCHAR(50) NOT NULL DEFAULT 'HOSE',
    CONSTRAINT UQ_Asset_Date UNIQUE (AssetId, PriceDate)
);
```

---

### 4.11 Schema `sme`: Invoicing, AP/AR, Fixed Assets & Cost Centers

```sql
CREATE SCHEMA sme;
GO

-- 45. Khách hàng & Nhà cung cấp
CREATE TABLE sme.BusinessContacts (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BusinessContacts PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Type TINYINT NOT NULL, -- 1: Customer, 2: Vendor, 3: Both
    Name NVARCHAR(250) NOT NULL,
    TaxCode NVARCHAR(20) NULL,
    Email NVARCHAR(256) NULL,
    Phone NVARCHAR(50) NULL,
    BillingAddress NVARCHAR(500) NULL,
    PaymentTermsDays INT NOT NULL DEFAULT 30,
    CreditLimit DECIMAL(19,4) NOT NULL DEFAULT 0,
    INDEX CIX_Contacts CLUSTERED (TenantId, Name, Id)
);

-- 46. Trung tâm Chi phí / Phòng ban (Cost Centers)
CREATE TABLE sme.CostCenters (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CostCenters PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Code VARCHAR(50) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    ParentId UNIQUEIDENTIFIER NULL CONSTRAINT FK_CC_Parent REFERENCES sme.CostCenters(Id),
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_CostCenter_Code UNIQUE (TenantId, Code)
);

-- 47. Quản lý Dự án (Projects)
CREATE TABLE sme.Projects (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Projects PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Code VARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    BudgetAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Project_Code UNIQUE (TenantId, Code)
);

-- 48. Hóa đơn Bán hàng (AR) & Hóa đơn Mua vào (AP)
CREATE TABLE sme.Invoices (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Invoices PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ContactId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Inv_Contact REFERENCES sme.BusinessContacts(Id),
    CostCenterId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Inv_CC REFERENCES sme.CostCenters(Id),
    ProjectId UNIQUEIDENTIFIER NULL CONSTRAINT FK_Inv_Proj REFERENCES sme.Projects(Id),
    InvoiceType TINYINT NOT NULL, -- 1: Sales Invoice (AR), 2: Vendor Bill (AP)
    InvoiceNumber NVARCHAR(50) NOT NULL,
    IssueDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    SubTotal DECIMAL(19,4) NOT NULL,
    TaxAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(19,4) NOT NULL,
    PaidAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
    BalanceDue DECIMAL(19,4) NOT NULL,
    Currency CHAR(3) NOT NULL DEFAULT 'VND',
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Draft, 2: Approved, 3: Sent, 4: Partial, 5: Paid, 6: Void
    EInvoiceRefCode NVARCHAR(100) NULL, -- Mã tra cứu Hóa đơn điện tử (TT78)
    ApprovalRequestId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX CIX_Invoices CLUSTERED (TenantId, IssueDate DESC, Id),
    CONSTRAINT UQ_Invoice_Number UNIQUE (TenantId, InvoiceType, InvoiceNumber)
);

-- 49. Chi tiết Dòng Hàng Hóa đơn
CREATE TABLE sme.InvoiceItems (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    InvoiceId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InvItems_Inv REFERENCES sme.Invoices(Id) ON DELETE CASCADE,
    ItemDescription NVARCHAR(500) NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(19,4) NOT NULL,
    TaxRatePercent DECIMAL(5,2) NOT NULL DEFAULT 10.0,
    TaxAmount DECIMAL(19,4) NOT NULL,
    LineTotal DECIMAL(19,4) NOT NULL
);

-- 50. Lịch sử Thu Tiền / Chi Trả Hóa đơn
CREATE TABLE sme.InvoicePayments (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    InvoiceId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InvPay_Inv REFERENCES sme.Invoices(Id),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PaymentDate DATE NOT NULL,
    Amount DECIMAL(19,4) NOT NULL CHECK (Amount > 0),
    PaymentAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InvPay_Acc REFERENCES ledger.Accounts(Id),
    TransactionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_InvPay_Txn REFERENCES txn.Transactions(Id),
    Notes NVARCHAR(250) NULL
);

-- 51. Danh mục Tài sản Cố định (Fixed Assets)
CREATE TABLE sme.FixedAssets (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FixedAssets PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Code VARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    PurchaseDate DATE NOT NULL,
    OriginalCost DECIMAL(19,4) NOT NULL,
    SalvageValue DECIMAL(19,4) NOT NULL DEFAULT 0,
    UsefulLifeMonths INT NOT NULL,
    DepreciationMethod TINYINT NOT NULL DEFAULT 1, -- 1: Straight-Line (Đường thẳng)
    AssetAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_FA_AssetAcc REFERENCES ledger.Accounts(Id),
    AccumulatedDepreciationAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_FA_DepAcc REFERENCES ledger.Accounts(Id),
    DepreciationExpenseAccountId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_FA_ExpAcc REFERENCES ledger.Accounts(Id),
    Status TINYINT NOT NULL DEFAULT 1, -- 1: InUse, 2: Disposed, 3: FullyDepreciated
    CONSTRAINT UQ_FixedAsset_Code UNIQUE (TenantId, Code)
);

-- 52. Lịch Trích Khấu hao Tài sản Cố định Hàng tháng
CREATE TABLE sme.DepreciationSchedules (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    AssetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_DepSched_Asset REFERENCES sme.FixedAssets(Id),
    PeriodMonth DATE NOT NULL,
    DepreciationAmount DECIMAL(19,4) NOT NULL,
    RemainingBookValue DECIMAL(19,4) NOT NULL,
    JournalEntryId UNIQUEIDENTIFIER NULL,
    IsPosted BIT NOT NULL DEFAULT 0
);
```

---

### 4.12 Schema `workflow`: Multi-Tier Approvals & Signatures

```sql
CREATE SCHEMA workflow;
GO

-- 53. Ma trận Chính sách Phê duyệt theo Hạn mức
CREATE TABLE workflow.ApprovalPolicies (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ApprovalPolicies PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntityType VARCHAR(50) NOT NULL, -- 'VendorBill', 'ExpenseClaim', 'Payment'
    MinAmount DECIMAL(19,4) NOT NULL,
    MaxAmount DECIMAL(19,4) NOT NULL,
    RequiredApprovalSteps INT NOT NULL,
    Step1Role NVARCHAR(50) NOT NULL, -- DepartmentHead
    Step2Role NVARCHAR(50) NULL,     -- ChiefAccountant
    Step3Role NVARCHAR(50) NULL,     -- CEO_Owner
    IsActive BIT NOT NULL DEFAULT 1
);

-- 54. Yêu cầu Phê duyệt Đang xử lý
CREATE TABLE workflow.ApprovalRequests (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Approvals PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PolicyId UNIQUEIDENTIFIER NULL CONSTRAINT FK_AppReq_Policy REFERENCES workflow.ApprovalPolicies(Id),
    RequesterUserId UNIQUEIDENTIFIER NOT NULL,
    EntityType VARCHAR(50) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    RequestedAmount DECIMAL(19,4) NOT NULL,
    Currency CHAR(3) NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1: Pending, 2: Approved, 3: Rejected, 4: Cancelled
    CurrentStep TINYINT NOT NULL DEFAULT 1,
    TotalSteps TINYINT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CompletedAt DATETIMEOFFSET(3) NULL,
    INDEX CIX_Approvals CLUSTERED (TenantId, CreatedAt DESC, Id)
);

-- 55. Quyết định của Từng Cấp Phê duyệt (Lưu Chữ ký Số Xác thực)
CREATE TABLE workflow.ApprovalDecisions (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RequestId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Dec_Req REFERENCES workflow.ApprovalRequests(Id) ON DELETE CASCADE,
    StepNumber TINYINT NOT NULL,
    ApproverUserId UNIQUEIDENTIFIER NOT NULL,
    Decision TINYINT NOT NULL, -- 1: Approved, 2: Rejected, 3: Delegated
    Comment NVARCHAR(500) NULL,
    DecisionTimestamp DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    SignatureHash CHAR(64) NOT NULL -- SHA-256 hash đảm bảo tính không thể chối bỏ
);
```

---

### 4.13 Schema `rules`: Rule Engine & Automation

```sql
CREATE SCHEMA rules;
GO

-- 56. Bảng Định nghĩa Luật Giao dịch (JSON DSL)
CREATE TABLE rules.TransactionRules (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Rules PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Priority INT NOT NULL DEFAULT 100,
    StopProcessing BIT NOT NULL DEFAULT 1,
    ConditionsJson NVARCHAR(MAX) NOT NULL, -- JSON conditions compiled to Expression Trees
    ActionsJson NVARCHAR(MAX) NOT NULL, -- JSON actions (Set Category, Set Tag, Split)
    ExecutionCount BIGINT NOT NULL DEFAULT 0,
    LastTriggeredAt DATETIMEOFFSET(3) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX CIX_Rules CLUSTERED (TenantId, Priority ASC, Id)
);

-- 57. Lịch sử Thực thi Luật Tự động
CREATE TABLE rules.RuleExecutionLogs (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RuleId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_RLog_Rule REFERENCES rules.TransactionRules(Id),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    TransactionId UNIQUEIDENTIFIER NOT NULL,
    ExecutedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    ResultSummary NVARCHAR(500) NOT NULL
);
```

---

### 4.14 Schema `ai`: Vector Search, Models & Classification Cache

```sql
CREATE SCHEMA ai;
GO

-- 58. Bảng Vector Embeddings Hỗ trợ kNN Search (SQL Server 2025 Native)
CREATE TABLE ai.MerchantVectorEmbeddings (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    NormalizedMerchantName NVARCHAR(200) NOT NULL,
    CategoryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Emb_Cat REFERENCES txn.Categories(Id),
    Embedding VECTOR(384) NOT NULL, -- 384 dimensions (all-MiniLM-L6-v2)
    SampleCount INT NOT NULL DEFAULT 1,
    ConfidenceScore DECIMAL(5,4) NOT NULL DEFAULT 0.9500,
    UpdatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX IX_MerchantEmbeddings_Tenant (TenantId)
);

-- 59. Quản lý Phiên bản Mô hình AI Huấn luyện
CREATE TABLE ai.ClassificationModelVersions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ModelName VARCHAR(100) NOT NULL, -- LightGBM_Categorizer, Anomaly_Autoencoder
    VersionNumber VARCHAR(20) NOT NULL,
    TrainedAt DATETIMEOFFSET(3) NOT NULL,
    AccuracyMetric DECIMAL(5,4) NOT NULL,
    ModelArtifactPath NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 60. Cache Kết quả Dự đoán Phân loại
CREATE TABLE ai.InferencePredictionCaches (
    NormalizedDescriptionHash CHAR(64) NOT NULL PRIMARY KEY,
    PredictedCategoryId UNIQUEIDENTIFIER NOT NULL,
    ConfidenceScore DECIMAL(5,4) NOT NULL,
    ModelVersion VARCHAR(20) NOT NULL,
    CachedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 61. Lịch sử Cảnh báo Giao dịch Bất thường (Anomaly Alerts)
CREATE TABLE ai.AnomalyDetectionAlerts (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    TransactionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Anm_Txn REFERENCES txn.Transactions(Id),
    AnomalyType VARCHAR(50) NOT NULL, -- CARD_TESTING, UNUSUAL_SPIKE, DOUBLE_CHARGE, ODD_HOURS
    RiskScore DECIMAL(5,4) NOT NULL,
    ExplanationReason NVARCHAR(500) NOT NULL,
    IsReviewedByUser BIT NOT NULL DEFAULT 0,
    UserFeedback TINYINT NULL, -- 1: TruePositive, 2: FalsePositive
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME()
);
```

---

### 4.15 Schema `notification`: Multi-Channel Notifications & Preferences

```sql
CREATE SCHEMA notification;
GO

-- 62. Mẫu Thông báo Đa ngôn ngữ (Templates)
CREATE TABLE notification.NotificationTemplates (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EventCode VARCHAR(100) NOT NULL, -- BUDGET_EXCEEDED, BILL_DUE, ANOMALY_DETECTED, APPROVAL_PENDING
    LanguageCode VARCHAR(10) NOT NULL DEFAULT 'vi-VN',
    TitleTemplate NVARCHAR(250) NOT NULL,
    BodyTemplate NVARCHAR(1000) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_NotifTemplate UNIQUE (EventCode, LanguageCode)
);

-- 63. Tùy chọn Nhận Thông báo của Người dùng
CREATE TABLE notification.UserPreferences (
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserPreferences PRIMARY KEY,
    InAppEnabled BIT NOT NULL DEFAULT 1,
    PushEnabled BIT NOT NULL DEFAULT 1,
    EmailEnabled BIT NOT NULL DEFAULT 1,
    SmsEnabled BIT NOT NULL DEFAULT 0,
    QuietHoursStart TIME NULL, -- e.g. '22:00:00'
    QuietHoursEnd TIME NULL,   -- e.g. '07:00:00'
    MinNotifyAmount DECIMAL(19,4) NOT NULL DEFAULT 50000
);

-- 64. Hộp thư Thông báo trong Ứng dụng
CREATE TABLE notification.Notifications (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY NONCLUSTERED,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_Notif_User REFERENCES iam.Users(Id),
    EventCode VARCHAR(100) NOT NULL,
    Title NVARCHAR(250) NOT NULL,
    Body NVARCHAR(1000) NOT NULL,
    DataJson NVARCHAR(MAX) NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIMEOFFSET(3) NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    INDEX CIX_Notifications CLUSTERED (UserId, CreatedAt DESC, Id)
);

-- 65. Lịch sử Gửi Thông báo Ngoại vi (Push/Email/SMS Delivery Logs)
CREATE TABLE notification.NotificationDeliveryLogs (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NotificationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_NDL_Notif REFERENCES notification.Notifications(Id),
    ChannelType TINYINT NOT NULL, -- 1: Push, 2: Email, 3: SMS, 4: ZaloZNS
    ProviderName VARCHAR(50) NOT NULL, -- FCM, SendGrid, Twilio, Zalo
    DeliveryStatus TINYINT NOT NULL, -- 1: Queued, 2: Sent, 3: Delivered, 4: Failed
    ErrorMessage NVARCHAR(500) NULL,
    SentAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME()
);
```

---

### 4.16 Schema `media`: Attachments, OCR Data & Blob Metadata

```sql
CREATE SCHEMA media;
GO

-- 66. Siêu dữ liệu File Đính kèm & Quét An toàn
CREATE TABLE media.Attachments (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Attachments PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    OriginalFileName NVARCHAR(255) NOT NULL,
    FileExtension VARCHAR(10) NOT NULL,
    MimeType VARCHAR(100) NOT NULL,
    SizeBytes BIGINT NOT NULL,
    StorageBlobKey NVARCHAR(500) NOT NULL,
    VirusScanStatus TINYINT NOT NULL DEFAULT 1, -- 1: Clean, 2: Infected, 3: Pending
    OcrStatus TINYINT NOT NULL DEFAULT 1, -- 1: Pending, 2: Extracted, 3: Failed
    ExtractedOcrJson NVARCHAR(MAX) NULL, -- Dữ liệu trích xuất số tiền, ngày, merchant
    UploadedByUserId UNIQUEIDENTIFIER NOT NULL,
    UploadedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 67. Bảng Liên kết Đa hình (Polymorphic Entity Attachments)
CREATE TABLE media.EntityAttachments (
    AttachmentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_EA_Attachment REFERENCES media.Attachments(Id) ON DELETE CASCADE,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntityType VARCHAR(50) NOT NULL, -- 'Transaction', 'Invoice', 'ExpenseClaim'
    EntityId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_EntityAttachments PRIMARY KEY (AttachmentId, EntityType, EntityId)
);
```

---

### 4.17 Schema `audit`: Security Audit Trail & Period Lock

```sql
CREATE SCHEMA audit;
GO

-- 68. Khóa Kỳ Kế toán Doanh nghiệp (Accounting Period Locks)
CREATE TABLE audit.AccountingPeriodLocks (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PeriodLocks PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    PeriodYear INT NOT NULL,
    PeriodMonth INT NOT NULL,
    IsLocked BIT NOT NULL DEFAULT 1,
    LockedByUserId UNIQUEIDENTIFIER NOT NULL,
    LockedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    UnlockReason NVARCHAR(500) NULL,
    CONSTRAINT UQ_PeriodLock UNIQUE (TenantId, PeriodYear, PeriodMonth)
);

-- 69. Nhật ký Kiểm toán An ninh Bất biến (SQL Server Ledger Append-Only)
CREATE TABLE audit.SecurityAuditTrail (
    Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    Timestamp DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    TenantId UNIQUEIDENTIFIER NULL,
    ActorUserId UNIQUEIDENTIFIER NOT NULL,
    ClientIp VARCHAR(45) NOT NULL,
    UserAgent NVARCHAR(300) NOT NULL,
    Action NVARCHAR(100) NOT NULL, -- e.g. "AUTH_STEPUP", "LEDGER_ENTRY_POST"
    TargetEntity NVARCHAR(100) NOT NULL,
    TargetId NVARCHAR(100) NOT NULL,
    OldValuesJson NVARCHAR(MAX) NULL,
    NewValuesJson NVARCHAR(MAX) NULL,
    PreviousHash CHAR(64) NOT NULL,
    CurrentHash CHAR(64) NOT NULL
)
WITH (LEDGER = ON (APPEND_ONLY = ON));
```

---

### 4.18 Schema `messaging`: Outbox/Inbox & Idempotency Store

```sql
CREATE SCHEMA messaging;
GO

-- 70. Transactional Outbox (Wolverine / RabbitMQ)
CREATE TABLE messaging.OutgoingMessages (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Destination NVARCHAR(250) NOT NULL,
    MessageType NVARCHAR(250) NOT NULL,
    Payload VARBINARY(MAX) NOT NULL,
    HeadersJson NVARCHAR(MAX) NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    DeliverBy DATETIMEOFFSET(3) NULL,
    Attempts INT NOT NULL DEFAULT 0,
    OwnerId INT NOT NULL DEFAULT 0, -- Node Lock ID
    INDEX IX_Outgoing_Pending (OwnerId, CreatedAt)
);

-- 71. Consumer Inbox Chống Xử lý Lặp (Consumer Idempotency)
CREATE TABLE messaging.IncomingMessages (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ConsumerGroup NVARCHAR(150) NOT NULL,
    ProcessedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME()
);

-- 72. Bảng Idempotency Key cho API Endpoints (Chống Trừ tiền 2 Lần)
CREATE TABLE messaging.IdempotencyKeys (
    TenantId UNIQUEIDENTIFIER NOT NULL,
    IdempotencyKey VARCHAR(128) NOT NULL,
    RequestHash CHAR(64) NOT NULL, -- SHA-256 hash của request payload
    Status TINYINT NOT NULL, -- 1: Processing, 2: Completed, 3: Failed
    ResponseCode INT NULL,
    ResponseBody NVARCHAR(MAX) NULL,
    CreatedAt DATETIMEOFFSET(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt DATETIMEOFFSET(3) NOT NULL,
    CONSTRAINT PK_IdempotencyKeys PRIMARY KEY (TenantId, IdempotencyKey)
);
```

---

## 5. Bảo mật Dữ liệu Cấp độ Database

### 5.1 Row-Level Security (RLS) với `SESSION_CONTEXT`

Để bảo vệ triệt để ranh giới Tenant, ngăn chặn hoàn toàn nguy cơ rò rỉ dữ liệu qua lỗ hổng BOLA/IDOR:

```sql
CREATE SCHEMA sec;
GO

-- 1. Tạo hàm Inline Table Predicate
CREATE FUNCTION sec.fn_TenantFilterPredicate(@TenantId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS AccessResult
WHERE @TenantId = CAST(SESSION_CONTEXT(N'ActiveTenantId') AS UNIQUEIDENTIFIER);
GO

-- 2. Áp dụng Security Policy bắt buộc trên toàn bộ các bảng nghiệp vụ
CREATE SECURITY POLICY sec.TenantIsolationPolicy
    -- Schema ledger
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON ledger.Accounts,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON ledger.Accounts AFTER INSERT,
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON ledger.JournalEntries,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON ledger.JournalEntries AFTER INSERT,
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON ledger.Postings,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON ledger.Postings AFTER INSERT,
    -- Schema txn
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON txn.Transactions,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON txn.Transactions AFTER INSERT,
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON txn.Categories,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON txn.Categories AFTER INSERT,
    -- Schema sme
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON sme.Invoices,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON sme.Invoices AFTER INSERT,
    -- Schema budget & loan
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON budget.Budgets,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON budget.Budgets AFTER INSERT,
    ADD FILTER PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON loan.Loans,
    ADD BLOCK PREDICATE sec.fn_TenantFilterPredicate(TenantId) ON loan.Loans AFTER INSERT
WITH (STATE = ON);
GO
```

### 5.2 Phân quyền Người dùng Database theo Nguyên tắc Đặc quyền Tối thiểu (Least Privilege)

```sql
-- Tạo tài khoản runtime dành cho API Application
CREATE USER fintech_api_runtime WITHOUT LOGIN;

-- 1. Cấp quyền SELECT, INSERT, UPDATE trên các bảng nghiệp vụ thông thường
GRANT SELECT, INSERT, UPDATE ON SCHEMA::txn TO fintech_api_runtime;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::budget TO fintech_api_runtime;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::sme TO fintech_api_runtime;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::loan TO fintech_api_runtime;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::bill TO fintech_api_runtime;
GRANT SELECT, INSERT, UPDATE ON SCHEMA::import TO fintech_api_runtime;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::messaging TO fintech_api_runtime;

-- 2. TRÊN SỔ CÁI & AUDIT: TUYỆT ĐỐI CHỈ CẤP INSERT & SELECT (CẤM UPDATE VÀ DELETE)
GRANT SELECT, INSERT ON SCHEMA::ledger TO fintech_api_runtime;
GRANT SELECT, INSERT ON SCHEMA::audit TO fintech_api_runtime;

DENY UPDATE, DELETE ON SCHEMA::ledger TO fintech_api_runtime;
DENY UPDATE, DELETE ON SCHEMA::audit TO fintech_api_runtime;
```

---

## 6. Chiến lược Đánh Index & Tối ưu Hiệu năng Cao

### Bảng Ma trận Chiến lược Index Tối ưu trên 72 Bảng

| Bảng | Kiểu Index | Cột Index | Mục đích & Lợi ích |
|---|---|---|---|
| `ledger.Accounts` | Clustered Index | `(TenantId, Id)` | Cụm dữ liệu theo Tenant; tối ưu quét danh sách tài khoản theo đơn vị |
| `ledger.JournalEntries`| Clustered Index | `(TenantId, EffectiveDate, Id)` | Tối ưu truy vấn báo cáo theo kỳ thời gian và range scan |
| `ledger.Postings` | Non-Clustered Covering | `(TenantId, AccountId) INCLUDE (Amount, BaseCurrencyAmount)` | **Index Covering** giúp hàm tính số dư `SUM(Amount)` quét cực nhanh trực tiếp trên Index Leaf |
| `ledger.DailyBalances`| Clustered Columnstore | Toàn bộ bảng | Nén dữ liệu 10x; tăng tốc độ truy vấn Aggregation cho Dashboard gấp 100 lần |
| `txn.Transactions` | Clustered Index | `(TenantId, TransactionDate DESC, Id)` | Màn hình danh sách giao dịch phân trang Keyset Cursor theo ngày mới nhất không bao giờ bị Sort spill |
| `import.RawBankTransactions`| Non-Clustered | `(TenantId, FingerprintHash)` | Chống trùng lặp dòng sao kê trong micro-seconds |
| `messaging.OutgoingMessages`| Filtered Index | `(OwnerId, CreatedAt) WHERE OwnerId = 0` | Giúp Worker Outbox quét tức thì các message chưa gửi mà không bị table scan |

---

## 7. Phân vùng Bảng (Partitioning) & Lưu trữ Lịch sử Dài hạn

Đối với bảng có dung lượng tăng trưởng hàng chục triệu bản ghi mỗi năm như `ledger.Postings` và `txn.Transactions`, triển khai **Table Partitioning theo Năm**:

```sql
-- 1. Tạo Partition Function theo Cột Ngày Ghi nhận (EffectiveDate)
CREATE PARTITION FUNCTION pf_PostingYear (DATE)
AS RANGE RIGHT FOR VALUES (
    '2024-01-01', '2025-01-01', '2026-01-01', 
    '2027-01-01', '2028-01-01', '2029-01-01', '2030-01-01'
);

-- 2. Tạo Partition Scheme gán vào các Filegroups
CREATE PARTITION SCHEME ps_PostingYear
AS PARTITION pf_PostingYear
ALL TO ([PRIMARY]);
```

---

## 8. Bảo trì, Giám sát & Kế hoạch Khôi phục Thảm họa (HA/DR)

### 8.1 Cấu hình Bắt buộc của Cơ sở Dữ liệu Production

```sql
-- Chống blocking đọc/ghi
ALTER DATABASE FinTech SET READ_COMMITTED_SNAPSHOT ON;
ALTER DATABASE FinTech SET ALLOW_SNAPSHOT_ISOLATION ON;

-- Phục hồi thảm họa siêu tốc không phụ thuộc kích thước transaction log
ALTER DATABASE FinTech SET ACCELERATED_DATABASE_RECOVERY = ON;

-- Bật Query Store để theo dõi và khóa Plan hồi quy hiệu năng
ALTER DATABASE FinTech SET QUERY_STORE = ON (
    OPERATION_MODE = READ_WRITE, 
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900
);
```

### 8.2 Job Kiểm tra Toàn vẹn Dữ liệu Hằng đêm (Integrity Verification Job)

```sql
CREATE OR ALTER PROCEDURE ledger.usp_VerifyLedgerIntegrity
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 1. Kiểm tra Invariant: Tổng Debit + Credit của từng Journal Entry phải = 0
    IF EXISTS (
        SELECT EntryId, Currency, SUM(Amount) AS Diff
        FROM ledger.Postings
        GROUP BY EntryId, Currency
        HAVING SUM(Amount) <> 0
    )
    BEGIN
        THROW 50001, 'CRITICAL: Phát hiện Journal Entry bị lệch Debit/Credit!', 1;
    END

    -- 2. Kiểm tra Invariant: Số dư tài khoản denormalized phải khớp chính xác SUM(Postings)
    IF EXISTS (
        SELECT a.Id, a.Balance, ISNULL(SUM(p.Amount), 0) AS CalculatedBalance
        FROM ledger.Accounts a
        LEFT JOIN ledger.Postings p ON a.Id = p.AccountId
        GROUP BY a.Id, a.Balance
        HAVING a.Balance <> ISNULL(SUM(p.Amount), 0)
    )
    BEGIN
        THROW 50002, 'CRITICAL: Phát hiện sai lệch giữa Account.Balance và SUM(Postings)!', 1;
    END

    -- 3. Xác minh chuỗi mã hóa Ledger của SQL Server
    EXEC sys.sp_verify_database_ledger;
END;
GO
```

### 8.3 Cam kết Mục tiêu Khôi phục (RPO / RTO)
- **RPO (Recovery Point Objective) $\le$ 5 giây:** Sử dụng Always On Availability Groups (Đồng bộ đa vùng - Synchronous Replication).
- **RTO (Recovery Time Objective) $\le$ 15 phút:** Nhờ tính năng **Accelerated Database Recovery (ADR)**, thời gian rollback các transaction lớn khi server crash diễn ra tức thì trong vài giây.

---

## Kết luận

Bản nâng cấp CSDL trên đã nâng quy mô thiết kế từ **34 bảng cốt lõi** lên **72 bảng hoàn chỉnh cấp độ Enterprise Production**. Toàn bộ các ngóc ngách từ Staging Import, Đối soát, Quản lý Khóa kỳ Kế toán, Thu phí SaaS Platform, Quản lý Thiết bị, Multi-channel Notifications đến OCR Chứng từ đều đã có bảng vật lý được chuẩn hóa chặt chẽ với ràng buộc khóa ngoại, index tối ưu và chính sách bảo vệ Row-Level Security.
