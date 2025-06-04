using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace FourFatesStudios.ProjectWarden.Characters.Controllers
{
    
    public class PlayerController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private Dictionary<ActionMap, InputActionMap> mapDict;

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
            mapDict = new Dictionary<ActionMap, InputActionMap>()
            {
                {ActionMap.Exploration, _playerInput.PlayerInputMap_EXPLORATION},
                {ActionMap.UINavigation, _playerInput.PlayerInputMap_COMBAT_UINAVIGATION},
                { ActionMap.ActionSelection, _playerInput.PlayerInputMap_COMBAT_ACTIONSELECTION}
            };
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
            // _playerInput.PlayerInputMap_COMBAT_ACTIONSELECTION.Enable();
            
            // Exploration Mapping
            _playerInput.PlayerInputMap_EXPLORATION.Interact.performed += Exploration_Interact_performed;
        }
    
        private void OnDisable() {
            _playerInput.PlayerInputMap_EXPLORATION.Disable();
            
            // Exploration Mapping
            _playerInput.PlayerInputMap_EXPLORATION.Interact.performed -= Exploration_Interact_performed;
        }

        public void EnableInputMapOnly(ActionMap actionMap) {
            _playerInput.Disable();
            mapDict.GetValueOrDefault(actionMap).Enable();
        }
    }
    
}