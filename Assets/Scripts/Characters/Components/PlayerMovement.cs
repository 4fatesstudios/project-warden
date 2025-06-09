using UnityEngine;
using FourFatesStudios.ProjectWarden.Characters.Controllers;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundedGravity = -2f;

        private PlayerController _playerController;
        private CharacterController _characterController;

        private float _verticalVelocity;

        private void Awake() {
            _playerController = GetComponent<PlayerController>();
            _characterController = GetComponent<CharacterController>();
        }

        private void Update() {
            HandleMovement();
        }

        private void HandleMovement() {
            Vector2 input = _playerController.GetMovementVectorNormalized();
            input = RotateVector2(input, 45f);
            Vector3 move = new Vector3(input.x, 0f, input.y);

            // Gravity
            if (_characterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = groundedGravity;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            move.y = _verticalVelocity;

            _characterController.Move(move * (moveSpeed * Time.deltaTime));
        }

        private static Vector2 RotateVector2(Vector2 v, float degrees) {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(
                v.x * cos + v.y * sin,
                -v.x * sin + v.y * cos
            );
        }
    }
}