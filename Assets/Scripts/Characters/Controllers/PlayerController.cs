using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;

    private void Awake() {
        _playerInput = new PlayerInput();
    }

    public Vector2 GetMovementVectorNormalized() {
        var movementVector = _playerInput.PlayerInputMap_EXPLORATION.MovementInput.ReadValue<Vector2>();
        movementVector = movementVector.normalized;
        return movementVector;
    }

    private void OnEnable() {
        _playerInput.PlayerInputMap_EXPLORATION.Enable();
    }

    private void OnDisable() {
        _playerInput.PlayerInputMap_EXPLORATION.Disable();
    }
}
