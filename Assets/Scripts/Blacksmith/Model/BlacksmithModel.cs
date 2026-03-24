using System;

namespace Features.Blacksmith.Model
{
    public class BlacksmithModel
    {
        public const int DefaultLives = 10;
        public const int LivesPerPickup = 10;
        public const int MaxLives = 100;

        public int Lives { get; private set; }
        public bool IsDead => Lives <= 0;

        // Event cho Presenter lắng nghe khi data thay đổi
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
}
