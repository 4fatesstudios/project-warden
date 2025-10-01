using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Manages drag and drop operations for ingredients
    /// Provides visual feedback and grid highlighting during drags
    /// </summary>
    public class IngredientDragDropManager : MonoBehaviour
    {
        [Header("Grid Highlighting")]
        [SerializeField] private bool showGridHighlight = true;
        [SerializeField] private Color validCellColor = new Color(0f, 1f, 0f, 0.3f);
        [SerializeField] private Color invalidCellColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private LayerMask gridLayerMask = -1;
        
        [Header("Visual Feedback")]
        [SerializeField] private bool showDropZoneIndicator = true;
        [SerializeField] private float highlightPulseSpeed = 2f;
        
        // State
        private DraggableIngredient currentDraggedItem;
        private Vector2Int currentHoveredCell = Vector2Int.one * -1;
        private GridGameManager gridManager;
        private List<GameObject> highlightObjects = new List<GameObject>();
        
        // Visual feedback objects
        private GameObject dropZoneIndicator;
        
        private void Awake()
        {
            gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogWarning("IngredientDragDropManager: GridGameManager not found!");
            }
        }
        
        public void OnDragStart(DraggableIngredient draggedItem, PointerEventData eventData)
        {
            currentDraggedItem = draggedItem;
            
            if (showDropZoneIndicator)
            {
                CreateDropZoneIndicator();
            }
            
            Debug.Log($"DragDropManager: Drag started for {draggedItem.AssignedIngredient?.ItemName}");
        }
        
        public void OnDragUpdate(DraggableIngredient draggedItem, PointerEventData eventData, bool isValidDrop)
        {
            if (currentDraggedItem != draggedItem) return;
            
            // Update grid cell highlighting
            UpdateGridHighlighting(eventData);
            
            // Update drop zone indicator
            if (dropZoneIndicator != null)
            {
                UpdateDropZoneIndicator(isValidDrop);
            }
        }
        
        public void OnDragEnd(DraggableIngredient draggedItem, PointerEventData eventData, bool wasDropped)
        {
            if (currentDraggedItem != draggedItem) return;
            
            // Clear all visual feedback
            ClearGridHighlighting();
            
            if (dropZoneIndicator != null)
            {
                Destroy(dropZoneIndicator);
                dropZoneIndicator = null;
            }
            
            currentDraggedItem = null;
            currentHoveredCell = Vector2Int.one * -1;
            
            Debug.Log($"DragDropManager: Drag ended for {draggedItem.AssignedIngredient?.ItemName}, dropped: {wasDropped}");
        }
        
        private void UpdateGridHighlighting(PointerEventData eventData)
        {
            if (!showGridHighlight || gridManager == null || currentDraggedItem?.AssignedIngredient == null)
                return;
            
            // Raycast to find grid position
            var mainCamera = global::UnityEngine.Camera.main;
            if (mainCamera == null) return;
            
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
            {
                Vector2Int gridPos = gridManager.WorldToGridPosition(hit.point);
                
                // Only update if we moved to a different cell
                if (gridPos != currentHoveredCell)
                {
                    ClearGridHighlighting();
                    currentHoveredCell = gridPos;
                    HighlightIngredientArea(gridPos, currentDraggedItem.AssignedIngredient);
                }
            }
            else
            {
                // Not over grid, clear highlighting
                if (currentHoveredCell != Vector2Int.one * -1)
                {
                    ClearGridHighlighting();
                    currentHoveredCell = Vector2Int.one * -1;
                }
            }
        }
        
        private void HighlightIngredientArea(Vector2Int gridPosition, Ingredient ingredient)
        {
            if (gridManager == null) return;
            
            // Check if placement is valid
            bool canPlace = gridManager.CanPlaceIngredient(ingredient, gridPosition);
            Color highlightColor = canPlace ? validCellColor : invalidCellColor;
            
            // Highlight all cells that would be occupied
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                    
                    // Check if cell is within grid bounds
                    if (IsValidGridPosition(cellPos.x, cellPos.y))
                    {
                        CreateCellHighlight(cellPos, highlightColor);
                    }
                }
            }
        }
        
        private void CreateCellHighlight(Vector2Int gridPosition, Color color)
        {
            if (gridManager == null) return;
            
            // Create highlight object
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            highlight.name = $"CellHighlight_{gridPosition.x}_{gridPosition.y}";
            
            // Remove collider to avoid interfering with raycasts
            Collider highlightCollider = highlight.GetComponent<Collider>();
            if (highlightCollider != null)
            {
                Destroy(highlightCollider);
            }
            
            // Position at cell center
            Vector3 worldPos = gridManager.GridToWorldPosition(gridPosition);
            worldPos.y += 0.01f; // Slightly above grid to avoid z-fighting
            highlight.transform.position = worldPos;
            
            // Scale to cell size
            float cellSize = gridManager.cellSize;
            highlight.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1f);
            
            // Set up material
            Renderer renderer = highlight.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                
                renderer.material = mat;
            }
            
            // Add pulsing animation
            CellHighlightAnimator animator = highlight.AddComponent<CellHighlightAnimator>();
            animator.Initialize(color, highlightPulseSpeed);
            
            highlightObjects.Add(highlight);
        }
        
        private void ClearGridHighlighting()
        {
            foreach (GameObject highlight in highlightObjects)
            {
                if (highlight != null)
                {
                    Destroy(highlight);
                }
            }
            highlightObjects.Clear();
        }
        
        private void CreateDropZoneIndicator()
        {
            dropZoneIndicator = new GameObject("Drop Zone Indicator");
            
            // Create a simple visual indicator (could be enhanced with custom graphics)
            GameObject indicatorQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            indicatorQuad.transform.SetParent(dropZoneIndicator.transform);
            indicatorQuad.transform.localPosition = Vector3.zero;
            indicatorQuad.transform.localScale = Vector3.one * 2f;
            
            // Remove collider
            Collider col = indicatorQuad.GetComponent<Collider>();
            if (col != null) Destroy(col);
            
            // Set up material
            Renderer renderer = indicatorQuad.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(1f, 1f, 1f, 0.1f);
                renderer.material = mat;
            }
            
            dropZoneIndicator.SetActive(false);
        }
        
        private void UpdateDropZoneIndicator(bool isValidDrop)
        {
            if (dropZoneIndicator == null) return;
            
            dropZoneIndicator.SetActive(true);
            
            // Position indicator at current mouse world position
            var mainCamera = global::UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
                dropZoneIndicator.transform.position = mouseWorldPos;
            }
            
            // Update color based on validity
            Renderer renderer = dropZoneIndicator.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Color targetColor = isValidDrop ? validCellColor : invalidCellColor;
                renderer.material.color = targetColor;
            }
        }
        
        // Public utility methods
        public bool IsDragging()
        {
            return currentDraggedItem != null;
        }
        
        public DraggableIngredient GetCurrentDraggedItem()
        {
            return currentDraggedItem;
        }
        
        public void SetGridLayerMask(LayerMask mask)
        {
            gridLayerMask = mask;
        }
        
        public void SetHighlightColors(Color validColor, Color invalidColor)
        {
            validCellColor = validColor;
            invalidCellColor = invalidColor;
        }
        
        private bool IsValidGridPosition(int x, int y)
        {
            if (gridManager == null) return false;
            return x >= 0 && x < gridManager.gridWidth && y >= 0 && y < gridManager.gridHeight;
        }
    }
    
    /// <summary>
    /// Simple component to animate cell highlight pulsing
    /// </summary>
    public class CellHighlightAnimator : MonoBehaviour
    {
        private Color baseColor;
        private float pulseSpeed;
        private Renderer cellRenderer;
        private float startTime;
        
        public void Initialize(Color color, float speed)
        {
            baseColor = color;
            pulseSpeed = speed;
            cellRenderer = GetComponent<Renderer>();
            startTime = Time.time;
        }
        
        private void Update()
        {
            if (cellRenderer?.material != null)
            {
                float pulse = Mathf.Sin((Time.time - startTime) * pulseSpeed) * 0.3f + 0.7f;
                Color pulsedColor = baseColor;
                pulsedColor.a *= pulse;
                cellRenderer.material.color = pulsedColor;
            }
        }
    }
}