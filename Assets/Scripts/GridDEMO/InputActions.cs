using UnityEngine;
using UnityEngine.InputSystem;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class InputActions : MonoBehaviour
    {
        public InputAction clickAction;
        public InputAction moveAction;
        public InputAction cancelAction;
        public InputAction rotateAction;
        
        [System.Serializable]
        public class PlayerActions
        {
            public InputAction Click;
            public InputAction Move;
            public InputAction Cancel;
            public InputAction Rotate;
        }
        
        public PlayerActions Player { get; private set; }
        
        private void Awake()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            if (Player != null) return; // Already initialized
            
            Player = new PlayerActions();
            
            // Define input actions
            Player.Click = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
            Player.Move = new InputAction("Move", InputActionType.Value, "<Mouse>/position");
            Player.Cancel = new InputAction("Cancel", InputActionType.Button, "<Mouse>/rightButton");
            Player.Rotate = new InputAction("Rotate", InputActionType.Button, "<Keyboard>/r");
        }
        
        public void Enable()
        {
            Debug.Log("InputActions.Enable() called");
            if (Player == null) 
            {
                Debug.LogWarning("InputActions.Player is null. Initializing...");
                Initialize();
            }
            
            Player.Click?.Enable();
            Player.Move?.Enable();
            Player.Cancel?.Enable();
            Player.Rotate?.Enable();
            Debug.Log("InputActions enabled successfully");
        }
        
        public void Disable()
        {
            if (Player == null) return;
            
            Player.Click?.Disable();
            Player.Move?.Disable();
            Player.Cancel?.Disable();
            Player.Rotate?.Disable();
        }
        
        private void OnDestroy()
        {
            Disable();
        }
    }
}