# Blacksmith Module — System Design

## 1. Tổng quan Kiến trúc

Module Blacksmith được xây dựng dựa trên kiến trúc **Model-View-Presenter (MVP)** chuẩn của dự án, kết hợp với pattern **Component-based** để tách biệt logic vật lý (Physics) khỏi logic hiển thị (UI).
### Sơ đồ luồng (Flow Overview)

### Sơ đồ luồng (Flow Overview)
- **User Input (UI)** → `BlacksmithView` → `BlacksmithPresenter` → xử lý logic → Cập nhật `BlacksmithModel`.
- **Data Change (Model Event)** → `BlacksmithPresenter` → `BlacksmithView` → Cập nhật trạng thái hiển thị (Text, Animation).
- **Physics Actions** → `BlacksmithView` gọi `CharacterMotor` để thực hiện hành động nhảy/rơi độc lập.

---

## 2. Các thành phần chính

### 📂 Model
- **`BlacksmithModel`**: Lớp chứa dữ liệu thuần túy (Pure C#).
    - Quản lý `Lives` (số mạng).
    - Validation: `MaxLives`, `CanAddLives`.
    - Phát sự kiện `OnLivesChanged` khi dữ liệu thay đổi.

### 📂 Presenter
- **`BlacksmithPresenter`**: Bộ điều phối trung tâm (Pure C#).
    - Implement `IBlacksmithPresenter` cho View gọi.
    - Lắng nghe sự kiện từ Model để cập nhật View qua `IBlacksmithView`.
    - Xử lý các lệnh nghiệp vụ: `Greet()`, `Jump()`, `AddLives()`.

### 📂 View
- **`BlacksmithView`**: Quản lý UI và tương tác Unity (MonoBehaviour).
    - Kết nối các UI Elements (Buttons, Text).
    - Chuyển tiếp (delegate) các sự kiện người dùng sang Presenter.
    - Chạy Animation qua `Animator`.
    - Điều khiển `CharacterMotor` khi có lệnh từ Presenter.
- **`CharacterMotor`**: Xử lý logic Physics (MonoBehaviour).
    - Tách biệt logic Jump/Fall/Gravity khỏi View.
    - Sử dụng `Raycast2D` để kiểm tra mặt đất (Ground check).

---

## 3. Luồng xử lý tiêu biểu (Key Workflows)

### 🏃 Hành động Nhảy (Jump)
1. **View**: User nhấn nút Jump → Gọi `_presenter.Jump()`.
2. **Presenter**: Gọi `_view.DoJump()`.
3. **View**: Gọi `motor.DoJump()`.
4. **Motor**: Thực hiện thay đổi `transform.position` và xử lý Gravity.

### 🍎 Nhặt Health (Health Pickup)
1. **View**: Phát hiện va chạm (Collision) với vật phẩm Health → Gọi `_presenter.AddLives()`.
2. **Presenter**: Gọi `_model.AddLives(amount)`.
3. **Model**: Cập nhật `Lives` → Phát sự kiện `OnLivesChanged`.
4. **Presenter**: Nhận sự kiện → Gọi `_view.SetLives(newLives)`.
5. **View**: Cập nhật text UI.

---

## 4. Cấu trúc thư mục

```text
Assets/Scripts/Blacksmith/
├── Blacksmith.asmdef        # Đóng gói module thành Assembly riêng
├── MODULE.md                # Tóm tắt API và Event (Contract cho AI/Dev)
├── System-Design.md         # [FILE NÀY] Tài liệu kiến trúc feature
├── Model/
│   └── BlacksmithModel.cs   # Data & Logic nghiệp vụ thuần
├── Presenter/
│   ├── IBlacksmithPresenter.cs
│   └── BlacksmithPresenter.cs # Điều phối logic
├── View/
│   ├── IBlacksmithView.cs
│   ├── BlacksmithView.cs    # UI & Unity Integration
│   └── CharacterMotor.cs    # Physics specialized logic
└── Tests/                   # Unit tests (Mocking View/Model)
```

---

## 5. Phụ thuộc & Giao tiếp
- **Dependency Injection**: Presenter nhận `IView` qua constructor, giúp dễ dàng Mocking khi viết Unit Test.
- **Loose Coupling**: View không biết Model, Model không biết Presenter/View. Giao tiếp hoàn toàn qua Interface và Action/Event.
