---
name: unity-mvp-skills
description: "Hướng dẫn triển khai kiến trúc MVP (Model-View-Presenter) trong Unity C# theo chuẩn dự án."
version: "1.1.0"
author: "Tuyen"
category: architecture
risk: safe
source: internal
date_added: "2026-03-24"
tags: [unity, csharp, mvp, architecture, design-pattern]
tools: [claude, cursor, gemini]
---

# Unity MVP Skills — Kiến trúc Model-View-Presenter cho Unity

> **Mục tiêu:** Cung cấp quy trình chuẩn để triển khai kiến trúc MVP trong Unity, giúp tách biệt logic nghiệp vụ khỏi UI/MonoBehaviour, tăng khả năng test và bảo trì code.

MVP (Model-View-Presenter) là design pattern phù hợp với Unity vì nó giải quyết bài toán cốt lõi: **MonoBehaviour vừa chứa logic, vừa quản lý UI, vừa xử lý physics** khiến code khó test, khó mở rộng. MVP tách rõ ràng 3 trách nhiệm: dữ liệu (Model), hiển thị (View), và điều phối (Presenter).

---

## Khi nào sử dụng

- Khi tạo mới một feature có UI tương tác với logic nghiệp vụ (inventory, shop, character stats...).
- Khi cần viết unit test cho logic game mà không phụ thuộc Unity runtime.
- Khi refactor MonoBehaviour đang chứa quá nhiều trách nhiệm (God Object).
- Khi nhiều developer cùng làm việc trên 1 feature (1 người làm UI, 1 người làm logic).

## Khi KHÔNG sử dụng

- Feature quá đơn giản, chỉ có 1-2 dòng logic (ví dụ: nút bật/tắt âm thanh).
- Script thuần animation/VFX không có business logic.
- Prototype nhanh cần ship trong vài giờ — MVP thêm boilerplate không cần thiết.

---

## Kiến trúc tổng quan

```
Assets/Scripts/<FeatureName>/
├── <FeatureName>.asmdef            ← BẮT BUỘC: Assembly Definition cho production code
├── MODULE.md                       ← BẮT BUỘC: Mô tả module cho AI Agent
├── Model/
│   └── <Feature>Model.cs           ← Pure C# class (POCO), chứa dữ liệu
├── View/
│   ├── I<Feature>View.cs           ← Interface cho View
│   └── <Feature>View.cs            ← MonoBehaviour, implement IView
├── Presenter/
│   ├── I<Feature>Presenter.cs      ← Interface cho Presenter
│   └── <Feature>Presenter.cs       ← Pure C# class, implement IPresenter
└── Tests/                           ← Unit test đóng gói cùng module
    ├── <FeatureName>.Tests.asmdef   ← BẮT BUỘC: Assembly Definition cho test code
    ├── <Feature>ModelTests.cs
    └── <Feature>PresenterTests.cs
```

### Luồng dữ liệu

```
┌──────────┐      User Event       ┌─────────────┐     Read/Write     ┌───────────┐
│          │ ───────────────────▶   │             │ ──────────────────▶ │           │
│   View   │                       │  Presenter  │                     │   Model   │
│ (Mono)   │ ◀───────────────────  │  (Pure C#)  │ ◀────────────────── │  (POCO)   │
│          │    Update UI via       │             │     Return data     │           │
└──────────┘    IView interface     └─────────────┘                    └───────────┘
```

**Quy tắc quan trọng:**
1. **View** chỉ biết `IPresenter` (không biết Model).
2. **Presenter** chỉ biết `IView` và `Model` (không biết MonoBehaviour).
3. **Model** không biết ai cả — thuần dữ liệu.

---

## Hướng dẫn thực hiện

### Bước 1: Tạo Model — Lớp dữ liệu thuần

Model là **Pure C# class (POCO)**, không kế thừa MonoBehaviour. Chỉ chứa properties/fields đại diện cho state.

```csharp
namespace Features.Inventory.Model
{
    public class InventoryModel
    {
        public int Gold { get; set; }
        public int MaxSlots { get; set; }
        public List<ItemData> Items { get; private set; }

        public InventoryModel(int initialGold = 100, int maxSlots = 20)
        {
            Gold = initialGold;
            MaxSlots = maxSlots;
            Items = new List<ItemData>();
        }

        // Business logic thuần túy có thể đặt ở Model
        public bool CanAddItem() => Items.Count < MaxSlots;
    }
}
```

