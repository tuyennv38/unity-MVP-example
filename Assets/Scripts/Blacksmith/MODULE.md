# Blacksmith Module

## Mục đích
Quản lý nhân vật Blacksmith — hệ thống lives, animation chào hỏi, và nhảy.

## API (IPresenter)
- `AddLives()` — Tăng lives cho Blacksmith (+10)
- `Greet()` — Phát animation chào hỏi
- `Jump()` — Thực hiện hành động nhảy

## Events phát ra
- `OnLivesChanged(int newLives)` — Khi số lives thay đổi
- `OnBlacksmithDied` — Khi lives về 0 (nếu có logic damage)

## Phụ thuộc bên ngoài
- Không có — module hoạt động độc lập
