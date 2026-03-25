# Unity MVP Example

> Dự án minh họa kiến trúc **Model-View-Presenter (MVP)** trong Unity C# — giúp tách biệt logic nghiệp vụ khỏi UI/MonoBehaviour, tăng khả năng unit test và bảo trì code.

## MVP là gì?

**Model-View-Presenter** là design pattern chia code thành 3 lớp rõ ràng:

```
┌──────────┐    User Event     ┌─────────────┐   Read/Write    ┌───────────┐
│          │ ────────────────▶  │             │ ──────────────▶ │           │
│   View   │                   │  Presenter  │                 │   Model   │
│ (Mono)   │ ◀────────────────  │  (Pure C#)  │ ◀────────────── │  (POCO)   │
│          │  Update UI via    │             │   Return data   │           │
└──────────┘  IView interface  └─────────────┘                 └───────────┘
```

| Lớp | Trách nhiệm | Loại class |
|-----|-------------|------------|
| **Model** | Chứa dữ liệu và business logic thuần | Pure C# (POCO) |
| **View** | Hiển thị UI, nhận user input, delegate sang Presenter | MonoBehaviour |
| **Presenter** | Điều phối Model ↔ View, xử lý logic nghiệp vụ | Pure C# |

**Quy tắc giao tiếp:**
- View chỉ biết `IPresenter` (không biết Model)
- Presenter chỉ biết `IView` và Model (không biết MonoBehaviour)
- Model không biết ai — thuần dữ liệu

---

## Ưu điểm

| # | Ưu điểm | Giải thích |
|---|---------|------------|
| 1 | **Unit Test dễ dàng** | Presenter và Model là Pure C# → test bằng NUnit mà không cần Unity runtime |
| 2 | **Tách biệt trách nhiệm** | Mỗi lớp chỉ làm 1 việc, tránh God Object |
| 3 | **Loose Coupling** | Giao tiếp qua Interface (IView, IPresenter) → dễ thay thế, mock |
| 4 | **Teamwork hiệu quả** | Developer UI làm View, developer logic làm Presenter — không conflict |
| 5 | **Dễ bảo trì** | Thay đổi UI không ảnh hưởng logic, thay đổi logic không ảnh hưởng UI |
| 6 | **Tái sử dụng** | Model có thể dùng chung giữa nhiều View/Presenter |

## Nhược điểm

| # | Nhược điểm | Giải thích |
|---|-----------|------------|
| 1 | **Boilerplate nhiều** | Mỗi feature cần tối thiểu 5 file (Model, IView, View, IPresenter, Presenter) |
| 2 | **Overhead cho feature đơn giản** | Nút bật/tắt âm thanh không cần kiến trúc phức tạp |
| 3 | **Learning curve** | Developer quen viết mọi thứ trong MonoBehaviour cần thời gian thích nghi |
| 4 | **Không phù hợp prototype** | Khi cần ship nhanh, MVP thêm thời gian setup đáng kể |
| 5 | **Quản lý lifecycle phức tạp hơn** | Presenter không có `Update()`, `OnDestroy()` — cần thiết kế thêm nếu cần |

---

## Ví dụ triển khai: Blacksmith Module

Module **Blacksmith** quản lý nhân vật thợ rèn với hệ thống lives, animation chào hỏi và nhảy.

### Cấu trúc thư mục

```
Assets/Scripts/Blacksmith/
├── Blacksmith.asmdef              ← Assembly Definition (production)
├── MODULE.md                      ← Mô tả module cho AI Agent / Developer
├── Model/
│   └── BlacksmithModel.cs         ← Pure C# — dữ liệu lives, validation
├── View/
│   ├── IBlacksmithView.cs         ← Interface View
│   ├── BlacksmithView.cs          ← MonoBehaviour — UI + delegate event
│   └── CharacterMotor.cs          ← Physics/movement tách riêng
├── Presenter/
│   ├── IBlacksmithPresenter.cs    ← Interface Presenter
│   └── BlacksmithPresenter.cs     ← Pure C# — điều phối Model ↔ View
└── Tests/
    ├── Blacksmith.Tests.asmdef    ← Assembly Definition (test)
    ├── TEST_CASES.md              ← Bảng test cases
    ├── BlacksmithModelTests.cs    ← 17 unit tests cho Model
    └── BlacksmithPresenterTests.cs ← 8 unit tests cho Presenter
```

### 1. Model — Dữ liệu thuần (POCO)

