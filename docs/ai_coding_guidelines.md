# 🤖 Hướng dẫn Quy tắc Lập trình AI cho FinTech Platform

Tài liệu này được tạo ra nhằm đồng bộ và chỉ thị cho AI Assistant (và toàn bộ lập trình viên) khi thực hiện các tác vụ phát triển trong repository `FindTechPlatform`.

Bản đặc tả Skill hoàn chỉnh của Antigravity được lưu tại:
👉 **[file:///.agents/skills/fintech-platform-coding-rules/SKILL.md](file:///d:/PROJECT/FindTechPlatform/.agents/skills/fintech-platform-coding-rules/SKILL.md)**

---

## Tóm tắt 5 Trụ cột Quy tắc Bắt buộc:

1. **🔴 Bảo vệ Toàn vẹn Tiền tệ (Money Integrity):**
   - Tuyệt đối cấm dùng `float`, `double` (C#) hay `number` (JS/TS) cho tính toán tiền.
   - Bắt buộc dùng `decimal` trong C# và `DECIMAL(19,4)` trong SQL Server.
   - Tiền tệ đi kèm mã Currency trong Value Object `Money`. Thuật toán chia tiền bảo toàn phần dư (Largest Remainder).
2. **🔴 Sổ cái Bất biến & Chống Tranh chấp (Ledger & Concurrency):**
   - Sổ cái kép Double-entry `Postings` là **Append-only** (cấm `UPDATE`/`DELETE`).
   - Cập nhật số dư nguyên tử bằng câu lệnh SQL với `ROWLOCK` có điều kiện.
   - Khóa tài khoản theo thứ tự $Id$ tăng dần để loại bỏ Deadlock. Bắt buộc có header `Idempotency-Key`.
3. **🔴 Bảo mật Chiều sâu (Defense-in-Depth):**
   - Chống BOLA/IDOR 2 lớp: Application Resource Authorization + SQL Server Row-Level Security (`SESSION_CONTEXT`).
   - Web SPA không lưu Access Token ở client; dùng Cookie mã hóa qua BFF Gateway (YARP).
   - Rule Engine không dùng `eval`/JS Engine; biên dịch an toàn qua C# Expression Trees.
4. **🔴 Kiến trúc Clean Architecture & C# 14 / .NET 10:**
   - 4 tầng chuẩn: `Domain` $\to$ `Application` $\to$ `Infrastructure` $\to$ `Presentation` + `Contracts`.
   - Cấm `JOIN` chéo giữa các Module; giao tiếp bất đồng bộ qua Wolverine/RabbitMQ.
   - Dùng `Guid.CreateVersion7()`, inject `TimeProvider` (không dùng `DateTime.UtcNow`), sử dụng thư viện Open Source (Mediator, Mapperly, Wolverine).
5. **🔴 Frontend (React 19 & React Native):**
   - Web: TanStack Router, TanStack Query, React Compiler, Zod.
   - Mobile: New Architecture (Fabric/TurboModules), offline mutation queue với Idempotency Key, lưu khóa trong Keychain/Keystore.
