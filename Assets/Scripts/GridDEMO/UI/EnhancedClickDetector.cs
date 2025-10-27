using UnityEngine;
using UnityEngine.EventSystems;

namespace FourFatesStudios.ProjectWarden.GridDEMO.UI
{
    /// <summary>
    /// DEPRECATED: This class has been replaced by ImprovedClickDetector.cs
    /// Please use ImprovedClickDetector instead for better performance and features.
    /// Enhanced click detection system for grid-based interactions
    /// </summary>
    [System.Obsolete("EnhancedClickDetector is deprecated. Use ImprovedClickDetector from FourFatesStudios.ProjectWarden.GridDemo.UI instead.", true)]
    public class EnhancedClickDetector : MonoBehaviour, IPointerClickHandler
    {
        [Header("Click Detection Settings")]
        [SerializeField] private LayerMask clickableLayers = -1;
        [SerializeField] private float maxClickDistance = 100f;
        [SerializeField] private bool enableDebugRays = false;
        
        private UnityEngine.Camera mainCamera;
        
        // Events for different click types
        public System.Action<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient, Vector2Int> OnIngredientClicked;
        public System.Action<Vector2Int> OnGridCellClicked;
        public System.Action<Vector2Int> OnObstacleClicked;
        
        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<UnityEngine.Camera>();
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            HandleClick(eventData.position);
        }
        
        private void HandleClick(Vector2 screenPosition)
        {
            if (mainCamera == null) return;
            
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            
            if (enableDebugRays)
            {
                Debug.DrawRay(ray.origin, ray.direction * maxClickDistance, Color.red, 1f);
            }
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, clickableLayers))
            {
                ProcessHit(hit);
            }
        }
        
        private void ProcessHit(RaycastHit hit)
        {
            Debug.Log($"Clicked on: {hit.collider.name} at position: {hit.point}");
            
            // Add your click handling logic here
            var clickableComponent = hit.collider.GetComponent<IClickable>();
            clickableComponent?.OnClicked();
        }
    }
    
    public interface IClickable
    {
        void OnClicked();
    }
}