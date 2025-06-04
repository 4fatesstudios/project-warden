using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace FourFatesStudios.ProjectWarden.Characters.Controllers
{
    
    public class PlayerController : MonoBehaviour
    {
        private PlayerInput _playerInput;

        public static event Action OnOpenSkills;
        public static event Action OnOpenItems;
        public static event Action OnGuard;
        public static event Action OnAttack;
        public static event Action OnCycleUp;
        public static event Action OnCycleDown;
        public static event Action OnCycleLeft;
        public static event Action OnCycleRight;
        public static event Action OnSelect;
        public static event Action OnBack;
        
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

        #region Combat Mapping

        private void Combat_OpenSkills_performed(InputAction.CallbackContext obj){
            OnOpenSkills?.Invoke();
        }
        
        private void Combat_OpenItems_performed(InputAction.CallbackContext obj){
            OnOpenItems?.Invoke();
        }
        
        private void Combat_Guard_performed(InputAction.CallbackContext obj){
            OnGuard?.Invoke();
        }
        
        private void Combat_Attack_performed(InputAction.CallbackContext obj){
            OnAttack?.Invoke();
        }
        
        private void Combat_CycleUp_performed(InputAction.CallbackContext obj){
            OnCycleUp?.Invoke();
        }
        
        private void Combat_CycleDown_performed(InputAction.CallbackContext obj){
            OnCycleDown?.Invoke();
        }
        
        private void Combat_CycleLeft_performed(InputAction.CallbackContext obj){
            OnCycleLeft?.Invoke();
        }
        
        private void Combat_CycleRight_performed(InputAction.CallbackContext obj){
            OnCycleRight?.Invoke();
        }
        
        private void Combat_Select_performed(InputAction.CallbackContext obj){
            OnSelect?.Invoke();
        }
        
        private void Combat_Back_performed(InputAction.CallbackContext obj){
            OnBack?.Invoke();
        }
        
        #endregion
        
        
        #region Exploration Mapping
        private void Exploration_Interact_performed(InputAction.CallbackContext obj) {
            OnExplorationInteractAction?.Invoke();
        }
        
        #endregion
    
        private void OnEnable() {
            _playerInput.PlayerInputMap_EXPLORATION.Enable();
            
            // Exploration Mapping
            _playerInput.PlayerInputMap_EXPLORATION.Interact.performed += Exploration_Interact_performed;
            
            //Combat Mapping
            _playerInput.PlayerInputMap_COMBAT.OpenSkills.performed += Combat_OpenSkills_performed;
            _playerInput.PlayerInputMap_COMBAT.OpenItems.performed += Combat_OpenItems_performed;
            _playerInput.PlayerInputMap_COMBAT.Guard.performed += Combat_Guard_performed;
            _playerInput.PlayerInputMap_COMBAT.Attack.performed += Combat_Attack_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleUp.performed += Combat_CycleUp_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleDown.performed += Combat_CycleDown_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleLeft.performed += Combat_CycleLeft_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleRight.performed += Combat_CycleRight_performed;
            _playerInput.PlayerInputMap_COMBAT.Select.performed += Combat_Select_performed;
            _playerInput.PlayerInputMap_COMBAT.Back.performed += Combat_Back_performed;
        }
    
        private void OnDisable() {
            _playerInput.PlayerInputMap_EXPLORATION.Disable();
            _playerInput.PlayerInputMap_COMBAT.Disable();
            
            //Combat Mapping
            _playerInput.PlayerInputMap_COMBAT.OpenSkills.performed -= Combat_OpenSkills_performed;
            _playerInput.PlayerInputMap_COMBAT.OpenItems.performed -= Combat_OpenItems_performed;
            _playerInput.PlayerInputMap_COMBAT.Guard.performed -= Combat_Guard_performed;
            _playerInput.PlayerInputMap_COMBAT.Attack.performed -= Combat_Attack_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleUp.performed -= Combat_CycleUp_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleDown.performed -= Combat_CycleDown_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleLeft.performed -= Combat_CycleLeft_performed;
            _playerInput.PlayerInputMap_COMBAT.CycleRight.performed -= Combat_CycleRight_performed;
            _playerInput.PlayerInputMap_COMBAT.Select.performed -= Combat_Select_performed;
            _playerInput.PlayerInputMap_COMBAT.Back.performed -= Combat_Back_performed;
            _playerInput.PlayerInputMap_EXPLORATION.Disable();
            
            // Exploration Mapping
            _playerInput.PlayerInputMap_EXPLORATION.Interact.performed -= Exploration_Interact_performed;
        }
    }
    
}