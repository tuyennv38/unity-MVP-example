using Features.Blacksmith.Model;
using Features.Blacksmith.View;

namespace Features.Blacksmith.Presenter
{
    public class BlacksmithPresenter : IBlacksmithPresenter
    {
        private readonly IBlacksmithView _view;
        private readonly BlacksmithModel _model;

        public BlacksmithPresenter(IBlacksmithView view)
        {
            _view = view;
            _model = new BlacksmithModel();

            // Lắng nghe Model event
            _model.OnLivesChanged += HandleLivesChanged;

            // Khởi tạo View ban đầu
            _view.SetLives(_model.Lives);
        }

        // ============================================
        // IBlacksmithPresenter Implementation
        // ============================================

        public void AddLives()
        {
            _model.AddLives(BlacksmithModel.LivesPerPickup);
        }

        public void Greet()
        {
            _view.PlayAnimation("greet");
        }

        public void Jump()
        {
            _view.DoJump();
        }

        // ============================================
        // Model Event Handlers
        // ============================================

        private void HandleLivesChanged(int newLives)
        {
            _view.SetLives(newLives);
        }
    }
}
