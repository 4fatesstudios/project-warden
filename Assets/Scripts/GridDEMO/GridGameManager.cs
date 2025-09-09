using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class GridGameManager : MonoBehaviour
    {
        [Header("Grid Configuration")]
        public int gridWidth = 5;
        public int gridHeight = 5;
        public float cellSize = 1f;
        public Vector3 gridStartPosition = Vector3.zero;
        
        [Header("Visual Configuration")]
        public Material defaultCellMaterial;
        public Material highlightedCellMaterial;
        public Material occupiedCellMaterial;
        
        [Header("Gameplay")]
        public List<Ingredient> availableIngredients = new List<Ingredient>();
        public Transform ingredientContainer;
        public UnityEngine.Camera gameCamera;
        
        private GridCell[,] gridCells;
        private GridVisualizer visualizer;
        private IngredientPlacer ingredientPlacer;
        private InputActions inputActions;
        
        private Ingredient currentSelectedIngredient;
        private Vector2Int hoveredCell = Vector2Int.one * -1;
        
        public static GridGameManager Instance { get; private set; }
        
        // Public property to access the current selected ingredient
        public Ingredient CurrentSelectedIngredient => currentSelectedIngredient;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeComponents();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeComponents()
        {
            // Initialize grid
            gridCells = new GridCell[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    gridCells[x, y] = new GridCell(x, y);
                }
            }
            
            // Initialize visualizer
            visualizer = GetComponent<GridVisualizer>();
            if (visualizer == null)
                visualizer = gameObject.AddComponent<GridVisualizer>();
            
            // Initialize ingredient placer
            ingredientPlacer = GetComponent<IngredientPlacer>();
            if (ingredientPlacer == null)
                ingredientPlacer = gameObject.AddComponent<IngredientPlacer>();
            
            // Auto-assign camera if not set
            if (gameCamera == null)
            {
                gameCamera = UnityEngine.Camera.main;
                if (gameCamera == null)
                {
                    // Fallback: find any camera in the scene
                    gameCamera = FindFirstObjectByType<UnityEngine.Camera>();
                }
                
                if (gameCamera == null)
                {
                    Debug.LogError("No camera found in scene! Please assign a camera to GridGameManager.gameCamera or ensure there's a Main Camera in the scene.");
                }
                else
                {
                    Debug.Log($"Auto-assigned camera: {gameCamera.name}");
                }
            }
            
            // Setup input
            SetupInput();
        }
        
        private void SetupInput()
        {
            // Get the InputActions component attached to this GameObject
            inputActions = GetComponent<InputActions>();
            if (inputActions == null)
                inputActions = gameObject.AddComponent<InputActions>();
            
            // Ensure the InputActions is properly initialized
            inputActions.Initialize();
                
            inputActions.Enable();
            
            inputActions.Player.Click.performed += OnClick;
            inputActions.Player.Move.performed += OnMouseMove;
        }
        
        private void OnClick(InputAction.CallbackContext context)
        {
            if (currentSelectedIngredient != null && hoveredCell.x >= 0 && hoveredCell.y >= 0)
            {
                TryPlaceIngredient(currentSelectedIngredient, hoveredCell);
            }
        }
        
        private void OnMouseMove(InputAction.CallbackContext context)
        {
            Vector2 screenPosition = context.ReadValue<Vector2>();
            UpdateHoveredCell(screenPosition);
        }
        
        private void UpdateHoveredCell(Vector2 screenPosition)
        {
            if (gameCamera == null)
            {
                Debug.LogWarning("GameCamera is not assigned! Cannot process mouse input.");
                return;
            }
            
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 localPosition = transform.InverseTransformPoint(hit.point);
                Vector2Int newHoveredCell = WorldToGridPosition(localPosition);
                
                if (newHoveredCell != hoveredCell)
                {
                    hoveredCell = newHoveredCell;
                    visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);
                }
            }
        }
        
        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - gridStartPosition;
            int x = Mathf.FloorToInt(localPosition.x / cellSize);
            int y = Mathf.FloorToInt(localPosition.z / cellSize);
            
            x = Mathf.Clamp(x, 0, gridWidth - 1);
            y = Mathf.Clamp(y, 0, gridHeight - 1);
            
            return new Vector2Int(x, y);
        }
        
        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            return gridStartPosition + new Vector3(
                gridPosition.x * cellSize + cellSize * 0.5f,
                0f,
                gridPosition.y * cellSize + cellSize * 0.5f
            );
        }
        
        public bool TryPlaceIngredient(Ingredient ingredient, Vector2Int position)
        {
            if (CanPlaceIngredient(ingredient, position))
            {
                ingredientPlacer.PlaceIngredient(ingredient, position);
                MarkCellsAsOccupied(ingredient, position);
                visualizer.RefreshGrid();
                return true;
            }
            return false;
        }
        
        public bool CanPlaceIngredient(Ingredient ingredient, Vector2Int position)
        {
            // Use shape data if available, fallback to rectangle
            if (ingredient.ShapeData != null)
            {
                return CanPlaceIngredientWithShape(ingredient, position);
            }
            else
            {
                return CanPlaceIngredientRectangle(ingredient, position);
            }
        }
        
        private bool CanPlaceIngredientWithShape(Ingredient ingredient, Vector2Int position)
        {
            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);
            
            // Check bounds
            if (position.x + shapeWidth > gridWidth || position.y + shapeHeight > gridHeight)
                return false;
            
            // Check each shape cell
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    // Only check cells that are part of the ingredient's shape
                    if (shape[x, y])
                    {
                        Vector2Int cellPos = position + new Vector2Int(x, y);
                        if (cellPos.x >= gridWidth || cellPos.y >= gridHeight || 
                            gridCells[cellPos.x, cellPos.y].IsOccupied)
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        
        private bool CanPlaceIngredientRectangle(Ingredient ingredient, Vector2Int position)
        {
            // Fallback to original rectangle-based placement
            if (position.x + ingredient.GridWidth > gridWidth || 
                position.y + ingredient.GridHeight > gridHeight)
                return false;
            
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    if (gridCells[position.x + x, position.y + y].IsOccupied)
                        return false;
                }
            }
            
            return true;
        }
        
        private void MarkCellsAsOccupied(Ingredient ingredient, Vector2Int position)
        {
            // Use shape data if available, fallback to rectangle
            if (ingredient.ShapeData != null)
            {
                MarkCellsWithShape(ingredient, position);
            }
            else
            {
                MarkCellsRectangle(ingredient, position);
            }
        }
        
        private void MarkCellsWithShape(Ingredient ingredient, Vector2Int position)
        {
            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);
            
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    // Only mark cells that are part of the ingredient's shape
                    if (shape[x, y])
                    {
                        Vector2Int cellPos = position + new Vector2Int(x, y);
                        if (cellPos.x < gridWidth && cellPos.y < gridHeight)
                        {
                            gridCells[cellPos.x, cellPos.y].SetOccupied(ingredient);
                        }
                    }
                }
            }
        }
        
        private void MarkCellsRectangle(Ingredient ingredient, Vector2Int position)
        {
            // Fallback to original rectangle-based marking
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    gridCells[position.x + x, position.y + y].SetOccupied(ingredient);
                }
            }
        }
        
        public void SelectIngredient(Ingredient ingredient)
        {
            currentSelectedIngredient = ingredient;
            visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);
        }
        
        public GridCell GetCell(int x, int y)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                return gridCells[x, y];
            return null;
        }
        
        public GridCell[,] GetAllCells()
        {
            return gridCells;
        }
        
        public void ClearGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    gridCells[x, y].Clear();
                }
            }
            
            ingredientPlacer.ClearAllIngredients();
            visualizer.RefreshGrid();
        }
        
        /// <summary>
        /// Get the list of world cells that would be occupied by an ingredient at a given position
        /// </summary>
        public List<Vector2Int> GetIngredientCells(Ingredient ingredient, Vector2Int position)
        {
            var cells = new List<Vector2Int>();
            
            if (ingredient.ShapeData != null)
            {
                var shape = ingredient.GetShape();
                int shapeWidth = shape.GetLength(0);
                int shapeHeight = shape.GetLength(1);
                
                for (int x = 0; x < shapeWidth; x++)
                {
                    for (int y = 0; y < shapeHeight; y++)
                    {
                        if (shape[x, y])
                        {
                            Vector2Int cellPos = position + new Vector2Int(x, y);
                            if (cellPos.x >= 0 && cellPos.x < gridWidth && 
                                cellPos.y >= 0 && cellPos.y < gridHeight)
                            {
                                cells.Add(cellPos);
                            }
                        }
                    }
                }
            }
            else
            {
                // Fallback to rectangle
                for (int x = 0; x < ingredient.GridWidth; x++)
                {
                    for (int y = 0; y < ingredient.GridHeight; y++)
                    {
                        Vector2Int cellPos = position + new Vector2Int(x, y);
                        if (cellPos.x >= 0 && cellPos.x < gridWidth && 
                            cellPos.y >= 0 && cellPos.y < gridHeight)
                        {
                            cells.Add(cellPos);
                        }
                    }
                }
            }
            
            return cells;
        }
        
        /// <summary>
        /// Remove an ingredient from the grid at a specific position
        /// </summary>
        public bool RemoveIngredientAt(Vector2Int position)
        {
            var instance = ingredientPlacer.GetIngredientAt(position);
            if (instance != null)
            {
                // Clear all cells occupied by this ingredient
                var occupiedCells = GetIngredientCells(instance.ingredient, instance.gridPosition);
                foreach (var cellPos in occupiedCells)
                {
                    gridCells[cellPos.x, cellPos.y].Clear();
                }
                
                // Remove the visual representation
                ingredientPlacer.RemoveIngredient(position);
                visualizer.RefreshGrid();
                return true;
            }
            return false;
        }
        
        private void OnDestroy()
        {
            inputActions?.Disable();
        }
    }
}