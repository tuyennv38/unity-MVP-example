# Blacksmith Test Cases

> **Chú thích Status:** ✅ = Passed (kết quả đúng mong đợi) | ❌ = Failed (kết quả không đúng mong đợi)

## BlacksmithModelTests

| Name | Param | Expect | Status |
|------|-------|--------|--------|
| Constructor_DefaultLives_Is10 | — | Lives=10 | ✅ |
| Constructor_CustomLives_SetsCorrectly | initialLives=50 | Lives=50 | ✅ |
| AddLives_IncreasesLives | initialLives=10, amount=10 | Lives=20 | ✅ |
| AddLives_CapsAtMaxLives | initialLives=95, amount=10 | Lives=100 (MaxLives) | ✅ |
| AddLives_ZeroAmount_NoChange | initialLives=10, amount=0 | Lives=10 | ✅ |
| AddLives_NegativeAmount_NoChange | initialLives=10, amount=-5 | Lives=10 | ✅ |
| AddLives_FiresOnLivesChangedEvent | initialLives=10, amount=10 | eventValue=20 | ✅ |
| AddLives_ZeroAmount_DoesNotFireEvent | initialLives=10, amount=0 | eventFired=false | ✅ |
| TakeDamage_ReducesLives | initialLives=50, damage=20 | Lives=30 | ✅ |
| TakeDamage_CapsAtZero | initialLives=5, damage=20 | Lives=0 | ✅ |
| TakeDamage_ZeroDamage_NoChange | initialLives=50, damage=0 | Lives=50 | ✅ |
| TakeDamage_NegativeDamage_NoChange | initialLives=50, damage=-10 | Lives=50 | ✅ |
| TakeDamage_FiresOnLivesChangedEvent | initialLives=50, damage=20 | eventValue=30 | ✅ |
| IsDead_WhenLivesZero_ReturnsTrue | initialLives=10, damage=10 | IsDead=true | ✅ |
| IsDead_WhenLivesAboveZero_ReturnsFalse | initialLives=10 | IsDead=false | ✅ |
| CanAddLives_WhenBelowMax_ReturnsTrue | initialLives=50 | CanAddLives()=true | ✅ |
| CanAddLives_WhenAtMax_ReturnsFalse | initialLives=100 | CanAddLives()=false | ✅ |

## BlacksmithPresenterTests

| Name | Param | Expect | Status |
|------|-------|--------|--------|
| Constructor_InitializesViewWithDefaultLives | — | View.LastLives=10 | ✅ |
| Constructor_CallsSetLivesOnce | — | View.SetLivesCallCount=1 | ✅ |
| AddLives_UpdatesViewViaModelEvent | — | View.LastLives=20 | ✅ |
| AddLives_CallsSetLivesAgain | — | View.SetLivesCallCount=2 | ✅ |
| AddLives_MultipleTimes_AccumulatesCorrectly | AddLives() ×3 | View.LastLives=40 | ✅ |
| Greet_PlaysGreetAnimation | — | View.LastAnimation="greet" | ✅ |
| Jump_CallsDoJumpOnView | — | View.JumpCalled=true | ✅ |
| Jump_DoesNotAffectLives | — | View.LastLives unchanged | ✅ |

---

> **Tổng:** ✅ 25/25 passed — 2026-03-25
