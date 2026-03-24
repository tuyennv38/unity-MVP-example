---
name: unity-coordinator-skills
description: "Hướng dẫn triển khai Coordinator Pattern để điều phối nhiều MVP module trong Unity."
version: "1.0.0"
author: "Tuyen"
category: architecture
risk: safe
source: internal
date_added: "2026-03-24"
tags: [unity, csharp, coordinator, mediator, architecture, design-pattern]
tools: [claude, cursor, gemini]
---

# Unity Coordinator Skills — Điều phối nhiều MVP Module

> **Mục tiêu:** Cung cấp quy trình chuẩn để các MVP module giao tiếp với nhau mà không bị chồng chéo, không phụ thuộc vào UI, thông qua Coordinator Pattern.

Khi dự án có nhiều module MVP (Inventory, Shop, HUD, Combat...), chúng cần giao tiếp với nhau nhưng **không được biết nhau trực tiếp**. Coordinator (còn gọi là Mediator/Orchestrator) là lớp trung gian đóng vai trò **"nhạc trưởng"** — lắng nghe event từ các module và điều phối hành động tương ứng.

> ⚠️ **CẢNH BÁO:** Các ví dụ dưới đây chỉ minh họa **pattern/cấu trúc**. Khi áp dụng vào dự án thực, **thay thế toàn bộ** tên class, property, namespace, và logic cho phù hợp với feature đang phát triển. **KHÔNG copy nguyên ví dụ.**

---

## Khi nào sử dụng

- Khi có **2+ MVP module** cần trao đổi dữ liệu hoặc phản ứng với sự kiện của nhau.
- Khi cần thêm module mới mà **không muốn sửa code module cũ**.
- Khi muốn tập trung **logic liên module** (cross-cutting concerns) vào 1 nơi duy nhất.
- Khi cần giữ mỗi module **độc lập, có thể test riêng**.

## Khi KHÔNG sử dụng

- Chỉ có **1 module MVP** — không cần điều phối.
- Hai module chỉ share **dữ liệu tĩnh** (config, constants) — dùng shared Model hoặc ScriptableObject.
- Prototype nhỏ — thêm Coordinator là over-engineering.

## Điều kiện tiên quyết

- Mỗi module đã được triển khai theo kiến trúc MVP (xem `mvp/SKILL.md`).
- Mỗi module **phải có `MODULE.md`** mô tả mục đích, API, events, và phụ thuộc.

### Quy trình bắt buộc trước khi viết Coordinator

1. **Thu thập `MODULE.md`** của tất cả module cần kết nối.
2. **Liệt kê events** từ mục "Events phát ra" của mỗi module.
3. **Xác định luồng** — Event nào của module A cần trigger hành động ở module B.
4. **Viết `GameEvents.cs`** dựa trên danh sách events đã thu thập.
5. **Viết Coordinator** dựa trên luồng đã xác định.

> ⚠️ **KHÔNG viết Coordinator nếu chưa đọc `MODULE.md` của tất cả module liên quan.** Nếu module chưa có `MODULE.md`, tạo nó trước (xem Bước 6 trong `mvp/SKILL.md`).

---

## Kiến trúc tổng quan

```
                         ┌──────────────┐
                         │  Coordinator │
                         │  (Biết tất   │
                         │  cả module)  │
                         └──────┬───────┘
                    ┌───────────┼───────────┐
                    ▼           ▼           ▼
             ┌──────────┐ ┌──────────┐ ┌──────────┐
             │ Module A │ │ Module B │ │ Module C │
             │ (MVP)    │ │ (MVP)    │ │ (MVP)    │
             └──────────┘ └──────────┘ └──────────┘
                  ↕             ↕           ↕
            Không biết    Không biết   Không biết
            B hoặc C      A hoặc C     A hoặc B
```

**Quy tắc cốt lõi:**
1. Module **chỉ phát event** khi có sự kiện quan trọng (không gọi trực tiếp module khác)
2. Coordinator **lắng nghe event** và gọi method trên các module liên quan
3. Thêm module mới → **chỉ sửa Coordinator**, không sửa module cũ