**Checklist Model:**
- ✅ Không `using UnityEngine` (trừ khi cần `Vector3`, `Color`...)
- ✅ Không kế thừa `MonoBehaviour` hay `ScriptableObject`
- ✅ Constructor khởi tạo giá trị mặc định
- ✅ Dùng properties (`{ get; set; }`) thay vì public fields

### Bước 2: Tạo View Interface — Hợp đồng giữa Presenter và View

Interface định nghĩa các phương thức mà Presenter có thể gọi trên View.

```csharp
namespace Features.Inventory.View
{
    public interface IInventoryView
    {
        // Presenter gọi để cập nhật UI
        void SetGold(int gold);
        void SetItemCount(int count, int max);
        void ShowMessage(string message);
        void RefreshItemList(List<ItemData> items);
    }
}
```

**Checklist IView:**
- ✅ Chỉ chứa methods mà Presenter cần gọi để cập nhật View
- ✅ Không chứa methods liên quan đến user input
- ✅ Tên method mô tả hành động trên View (`SetX`, `ShowX`, `RefreshX`, `PlayX`)

### Bước 3: Tạo Presenter Interface — API nghiệp vụ

Interface định nghĩa các hành động nghiệp vụ mà View có thể trigger.

```csharp
namespace Features.Inventory.Presenter
{
    public interface IInventoryPresenter
    {
        // View gọi khi user tương tác
        void AddItem(ItemData item);
        void RemoveItem(int slotIndex);
        void SellItem(int slotIndex);
    }
}
```

**Checklist IPresenter:**
- ✅ Tên method theo **hành động nghiệp vụ** (không theo UI event)
- ✅ `AddItem` thay vì `OnAddButtonClicked`
- ✅ Không trả về giá trị UI-specific

### Bước 4: Implement Presenter — Logic nghiệp vụ

Presenter là **Pure C# class**, nhận `IView` qua constructor, sở hữu Model.

```csharp
using Features.Inventory.Model;
using Features.Inventory.View;

namespace Features.Inventory.Presenter
{
    public class InventoryPresenter : IInventoryPresenter
    {
        private readonly IInventoryView _view;
        private readonly InventoryModel _model;

        public InventoryPresenter(IInventoryView view)
        {
            _view = view;
            _model = new InventoryModel();

            // Khởi tạo View ban đầu
            _view.SetGold(_model.Gold);
            _view.SetItemCount(_model.Items.Count, _model.MaxSlots);
        }

        public void AddItem(ItemData item)
        {
            if (!_model.CanAddItem())
            {
                _view.ShowMessage("Inventory is full!");
                return;
            }

            _model.Items.Add(item);
            _view.RefreshItemList(_model.Items);
            _view.SetItemCount(_model.Items.Count, _model.MaxSlots);
        }

        public void RemoveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _model.Items.Count) return;

            _model.Items.RemoveAt(slotIndex);
            _view.RefreshItemList(_model.Items);
            _view.SetItemCount(_model.Items.Count, _model.MaxSlots);
        }

        public void SellItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _model.Items.Count) return;

            var item = _model.Items[slotIndex];
            _model.Gold += item.SellPrice;
            _model.Items.RemoveAt(slotIndex);

            _view.SetGold(_model.Gold);
            _view.RefreshItemList(_model.Items);
            _view.SetItemCount(_model.Items.Count, _model.MaxSlots);
        }
    }
}
```

**Checklist Presenter:**
- ✅ Pure C# class — không kế thừa MonoBehaviour
- ✅ Nhận `IView` qua constructor (Dependency Injection)
- ✅ Tự tạo Model (hoặc nhận qua constructor nếu cần share)
- ✅ Mọi thay đổi Model đều kèm cập nhật View qua interface
- ✅ Không `using UnityEngine` (trừ khi thực sự cần)

### Bước 5: Implement View — MonoBehaviour + IView

View là **MonoBehaviour**, implement `IView`, tạo Presenter trong `Start()`.

