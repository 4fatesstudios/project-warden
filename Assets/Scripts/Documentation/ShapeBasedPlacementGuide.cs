/*
 * SHAPE-BASED INGREDIENT PLACEMENT SYSTEM
 * =======================================
 * 
 * This guide explains how to use the new integrated shape-based ingredient placement system
 * that combines the AlchemySystemEditor shape persistence with the GridDemo placement system.
 * 
 * 
 * ## KEY FEATURES
 * ===============
 * 
 * ✅ **Shape-Based Placement**: Ingredients can now have complex shapes (L-shapes, T-shapes, etc.)
 * ✅ **Visual Preview**: Real-time shape preview during placement with valid/invalid highlighting
 * ✅ **Backward Compatibility**: Falls back to rectangle placement for ingredients without shape data
 * ✅ **Persistent Shapes**: Shapes are saved with ingredient assets and loaded automatically
 * ✅ **Template System**: Pre-defined shapes (Rectangle, Cross, L-Shape, etc.) available
 * ✅ **Editor Integration**: Shape editing directly in AlchemySystemEditor
 * 
 * 
 * ## HOW TO USE
 * =============
 * 
 * ### 1. Creating Shape-Based Ingredients
 * 
 * ```csharp
 * // In the AlchemySystemEditor:
 * // 1. Select an ingredient in the Ingredients tab
 * // 2. Use the integrated grid editor to design the shape
 * // 3. Click individual cells to toggle ingredient presence
 * // 4. Use templates for common shapes
 * // 5. Save the shape to the ingredient asset
 * 
 * // Programmatically:
 * var ingredient = ScriptableObject.CreateInstance<Ingredient>();
 * ingredient.ApplyShapeTemplate(ShapeTemplate.Cross);
 * // or
 * bool[,] customShape = new bool[3,3] { 
 *     {false, true, false}, 
 *     {true, true, true}, 
 *     {false, true, false} 
 * };
 * ingredient.SetShape(customShape);
 * ```
 * 
 * 
 * ### 2. Placing Ingredients in Grid
 * 
 * ```csharp
 * // The placement system automatically detects shape data:
 * 
 * // Check if ingredient can be placed (considers shape)
 * bool canPlace = gridManager.CanPlaceIngredient(ingredient, position);
 * 
 * // Place ingredient (uses shape if available, falls back to rectangle)
 * bool success = gridManager.TryPlaceIngredient(ingredient, position);
 * 
 * // Get cells that would be occupied by ingredient
 * List<Vector2Int> cells = gridManager.GetIngredientCells(ingredient, position);
 * 
 * // Remove ingredient from grid
 * bool removed = gridManager.RemoveIngredientAt(position);
 * ```
 * 
 * 
 * ### 3. Visual System Integration
 * 
 * ```csharp
 * // The GridVisualizer automatically shows shape-based previews:
 * 
 * // Update highlight preview (shows exact shape)
 * gridVisualizer.UpdateHighlight(hoveredCell, selectedIngredient);
 * 
 * // The IngredientPlacer creates visuals based on shape:
 * // - Rectangle ingredients: Single scaled cube
 * // - Shape-based ingredients: Multiple cubes for each active cell
 * ```
 * 
 * 
 * ### 4. Shape Templates Available
 * 
 * ```csharp
 * public enum ShapeTemplate
 * {
 *     Rectangle,    // Standard rectangle (backward compatible)
 *     Cross,        // + shape
 *     LShape,       // L shape
 *     TShape,       // T shape
 *     ZShape,       // Z/S shape
 *     Diamond,      // Diamond shape
 *     Single        // Single cell
 * }
 * 
 * // Apply template to ingredient
 * ingredient.ApplyShapeTemplate(ShapeTemplate.Cross);
 * ```
 * 
 * 
 * ## TECHNICAL ARCHITECTURE
 * =========================
 * 
 * ### Core Components:
 * 
 * **Ingredient.cs** (Enhanced)
 * - `ShapeData` property for persistent shape storage
 * - `GetShape()` method to retrieve bool[,] array
 * - `SetShape()` method to save shape data
 * - `ApplyShapeTemplate()` for predefined shapes
 * 
 * **GridGameManager.cs** (Enhanced)
 * - `CanPlaceIngredient()` checks shape-based collision
 * - `MarkCellsAsOccupied()` marks only active shape cells
 * - `GetIngredientCells()` returns cells for any ingredient
 * - Backward compatible with rectangle placement
 * 
 * **IngredientPlacer.cs** (Enhanced)
 * - `CreateShapeBasedVisual()` creates multi-cube representations
 * - `CreateRectangleVisual()` for backward compatibility
 * - Individual cell configuration for shape variations
 * 
 * **GridVisualizer.cs** (Enhanced)
 * - `UpdateHighlight()` shows exact shape preview
 * - Accurate visual feedback for valid/invalid placement
 * 
 * **AlchemySystemEditor.cs** (Enhanced)
 * - Integrated shape editing UI
 * - Persistent grid editor with save/load
 * - Template application and import/export
 * 
 * 
 * ## DEMO SCENE SETUP
 * ===================
 * 
 * 1. Create a GameObject with GridGameManager
 * 2. Add ShapeBasedPlacementDemo component
 * 3. Assign demo ingredients with different shapes
 * 4. Set up UI buttons for ingredient selection
 * 5. Run scene and test placement!
 * 
 * 
 * ## BACKWARD COMPATIBILITY
 * =========================
 * 
 * ✅ Existing ingredients without shape data work unchanged
 * ✅ Rectangle placement still supported
 * ✅ All existing scripts continue to function
 * ✅ Shape data is optional - system gracefully falls back
 * 
 * 
 * ## PERFORMANCE NOTES
 * ====================
 * 
 * - Shape data is cached on ingredients for fast access
 * - Placement validation is O(shape_cells) instead of O(rectangle_area)
 * - Visual system creates individual cubes for accurate representation
 * - Memory usage increases slightly due to shape storage
 * 
 * 
 * ## NEXT STEPS / EXTENSIONS
 * ==========================
 * 
 * Potential future enhancements:
 * - Rotation support for shaped ingredients
 * - Shape-based reaction areas
 * - Procedural shape generation
 * - Shape combinations and merging
 * - 3D shape support (multiple layers)
 * 
 */

