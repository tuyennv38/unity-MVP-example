using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Features.Blacksmith.Presenter;

namespace Features.Blacksmith.View
{
    public class BlacksmithView : MonoBehaviour, IBlacksmithView
    {
        [Header("UI References")]
        [SerializeField] private Text livesLabel;

        [Header("Components")]
        [SerializeField] private CharacterMotor motor;

        private IBlacksmithPresenter _presenter;
        private Animator _anim;

        void Start()
        {
            _presenter = new BlacksmithPresenter(this);
            _anim = GetComponent<Animator>();
        }

        // ============================================
        // Region: User Events → Delegate to Presenter
        // ============================================

        /// <summary>
        /// Gọi từ UI Button — chào hỏi
        /// </summary>
        public void OnGreetButtonClicked()
        {
            _presenter.Greet();
        }

        /// <summary>
        /// Gọi từ UI Button — nhảy
        /// </summary>
        public void OnJumpButtonClicked()
        {
            _presenter.Jump();
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("Health"))
            {
                StartCoroutine(HandleHealthPickup(collider.gameObject));
            }
        }

        private IEnumerator HandleHealthPickup(GameObject pickup)
        {
            pickup.SetActive(false);
            _presenter.AddLives();

            yield return new WaitForSeconds(2.0f);
            pickup.SetActive(true);
        }

        // ============================================
        // Region: IBlacksmithView — Presenter calls these
        // ============================================

        public void SetLives(int lives)
        {
            livesLabel.text = "Lives: " + lives.ToString();
        }

        public void PlayAnimation(string name)
        {
            if (_anim != null)
            {
                _anim.SetTrigger(name);
            }
        }

        public void DoJump()
        {
            if (motor != null)
            {
                motor.DoJump();
            }
        }
    }
}
