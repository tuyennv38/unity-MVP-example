using Features.Blacksmith.Model;
using Features.Blacksmith.Presenter;
using Features.Blacksmith.View;

namespace Tests;

/// <summary>
/// Mock View — ghi lại mọi lời gọi từ Presenter để assert.
/// </summary>
public class MockBlacksmithView : IBlacksmithView
{
    public int LastLives;
    public int SetLivesCallCount;
    public string? LastAnimation;
    public bool JumpCalled;

    public void SetLives(int lives)
    {
        LastLives = lives;
        SetLivesCallCount++;
    }

    public void PlayAnimation(string name) => LastAnimation = name;
    public void DoJump() => JumpCalled = true;
}

[TestFixture]
public class BlacksmithPresenterTests
{
    private MockBlacksmithView _mockView = null!;
    private BlacksmithPresenter _presenter = null!;

    [SetUp]
    public void SetUp()
    {
        _mockView = new MockBlacksmithView();
        _presenter = new BlacksmithPresenter(_mockView);
    }

    // ============================================
    // Constructor Tests
    // ============================================

    [Test]
    public void Constructor_InitializesViewWithDefaultLives()
    {
        Assert.That(_mockView.LastLives, Is.EqualTo(BlacksmithModel.DefaultLives));
    }

    [Test]
    public void Constructor_CallsSetLivesOnce()
    {
        // SetLives gọi 1 lần trong constructor
        Assert.That(_mockView.SetLivesCallCount, Is.EqualTo(1));
    }

    // ============================================
    // AddLives Tests
    // ============================================

    [Test]
    public void AddLives_UpdatesViewViaModelEvent()
    {
        _presenter.AddLives();

        // 10 (default) + 10 (LivesPerPickup) = 20
        Assert.That(_mockView.LastLives, Is.EqualTo(20));
    }

    [Test]
    public void AddLives_CallsSetLivesAgain()
    {
        _presenter.AddLives();

        // 1 from constructor + 1 from AddLives = 2
        Assert.That(_mockView.SetLivesCallCount, Is.EqualTo(2));
    }

    [Test]
    public void AddLives_MultipleTimes_AccumulatesCorrectly()
    {
        _presenter.AddLives(); // 10 + 10 = 20
        _presenter.AddLives(); // 20 + 10 = 30
        _presenter.AddLives(); // 30 + 10 = 40

        Assert.That(_mockView.LastLives, Is.EqualTo(40));
    }

    // ============================================
    // Greet Tests
    // ============================================

    [Test]
    public void Greet_PlaysGreetAnimation()
    {
        _presenter.Greet();
        Assert.That(_mockView.LastAnimation, Is.EqualTo("greet"));
    }

    // ============================================
    // Jump Tests
    // ============================================

    [Test]
    public void Jump_CallsDoJumpOnView()
    {
        _presenter.Jump();
        Assert.That(_mockView.JumpCalled, Is.True);
    }

    [Test]
    public void Jump_DoesNotAffectLives()
    {
        int livesBefore = _mockView.LastLives;
        _presenter.Jump();
        Assert.That(_mockView.LastLives, Is.EqualTo(livesBefore));
    }
}
