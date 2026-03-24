# Blacksmith Module

## Mục đích
Quản lý nhân vật Blacksmith — hệ thống lives, animation chào hỏi, và nhảy.

## Thành phần (Classes & Interfaces)

| File | Class/Interface | Loại | Ý nghĩa |
|------|----------------|------|---------|
| `Model/BlacksmithModel.cs` | `BlacksmithModel` | Class (POCO) | Chứa dữ liệu lives, validation (MaxLives, CanAddLives), phát event khi data thay đổi |
| `View/IBlacksmithView.cs` | `IBlacksmithView` | Interface | Hợp đồng cho View — định nghĩa các method Presenter được phép gọi trên View |
| `View/BlacksmithView.cs` | `BlacksmithView` | MonoBehaviour | Implement IBlacksmithView, quản lý UI (Text, Button), delegate user event sang Presenter |
| `View/CharacterMotor.cs` | `CharacterMotor` | MonoBehaviour | Physics/movement riêng — jump, fall, ground check (tách khỏi View) |
| `Presenter/IBlacksmithPresenter.cs` | `IBlacksmithPresenter` | Interface | Hợp đồng cho Presenter — định nghĩa các hành động nghiệp vụ View được phép gọi |
| `Presenter/BlacksmithPresenter.cs` | `BlacksmithPresenter` | Class (Pure C#) | Điều phối giữa Model và View, lắng nghe Model event, gọi View method |

## API (IPresenter)
- `AddLives()` — Tăng lives cho Blacksmith (+10)
- `Greet()` — Phát animation chào hỏi
- `Jump()` — Thực hiện hành động nhảy

## Events phát ra
- `OnLivesChanged(int newLives)` — Khi số lives thay đổi
- `OnBlacksmithDied` — Khi lives về 0 (nếu có logic damage)

## Phụ thuộc bên ngoài
- Không có — module hoạt động độc lập
