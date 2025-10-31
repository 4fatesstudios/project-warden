using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Makes ingredient UI elements draggable from the left panel to the grid
    /// </summary>
    public class DraggableIngredient : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Dragging Settings")]
        [SerializeField] private bool enableDragAndDrop = true;
        [SerializeField] private float dragScale = 0.9f;
        [SerializeField] private float dragAlpha = 0.7f;
        [SerializeField] private LayerMask gridLayerMask = -1;
        
        [Header("Visual Feedback")]
        #pragma warning disable 0414
        [SerializeField] private bool showGridPreview = true;
        #pragma warning restore 0414
        [SerializeField] private Color validDropColor = Color.green;
        [SerializeField] private Color invalidDropColor = Color.red;
        
        public Ingredient AssignedIngredient { get; private set; }
        
        // Drag state
        private bool isDragging = false;
        private GameObject dragPreview;
        private RectTransform originalParent;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private CanvasGroup canvasGroup;
        private GridGameManager gridManager;
        private IngredientDragDropManager dragDropManager;
        
        // Visual components
        private Image buttonImage;
        private TextMeshProUGUI buttonText;
        private Button button;
        
        public void Initialize(Ingredient ingredient, GridGameManager manager)
        {
            AssignedIngredient = ingredient;
            gridManager = manager;
            
            // Get components
            buttonImage = GetComponent<Image>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            button = GetComponent<Button>();
            
            // Ensure we have a CanvasGroup for alpha control
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Find or create drag drop manager
            dragDropManager = FindFirstObjectByType<IngredientDragDropManager>();
            if (dragDropManager == null)
            {
                GameObject managerObj = new GameObject("Ingredient DragDrop Manager");
                dragDropManager = managerObj.AddComponent<IngredientDragDropManager>();
            }
            
            // Set up button click as fallback
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClick);
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            // Regular click functionality (select ingredient)
            if (!isDragging)
            {
                OnButtonClick();
            }
        }
        
        private void OnButtonClick()
        {
            if (gridManager != null && AssignedIngredient != null)
            {
                gridManager.SelectIngredient(AssignedIngredient);
                Debug.Log($"DraggableIngredient: Selected {AssignedIngredient.ItemName}");
                
                // Visual feedback for selection
                UpdateSelectionVisual(true);
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!enableDragAndDrop || AssignedIngredient == null) return;
            
            isDragging = true;
            
            // Store original state
            originalParent = transform.parent as RectTransform;
            originalPosition = transform.position;
            originalScale = transform.localScale;
            
            // Create drag preview
            CreateDragPreview();
            
            // Adjust original button visual
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
            
            // Notify drag drop manager
            dragDropManager.OnDragStart(this, eventData);
            
            Debug.Log($"DraggableIngredient: Started dragging {AssignedIngredient.ItemName}");
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || dragPreview == null) return;
            
            // Update drag preview position
            Vector3 screenPosition = eventData.position;
            dragPreview.transform.position = screenPosition;
            
            // Check for valid drop zone
            bool isValidDrop = IsOverValidDropZone(eventData);
            
            // Update preview visuals
            UpdateDragPreviewVisual(isValidDrop);
            
            // Notify drag drop manager for grid highlighting
            dragDropManager.OnDragUpdate(this, eventData, isValidDrop);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            
            // Check if we're over a valid drop zone
            bool dropped = TryDropIngredient(eventData);
            
            // Clean up drag preview
            if (dragPreview != null)
            {
                Destroy(dragPreview);
                dragPreview = null;
            }
            
            // Restore original button visual
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Notify drag drop manager
            dragDropManager.OnDragEnd(this, eventData, dropped);
            
            Debug.Log($"DraggableIngredient: Finished dragging {AssignedIngredient.ItemName}, dropped: {dropped}");
        }
        
        private void CreateDragPreview()
        {
            // Create preview GameObject
            dragPreview = new GameObject("Drag Preview");
            dragPreview.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
            
            // Copy visual elements
            RectTransform previewRect = dragPreview.AddComponent<RectTransform>();
            previewRect.sizeDelta = (transform as RectTransform).sizeDelta;
            
            Image previewImage = dragPreview.AddComponent<Image>();
            if (buttonImage != null)
            {
                previewImage.sprite = buttonImage.sprite;
                previewImage.color = buttonImage.color;
            }
            
            // Add text if present
            if (buttonText != null)
            {
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(dragPreview.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                TextMeshProUGUI previewText = textObj.AddComponent<TextMeshProUGUI>();
                previewText.text = buttonText.text;
                previewText.fontSize = buttonText.fontSize;
                previewText.alignment = buttonText.alignment;
                previewText.color = buttonText.color;
            }
            
            // Apply drag scale
            dragPreview.transform.localScale = Vector3.one * dragScale;
            
            // Set high sorting order to appear on top
            Canvas previewCanvas = dragPreview.AddComponent<Canvas>();
            previewCanvas.overrideSorting = true;
            previewCanvas.sortingOrder = 1000;
            
            // Disable raycast blocking
            CanvasGroup previewGroup = dragPreview.AddComponent<CanvasGroup>();
            previewGroup.blocksRaycasts = false;
        }
        
        private bool IsOverValidDropZone(PointerEventData eventData)
        {
            // Check if mouse is over the grid area
            var mainCamera = global::UnityEngine.Camera.main;
            if (mainCamera == null) return false;
            
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
            {
                // Check if the grid position is valid for this ingredient
                if (gridManager != null)
                {
                    Vector2Int gridPos = gridManager.WorldToGridPosition(hit.point);
                    return gridManager.CanPlaceIngredient(AssignedIngredient, gridPos);
                }
            }
            
            return false;
        }
        
        private bool TryDropIngredient(PointerEventData eventData)
        {
            if (gridManager == null || AssignedIngredient == null) return false;
            
            // Raycast to find grid position
            var mainCamera = global::UnityEngine.Camera.main;
            if (mainCamera == null) return false;
            
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
            {
                Vector2Int gridPos = gridManager.WorldToGridPosition(hit.point);
                
                // Try to place the ingredient
                if (gridManager.CanPlaceIngredient(AssignedIngredient, gridPos))
                {
                    bool success = gridManager.TryPlaceIngredient(AssignedIngredient, gridPos);
                    if (success)
                    {
                        // Auto-select this ingredient after successful drop
                        gridManager.SelectIngredient(AssignedIngredient);
                        UpdateSelectionVisual(true);
                    }
                    return success;
                }
            }
            
            return false;
        }
        
        private void UpdateDragPreviewVisual(bool isValidDrop)
        {
            if (dragPreview == null) return;
            
            Image previewImage = dragPreview.GetComponent<Image>();
            if (previewImage != null)
            {
                Color targetColor = isValidDrop ? validDropColor : invalidDropColor;
                targetColor.a = 0.8f;
                previewImage.color = Color.Lerp(previewImage.color, targetColor, Time.deltaTime * 10f);
            }
        }
        
        public void UpdateSelectionVisual(bool isSelected)
        {
            if (buttonImage == null) return;
            
            if (isSelected)
            {
                // Highlight as selected
                buttonImage.color = Color.white;
            }
            else
            {
                // Return to original color
                Color originalColor = GetAspectColor(AssignedIngredient?.IngredientAspect ?? Aspect.Corporeal);
                buttonImage.color = originalColor;
            }
        }
        
        private Color GetAspectColor(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Scorch: 
                    return new Color(0.9f, 0.3f, 0.3f, 0.8f);
                case Aspect.Frigid: 
                    return new Color(0.3f, 0.7f, 0.9f, 0.8f);
                case Aspect.Arc: 
                    return new Color(0.9f, 0.9f, 0.3f, 0.8f);
                case Aspect.Caustic: 
                    return new Color(0.7f, 0.5f, 0.2f, 0.8f);
                case Aspect.Corporeal: 
                    return new Color(0.6f, 0.6f, 0.6f, 0.8f);
                case Aspect.Divine: 
                    return new Color(0.9f, 0.9f, 0.9f, 0.8f);
                default: 
                    return new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }
        }
        
        public void SetDragEnabled(bool enabled)
        {
            enableDragAndDrop = enabled;
        }
    }
}