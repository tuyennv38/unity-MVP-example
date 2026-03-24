using UnityEngine;

namespace Features.Blacksmith.View
{
    /// <summary>
    /// CharacterMotor — MonoBehaviour riêng cho physics/movement.
    /// Tách ra khỏi BlacksmithView để giữ View thuần UI.
    /// </summary>
    public class CharacterMotor : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpSpeed = 8.0f;
        [SerializeField] private float fallAcceleration = 2.0f;
        [SerializeField] private float groundCheckOffset = 2.0f;
        [SerializeField] private float groundCheckDistance = 0.001f;

        private bool _isJumping;
        private bool _isFalling;
        private float _currentJumpSpeed;
        private float _currentFallSpeed;
        private float _initialJumpSpeed;

        void Awake()
        {
            _initialJumpSpeed = jumpSpeed;
            _currentFallSpeed = 5.0f;
        }

        void Update()
        {
            if (_isJumping)
            {
                HandleJump();
            }
            else if (_isFalling)
            {
                HandleFall();
            }
        }

        public void DoJump()
        {
            if (_isJumping || _isFalling) return;

            _isJumping = true;
            _currentJumpSpeed = _initialJumpSpeed;
        }

        private void HandleJump()
        {
            transform.position += transform.up * _currentJumpSpeed * Time.deltaTime;
            _currentJumpSpeed -= _initialJumpSpeed * Time.deltaTime;

            if (_currentJumpSpeed <= 0)
            {
                _currentJumpSpeed = _initialJumpSpeed;
                _isJumping = false;
                _isFalling = true;
            }
        }

        private void HandleFall()
        {
            Vector2 rayOrigin = transform.position - transform.up * groundCheckOffset;

            if (Physics2D.Raycast(rayOrigin, -transform.up, groundCheckDistance))
            {
                _currentFallSpeed = 5.0f;
                _isFalling = false;
            }
            else
            {
                _currentFallSpeed += fallAcceleration * Time.deltaTime;
                transform.position += -transform.up * _currentFallSpeed * Time.deltaTime;
            }
        }
    }
}