```csharp
using UnityEngine;
using UnityEngine.UI;
using Features.Inventory.Presenter;
using System.Collections.Generic;

namespace Features.Inventory.View
{
    public class InventoryView : MonoBehaviour, IInventoryView
    {
        [Header("UI References")]
        [SerializeField] private Text goldLabel;
        [SerializeField] private Text itemCountLabel;
        [SerializeField] private Text messageLabel;
        [SerializeField] private Transform itemContainer;

        private IInventoryPresenter _presenter;

        void Start()
        {
            _presenter = new InventoryPresenter(this);
        }

        // ============================================
        // Region: User Events → Delegate to Presenter
        // ============================================

        public void OnAddItemButtonClicked()
        {
            var newItem = new ItemData("Sword", 50);
            _presenter.AddItem(newItem);
        }

        public void OnSellButtonClicked(int slotIndex)
        {
            _presenter.SellItem(slotIndex);
        }

        // ============================================
        // Region: IView Implementation → Presenter calls these
        // ============================================

        public void SetGold(int gold)
        {
            goldLabel.text = $"Gold: {gold}";
        }

        public void SetItemCount(int count, int max)
        {
            itemCountLabel.text = $"Items: {count}/{max}";
        }

        public void ShowMessage(string message)
        {
            messageLabel.text = message;
        }

        public void RefreshItemList(List<ItemData> items)
        {
            // Clear and rebuild UI list
            foreach (Transform child in itemContainer)
                Destroy(child.gameObject);

            foreach (var item in items)
            {
                // Instantiate item UI prefab...
            }
        }
    }
}
```

**Checklist View:**
- ✅ Kế thừa `MonoBehaviour` + implement `IView`
- ✅ Tạo Presenter trong `Start()` hoặc `Awake()`
- ✅ Giữ reference Presenter qua interface `IPresenter`
- ✅ User events → delegate sang Presenter (không xử lý logic trong View)
- ✅ IView methods → chỉ cập nhật UI (không chứa logic nghiệp vụ)
- ✅ Tách rõ 2 region: "Events → Presenter" và "IView Implementation"

### Bước 6: Tạo MODULE.md — Hợp đồng module cho AI Agent

> **BẮT BUỘC.** File này giúp AI Agent (và developer) hiểu nhanh module làm gì, phát event gì, cần gì từ bên ngoài — đặc biệt quan trọng khi tạo Coordinator kết nối nhiều module.

Đặt `MODULE.md` ở **thư mục gốc** của mỗi module MVP:

```markdown
# <Feature> Module

## Mục đích
[1-2 câu mô tả nhiệm vụ cốt lõi của module]

## Thành phần (Classes & Interfaces)

| File | Class/Interface | Loại | Ý nghĩa |
|------|----------------|------|---------|
| `Model/<Feature>Model.cs` | `<Feature>Model` | Class (POCO) | [Chứa dữ liệu gì, validation gì] |
| `View/I<Feature>View.cs` | `I<Feature>View` | Interface | Hợp đồng View — method Presenter được phép gọi |
| `View/<Feature>View.cs` | `<Feature>View` | MonoBehaviour | Implement IView, quản lý UI, delegate event |
| `Presenter/I<Feature>Presenter.cs` | `I<Feature>Presenter` | Interface | Hợp đồng Presenter — hành động View được phép gọi |
| `Presenter/<Feature>Presenter.cs` | `<Feature>Presenter` | Class (Pure C#) | Điều phối Model ↔ View |

## API (IPresenter)
- `MethodA(params)` — [Mô tả hành động]
- `MethodB(params)` — [Mô tả hành động]

## Events phát ra
- `OnEventA` — [Khi nào phát]
- `OnEventB(dataType)` — [Khi nào phát, data gì]

## Phụ thuộc bên ngoài
- [Data hoặc service cần được cung cấp từ Coordinator]
- Hoặc: "Không có" nếu module hoàn toàn độc lập
```

**Checklist MODULE.md:**
- ✅ Đặt tại thư mục gốc module: `Features/<Name>/MODULE.md`
- ✅ Mục đích mô tả trong **1-2 câu** — không dài quá
- ✅ API liệt kê **tất cả** method public từ `IPresenter`
- ✅ Events liệt kê **tất cả** event mà module phát ra (cho Coordinator lắng nghe)
- ✅ Phụ thuộc ghi rõ data/service cần từ bên ngoài
- ✅ **Cập nhật MODULE.md** mỗi khi thêm/xóa method hoặc event

### Bước 7: Tạo Assembly Definitions + Unit Test

> **BẮT BUỘC.** Unity Test Framework yêu cầu Assembly Definition files (`.asmdef`) để test assembly có thể reference production code. Không có `.asmdef` → test sẽ **không compile**.