using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.Documentation
{
    /// <summary>
    /// Example implementation showing how to use the shape-based placement system
    /// </summary>
    public class ShapeBasedPlacementExample : MonoBehaviour
    {
        [Header("Example Ingredients")]
        public Ingredient rectangleIngredient;
        public Ingredient crossShapeIngredient;
        public Ingredient lShapeIngredient;
        
        [Header("Grid Reference")]
        public GridGameManager gridManager;
        
        private void Start()
        {
            // Example: Create ingredients with different shapes
            CreateExampleIngredients();
            
            // Example: Test placement logic
            TestPlacement();
        }
        
        private void CreateExampleIngredients()
        {
            if (crossShapeIngredient != null)
            {
                // Apply cross template
                crossShapeIngredient.ApplyShapeTemplate(ShapeTemplate.Cross);
                Debug.Log($"Applied Cross template to {crossShapeIngredient.ItemName}");
            }
            
            if (lShapeIngredient != null)
            {
                // Apply L-shape template
                lShapeIngredient.ApplyShapeTemplate(ShapeTemplate.LShape);
                Debug.Log($"Applied L-Shape template to {lShapeIngredient.ItemName}");
            }
        }
        
        private void TestPlacement()
        {
            if (gridManager == null) return;
            
            // Test placement at different positions
            var testPositions = new Vector2Int[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(2, 1),
                new Vector2Int(1, 3)
            };
            
            var testIngredients = new Ingredient[] 
            { 
                rectangleIngredient, 
                crossShapeIngredient, 
                lShapeIngredient 
            };
            
            for (int i = 0; i < testPositions.Length && i < testIngredients.Length; i++)
            {
                var ingredient = testIngredients[i];
                var position = testPositions[i];
                
                if (ingredient != null)
                {
                    bool canPlace = gridManager.CanPlaceIngredient(ingredient, position);
                    Debug.Log($"Can place {ingredient.ItemName} at {position}: {canPlace}");
                    
                    if (canPlace)
                    {
                        gridManager.TryPlaceIngredient(ingredient, position);
                        
                        // Show which cells are occupied
                        var occupiedCells = gridManager.GetIngredientCells(ingredient, position);
                        Debug.Log($"{ingredient.ItemName} occupies {occupiedCells.Count} cells: " +
                                 string.Join(", ", occupiedCells));
                    }
                }
            }
        }
    }
}