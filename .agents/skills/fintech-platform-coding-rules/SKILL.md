---
name: fintech-platform-coding-rules
description: >-
  Bộ quy tắc và tiêu chuẩn bắt buộc dành cho AI khi lập trình hệ thống FinTech Platform
  (Clean Architecture, .NET 10, C# 14, SQL Server 2025, React 19, React Native).
  Áp dụng khi sinh mã, sửa lỗi, review code, thiết kế database hoặc cấu hình bảo mật.
---

# 🤖 BỘ QUY TẮC BẮT BUỘC KHI AI VIẾT CODE CHO FINTECH PLATFORM

Tài liệu này quy định các **nguyên tắc bất khả xâm phạm**, **chuẩn code**, **kiến trúc tầng lớp** và **quy tắc an toàn** mà AI Assistant (và lập trình viên) BẮT BUỘC phải tuân thủ tuyệt đối trong toàn bộ vòng đời phát triển dự án `FindTechPlatform`.

---

## 1. NGUYÊN TẮC BẤT KHẢ XÂM PHẠM (IRON RULES)

### 🔴 QUY TẮC 1: TIỀN TỆ & TÍNH TOÁN TÀI CHÍNH (MONEY INTEGRITY)
1. **CẤM DÙNG SỐ DẤU PHẨY ĐỘNG:**
   - Tuyệt đối KHÔNG BAO GIỜ dùng `float`, `double` (trong C#) hay `number` (trong JavaScript/TypeScript) để tính toán tiền tệ.
   - **Backend:** Luôn dùng `decimal` trong C# và `DECIMAL(19,4)` trong SQL Server. (Không dùng kiểu `money` của SQL Server vì lỗi làm tròn 4 số khi chia).
   - **Frontend (Web/Mobile):** Dùng `string` cho API payload và dùng thư viện `big.js` / `decimal.js` cho mọi phép tính trên client. Chỉ convert sang hiển thị ở bước cuối qua `Intl.NumberFormat`.
2. **TIỀN PHẢI LUÔN GẮN VỚI MÃ TIỀN TỆ:**
   - Dùng Value Object `Money(decimal Amount, Currency Currency)`.
   - Cấm các phép toán cộng/trừ khác mã tiền tệ nếu không có tỷ giá quy đổi tường minh.
3. **BẢO TOÀN PHẦN DƯ KHI CHIA TIỀN (LARGEST REMAINDER):**
   - Khi chia tiền cho nhiều phần (trả góp, tách hóa đơn), tổng các phần sau khi làm tròn phải đúng chính xác 100% số tiền gốc. Dùng thuật toán *Largest Remainder Allocation*, cấm để mất hoặc dôi 1 đồng.
4. **LÀM TRÒN:**
   - Chỉ làm tròn ở bước kết quả cuối cùng. Lãi suất và amortizations ưu tiên Banker's Rounding (`MidpointRounding.ToEven` hoặc `AwayFromZero` tùy quy định sản phẩm).

---

### 🔴 QUY TẮC 2: SỔ CÁI BẤT BIẾN & CONCURRENCY (LEDGER & ACID)
1. **DOUBLE-ENTRY LEDGER BẤT BIẾN:**
   - Bảng `Postings` và `JournalEntries` là **APPEND-ONLY**.
   - CẤM viết câu lệnh `UPDATE` hoặc `DELETE` trên bảng `Postings`. Mọi sai sót phải giải quyết bằng **Bút toán Đảo (Reversal Entry)**.
   - Invariant: Tổng Debit trừ Tổng Credit của một bút toán phải bằng `0` theo từng loại tiền tệ.
2. **CẬP NHẬT SỐ DƯ NGUYÊN TỬ (NO LOST UPDATES):**
   - Cấm đọc số dư về Application rồi tính toán rồi gọi `UPDATE Balance = @computed`.
   - Bắt buộc cập nhật số dư nguyên tử tại Database:
     ```sql
     UPDATE ledger.Accounts WITH (ROWLOCK)
     SET Balance = Balance + @delta, Version = Version + 1
     WHERE Id = @id AND (AllowNegative = 1 OR Balance + @delta >= 0);
     ```
3. **CHỐNG DEADLOCK BẰNG THỨ TỰ KHÓA CỐ ĐỊNH:**
   - Khi một giao dịch tác động lên nhiều tài khoản (ví dụ chuyển khoản $A \to B$), luôn sắp xếp danh sách tài khoản theo thứ tự tăng dần của `AccountId` trước khi thực thi khóa.
4. **BẮT BUỘC IDEMPOTENCY KEY:**
   - Mọi API làm biến động tài chính hoặc ghi sổ phải có `Idempotency-Key` header (UUID). Nếu client retry do lỗi mạng, server phải trả về kết quả đã lưu mà không ghi nhận lần 2.

---

### 🔴 QUY TẮC 3: BẢO MẬT PHÒNG THỦ CHIỀU SÂU (DEFENSE-IN-DEPTH)
1. **CHỐNG BOLA / IDOR 2 LỚP:**
   - Lớp 1 (Application): Dùng ASP.NET Core Resource Authorization kiểm tra quyền sở hữu Tenant/User. Nếu sai quyền, trả về `404 Not Found` (không trả 403 để tránh rò rỉ ID).
   - Lớp 2 (Database): Bắt buộc kích hoạt **SQL Server Row-Level Security (RLS)** thông qua `SESSION_CONTEXT(N'ActiveTenantId')`.
2. **KHÔNG LƯU TOKEN Ở CLIENT (BFF PATTERN):**
   - Web SPA không được lưu Access Token vào `localStorage` hay `sessionStorage`. Toàn bộ xác thực Web đi qua **BFF Gateway (YARP)** dùng `HttpOnly`, `Secure`, `SameSite=Strict` Cookie.
3. **KHÔNG LOG THÔNG TIN NHẠY CẢM (PII & SECRETS):**
   - Tuyệt đối không in vào Serilog/Console: Số tài khoản đầy đủ, số thẻ, CVV, mật khẩu, Token, OTP. Cấu hình Masking tự động cho Serilog.
4. **CHỐNG RCE TRONG RULE ENGINE:**
   - Cấm dùng `eval`, JavaScript V8 Engine, hay Roslyn scripting tùy tiện trong Rule Engine. Mọi quy tắc phải được biên dịch thông qua **C# Expression Trees (`System.Linq.Expressions`)** hoặc sandbox CEL.

---

## 2. QUY CHUẨN KIẾN TRÚC & TỔ CHỨC CODE (CLEAN ARCHITECTURE)

### 2.1 Cấu trúc Chuẩn của Mỗi Module (Pragmatic Modular Monolith)
Mỗi module (Ledger, Accounts, Transactions, v.v.) tuân thủ mô hình Clean Architecture tinh gọn gồm **2 Project C#**:
```
src/Modules/<ModuleName>/
├── FinTech.Modules.<ModuleName>/           # Project C# chính của module
│   ├── Domain/                            # Thực thể (Entities), Value Objects, Domain Events, Enums, Exceptions
│   ├── Application/                       # Commands, Queries, Handlers, Validators, DTOs, Repository Interfaces
│   ├── Infrastructure/                    # EF Core DbContext, Dapper Repositories, Migrations, External Adapters
│   ├── Endpoints/                         # Minimal API Endpoints (MapEndpoints)
│   └── <ModuleName>ModuleExtensions.cs    # DI Registration (AddModule) & Endpoint Mapping (MapEndpoints)
└── FinTech.Modules.<ModuleName>.Contracts/ # Project C# chứa Public Integration Events, Interfaces & DTOs
```

### 2.2 Quy tắc Phụ thuộc (Dependencies Rule)
- **Domain:** KHÔNG phụ thuộc vào bất kỳ thư viện bên ngoài nào (kể cả EF Core, Newtonsoft.Json hay ASP.NET Core). Chỉ được dùng `FinTech.SharedKernel`.
- **Application:** Chỉ phụ thuộc vào `Domain`. KHÔNG tham chiếu trực tiếp SQL Server, HTTP context hay chi tiết hạ tầng.
- **Infrastructure:** Phụ thuộc vào `Application` và `Domain`. Hiện thực hóa các interface.
- **Presentation:** Phụ thuộc vào `Application`. Chỉ chịu trách nhiệm map HTTP request/response.
- **Giao tiếp giữa các Module:**
  - Cấm `JOIN` bảng giữa 2 module khác nhau.
  - Module A chỉ được gọi Module B thông qua `Contracts` công khai hoặc qua **Asynchronous Integration Events (Wolverine/RabbitMQ)**.

### 2.3 Tổ chức Vertical Slice trong Application
Bên trong tầng `Application`, gom nhóm code theo Use-case (Feature), không gom theo tầng kỹ thuật:
```
Application/
├── Entries/
│   ├── PostEntry/
│   │   ├── PostEntryCommand.cs
│   │   ├── PostEntryHandler.cs
│   │   ├── PostEntryValidator.cs
│   │   └── PostEntryResponse.cs
│   └── ReverseEntry/
└── Accounts/
    └── GetAccountBalance/
        ├── GetAccountBalanceQuery.cs
        └── GetAccountBalanceHandler.cs
```

---

## 3. QUY TẮC CÔNG NGHỆ & THƯ VIỆN (.NET 10, C# 14, SQL SERVER)

### 3.1 C# 14 & .NET 10 Best Practices
- **Nullability:** Luôn bật `<Nullable>enable</Nullable>`. Không bao giờ bỏ qua cảnh báo null; xử lý bằng null-coalescing hoặc `Result<T>`.
- **GUID v7:** Dùng `Guid.CreateVersion7()` cho khóa chính để có thứ tự thời gian, tối ưu B-Tree Index SQL Server (thay thế GUID v4 ngẫu nhiên).
- **Time Abstraction:** KHÔNG dùng `DateTime.Now` hay `DateTime.UtcNow`. Bắt buộc inject `TimeProvider` để hỗ trợ unit test thời gian bằng `FakeTimeProvider`.
- **Thư viện CQRS & Mapping:**
  - Dùng **`Mediator`** (Source Generator) hoặc **`Wolverine`**. KHÔNG dùng MediatR bản thương mại.
  - Dùng **`Mapperly`** (Source Generator) hoặc Map thủ công. KHÔNG dùng AutoMapper.
  - Dùng **`Result<T>` pattern**, không dùng Exception cho luồng nghiệp vụ thông thường.

### 3.2 SQL Server 2025 Best Practices
- **Snapshot Isolation:** Luôn bật `READ_COMMITTED_SNAPSHOT ON` và `ALLOW_SNAPSHOT_ISOLATION ON` để reader không block writer.
- **Ledger Tables:** Các bảng sổ cái và audit log bắt buộc khai báo `WITH (LEDGER = ON (APPEND_ONLY = ON))`.
- **Temporal Tables:** Các bảng cần lịch sử (Budget, Loan Schedule) khai báo `PERIOD FOR SYSTEM_TIME`.
- **Kiểu dữ liệu:**
  - Tiền tệ: `DECIMAL(19,4)`.
  - Tỷ giá / Crypto / Số lượng chứng khoán: `DECIMAL(28,10)`.
  - Chuỗi mã / Code: `VARCHAR` (ASCII). Chuỗi hiển thị / Tên / Mô tả: `NVARCHAR` (Unicode UTF-16).
  - Khóa chính Tenant/Entity: `UNIQUEIDENTIFIER`.

---

## 4. QUY CHUẨN FRONTEND (REACT 19 & REACT NATIVE)

### 4.1 React 19 (Web SPA)
- **State Management:** Dùng **TanStack Query** cho server state; **Zustand** cho client state tối thiểu.
- **Routing & Forms:** Dùng **TanStack Router** (type-safe) + **React Hook Form** + **Zod**.
- **Không tự memo thừa thãi:** Tận dụng **React Compiler** tự động tối ưu render.
- **Hiển thị tiền tệ:** Luôn format qua helper chuẩn `formatMoney(amount, currency)` dựa trên `Intl.NumberFormat('vi-VN', ...)`.

### 4.2 React Native / Expo (Mobile)
- **New Architecture:** Chạy trên kiến trúc mới (Fabric + TurboModules + Hermes).
- **Offline Mutation:** Với các giao dịch nhập tay khi mất mạng, lưu vào hàng đợi SQLite/MMKV cục bộ và đồng bộ lại với cùng `Idempotency-Key`.
- **Bảo mật phần cứng:** Lưu Refresh Token / Passkey trong `expo-secure-store`. Dùng `expo-local-authentication` cho mở app và step-up auth. Bật cờ chặn chụp màn hình trên các view nhạy cảm.

---

## 5. CHECKLIST TRƯỚC KHI AI COMMIT CODE HOẶC TẠO FILE MỚI

Trước khi sinh code hoặc đánh dấu hoàn tất task, AI phải tự kiểm tra:
1. [ ] Có dùng `float`, `double` hay `number` cho tiền không? *(Nếu có: Sửa ngay sang `decimal` / `string`)*.
2. [ ] Giao dịch có đảm bảo Debit = Credit không?
3. [ ] Endpoint ghi nhận có kiểm tra `Idempotency-Key` không?
4. [ ] Entity/Query có bị thiếu điều kiện lọc `TenantId` không?
5. [ ] Có gọi `UPDATE`/`DELETE` trực tiếp trên sổ cái không? *(Nếu có: Đổi sang bút toán đảo)*.
6. [ ] Có dùng `DateTime.UtcNow` trực tiếp thay vì `TimeProvider` không?
7. [ ] Có viết Unit Test / Concurrency Test cho logic nghiệp vụ lõi không?
8. [ ] File code có tuân thủ cấu trúc thư mục Clean Architecture của module không?