#### Bước 7a: Tạo `<FeatureName>.asmdef` — Assembly cho production code

Đặt tại **thư mục gốc** của module (cùng cấp `MODULE.md`). File này scope toàn bộ code trong `Model/`, `View/`, `Presenter/`.

```json
{
    "name": "<FeatureName>",
    "rootNamespace": "Features.<FeatureName>",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

> **Lưu ý:** `.asmdef` scope theo **vị trí thư mục** — nó bao gồm tất cả `.cs` files trong thư mục chứa nó và các thư mục con (trừ thư mục con có `.asmdef` riêng). Vì vậy **KHÔNG di chuyển** file này vào thư mục con.

#### Bước 7b: Tạo `<FeatureName>.Tests.asmdef` — Assembly cho test code

Đặt trong `Tests/`. File này reference production assembly + NUnit + Unity Test Runner.

```json
{
    "name": "<FeatureName>.Tests",
    "rootNamespace": "Features.<FeatureName>.Tests",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "<FeatureName>"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**Giải thích các field quan trọng:**
- `references`: phải chứa `"<FeatureName>"` (đúng tên trong production `.asmdef`)
- `includePlatforms: ["Editor"]`: test chỉ chạy trong Editor, không build vào player
- `overrideReferences: true` + `precompiledReferences: ["nunit.framework.dll"]`: bắt buộc để dùng NUnit
- `defineConstraints: ["UNITY_INCLUDE_TESTS"]`: chỉ compile khi test framework active

#### Bước 7c: Viết Test files

Đặt test trong `<FeatureName>/Tests/`. Tạo **MockView** để test Presenter mà không cần Unity.

**Template test file:**

```csharp
using NUnit.Framework;                    // ← BẮT BUỘC: NUnit attributes
using Features.<FeatureName>.Model;       // ← Reference production code
using Features.<FeatureName>.Presenter;
using Features.<FeatureName>.View;

namespace Features.<FeatureName>.Tests    // ← Namespace theo convention
{
    // MockView — ghi lại mọi lời gọi từ Presenter để assert
    public class Mock<Feature>View : I<Feature>View
    {
        public int LastValue;
        public bool MethodCalled;

        public void SetValue(int v) => LastValue = v;
        public void DoAction() => MethodCalled = true;
    }

    [TestFixture]                         // ← BẮT BUỘC: đánh dấu class chứa test
    public class <Feature>PresenterTests
    {
        private Mock<Feature>View _mockView;
        private <Feature>Presenter _presenter;

        [SetUp]                           // ← Chạy trước MỖI test method
        public void SetUp()
        {
            _mockView = new Mock<Feature>View();
            _presenter = new <Feature>Presenter(_mockView);
        }

        [Test]                            // ← BẮT BUỘC: đánh dấu method là test
        public void Constructor_InitializesView()
        {
            // Arrange + Act: đã thực hiện trong SetUp
            // Assert
            Assert.That(_mockView.LastValue, Is.EqualTo(expectedDefault));
        }
    }
}
```

**Test Model:** Kiểm tra business logic (validation, cap, events) — không cần mock.
**Test Presenter:** Dùng MockView implement `IView`, kiểm tra Presenter gọi đúng method trên View.

**Checklist Test:**
- ✅ Tạo `<FeatureName>.asmdef` tại thư mục gốc module
- ✅ Tạo `<FeatureName>.Tests.asmdef` trong `Tests/` — reference production assembly + NUnit
- ✅ Đặt test files trong `<FeatureName>/Tests/` — đóng gói cùng module
- ✅ Tên file: `<Feature>ModelTests.cs` và `<Feature>PresenterTests.cs`
- ✅ **Bắt buộc** `using NUnit.Framework;` trong mọi file test
- ✅ Namespace: `Features.<FeatureName>.Tests`
- ✅ Model tests **không cần mock** — test trực tiếp pure C# class
- ✅ Presenter tests **dùng MockView** implement `IView`
- ✅ Mỗi test method test **1 hành vi duy nhất** (pattern AAA: Arrange-Act-Assert)
- ✅ Không dùng cú pháp `= null!` (C# 9 không hỗ trợ)
- ✅ Chạy test: **Unity Test Runner** (`Window > General > Test Runner`) hoặc qua MCP `run_tests`

> ⚠️ **KHÔNG dùng `dotnet test`** — Unity có Test Runner riêng tích hợp NUnit. Tất cả test là **EditMode** (pure logic), chạy nhanh trong Editor mà không cần Play Mode.

---

## Ví dụ

> ⚠️ **CẢNH BÁO:** Các ví dụ dưới đây chỉ minh họa **pattern/cấu trúc** của kiến trúc MVP. Khi áp dụng vào dự án thực, **thay thế toàn bộ** tên class, property, namespace, và logic cho phù hợp với feature đang phát triển. **KHÔNG copy nguyên ví dụ.**

### Ví dụ 1: Character Health System (Đơn giản)

**Model:**
```csharp
namespace Features.Health.Model
{
    public class HealthModel
    {
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }

        public HealthModel(int maxHP = 100)
        {
            MaxHP = maxHP;
            CurrentHP = maxHP;
        }

        public bool IsDead => CurrentHP <= 0;

        public void TakeDamage(int damage)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - damage);
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        }
    }
}
```

**IView + IPresenter:**
```csharp
public interface IHealthView
{
    void SetHP(int current, int max);
    void PlayHitEffect();
    void PlayDeathAnimation();
}

public interface IHealthPresenter
{
    void OnDamageTaken(int damage);
    void OnHealPickup(int amount);
}
```

**Presenter:**
```csharp
public class HealthPresenter : IHealthPresenter
{
    private readonly IHealthView _view;
    private readonly HealthModel _model;

    public HealthPresenter(IHealthView view, int maxHP = 100)
    {
        _view = view;
        _model = new HealthModel(maxHP);
        _view.SetHP(_model.CurrentHP, _model.MaxHP);
    }

    public void OnDamageTaken(int damage)
    {
        _model.TakeDamage(damage);
        _view.SetHP(_model.CurrentHP, _model.MaxHP);
        _view.PlayHitEffect();

        if (_model.IsDead)
            _view.PlayDeathAnimation();
    }

    public void OnHealPickup(int amount)
    {
        _model.Heal(amount);
        _view.SetHP(_model.CurrentHP, _model.MaxHP);
    }
}
```

### Ví dụ 2: Shop System (Nhiều Model)

```csharp
// Presenter có thể sở hữu nhiều Model
public class ShopPresenter : IShopPresenter
{
    private readonly IShopView _view;
    private readonly ShopModel _shopModel;
    private readonly PlayerWalletModel _walletModel;

    public ShopPresenter(IShopView view, PlayerWalletModel wallet)
    {
        _view = view;
        _shopModel = new ShopModel();
        _walletModel = wallet; // Shared model từ bên ngoài
    }

    public void BuyItem(string itemId)
    {
        var item = _shopModel.GetItem(itemId);
        if (item == null)
        {
            _view.ShowError("Item not found");
            return;
        }

        if (_walletModel.Gold < item.Price)
        {
            _view.ShowError("Not enough gold");
            return;
        }

        _walletModel.Gold -= item.Price;
        _view.SetGold(_walletModel.Gold);
        _view.ShowSuccess($"Bought {item.Name}!");
    }
}
```

### Ví dụ 3: Unit Test cho Presenter (không cần Unity)

```csharp
using NUnit.Framework;
using Features.Health.Model;
using Features.Health.Presenter;
using Features.Health.View;

namespace Features.Health.Tests
{
    /// <summary>
    /// Mock View — ghi lại mọi lời gọi từ Presenter để assert.
    /// </summary>
    public class MockHealthView : IHealthView
    {
        public int LastCurrentHP;
        public int LastMaxHP;
        public bool HitEffectPlayed;
        public bool DeathAnimPlayed;

        public void SetHP(int current, int max)
        {
            LastCurrentHP = current;
            LastMaxHP = max;
        }

        public void PlayHitEffect() => HitEffectPlayed = true;
        public void PlayDeathAnimation() => DeathAnimPlayed = true;
    }

    [TestFixture]
    public class HealthPresenterTests
    {
        private MockHealthView _mockView;
        private HealthPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _mockView = new MockHealthView();
            _presenter = new HealthPresenter(_mockView, maxHP: 100);
        }

        [Test]
        public void Constructor_InitializesViewWithMaxHP()
        {
            Assert.That(_mockView.LastCurrentHP, Is.EqualTo(100));
            Assert.That(_mockView.LastMaxHP, Is.EqualTo(100));
        }

        [Test]
        public void TakeDamage_ReducesHP()
        {
            _presenter.OnDamageTaken(30);

            Assert.That(_mockView.LastCurrentHP, Is.EqualTo(70));
            Assert.That(_mockView.HitEffectPlayed, Is.True);
        }

        [Test]
        public void TakeDamage_WhenDead_PlaysDeathAnimation()
        {
            _presenter.OnDamageTaken(200);

            Assert.That(_mockView.LastCurrentHP, Is.EqualTo(0));
            Assert.That(_mockView.DeathAnimPlayed, Is.True);
        }
    }
}
```

---

## Best Practices

- ✅ **Model là POCO** — Không kế thừa MonoBehaviour, không dùng UnityEngine khi không cần
- ✅ **Presenter là Pure C#** — Dễ unit test, không phụ thuộc Unity lifecycle
- ✅ **Giao tiếp qua Interface** — `IView` và `IPresenter` giúp loose coupling
- ✅ **View chỉ làm 2 việc:** nhận user event → delegate sang Presenter, và implement IView methods để cập nhật UI
- ✅ **Tách rõ region** trong View: "Events → Presenter" và "IView Implementation"
- ✅ **Đặt tên method IPresenter theo nghiệp vụ** — `AddItem`, `Greet`, `Jump` (không phải `OnButtonClick`)
- ✅ **Đặt tên method IView theo hành động UI** — `SetLives`, `PlayAnimation`, `ShowMessage`
- ❌ **KHÔNG để logic nghiệp vụ trong View** — View không nên đọc/ghi Model trực tiếp
- ❌ **KHÔNG để View biết Model** — View chỉ biết IPresenter
- ❌ **KHÔNG để Presenter gọi MonoBehaviour API** — Presenter không dùng `GetComponent`, `Instantiate`, `StartCoroutine`

---

## Anti-Patterns (KHÔNG làm)

### ❌ Anti-Pattern 1: God View — View chứa logic nghiệp vụ

```csharp
// SAI — View tự xử lý logic
public class BadView : MonoBehaviour
{
    public void OnBuyButtonClicked()
    {
        if (playerGold >= item.Price) // Logic nghiệp vụ trong View!
        {
            playerGold -= item.Price;
            goldLabel.text = "Gold: " + playerGold;
        }
    }
}
```
**Tại sao sai:** Không thể unit test logic mua bán mà không chạy Unity.

**Cách đúng:** Delegate sang Presenter:
```csharp
public void OnBuyButtonClicked()
{
    _presenter.BuyItem(selectedItemId); // Presenter xử lý logic
}
```

### ❌ Anti-Pattern 2: Presenter phụ thuộc MonoBehaviour

```csharp
// SAI — Presenter dùng Unity API
public class BadPresenter
{
    public void SpawnEnemy()
    {
        GameObject.Instantiate(enemyPrefab); // Unity API trong Presenter!
    }
}
```
**Tại sao sai:** Presenter không thể test ngoài Unity runtime.

**Cách đúng:** Thêm method vào IView:
```csharp
// IView
void SpawnEnemy(EnemyData data);

// Presenter
public void OnSpawnRequested()
{
    var data = _model.GetNextEnemy();
    _view.SpawnEnemy(data); // View xử lý Instantiate
}
```

### ❌ Anti-Pattern 3: View chứa movement/physics logic

```csharp
// SAI — physics code trộn lẫn với MVP View
public class BadCharacterView : MonoBehaviour, ICharacterView
{
    void Update()
    {
        // Cả trăm dòng physics, raycasting, movement...
        if (jump) {
            transform.position += transform.up * jumpSpeed * Time.deltaTime;
            // ...
        }
    }
}
```
**Tại sao sai:** View trở thành God Object, khó bảo trì.

**Cách đúng:** Tách movement ra component riêng:
```csharp
// CharacterMotor.cs — MonoBehaviour riêng cho movement
public class CharacterMotor : MonoBehaviour
{
    public void DoJump() { /* physics logic */ }
}

// View chỉ delegate
public class CharacterView : MonoBehaviour, ICharacterView
{
    [SerializeField] private CharacterMotor motor;

    public void DoJump() => motor.DoJump();
}
```

---

## Skill liên quan

- `@skill-creator` — Dùng để tạo skill mới theo chuẩn dự án
- `coordinator/SKILL.md` — Khi cần điều phối nhiều MVP module giao tiếp với nhau (Coordinator Pattern)
