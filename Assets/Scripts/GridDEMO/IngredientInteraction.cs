using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class IngredientInteraction : MonoBehaviour
    {
        public Ingredient ingredient;
        public string ingredientName;
        
        private MeshRenderer meshRenderer;
        private Material originalMaterial;
        private bool isHovered = false;
        
        public void Initialize(Ingredient ing, string name)
        {
            ingredient = ing;
            ingredientName = name;
            meshRenderer = GetComponent<MeshRenderer>();
            originalMaterial = meshRenderer.material;
        }
        
        private void OnMouseEnter()
        {
            if (!isHovered)
            {
                isHovered = true;
                HighlightIngredient(true);
                ShowTooltip();
            }
        }
        
        private void OnMouseExit()
        {
            if (isHovered)
            {
                isHovered = false;
                HighlightIngredient(false);
                HideTooltip();
            }
        }
        
        private void OnMouseDown()
        {
            if (Input.GetMouseButtonDown(1)) // Right click to remove
            {
                RemoveIngredient();
            }
            // Left click is now handled by ClickDetectionManager
            // No longer showing popup here
        }
        
        private void HighlightIngredient(bool highlight)
        {
            if (meshRenderer != null)
            {
                if (highlight)
                {
                    Material highlightMaterial = new Material(originalMaterial);
                    highlightMaterial.color = highlightMaterial.color * 1.5f; // Brighten
                    highlightMaterial.SetFloat("_EmissionIntensity", 0.5f);
                    meshRenderer.material = highlightMaterial;
                }
                else
                {
                    meshRenderer.material = originalMaterial;
                }
            }
        }
        
        private void ShowTooltip()
        {
            if (ingredient != null)
            {
                string tooltip = $"{ingredient.ItemName}\n" +
                               $"Aspect: {ingredient.IngredientAspect}\n" +
                               $"Potency: {ingredient.Potency}\n" +
                               $"Size: {ingredient.GridWidth}x{ingredient.GridHeight}\n" +
                               $"Archetype: {ingredient.IngredientArchetype}";
                
                // For now, just log the tooltip. In a full implementation, 
                // you'd show this in a UI panel
                Debug.Log($"Tooltip: {tooltip}");
            }
        }
        
        private void HideTooltip()
        {
            // Hide tooltip UI
        }
        
        private void ShowDetailedInfo()
        {
            if (ingredient != null)
            {
                string info = $"=== {ingredient.ItemName} ===\n" +
                             $"Description: {ingredient.ItemDescription}\n" +
                             $"Rarity: {ingredient.ItemRarity}\n" +
                             $"Aspect: {ingredient.IngredientAspect}\n" +
                             $"Archetype: {ingredient.IngredientArchetype}\n" +
                             $"Potency: {ingredient.Potency}/5\n" +
                             $"Grid Size: {ingredient.GridWidth}x{ingredient.GridHeight}\n" +
                             $"Corrupted: {ingredient.IsCorrupted}\n" +
                             $"Stability: {ingredient.StabilityRating:P0}";
                
                if (ingredient.UnlocksAdditionalSpace)
                {
                    info += $"\nUnlocks {ingredient.AdditionalSpaceCount} additional grid spaces";
                }
                
                Debug.Log(info);
            }
        }
        
        private void RemoveIngredient()
        {
            // Find the grid position of this ingredient
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                Vector3 worldPos = transform.position;
                Vector2Int gridPos = gridManager.WorldToGridPosition(worldPos);
                
                // Clear the grid cells
                if (ingredient != null)
                {
                    for (int x = 0; x < ingredient.GridWidth; x++)
                    {
                        for (int y = 0; y < ingredient.GridHeight; y++)
                        {
                            GridCell cell = gridManager.GetCell(gridPos.x + x, gridPos.y + y);
                            if (cell != null)
                            {
                                cell.Clear();
                            }
                        }
                    }
                }
                
                // Remove from placer
                IngredientPlacer placer = gridManager.GetComponent<IngredientPlacer>();
                if (placer != null)
                {
                    placer.RemoveIngredient(gridPos);
                }
                
                // Refresh visuals
                GridVisualizer visualizer = gridManager.GetComponent<GridVisualizer>();
                if (visualizer != null)
                {
                    visualizer.RefreshGrid();
                }
                
                Debug.Log($"Removed {ingredientName} from grid");
            }
        }
    }
}