```csharp
public class BlacksmithModel
{
    public const int DefaultLives = 10;
    public const int LivesPerPickup = 10;
    public const int MaxLives = 100;

    public int Lives { get; private set; }
    public bool IsDead => Lives <= 0;

    public event Action<int> OnLivesChanged;

    public BlacksmithModel(int initialLives = DefaultLives)
    {
        Lives = initialLives;
    }

    public bool CanAddLives() => Lives < MaxLives;

    public void AddLives(int amount)
    {
        if (amount <= 0) return;
        Lives = Math.Min(Lives + amount, MaxLives);
        OnLivesChanged?.Invoke(Lives);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        Lives = Math.Max(0, Lives - damage);
        OnLivesChanged?.Invoke(Lives);
    }
}
```

> **Lưu ý:** Model không `using UnityEngine`, không kế thừa `MonoBehaviour` → test được 100% ngoài Unity.

### 2. View Interface — Hợp đồng UI

```csharp
public interface IBlacksmithView
{
    void SetLives(int lives);       // Cập nhật số lives trên UI
    void PlayAnimation(string name); // Phát animation
    void DoJump();                   // Thực hiện nhảy
}
```

### 3. Presenter Interface — Hợp đồng nghiệp vụ

```csharp
public interface IBlacksmithPresenter
{
    void AddLives();  // Tăng lives
    void Greet();     // Phát animation chào
    void Jump();      // Nhảy
}
```

### 4. Presenter — Điều phối logic

```csharp
public class BlacksmithPresenter : IBlacksmithPresenter
{
    private readonly IBlacksmithView _view;
    private readonly BlacksmithModel _model;

    public BlacksmithPresenter(IBlacksmithView view)
    {
        _view = view;
        _model = new BlacksmithModel();
        _model.OnLivesChanged += HandleLivesChanged;
        _view.SetLives(_model.Lives);  // Khởi tạo UI
    }

    public void AddLives() => _model.AddLives(BlacksmithModel.LivesPerPickup);
    public void Greet()    => _view.PlayAnimation("greet");
    public void Jump()     => _view.DoJump();

    private void HandleLivesChanged(int newLives) => _view.SetLives(newLives);
}
```

> **Lưu ý:** Presenter nhận `IView` qua constructor (Dependency Injection), tự tạo Model, không dùng Unity API.

### 5. View — MonoBehaviour + IView

```csharp
public class BlacksmithView : MonoBehaviour, IBlacksmithView
{
    [SerializeField] private Text livesLabel;
    [SerializeField] private Button greetButton;
    [SerializeField] private Button jumpButton;
    [SerializeField] private CharacterMotor motor;

    private IBlacksmithPresenter _presenter;
    private Animator _anim;

    void Start()
    {
        _presenter = new BlacksmithPresenter(this);
        _anim = GetComponent<Animator>();
        greetButton?.onClick.AddListener(() => _presenter.Greet());
        jumpButton?.onClick.AddListener(() => _presenter.Jump());
    }

    // === IBlacksmithView Implementation ===
    public void SetLives(int lives) => livesLabel.text = "Lives: " + lives;
    public void PlayAnimation(string name) => _anim?.SetTrigger(name);
    public void DoJump() => motor?.DoJump();
}
```

> **Lưu ý:** View chỉ làm 2 việc: (1) nhận event từ user → delegate sang Presenter, (2) implement IView methods → cập nhật UI. Physics được tách ra `CharacterMotor` riêng.

### 6. Unit Test — MockView + NUnit

```csharp
// MockView — ghi lại mọi lời gọi từ Presenter để assert
public class MockBlacksmithView : IBlacksmithView
{
    public int LastLives;
    public string LastAnimation;
    public bool JumpCalled;

    public void SetLives(int lives) => LastLives = lives;
    public void PlayAnimation(string name) => LastAnimation = name;
    public void DoJump() => JumpCalled = true;
}

[TestFixture]
public class BlacksmithPresenterTests
{
    private MockBlacksmithView _mockView;
    private BlacksmithPresenter _presenter;

    [SetUp]
    public void SetUp()
    {
        _mockView = new MockBlacksmithView();
        _presenter = new BlacksmithPresenter(_mockView);
    }

    [Test]
    public void Constructor_InitializesViewWithDefaultLives()
    {
        Assert.That(_mockView.LastLives, Is.EqualTo(10));
    }

    [Test]
    public void AddLives_UpdatesViewViaModelEvent()
    {
        _presenter.AddLives();
        Assert.That(_mockView.LastLives, Is.EqualTo(20)); // 10 + 10
    }

    [Test]
    public void Greet_PlaysGreetAnimation()
    {
        _presenter.Greet();
        Assert.That(_mockView.LastAnimation, Is.EqualTo("greet"));
    }
}
```

> **Kết quả:** ✅ **25/25 tests passed** — 17 Model tests + 8 Presenter tests, tất cả chạy trong EditMode mà không cần Unity runtime.

---

## Yêu cầu

- Unity 2021.3+ (LTS khuyến nghị)
- .NET Standard 2.1 / C# 9

## License

MIT
