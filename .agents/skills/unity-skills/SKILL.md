---
name: unity-skills
description: "Hướng dẫn tổng hợp các kiến trúc và pattern chuẩn cho Unity C# — tự động detect và chuyển hướng đến sub-skill phù hợp."
version: "1.0.0"
author: "Tuyen"
category: architecture
risk: safe
source: internal
date_added: "2026-03-24"
tags: [unity, csharp, architecture, mvp, coordinator]
tools: [claude, cursor, gemini]
---

# Unity Skills — Kiến trúc và Pattern chuẩn cho Unity

> **Mục tiêu:** Đây là skill gốc (parent) để phát hiện ngữ cảnh yêu cầu và chuyển hướng đến đúng sub-skill chuyên biệt.

---

## Danh sách Sub-Skills

| Sub-Skill | Đường dẫn | Mô tả |
|-----------|-----------|-------|
| **MVP** | `mvp/SKILL.md` | Tạo module feature theo kiến trúc Model-View-Presenter |
| **Coordinator** | `coordinator/SKILL.md` | Điều phối nhiều MVP module giao tiếp với nhau |

---

## Quy tắc phát hiện (Detection Rules)

Đọc yêu cầu của user và áp dụng quy tắc sau:

### → Dùng `mvp/SKILL.md` khi:

- User yêu cầu **tạo mới 1 tính năng/feature/module** (inventory, shop, health, login...).
- User nói **"tạo MVP"**, **"tạo module"**, **"tạo feature"**.
- User muốn **tách logic ra khỏi MonoBehaviour**.
- User muốn **refactor** một script thành kiến trúc MVP.

**Từ khóa nhận diện:** `tạo feature`, `tạo module`, `tạo MVP`, `Model View Presenter`, `tách logic`, `refactor`.

### → Dùng `coordinator/SKILL.md` khi:

- User yêu cầu **kết hợp/liên kết nhiều module** với nhau.
- User nói **"điều phối"**, **"coordinator"**, **"giao tiếp giữa các module"**.
- User muốn module A **phản ứng** với sự kiện của module B.
- User hỏi về **event system**, **cross-module communication**.

**Từ khóa nhận diện:** `điều phối`, `coordinator`, `kết hợp`, `liên kết module`, `giao tiếp`, `event`, `nhiều module`.

### → Dùng CẢ HAI khi:

- User muốn **tạo nhiều module MVP VÀ kết nối chúng** trong cùng 1 yêu cầu.
- Trong trường hợp này: đọc `mvp/SKILL.md` trước để tạo từng module, sau đó đọc `coordinator/SKILL.md` để kết nối.

---

## Hướng dẫn sử dụng

1. **Đọc file SKILL.md này** để xác định sub-skill phù hợp.
2. **Đọc tiếp sub-skill** tương ứng bằng cách mở file trong thư mục con.
3. **Thực hiện** theo hướng dẫn trong sub-skill đó.

> ⚠️ **KHÔNG triển khai code chỉ dựa trên file này.** File này chỉ làm nhiệm vụ detect và chuyển hướng. Mọi hướng dẫn chi tiết nằm trong sub-skill.
