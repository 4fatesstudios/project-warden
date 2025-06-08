using UnityEngine;
using FourFatesStudios.ProjectWarden.Characters.Controllers;

namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        private PlayerController _playerController;
        private CharacterController _characterController;
    
        private void Awake() {
            _playerController = GetComponent<PlayerController>();
            _characterController = GetComponent<CharacterController>();
        }
    
        private void Update() {
            HandleMovement();
        }
    
        private void HandleMovement() {
            var input = _playerController.GetMovementVectorNormalized();
            input = RotateVector2(input, 45f); // <-- Rotate input 45 degrees clockwise
            var move = new Vector3(input.x, 0f, input.y); // X and Z for movement
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