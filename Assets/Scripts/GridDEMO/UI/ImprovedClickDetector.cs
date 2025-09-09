using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Improved click detection that properly handles UI vs world clicks
    /// Hides right panel when clicking outside UI or grid areas
    /// Shows right panel only when clicking on ingredients
    /// </summary>
    public class ImprovedClickDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private LayerMask gridLayerMask = -1;
        [SerializeField] private LayerMask ingredientLayerMask = -1;
        [SerializeField] private bool enableDebugLogs;
        [SerializeField] private bool rightPanelHiddenByDefault = true;
        
        // Events
        public System.Action<Ingredient, Vector2Int> OnIngredientClicked;
        public System.Action OnEmptySpaceClicked;
        public System.Action<Vector2Int> OnGridCellClicked;
        public System.Action OnUIClicked;
        
        // References
        private UnityEngine.Camera mainCamera;
        private GridGameManager gridManager;
        private RightPanelManager rightPanelManager;
        private Mouse mouse;
        
        private void Start()
        {
            // Get references
            mainCamera = global::UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
            }
            
            gridManager = FindFirstObjectByType<GridGameManager>();
            rightPanelManager = FindFirstObjectByType<RightPanelManager>();
            mouse = Mouse.current;
            
            if (mainCamera == null)
            {
                Debug.LogError("ImprovedClickDetector: No camera found!");
            }
            
            if (gridManager == null)
            {
                Debug.LogError("ImprovedClickDetector: No GridGameManager found!");
            }
            
            // Hide right panel initially if configured
            if (rightPanelHiddenByDefault && rightPanelManager != null)
            {
                StartCoroutine(HideRightPanelInitially());
            }
        }
        
        private System.Collections.IEnumerator HideRightPanelInitially()
        {
            yield return new WaitForEndOfFrame();
            if (rightPanelManager != null)
            {
                rightPanelManager.HidePanel(false);
                if (enableDebugLogs)
                    Debug.Log("ImprovedClickDetector: Right panel hidden initially");
            }
        }
        
        private void Update()
        {
            HandleMouseInput();
        }
        
        private void HandleMouseInput()
        {
            // Use Input System if available, fallback to legacy Input
            bool mousePressed = false;
            Vector2 mousePosition = Vector2.zero;
            
            if (mouse != null)
            {
                mousePressed = mouse.leftButton.wasPressedThisFrame;
                mousePosition = mouse.position.ReadValue();
            }
            else
            {
                mousePressed = Input.GetMouseButtonDown(0);
                mousePosition = Input.mousePosition;
            }
            
            if (mousePressed)
            {
                ProcessClick(mousePosition);
            }
        }
        
        private void ProcessClick(Vector2 mousePosition)
        {
            if (mainCamera == null) return;
            
            // Priority 1: Check if clicking on UI (highest priority)
            if (IsClickingOnUI(mousePosition))
            {
                if (enableDebugLogs) Debug.Log("ImprovedClickDetector: Clicked on UI - panel stays open");
                OnUIClicked?.Invoke();
                return;
            }
            
            // Priority 2: Check if clicking on ingredient
            if (CheckIngredientClick(mousePosition))
            {
                return; // Ingredient click handled
            }
            
            // Priority 3: Check if clicking on empty grid
            if (CheckGridClick(mousePosition))
            {
                if (enableDebugLogs) Debug.Log("ImprovedClickDetector: Clicked on empty grid - hiding panel");
                HideRightPanel();
                OnEmptySpaceClicked?.Invoke();
                return;
            }
            
            // Priority 4: Empty space (lowest priority)
            if (enableDebugLogs) Debug.Log("ImprovedClickDetector: Clicked on empty space - hiding panel");
            HideRightPanel();
            OnEmptySpaceClicked?.Invoke();
        }
        
        private bool IsClickingOnUI(Vector2 mousePosition)
        {
            // Use EventSystem to detect UI clicks
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition
            };
            
            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResults);
            
            foreach (var result in raycastResults)
            {
                // Check if clicking on any UI element
                if (result.gameObject.GetComponent<UnityEngine.UI.Graphic>() != null ||
                    result.gameObject.name.Contains("Compact Sidebar") ||
                    result.gameObject.name.Contains("Right Info Panel") ||
                    result.gameObject.name.Contains("Button") ||
                    result.gameObject.name.Contains("Scroll"))
                {
                    if (enableDebugLogs)
                        Debug.Log($"ImprovedClickDetector: UI click detected on {result.gameObject.name}");
                    
                    return true;
                }
            }
            
            return false;
        }
        
        private bool CheckIngredientClick(Vector2 mousePosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ingredientLayerMask))
            {
                IngredientInteraction interaction = hit.collider.GetComponent<IngredientInteraction>();
                if (interaction != null && interaction.ingredient != null)
                {
                    Vector2Int gridPosition = Vector2Int.zero;
                    if (gridManager != null)
                    {
                        gridPosition = gridManager.WorldToGridPosition(hit.point);
                    }
                    
                    if (enableDebugLogs)
                        Debug.Log($"ImprovedClickDetector: Ingredient clicked - {interaction.ingredient.ItemName}");
                    
                    OnIngredientClicked?.Invoke(interaction.ingredient, gridPosition);
                    return true;
                }
            }
            
            return false;
        }
        
        private bool CheckGridClick(Vector2 mousePosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridLayerMask))
            {
                // Make sure it's not an ingredient
                if (hit.collider.GetComponent<IngredientInteraction>() == null)
                {
                    if (gridManager != null)
                    {
                        Vector2Int gridPosition = gridManager.WorldToGridPosition(hit.point);
                        OnGridCellClicked?.Invoke(gridPosition);
                        
                        // Try to place ingredient if one is selected
                        TriggerNormalGridPlacement(gridPosition);
                    }
                    
                    return true;
                }
            }
            
            return false;
        }
        
        private void HideRightPanel()
        {
            if (rightPanelManager != null)
            {
                rightPanelManager.HidePanel();
            }
        }
        
        private bool IsGridHit(RaycastHit hit)
        {
            return ((1 << hit.collider.gameObject.layer) & gridLayerMask) != 0;
        }
        
        private Vector2Int GetGridPositionFromWorldPosition(Vector3 worldPosition)
        {
            if (gridManager != null)
            {
                return gridManager.WorldToGridPosition(worldPosition);
            }
            return Vector2Int.zero;
        }
        
        private void TriggerNormalGridPlacement(Vector2Int gridPos)
        {
            if (gridManager != null && gridManager.CurrentSelectedIngredient != null)
            {
                bool success = gridManager.TryPlaceIngredient(gridManager.CurrentSelectedIngredient, gridPos);
                if (success && enableDebugLogs)
                {
                    Debug.Log($"ImprovedClickDetector: Placed {gridManager.CurrentSelectedIngredient.ItemName} at {gridPos}");
                }
            }
        }
        
        // Helper method for verification
        public bool HasEventSubscribers()
        {
            return OnIngredientClicked != null || OnEmptySpaceClicked != null;
        }
    }
    
    public enum ClickTarget
    {
        LeftPanelUI,
        RightPanelUI,
        GridWithIngredient,
        EmptyGrid,
        EmptySpace
    }
}
