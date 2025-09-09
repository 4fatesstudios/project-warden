using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections.Generic;

namespace FourFatesStudios.ProjectWarden.GridDemo.Examples
{
    /// <summary>
    /// A simple example showing how to set up and use the Grid Demo system programmatically
    /// </summary>
    public class SimpleGridExample : MonoBehaviour
    {
        [Header("Grid Configuration")]
        public int gridSize = 6;
        public float cellSize = 1f;
        
        [Header("Demo Ingredients")]
        public List<Ingredient> demoIngredients = new List<Ingredient>();
        
        [Header("Auto Setup")]
        public bool setupOnStart = true;
        public bool findIngredientsAutomatically = true;
        
        private GridGameManager gridManager;
        private GridDemoUI demoUI;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupGridDemo();
            }
        }
        
        [ContextMenu("Setup Grid Demo")]
        public void SetupGridDemo()
        {
            Debug.Log("Setting up Grid Demo...");
            
            // Find or create grid manager
            gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                CreateGridManager();
            }
            
            // Configure the grid
            ConfigureGrid();
            
            // Setup ingredients
            SetupIngredients();
            
            // Setup UI if available
            SetupUI();
            
            Debug.Log("Grid Demo setup complete!");
        }
        
        private void CreateGridManager()
        {
            GameObject gridObj = new GameObject("GridGameManager");
            gridManager = gridObj.AddComponent<GridGameManager>();
            gridObj.AddComponent<GridVisualizer>();
            gridObj.AddComponent<IngredientPlacer>();
            gridObj.AddComponent<GridDemoGameplay>();
            
            Debug.Log("Created GridGameManager with all components");
        }
        
        private void ConfigureGrid()
        {
            if (gridManager == null) return;
            
            gridManager.gridWidth = gridSize;
            gridManager.gridHeight = gridSize;
            gridManager.cellSize = cellSize;
            gridManager.gridStartPosition = Vector3.zero;
            
            Debug.Log($"Configured grid: {gridSize}x{gridSize}, cell size: {cellSize}");
        }
        
        private void SetupIngredients()
        {
            if (gridManager == null) return;
            
            if (findIngredientsAutomatically)
            {
                FindAllIngredients();
            }
            
            gridManager.availableIngredients = new List<Ingredient>(demoIngredients);
            Debug.Log($"Setup {demoIngredients.Count} ingredients for demo");
            
            // Auto-select first ingredient if available
            if (demoIngredients.Count > 0 && demoIngredients[0] != null)
            {
                gridManager.SelectIngredient(demoIngredients[0]);
                Debug.Log($"Auto-selected: {demoIngredients[0].ItemName}");
            }
        }
        
        private void FindAllIngredients()
        {
            demoIngredients.Clear();
            
            // Find all ingredient assets in the project
            Ingredient[] allIngredients = Resources.FindObjectsOfTypeAll<Ingredient>();
            
            foreach (Ingredient ingredient in allIngredients)
            {
                // Skip ingredients that are part of the editor (not actual assets)
                if (ingredient != null && !string.IsNullOrEmpty(ingredient.ItemName))
                {
                    demoIngredients.Add(ingredient);
                }
            }
            
            Debug.Log($"Found {demoIngredients.Count} ingredients automatically");
        }
        
        private void SetupUI()
        {
            demoUI = FindFirstObjectByType<GridDemoUI>();
            if (demoUI != null)
            {
                Debug.Log("Found existing GridDemoUI - ready to use!");
                return;
            }
            
            // Create basic UI setup
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                CreateBasicCanvas();
            }
            
            Debug.Log("UI setup complete");
        }
        
        private void CreateBasicCanvas()
        {
            GameObject canvasObj = new GameObject("GridDemo Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create EventSystem if needed
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        [ContextMenu("Demo Placement")]
        public void DemoIngredientPlacement()
        {
            if (gridManager == null || demoIngredients.Count == 0)
            {
                Debug.LogWarning("Grid not setup or no ingredients available");
                return;
            }
            
            // Clear grid first
            gridManager.ClearGrid();
            
            // Place a few ingredients randomly
            for (int i = 0; i < Mathf.Min(3, demoIngredients.Count); i++)
            {
                Ingredient ingredient = demoIngredients[i];
                if (ingredient == null) continue;
                
                // Try to find a valid position
                for (int attempts = 0; attempts < 10; attempts++)
                {
                    Vector2Int randomPos = new Vector2Int(
                        Random.Range(0, gridSize - ingredient.GridWidth + 1),
                        Random.Range(0, gridSize - ingredient.GridHeight + 1)
                    );
                    
                    if (gridManager.TryPlaceIngredient(ingredient, randomPos))
                    {
                        Debug.Log($"Placed {ingredient.ItemName} at {randomPos}");
                        break;
                    }
                }
            }
        }
        
        [ContextMenu("Clear Grid")]
        public void ClearGrid()
        {
            if (gridManager != null)
            {
                gridManager.ClearGrid();
                Debug.Log("Grid cleared");
            }
        }
        
        [ContextMenu("Show Grid Info")]
        public void ShowGridInfo()
        {
            if (gridManager == null)
            {
                Debug.Log("No grid manager found");
                return;
            }
            
            int occupiedCells = 0;
            GridCell[,] cells = gridManager.GetAllCells();
            
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    if (cells[x, y].IsOccupied)
                        occupiedCells++;
                }
            }
            
            float occupancyRate = (float)occupiedCells / (gridManager.gridWidth * gridManager.gridHeight) * 100f;
            
            Debug.Log($"Grid Info:\n" +
                     $"Size: {gridManager.gridWidth}x{gridManager.gridHeight}\n" +
                     $"Total cells: {gridManager.gridWidth * gridManager.gridHeight}\n" +
                     $"Occupied cells: {occupiedCells}\n" +
                     $"Occupancy rate: {occupancyRate:F1}%\n" +
                     $"Available ingredients: {gridManager.availableIngredients.Count}");
        }
        
        // Helper method for testing reactions
        [ContextMenu("Test Reactions")]
        public void TestElementReactions()
        {
            if (gridManager == null || demoIngredients.Count < 2)
            {
                Debug.LogWarning("Need at least 2 ingredients to test reactions");
                return;
            }
            
            gridManager.ClearGrid();
            
            // Place two different ingredients next to each other
            var ingredient1 = demoIngredients[0];
            var ingredient2 = demoIngredients[demoIngredients.Count > 1 ? 1 : 0];
            
            if (ingredient1 != null && ingredient2 != null)
            {
                gridManager.TryPlaceIngredient(ingredient1, new Vector2Int(2, 2));
                gridManager.TryPlaceIngredient(ingredient2, new Vector2Int(3, 2));
                
                Debug.Log($"Placed {ingredient1.ItemName} ({ingredient1.IngredientAspect}) next to " +
                         $"{ingredient2.ItemName} ({ingredient2.IngredientAspect}) to test reactions");
            }
        }
    }
}