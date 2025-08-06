using UnityEngine;

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
        var move = new Vector3(input.x, 0f, input.y); // X and Z for movement
        _characterController.Move(move * (moveSpeed * Time.deltaTime));
    }
}