### Cấu trúc thư mục

```
Assets/Scripts/
├── Core/
│   ├── GameCoordinator.cs           ← Điều phối tất cả module
│   └── Events/
│       └── GameEvents.cs            ← Định nghĩa event data
├── Features/
│   ├── FeatureA/                    ← Module MVP #1
│   │   ├── Model/
│   │   ├── View/
│   │   └── Presenter/
│   ├── FeatureB/                    ← Module MVP #2
│   │   ├── Model/
│   │   ├── View/
│   │   └── Presenter/
│   └── FeatureC/                    ← Module MVP #3
│       ├── Model/
│       ├── View/
│       └── Presenter/
```

---

## Hướng dẫn thực hiện

### Bước 1: Định nghĩa Event — Ngôn ngữ chung giữa các module

Event là **data thuần**, không chứa logic. Đặt trong `Core/Events/`. Dùng `System.Action` hoặc custom event system.

```csharp
using System;

namespace Core.Events
{
    /// <summary>
    /// Static event bus — Ngôn ngữ chung giữa các module.
    /// Mỗi module chỉ Raise event, Coordinator lắng nghe và điều phối.
    /// </summary>
    public static class GameEvents
    {
        // Khai báo event với kiểu dữ liệu phù hợp
        public static event Action<string, int> OnItemPurchased;    // itemId, price
        public static event Action OnInventoryFull;
        public static event Action OnPlayerDied;
        public static event Action<int> OnGoldChanged;              // newAmount

        // Raise methods — module gọi khi cần phát event
        public static void RaiseItemPurchased(string itemId, int price)
            => OnItemPurchased?.Invoke(itemId, price);

        public static void RaiseInventoryFull()
            => OnInventoryFull?.Invoke();

        public static void RaisePlayerDied()
            => OnPlayerDied?.Invoke();

        public static void RaiseGoldChanged(int newAmount)
            => OnGoldChanged?.Invoke(newAmount);
    }
}
```

**Checklist Event:**
- ✅ Đặt trong `Core/Events/` — không thuộc module nào
- ✅ Dùng `?.Invoke()` để tránh NullReferenceException
- ✅ Tên event bắt đầu bằng `On` + **hành động đã xảy ra** (past tense): `OnItemPurchased`, `OnPlayerDied`
- ❌ KHÔNG để event chứa logic — chỉ truyền data

### Bước 2: Presenter phát event thay vì gọi module khác

Khi Presenter cần thông báo cho "thế giới bên ngoài", chỉ gọi `GameEvents.RaiseXxx()`.

```csharp
public class ShopPresenter : IShopPresenter
{
    private readonly IShopView _view;
    private readonly ShopModel _model;

    public void BuyItem(string itemId)
    {
        var item = _model.GetItem(itemId);
        if (item == null) return;

        // ✅ Phát event — Coordinator sẽ xử lý phần còn lại
        GameEvents.RaiseItemPurchased(item.Id, item.Price);
        _view.ShowSuccess($"Bought {item.Name}!");

        // ❌ KHÔNG gọi: _inventoryPresenter.AddItem(itemId);
    }
}
```

**Checklist Presenter (khi dùng Coordinator):**
- ✅ Import `Core.Events` để gọi `GameEvents.RaiseXxx()`
- ✅ Chỉ phát event về **sự kiện đã hoàn thành** trong module mình
- ❌ KHÔNG import namespace của module khác
- ❌ KHÔNG giữ reference tới Presenter/View của module khác

### Bước 3: Tạo Coordinator — Nhạc trưởng điều phối

Coordinator là **MonoBehaviour** duy nhất biết tất cả module. Đăng ký event trong `OnEnable`, hủy trong `OnDisable`.

