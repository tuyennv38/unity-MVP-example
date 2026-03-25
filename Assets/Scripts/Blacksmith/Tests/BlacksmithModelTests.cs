using NUnit.Framework;
using Features.Blacksmith.Model;

namespace Features.Blacksmith.Tests
{
    [TestFixture]
    public class BlacksmithModelTests
    {
        // ============================================
        // Constructor Tests
        // ============================================

        [Test]
        public void Constructor_DefaultLives_Is10()
        {
            var model = new BlacksmithModel();
            Assert.That(model.Lives, Is.EqualTo(BlacksmithModel.DefaultLives));
        }

        [Test]
        public void Constructor_CustomLives_SetsCorrectly()
        {
            var model = new BlacksmithModel(initialLives: 50);
            Assert.That(model.Lives, Is.EqualTo(50));
        }

        // ============================================
        // AddLives Tests
        // ============================================

        [Test]
        public void AddLives_IncreasesLives()
        {
            var model = new BlacksmithModel(initialLives: 10);
            model.AddLives(10);
            Assert.That(model.Lives, Is.EqualTo(20));
        }

        [Test]
        public void AddLives_CapsAtMaxLives()
        {
            var model = new BlacksmithModel(initialLives: 95);
            model.AddLives(10);
            Assert.That(model.Lives, Is.EqualTo(BlacksmithModel.MaxLives));
        }

        [Test]
        public void AddLives_ZeroAmount_NoChange()
        {
            var model = new BlacksmithModel(initialLives: 10);
            model.AddLives(0);
            Assert.That(model.Lives, Is.EqualTo(10));
        }

        [Test]
        public void AddLives_NegativeAmount_NoChange()
        {
            var model = new BlacksmithModel(initialLives: 10);
            model.AddLives(-5);
            Assert.That(model.Lives, Is.EqualTo(10));
        }

        [Test]
        public void AddLives_FiresOnLivesChangedEvent()
        {
            var model = new BlacksmithModel(initialLives: 10);
            int? eventValue = null;
            model.OnLivesChanged += (lives) => eventValue = lives;

            model.AddLives(10);

            Assert.That(eventValue, Is.EqualTo(20));
        }

        [Test]
        public void AddLives_ZeroAmount_DoesNotFireEvent()
        {
            var model = new BlacksmithModel(initialLives: 10);
            bool eventFired = false;
            model.OnLivesChanged += (_) => eventFired = true;

            model.AddLives(0);

            Assert.That(eventFired, Is.False);
        }

        // ============================================
        // TakeDamage Tests
        // ============================================

        [Test]
        public void TakeDamage_ReducesLives()
        {
            var model = new BlacksmithModel(initialLives: 50);
            model.TakeDamage(20);
            Assert.That(model.Lives, Is.EqualTo(30));
        }

        [Test]
        public void TakeDamage_CapsAtZero()
        {
            var model = new BlacksmithModel(initialLives: 5);
            model.TakeDamage(20);
            Assert.That(model.Lives, Is.EqualTo(0));
        }

        [Test]
        public void TakeDamage_ZeroDamage_NoChange()
        {
            var model = new BlacksmithModel(initialLives: 50);
            model.TakeDamage(0);
            Assert.That(model.Lives, Is.EqualTo(50));
        }

        [Test]
        public void TakeDamage_NegativeDamage_NoChange()
        {
            var model = new BlacksmithModel(initialLives: 50);
            model.TakeDamage(-10);
            Assert.That(model.Lives, Is.EqualTo(50));
        }

        [Test]
        public void TakeDamage_FiresOnLivesChangedEvent()
        {
            var model = new BlacksmithModel(initialLives: 50);
            int? eventValue = null;
            model.OnLivesChanged += (lives) => eventValue = lives;

            model.TakeDamage(20);

            Assert.That(eventValue, Is.EqualTo(30));
        }

        // ============================================
        // IsDead Tests
        // ============================================

        [Test]
        public void IsDead_WhenLivesZero_ReturnsTrue()
        {
            var model = new BlacksmithModel(initialLives: 10);
            model.TakeDamage(10);
            Assert.That(model.IsDead, Is.True);
        }

        [Test]
        public void IsDead_WhenLivesAboveZero_ReturnsFalse()
        {
            var model = new BlacksmithModel(initialLives: 10);
            Assert.That(model.IsDead, Is.False);
        }

        // ============================================
        // CanAddLives Tests
        // ============================================

        [Test]
        public void CanAddLives_WhenBelowMax_ReturnsTrue()
        {
            var model = new BlacksmithModel(initialLives: 50);
            Assert.That(model.CanAddLives(), Is.True);
        }

        [Test]
        public void CanAddLives_WhenAtMax_ReturnsFalse()
        {
            var model = new BlacksmithModel(initialLives: BlacksmithModel.MaxLives);
            Assert.That(model.CanAddLives(), Is.False);
        }
    }
}
