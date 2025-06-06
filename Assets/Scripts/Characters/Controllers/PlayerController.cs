using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FourFatesStudios.ProjectWarden.Characters.Controllers
{
    
    public class PlayerController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        
        #region Exploration Callbacks
        public static event Action OnExplorationInteractAction;
        
        #endregion
    
        private void Awake() {
            _playerInput = new PlayerInput();
        }
    
        public Vector2 GetMovementVectorNormalized() {
            var movementVector = _playerInput.PlayerInputMap_EXPLORATION.MovementInput.ReadValue<Vector2>();
            movementVector = movementVector.normalized;
            return movementVector;
        }
        
        #region Exploration Mapping
        private void Exploration_Interact_performed(InputAction.CallbackContext obj) {
            OnExplorationInteractAction?.Invoke();
        }
        
        #endregion
    
        private void OnEnable() {
            _playerInput.PlayerInputMap_EXPLORATION.Enable();
            
            // Exploration Mapping
            _playerInput.PlayerInputMap_EXPLORATION.Interact.performed += Exploration_Interact_performed;
        }


        private void OnDisable() {
            _playerInput.PlayerInputMap_EXPLORATION.Disable();
            
            // Exploration Mapping
            _playerInput.PlayerInputMap_EXPLORATION.Interact.performed -= Exploration_Interact_performed;
        }
    }
    
}