```csharp
using UnityEngine;
using Core.Events;

namespace Core
{
    /// <summary>
    /// GameCoordinator — NƠI DUY NHẤT biết tất cả module.
    /// Lắng nghe event và điều phối hành động giữa các module.
    /// </summary>
    public class GameCoordinator : MonoBehaviour
    {
        // References tới các Presenter (qua INTERFACE, không concrete class)
        private IInventoryPresenter _inventory;
        private IShopPresenter _shop;
        private IHUDPresenter _hud;

        // Shared state giữa các module (nếu cần)
        private PlayerWalletModel _wallet;

        void Awake()
        {
            _wallet = new PlayerWalletModel(startingGold: 500);
            // Khởi tạo hoặc lấy reference tới các module
        }

        void OnEnable()
        {
            // ĐĂNG KÝ lắng nghe event
            GameEvents.OnItemPurchased += HandleItemPurchased;
            GameEvents.OnInventoryFull += HandleInventoryFull;
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }

        void OnDisable()
        {
            // BẮT BUỘC hủy đăng ký — tránh memory leak
            GameEvents.OnItemPurchased -= HandleItemPurchased;
            GameEvents.OnInventoryFull -= HandleInventoryFull;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }

        // ============================================
        // Orchestration Logic — Logic liên module
        // ============================================

        private void HandleItemPurchased(string itemId, int price)
        {
            _wallet.Gold -= price;
            _inventory.AddItem(itemId);
            _hud.UpdateGold(_wallet.Gold);
        }

        private void HandleInventoryFull()
        {
            _shop.SetBuyEnabled(false);
            _hud.ShowWarning("Inventory is full!");
        }

        private void HandlePlayerDied()
        {
            _inventory.DropAllItems();
            _hud.ShowGameOver();
        }
    }
}
```

**Checklist Coordinator:**
- ✅ Là **nơi duy nhất** biết tất cả module
- ✅ Giữ reference qua **interface** (`IInventoryPresenter`), không concrete class
- ✅ Đăng ký event trong `OnEnable`, **hủy trong `OnDisable`**
- ✅ Mỗi `Handle` method mô tả **luồng nghiệp vụ liên module** rõ ràng
- ❌ KHÔNG chứa business logic đơn lẻ (đó là việc của Presenter)
- ❌ KHÔNG truy cập View trực tiếp — chỉ gọi qua Presenter interface

---

## Ví dụ

### Ví dụ 1: Luồng "Player mua item"

```
User click Buy → ShopView → ShopPresenter.BuyItem()
                                    │
                                    ▼
                        GameEvents.RaiseItemPurchased()
                                    │
                                    ▼
                        GameCoordinator.HandleItemPurchased()
                           │         │          │
                           ▼         ▼          ▼
                      Wallet    Inventory     HUD
                      -= price  .AddItem()   .UpdateGold()
```

Mỗi module chỉ làm phần việc của mình, Coordinator kết nối tất cả.

### Ví dụ 2: Nhiều Coordinator theo feature group

Khi dự án lớn (5+ module), tách thành nhiều Coordinator:

```
Assets/Scripts/Core/
├── Coordinators/
│   ├── EconomyCoordinator.cs    ← Shop + Inventory + Wallet
│   ├── CombatCoordinator.cs     ← Combat + Health + Skills
│   └── UICoordinator.cs         ← HUD + Menu + Settings
```

```csharp
// Mỗi Coordinator chỉ quản lý nhóm module liên quan
public class EconomyCoordinator : MonoBehaviour
{
    private IShopPresenter _shop;
    private IInventoryPresenter _inventory;
    private IWalletPresenter _wallet;

    // Chỉ lắng nghe event liên quan đến economy
    void OnEnable()
    {
        GameEvents.OnItemPurchased += HandleItemPurchased;
        GameEvents.OnItemSold += HandleItemSold;
    }
    // ...
}
```

### Ví dụ 3: Unit Test cho Coordinator

