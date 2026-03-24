namespace Features.Blacksmith.View
{
    public interface IBlacksmithView
    {
        void SetLives(int lives);
        void PlayAnimation(string name);
        void DoJump();
    }
}