```csharp
using NUnit.Framework;

public class MockInventoryPresenter : IInventoryPresenter
{
    public string LastAddedItem;
    public void AddItem(string itemId) => LastAddedItem = itemId;
    public void DropAllItems() { }
}

public class MockHUDPresenter : IHUDPresenter
{
    public int LastGold;
    public void UpdateGold(int gold) => LastGold = gold;
    public void ShowWarning(string msg) { }
    public void ShowGameOver() { }
}

[TestFixture]
public class CoordinatorLogicTests
{
    [Test]
    public void HandleItemPurchased_UpdatesAllModules()
    {
        var mockInventory = new MockInventoryPresenter();
        var mockHUD = new MockHUDPresenter();
        var wallet = new PlayerWalletModel(startingGold: 500);

        // Simulate coordinator logic
        wallet.Gold -= 100;
        mockInventory.AddItem("sword_01");
        mockHUD.UpdateGold(wallet.Gold);

        Assert.AreEqual("sword_01", mockInventory.LastAddedItem);
        Assert.AreEqual(400, mockHUD.LastGold);
    }
}
```

---

## Khi nào dùng cách nào

| Tình huống | Giải pháp |
|---|---|
| 2-3 module, giao tiếp đơn giản | **Static event** (`GameEvents`) + **1 Coordinator** |
| 5+ module, logic phức tạp | **Static event** + **nhiều Coordinator** theo feature group |
| Module dạng plugin, cần decouple hoàn toàn | **Interface-based injection** — Coordinator inject callback vào Presenter |

---

## Best Practices

- ✅ **Event là past tense** — `OnItemPurchased` (đã mua), không phải `OnBuyItem` (đang mua)
- ✅ **1 Coordinator = 1 nhóm module liên quan** — Không nhồi tất cả vào 1 class
- ✅ **Luôn hủy đăng ký** event trong `OnDisable`/`OnDestroy` — tránh memory leak
- ✅ **Handle method có tên rõ ràng** — `HandleItemPurchased`, `HandlePlayerDied`
- ✅ **Coordinator gọi Presenter qua interface** — không gọi concrete class
- ❌ **KHÔNG** để 2 Presenter import namespace của nhau
- ❌ **KHÔNG** để View biết sự tồn tại của module khác
- ❌ **KHÔNG** để Coordinator chứa business logic đơn lẻ — đó là việc của Presenter

---

## Anti-Patterns (KHÔNG làm)

### ❌ Anti-Pattern 1: Presenter gọi Presenter trực tiếp

```csharp
// SAI — ShopPresenter biết InventoryPresenter
public class ShopPresenter : IShopPresenter
{
    private InventoryPresenter _inventory; // TIGHT COUPLING!

    public void BuyItem(string itemId)
    {
        _inventory.AddItem(itemId);  // Gọi trực tiếp → chồng chéo
    }
}
```
**Tại sao sai:** Xóa/refactor InventoryPresenter → ShopPresenter bị vỡ. Không thể test riêng.

**Cách đúng:** Phát event `GameEvents.RaiseItemPurchased()`, Coordinator xử lý.

### ❌ Anti-Pattern 2: Quên hủy đăng ký event

```csharp
// SAI — Chỉ đăng ký mà không hủy
void OnEnable()
{
    GameEvents.OnPlayerDied += HandlePlayerDied;
}
// Thiếu OnDisable → memory leak khi scene unload!
```
**Cách đúng:** Luôn cặp `OnEnable`/`OnDisable` hoặc `Start`/`OnDestroy`.

### ❌ Anti-Pattern 3: God Coordinator

```csharp
// SAI — 1 Coordinator xử lý 20 module, 500 dòng code
public class GodCoordinator : MonoBehaviour
{
    // 20 references...
    // 50 Handle methods...
}
```
**Cách đúng:** Tách thành nhiều Coordinator theo nhóm feature (Economy, Combat, UI...).

---

## Skill liên quan

- `mvp/SKILL.md` — Tạo từng module MVP riêng lẻ (Model, View, Presenter)
- `@skill-creator` — Dùng để tạo skill mới theo chuẩn dự án
