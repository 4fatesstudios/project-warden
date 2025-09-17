using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class GridGameManager : MonoBehaviour
    {
        [Header("Grid Configuration")] public int gridWidth = 5;
        public int gridHeight = 5;
        public float cellSize = 1f;
        public Vector3 gridStartPosition = Vector3.zero;

        [Header("Visual Configuration")] public Material defaultCellMaterial;
        public Material highlightedCellMaterial;
        public Material occupiedCellMaterial;

        [Header("Obstacle System")] public bool enableObstacles = true;
        [Range(0f, 0.3f)] public float obstacleSpawnChance = 0.15f;
        public List<AspectObstacle> aspectObstacles = new List<AspectObstacle>();

        [Header("Debug Systems")] [SerializeField]
        private DebugSystemConfig debugSystemConfig;

        [SerializeField] private ObstacleSpawnDebugger obstacleSpawnDebugger;

        [Header("Proficiency Grading")] public bool enableProficiencyGrading = true;
        public ProficiencyWeights gradingWeights = new ProficiencyWeights();
        [Header("Gameplay")] public List<Ingredient> availableIngredients = new List<Ingredient>();
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
            DebugSystemConfig.LogGridState("InitializeComponents called - this will recreate the grid!");
            DebugSystemConfig.LogGridState($"Stack trace: {System.Environment.StackTrace}");

            // Initialize debug systems first
            SetupDebugSystems();

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

            // Add PlacementTester for runtime debugging (only in development builds)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (GetComponent<PlacementTester>() == null)
            {
                gameObject.AddComponent<PlacementTester>();
                DebugSystemConfig.LogTesting("Added PlacementTester component for runtime debugging");
            }
#endif

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
                    Debug.LogError(
                        "No camera found in scene! Please assign a camera to GridGameManager.gameCamera or ensure there's a Main Camera in the scene.");
                }
                else
                {
                    DebugSystemConfig.LogGridState($"Auto-assigned camera: {gameCamera.name}");
                }
            }

            // Auto-load ingredients if none are assigned
            if (availableIngredients.Count == 0)
            {
                LoadTestIngredients();
            }

            // Setup input
            SetupInput();

            // Center camera on grid after initialization
            CenterCameraOnGrid();

            // Initialize obstacle and grading systems
            if (enableObstacles)
            {
                InitializeObstacleSystems();
            }

            if (enableProficiencyGrading)
            {
                InitializeProficiencySystem();
            }
        }

        /// <summary>
        /// Setup debug systems and ensure they're available
        /// </summary>
        private void SetupDebugSystems()
        {
            // Setup DebugSystemConfig
            if (debugSystemConfig == null)
            {
                debugSystemConfig = GetComponent<DebugSystemConfig>();
                if (debugSystemConfig == null)
                {
                    debugSystemConfig = gameObject.AddComponent<DebugSystemConfig>();
                    DebugSystemConfig.LogTesting("Added DebugSystemConfig component");
                }
            }

            // Setup ObstacleSpawnDebugger
            if (obstacleSpawnDebugger == null)
            {
                obstacleSpawnDebugger = GetComponent<ObstacleSpawnDebugger>();
                if (obstacleSpawnDebugger == null)
                {
                    obstacleSpawnDebugger = gameObject.AddComponent<ObstacleSpawnDebugger>();
                    DebugSystemConfig.LogObstacleSpawn("Added ObstacleSpawnDebugger component");
                }
            }
        }

        private void LoadTestIngredients()
        {
            DebugSystemConfig.LogTesting("Loading test ingredients from Resources...");

            // Try to load from different possible locations
            string[] possiblePaths =
            {
                "Items/Ingredients/TestIngredient1",
                "TestIngredients/Ice Crystal",
                "TestIngredients/Life Bloom"
            };

            foreach (string path in possiblePaths)
            {
                var ingredient = Resources.Load<Ingredient>(path);
                if (ingredient != null)
                {
                    availableIngredients.Add(ingredient);
                    DebugSystemConfig.LogTesting($"Loaded ingredient: {ingredient.ItemName}");
                }
                else
                {
                    DebugSystemConfig.LogTesting($"Could not load ingredient from: {path}");
                }
            }

            DebugSystemConfig.LogTesting($"Total ingredients loaded: {availableIngredients.Count}");
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

        /// <summary>
        /// Centers the camera on the grid with an optimal viewing angle and distance
        /// </summary>
        [ContextMenu("Center Camera on Grid")]
        public void CenterCameraOnGrid()
        {
            if (gameCamera == null)
            {
                Debug.LogWarning("Cannot center camera: gameCamera is null");
                return;
            }

            // Calculate grid center in world space
            Vector3 gridCenter = CalculateGridCenter();

            // Calculate optimal camera distance based on grid size - much further back
            float gridDiagonal = Mathf.Sqrt(gridWidth * gridWidth + gridHeight * gridHeight) * cellSize;
            float cameraDistance = Mathf.Max(gridDiagonal * 1.2f, 12f); // Increased distance and minimum

            // Position camera at an angle above and behind the grid center - further back
            Vector3 cameraOffset = new Vector3(
                gridCenter.x,
                cameraDistance * 0.9f, // Height above grid (increased)
                gridCenter.z - cameraDistance * 0.8f // Much further back from center
            );

            gameCamera.transform.position = cameraOffset;

            // Look at the grid center
            gameCamera.transform.LookAt(gridCenter);

            Debug.Log($"Camera positioned further back - Grid center: {gridCenter}, Camera position: {cameraOffset}");
        }

        /// <summary>
        /// Calculates the world space center of the grid
        /// </summary>
        private Vector3 CalculateGridCenter()
        {
            Vector2Int centerGridPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
            Vector3 centerWorldPos = GridToWorldPosition(centerGridPos);

            // Adjust for even-sized grids
            if (gridWidth % 2 == 0)
                centerWorldPos.x -= cellSize * 0.5f;
            if (gridHeight % 2 == 0)
                centerWorldPos.z -= cellSize * 0.5f;

            return centerWorldPos;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            // Only allow placement if mouse is currently over the grid and game has focus
            if (isMouseOverGrid && isGameFocused && currentSelectedIngredient != null && hoveredCell.x >= 0 &&
                hoveredCell.y >= 0)
            {
                TryPlaceIngredient(currentSelectedIngredient, hoveredCell);
            }
            else if (!isMouseOverGrid)
            {
                Debug.Log("🚫 Cannot place ingredient - mouse is not over the grid");
            }
            else if (!isGameFocused)
            {
                Debug.Log("🚫 Cannot place ingredient - game does not have focus");
            }
        }

        private void OnMouseMove(InputAction.CallbackContext context)
        {
            Vector2 screenPosition = context.ReadValue<Vector2>();
            UpdateHoveredCell(screenPosition);
        }

        private bool isMouseOverGrid = false;
        private bool isGameFocused = true;

        private void OnApplicationFocus(bool hasFocus)
        {
            isGameFocused = hasFocus;
            if (!hasFocus)
            {
                // Clear hover preview when game loses focus
                isMouseOverGrid = false;
                hoveredCell = new Vector2Int(-1, -1);
                visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);
            }
        }

        private void UpdateHoveredCell(Vector2 screenPosition)
        {
            if (gameCamera == null)
            {
                Debug.LogWarning("GameCamera is not assigned! Cannot process mouse input.");
                return;
            }

            if (!isGameFocused)
            {
                // Game doesn't have focus, clear any previews
                if (isMouseOverGrid)
                {
                    isMouseOverGrid = false;
                    hoveredCell = new Vector2Int(-1, -1);
                    visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);
                }

                return;
            }

            Ray ray = gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 localPosition = transform.InverseTransformPoint(hit.point);
                Vector2Int newHoveredCell = WorldToGridPosition(localPosition);

                // Check if the cell is within valid grid bounds
                bool isWithinBounds = newHoveredCell.x >= 0 && newHoveredCell.x < gridWidth &&
                                      newHoveredCell.y >= 0 && newHoveredCell.y < gridHeight;

                if (isWithinBounds)
                {
                    isMouseOverGrid = true;
                    if (newHoveredCell != hoveredCell)
                    {
                        hoveredCell = newHoveredCell;
                        visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);

                        // Show enhanced debug info for the hovered position and ingredient shape
                        if (currentSelectedIngredient != null)
                        {
                            DebugIngredientShapeCollision(currentSelectedIngredient, hoveredCell);
                        }

                        // Verify grid state on mouse hover (if enabled - can be performance intensive)
                        if (verifyOnMouseHover)
                        {
                            VerifyAllPlacedIngredientsQuiet();
                        }
                    }
                }
                else
                {
                    // Mouse hit the grid plane but outside valid bounds
                    if (isMouseOverGrid)
                    {
                        isMouseOverGrid = false;
                        hoveredCell = new Vector2Int(-1, -1);
                        visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);
                    }
                }
            }
            else
            {
                // No raycast hit on grid, clear preview
                if (isMouseOverGrid)
                {
                    isMouseOverGrid = false;
                    hoveredCell = new Vector2Int(-1, -1);
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
            DebugSystemConfig.LogIngredientPlacement(
                $"=== TryPlaceIngredient: {ingredient.ItemName} at {position} ===");

            bool canPlace = CanPlaceIngredient(ingredient, position);
            DebugSystemConfig.LogIngredientPlacement($"CanPlaceIngredient result: {canPlace}");

            if (canPlace)
            {
                // Check obstacle interactions for each cell the ingredient will occupy
                bool obstacleCheckPassed = true;
                var cellsToOccupy = GetIngredientCells(ingredient, position);

                foreach (var cellPos in cellsToOccupy)
                {
                    if (!HandleObstacleInteraction(ingredient, cellPos))
                    {
                        obstacleCheckPassed = false;
                        break;
                    }
                }

                if (!obstacleCheckPassed)
                {
                    DebugSystemConfig.LogObstacleInteraction(
                        $"Obstacle interaction failed for {ingredient.ItemName} at {position}");
                    return false;
                }

                DebugSystemConfig.LogIngredientPlacement(
                    $"Placement approved! Delegating to IngredientPlacer for unified placement");

                // Let IngredientPlacer handle both visual placement AND grid occupancy as a unified operation
                ingredientPlacer.PlaceIngredient(ingredient, position);

                // Simple verification that IngredientPlacer did its job correctly
                bool placementSuccess = VerifyIngredientPlacement(ingredient, position);
                if (placementSuccess)
                {
                    DebugSystemConfig.LogIngredientPlacement(
                        $"SUCCESS - {ingredient.ItemName} placed and verified at {position}");

                    // Clear highlights after successful placement
                    ClearIngredientSelection();

                    visualizer.RefreshGrid();
                    DebugGridStateAfterPlacement(ingredient, position);

                    // Calculate proficiency grade if enabled
                    if (enableProficiencyGrading)
                    {
                        var grade = CalculateCurrentProficiency();
                        DebugSystemConfig.LogProficiencyGrading(
                            $"Current Grade: {grade.gradeLevel} ({grade.overallScore:F1}%)");
                    }

                    // Check for recipe matches after placing ingredient
                    CheckForRecipeMatches();

                    return true;
                }
                else
                {
                    DebugSystemConfig.LogErrorRecovery(
                        $"FAILED - IngredientPlacer could not complete placement for {ingredient.ItemName} at {position}");
                    return false;
                }
            }
            else
            {
                DebugSystemConfig.LogCollisionDetection(
                    $"Cannot place {ingredient.ItemName} at {position} - collision detected!");
                // Let's see WHY it failed
                DebugWhyPlacementFailed(ingredient, position);
            }

            return false;
        }

        private void DebugWhyPlacementFailed(Ingredient ingredient, Vector2Int position)
        {
            Debug.Log($"🔍 Analyzing why placement failed for {ingredient.ItemName} at {position}:");

            // Check bounds
            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            if (position.x + shapeWidth > gridWidth || position.y + shapeHeight > gridHeight)
            {
                Debug.Log(
                    $"  ❌ BOUNDS: Shape extends beyond grid. Shape size: {shapeWidth}x{shapeHeight}, Grid size: {gridWidth}x{gridHeight}");
                return;
            }

            if (position.x < 0 || position.y < 0)
            {
                Debug.Log($"  ❌ BOUNDS: Negative position not allowed");
                return;
            }

            // Check individual cells
            Debug.Log($"  ✅ BOUNDS: OK. Checking individual cells:");
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    if (shape[x, y])
                    {
                        Vector2Int cellPos = position + new Vector2Int(x, y);
                        bool isOccupied = gridCells[cellPos.x, cellPos.y].IsOccupied;
                        string occupant = gridCells[cellPos.x, cellPos.y].OccupiedByIngredient?.ItemName ?? "None";

                        if (isOccupied)
                        {
                            Debug.Log($"  ❌ COLLISION: Cell ({cellPos.x},{cellPos.y}) occupied by {occupant}");
                        }
                        else
                        {
                            Debug.Log($"  ✅ OK: Cell ({cellPos.x},{cellPos.y}) is free");
                        }
                    }
                }
            }
        }

        private void DebugGridStateAfterPlacement(Ingredient ingredient, Vector2Int position)
        {
            Debug.Log($"Grid state after placing {ingredient.ItemName} at {position}:");
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
                        bool isOccupied = gridCells[cellPos.x, cellPos.y].IsOccupied;
                        string occupant = gridCells[cellPos.x, cellPos.y].OccupiedByIngredient?.ItemName ?? "None";
                        Debug.Log(
                            $"  Shape cell ({x},{y}) -> Grid cell ({cellPos.x},{cellPos.y}): Occupied = {isOccupied}, By = {occupant}");
                    }
                }
            }
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
            DebugSystemConfig.LogCollisionDetection(
                $"CanPlaceIngredientWithShape: {ingredient.ItemName} at {position}");

            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            DebugSystemConfig.LogCollisionDetection($"Shape dimensions: {shapeWidth}x{shapeHeight}");

            // Check bounds
            if (position.x + shapeWidth > gridWidth || position.y + shapeHeight > gridHeight)
            {
                DebugSystemConfig.LogCollisionDetection(
                    $"Out of bounds: position {position} + shape ({shapeWidth},{shapeHeight}) exceeds grid ({gridWidth},{gridHeight})");
                return false;
            }

            // Debug: Print shape pattern
            DebugSystemConfig.LogCollisionDetection("Shape pattern:");
            for (int y = shapeHeight - 1; y >= 0; y--)
            {
                string row = $"Y={y}: ";
                for (int x = 0; x < shapeWidth; x++)
                {
                    row += shape[x, y] ? "[#]" : "[ ]";
                }

                DebugSystemConfig.LogCollisionDetection(row);
            }

            // Check each shape cell
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    // Only check cells that are part of the ingredient's shape
                    if (shape[x, y])
                    {
                        Vector2Int cellPos = position + new Vector2Int(x, y);

                        // Bounds check for this specific cell
                        if (cellPos.x >= gridWidth || cellPos.y >= gridHeight || cellPos.x < 0 || cellPos.y < 0)
                        {
                            DebugSystemConfig.LogCollisionDetection(
                                $"Shape cell ({x},{y}) -> Grid cell ({cellPos.x},{cellPos.y}) is out of bounds");
                            return false;
                        }

                        // Check if occupied
                        bool isOccupied = gridCells[cellPos.x, cellPos.y].IsOccupied;
                        string occupant = gridCells[cellPos.x, cellPos.y].OccupiedByIngredient?.ItemName ?? "None";

                        DebugSystemConfig.LogCollisionDetection(
                            $"Shape cell ({x},{y}) -> Grid cell ({cellPos.x},{cellPos.y}): Occupied = {isOccupied}, By = {occupant}");

                        if (isOccupied)
                        {
                            DebugSystemConfig.LogCollisionDetection(
                                $"COLLISION: Cell ({cellPos.x},{cellPos.y}) is occupied by {occupant}");
                            return false;
                        }
                    }
                }
            }

            DebugSystemConfig.LogCollisionDetection("No collisions detected - placement allowed");
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
            DebugSystemConfig.LogIngredientPlacement($"MarkCellsAsOccupied: {ingredient.ItemName} at {position}");

            // Use shape data if available, fallback to rectangle
            if (ingredient.ShapeData != null)
            {
                DebugSystemConfig.LogIngredientPlacement($"Using shape-based marking for {ingredient.ItemName}");
                MarkCellsWithShape(ingredient, position);
            }
            else
            {
                DebugSystemConfig.LogIngredientPlacement($"Using rectangle-based marking for {ingredient.ItemName}");
                MarkCellsRectangle(ingredient, position);
            }

            // Verify that cells were actually marked
            DebugSystemConfig.LogIngredientPlacement(
                $"Verification: Checking if cells were properly marked for {ingredient.ItemName}");
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
                        if (cellPos.x < gridWidth && cellPos.y < gridHeight)
                        {
                            bool isOccupied = gridCells[cellPos.x, cellPos.y].IsOccupied;
                            string occupant = gridCells[cellPos.x, cellPos.y].OccupiedByIngredient?.ItemName ??
                                              "None";
                            DebugSystemConfig.LogIngredientPlacement(
                                $"  VERIFY: Cell ({cellPos.x},{cellPos.y}) -> Occupied = {isOccupied}, By = {occupant}");

                            if (!isOccupied)
                            {
                                DebugSystemConfig.LogErrorRecovery(
                                    $"  MARKING FAILED: Cell ({cellPos.x},{cellPos.y}) should be occupied but isn't!");
                            }
                        }
                    }
                }
            }
        }


        private void MarkCellsWithShape(Ingredient ingredient, Vector2Int position)
        {
            DebugSystemConfig.LogIngredientPlacement($"MarkCellsWithShape: {ingredient.ItemName} at {position}");

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
                            DebugSystemConfig.LogIngredientPlacement(
                                $"  Marking cell ({cellPos.x},{cellPos.y}) as occupied by {ingredient.ItemName}");
                            gridCells[cellPos.x, cellPos.y].SetOccupied(ingredient);
                        }
                        else
                        {
                            DebugSystemConfig.LogErrorRecovery(
                                $"  Trying to mark out-of-bounds cell ({cellPos.x},{cellPos.y})!");
                        }
                    }
                }
            }
        }

        private void MarkCellsRectangle(Ingredient ingredient, Vector2Int position)
        {
            DebugSystemConfig.LogIngredientPlacement(
                $"MarkCellsRectangle: {ingredient.ItemName} at {position}, size {ingredient.GridWidth}x{ingredient.GridHeight}");

            // Fallback to original rectangle-based marking
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    Vector2Int cellPos = position + new Vector2Int(x, y);
                    DebugSystemConfig.LogIngredientPlacement(
                        $"  Marking rectangle cell ({cellPos.x},{cellPos.y}) as occupied by {ingredient.ItemName}");
                    gridCells[cellPos.x, cellPos.y].SetOccupied(ingredient);
                }
            }
        }

        public void SelectIngredient(Ingredient ingredient)
        {
            currentSelectedIngredient = ingredient;
            visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);

            // Verify grid state when selecting ingredients (if enabled)
            if (verifyOnIngredientSelection)
            {
                string ingredientName = ingredient?.ItemName ?? "None";
                DebugSystemConfig.LogTesting($"Verifying grid state on ingredient selection: {ingredientName}");
                VerifyAllPlacedIngredientsQuiet();
            }
        }

        /// <summary>
        /// Clear current ingredient selection and any highlight previews
        /// </summary>
        public void ClearIngredientSelection()
        {
            Debug.Log("🔄 Clearing ingredient selection and highlights");
            currentSelectedIngredient = null;
            hoveredCell = Vector2Int.one * -1; // Reset to invalid position

            // Clear any visual highlights
            if (visualizer != null)
            {
                visualizer.ClearHighlights();

                // Force refresh the entire grid to ensure all cells reset to white
                visualizer.RefreshGrid();
                Debug.Log("🔄 Grid refreshed - all cells should now be white (empty) or white (occupied)");
            }
        }

        /// <summary>
        /// Verifies that an ingredient placement actually marked all required cells as occupied
        /// </summary>
        /// <param name="ingredient">The ingredient that was placed</param>
        /// <param name="position">The position where it was placed</param>
        /// <returns>True if all required cells are properly marked as occupied</returns>
        private bool VerifyIngredientPlacement(Ingredient ingredient, Vector2Int position)
        {
            Debug.Log($"🔍 VerifyIngredientPlacement: Checking {ingredient.ItemName} at {position}");

            var expectedCells = GetIngredientCells(ingredient, position);
            int totalExpectedCells = expectedCells.Count;
            int properlyOccupiedCells = 0;

            foreach (var cellPos in expectedCells)
            {
                var cell = GetCell(cellPos.x, cellPos.y);
                if (cell != null)
                {
                    bool isOccupied = cell.IsOccupied;
                    string occupantName = cell.OccupiedByIngredient?.ItemName ?? "None";
                    bool occupiedByCorrectIngredient = cell.OccupiedByIngredient == ingredient;

                    Debug.Log(
                        $"🔍 Cell ({cellPos.x},{cellPos.y}): Occupied={isOccupied}, By={occupantName}, CorrectIngredient={occupiedByCorrectIngredient}");

                    if (isOccupied && occupiedByCorrectIngredient)
                    {
                        properlyOccupiedCells++;
                    }
                    else if (!isOccupied)
                    {
                        Debug.LogError(
                            $"🚨 VERIFICATION ERROR: Cell ({cellPos.x},{cellPos.y}) should be occupied by {ingredient.ItemName} but is empty!");
                    }
                    else if (isOccupied && !occupiedByCorrectIngredient)
                    {
                        Debug.LogError(
                            $"🚨 VERIFICATION ERROR: Cell ({cellPos.x},{cellPos.y}) occupied by wrong ingredient: {occupantName} instead of {ingredient.ItemName}!");
                    }
                }
                else
                {
                    Debug.LogError(
                        $"🚨 VERIFICATION ERROR: Could not get cell at ({cellPos.x},{cellPos.y}) - out of bounds?");
                }
            }

            bool verificationPassed = (properlyOccupiedCells == totalExpectedCells);

            Debug.Log(
                $"🔍 Verification result: {properlyOccupiedCells}/{totalExpectedCells} cells properly occupied = {(verificationPassed ? "PASS" : "FAIL")}");

            return verificationPassed;
        }

        /// <summary>
        /// Quick verification method that can be called from context menu
        /// </summary>
        [ContextMenu("Verify All Placed Ingredients")]
        public void VerifyAllPlacedIngredients()
        {
            Debug.Log("🔍 === VERIFYING ALL PLACED INGREDIENTS ===");

            var placedIngredients = ingredientPlacer.GetAllPlacedIngredients();
            int totalIngredients = placedIngredients.Count;
            int verifiedIngredients = 0;

            foreach (var instance in placedIngredients)
            {
                bool verified = VerifyIngredientPlacement(instance.ingredient, instance.gridPosition);
                if (verified)
                {
                    verifiedIngredients++;
                    Debug.Log($"✅ {instance.ingredient.ItemName} at {instance.gridPosition} verification PASSED");
                }
                else
                {
                    Debug.LogError($"❌ {instance.ingredient.ItemName} at {instance.gridPosition} verification FAILED");
                }
            }

            Debug.Log(
                $"🔍 Verification complete: {verifiedIngredients}/{totalIngredients} ingredients properly occupy their cells");

            if (verifiedIngredients != totalIngredients)
            {
                Debug.LogError("🚨 Some ingredients failed verification! There may be a bug in the occupancy system.");
            }
        }

        /// <summary>
        /// Quiet verification that only logs issues, not successes (for frequent checking)
        /// </summary>
        private void VerifyAllPlacedIngredientsQuiet()
        {
            var placedIngredients = ingredientPlacer?.GetAllPlacedIngredients();
            if (placedIngredients == null || placedIngredients.Count == 0)
                return;

            int failedIngredients = 0;

            foreach (var instance in placedIngredients)
            {
                bool verified = VerifyIngredientPlacementQuiet(instance.ingredient, instance.gridPosition);
                if (!verified)
                {
                    failedIngredients++;
                    Debug.LogWarning(
                        $"🔍 QUIET CHECK: {instance.ingredient.ItemName} at {instance.gridPosition} verification FAILED");
                }
            }

            if (failedIngredients > 0)
            {
                Debug.LogWarning(
                    $"🔍 QUIET CHECK: {failedIngredients}/{placedIngredients.Count} ingredients failed verification");
            }
        }

        /// <summary>
        /// Quiet version of VerifyIngredientPlacement that doesn't log success details
        /// </summary>
        private bool VerifyIngredientPlacementQuiet(Ingredient ingredient, Vector2Int position)
        {
            var expectedCells = GetIngredientCells(ingredient, position);
            int totalExpectedCells = expectedCells.Count;
            int properlyOccupiedCells = 0;

            foreach (var cellPos in expectedCells)
            {
                var cell = GetCell(cellPos.x, cellPos.y);
                if (cell != null)
                {
                    bool isOccupied = cell.IsOccupied;
                    bool occupiedByCorrectIngredient = cell.OccupiedByIngredient == ingredient;

                    if (isOccupied && occupiedByCorrectIngredient)
                    {
                        properlyOccupiedCells++;
                    }
                    else if (!isOccupied)
                    {
                        Debug.LogWarning(
                            $"🔍 Cell ({cellPos.x},{cellPos.y}) should be occupied by {ingredient.ItemName} but is empty!");
                    }
                    else if (isOccupied && !occupiedByCorrectIngredient)
                    {
                        Debug.LogWarning(
                            $"🔍 Cell ({cellPos.x},{cellPos.y}) occupied by wrong ingredient: {cell.OccupiedByIngredient?.ItemName} instead of {ingredient.ItemName}!");
                    }
                }
            }

            return (properlyOccupiedCells == totalExpectedCells);
        }

        /// <summary>
        /// Comprehensive occupancy audit for debugging
        /// </summary>
        [ContextMenu("Full Occupancy Audit")]
        public void FullOccupancyAudit()
        {
            Debug.Log("🔍 === FULL OCCUPANCY AUDIT ===");

            // Count occupied vs empty cells
            int occupiedCells = 0;
            int emptyCells = 0;
            int inconsistentCells = 0;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = GetCell(x, y);
                    if (cell != null)
                    {
                        if (cell.IsOccupied)
                        {
                            occupiedCells++;

                            // Check if the occupying ingredient actually claims this cell
                            if (cell.OccupiedByIngredient != null)
                            {
                                var placedIngredients = ingredientPlacer.GetAllPlacedIngredients();
                                bool foundMatchingIngredient = false;

                                foreach (var instance in placedIngredients)
                                {
                                    if (instance.ingredient == cell.OccupiedByIngredient)
                                    {
                                        var expectedCells =
                                            GetIngredientCells(instance.ingredient, instance.gridPosition);
                                        if (expectedCells.Contains(new Vector2Int(x, y)))
                                        {
                                            foundMatchingIngredient = true;
                                            break;
                                        }
                                    }
                                }

                                if (!foundMatchingIngredient)
                                {
                                    inconsistentCells++;
                                    Debug.LogError(
                                        $"🚨 INCONSISTENT: Cell ({x},{y}) claims to be occupied by {cell.OccupiedByIngredient.ItemName} but no matching placed ingredient found!");
                                }
                            }
                            else
                            {
                                inconsistentCells++;
                                Debug.LogError(
                                    $"🚨 INCONSISTENT: Cell ({x},{y}) is marked occupied but has no OccupiedByIngredient!");
                            }
                        }
                        else
                        {
                            emptyCells++;
                        }
                    }
                }
            }

            int totalCells = gridWidth * gridHeight;
            float occupancyPercent = (occupiedCells * 100f) / totalCells;

            Debug.Log($"🔍 Audit results:");
            Debug.Log($"  📊 Total cells: {totalCells}");
            Debug.Log($"  ✅ Occupied cells: {occupiedCells} ({occupancyPercent:F1}%)");
            Debug.Log($"  ⬜ Empty cells: {emptyCells}");
            Debug.Log($"  ⚠️ Inconsistent cells: {inconsistentCells}");

            if (inconsistentCells > 0)
            {
                Debug.LogError("🚨 AUDIT FAILED: Found inconsistent cell states! The occupancy system has bugs.");
            }
            else
            {
                Debug.Log("✅ AUDIT PASSED: All cell states are consistent.");
            }
        }

        /// <summary>
        /// Debug method to check grid state
        /// </summary>
        [ContextMenu("Debug Grid State")]
        public void DebugGridState()
        {
            Debug.Log("=== GRID STATE DEBUG ===");
            for (int y = gridHeight - 1; y >= 0; y--) // Print top to bottom
            {
                string row = $"Row {y}: ";
                for (int x = 0; x < gridWidth; x++)
                {
                    string cell = gridCells[x, y].IsOccupied
                        ? $"[{gridCells[x, y].OccupiedByIngredient?.ItemName?.Substring(0, 1) ?? "?"}]"
                        : "[ ]";
                    row += cell;
                }

                Debug.Log(row);
            }

            Debug.Log("========================");
        }

        /// <summary>
        /// Debug method to test collision detection specifically
        /// </summary>
        [ContextMenu("Test Collision Detection")]
        public void TestCollisionDetection()
        {
            if (currentSelectedIngredient == null)
            {
                Debug.LogWarning("No ingredient selected for collision test");
                return;
            }

            Debug.Log($"=== TESTING COLLISION FOR {currentSelectedIngredient.ItemName} ===");

            // Test a few positions
            Vector2Int[] testPositions = { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) };

            foreach (var pos in testPositions)
            {
                bool canPlace = CanPlaceIngredient(currentSelectedIngredient, pos);
                Debug.Log($"Position {pos}: CanPlace = {canPlace}");

                if (pos.x < gridWidth && pos.y < gridHeight)
                {
                    bool isOccupied = gridCells[pos.x, pos.y].IsOccupied;
                    string occupant = gridCells[pos.x, pos.y].OccupiedByIngredient?.ItemName ?? "None";
                    Debug.Log($"  Cell {pos} occupied: {isOccupied}, by: {occupant}");
                }
            }
        }

        /// <summary>
        /// Comprehensive collision detection test
        /// </summary>
        [ContextMenu("Test Collision Step by Step")]
        public void TestCollisionStepByStep()
        {
            Debug.Log("=== COLLISION DETECTION STEP-BY-STEP TEST ===");

            if (availableIngredients.Count < 2)
            {
                Debug.LogError("Need at least 2 ingredients in availableIngredients list to test collision");
                return;
            }

            var ingredient1 = availableIngredients[0];
            var ingredient2 = availableIngredients[1];

            Debug.Log($"Testing with Ingredient 1: {ingredient1.ItemName}");
            Debug.Log($"Testing with Ingredient 2: {ingredient2.ItemName}");

            // Clear grid first
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    gridCells[x, y].Clear();
                }
            }

            // Test position for both ingredients
            Vector2Int testPos = new Vector2Int(1, 1);

            Debug.Log($"\n--- STEP 1: Place first ingredient at {testPos} ---");
            bool result1 = TryPlaceIngredient(ingredient1, testPos);
            Debug.Log($"Result: {result1}");

            Debug.Log($"\n--- STEP 2: Current grid state ---");
            DebugGridState();

            Debug.Log($"\n--- STEP 3: Try to place second ingredient at same position {testPos} ---");
            bool result2 = TryPlaceIngredient(ingredient2, testPos);
            Debug.Log($"Result: {result2}");

            if (result2)
            {
                Debug.LogError("❌ COLLISION DETECTION FAILED! Second ingredient was placed despite collision!");
                Debug.Log("This confirms the bug - investigating further...");

                // Let's check what the collision detection thinks about this
                Debug.Log($"\n--- STEP 4: Manual collision check ---");
                bool shouldCollide = CanPlaceIngredient(ingredient2, testPos);
                Debug.Log($"CanPlaceIngredient says: {shouldCollide} (should be FALSE)");

            }
            else
            {
                Debug.Log("✅ Collision detection working correctly!");
            }

            Debug.Log($"\n--- STEP 5: Final grid state ---");
            DebugGridState();

            Debug.Log("=== TEST COMPLETE ===");
        }

        /// <summary>
        /// Toggle collision detection debug logging on/off
        /// </summary>
        [ContextMenu("Toggle Collision Debug Logging")]
        public void ToggleCollisionDebugLogging()
        {
            DebugSystemConfig.ToggleCollisionDetectionDebug();
        }

        /// <summary>
        /// Toggle continuous verification on/off
        /// </summary>
        [ContextMenu("Toggle Continuous Verification")]
        public void ToggleContinuousVerification()
        {
            enableContinuousVerification = !enableContinuousVerification;
            Debug.Log(
                $"Continuous verification: {(enableContinuousVerification ? "ENABLED" : "DISABLED")} (Interval: {verificationInterval}s)");
        }

        /// <summary>
        /// Set verification to check every frame (intensive debugging)
        /// </summary>
        [ContextMenu("Enable Frame-by-Frame Verification")]
        public void EnableFrameByFrameVerification()
        {
            enableContinuousVerification = true;
            verificationInterval = 0.0f;
            verifyOnMouseHover = true;
            Debug.LogWarning(
                "🔍 FRAME-BY-FRAME VERIFICATION ENABLED - This is very intensive! Only use for debugging.");
        }

        /// <summary>
        /// Set verification back to normal intervals
        /// </summary>
        [ContextMenu("Reset Normal Verification")]
        public void ResetNormalVerification()
        {
            enableContinuousVerification = true;
            verificationInterval = 2.0f;
            verifyOnMouseHover = false;
            Debug.Log("🔍 Verification reset to normal intervals (2 seconds)");
        }

        // Debug settings now handled by DebugSystemConfig

        [Header("Continuous Verification")] [SerializeField]
        private bool enableContinuousVerification = true;

        [SerializeField] private float verificationInterval = 2.0f; // Check every 2 seconds
        [SerializeField] private bool verifyOnIngredientSelection = true;
        [SerializeField] private bool verifyOnMouseHover = false; // Can be enabled for intensive debugging

        private float lastVerificationTime = 0f;

        [ContextMenu("Test Shape Orientation")]
        public void TestShapeOrientation()
        {
            if (currentSelectedIngredient == null)
            {
                Debug.LogWarning("No ingredient selected for orientation test");
                return;
            }

            var shape = currentSelectedIngredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            Debug.Log($"=== SHAPE ORIENTATION TEST FOR {currentSelectedIngredient.ItemName} ===");
            Debug.Log($"Shape dimensions: {shapeWidth}x{shapeHeight}");
            Debug.Log(
                $"Grid Width: {currentSelectedIngredient.GridWidth}, Grid Height: {currentSelectedIngredient.GridHeight}");

            // Print the shape as it appears in game coordinates (looking down from above)
            Debug.Log("Shape in 3D game coordinates (Y=0 at near edge, viewed from above):");
            for (int y = shapeHeight - 1; y >= 0; y--) // Print top to bottom like how you'd see it in 3D
            {
                string row = $"Y={y}: ";
                for (int x = 0; x < shapeWidth; x++)
                {
                    row += shape[x, y] ? "[#]" : "[ ]";
                }

                Debug.Log(row);
            }

            Debug.Log("Shape data structure (raw array access):");
            for (int y = 0; y < shapeHeight; y++)
            {
                string row = $"Array[x,{y}]: ";
                for (int x = 0; x < shapeWidth; x++)
                {
                    row += shape[x, y] ? "[#]" : "[ ]";
                }

                Debug.Log(row);
            }

            // Check ShapeData if available
            if (currentSelectedIngredient.ShapeData != null)
            {
                Debug.Log("Active cells from ShapeData:");
                foreach (var cell in currentSelectedIngredient.ShapeData.ActiveCells)
                {
                    Debug.Log($"  Active cell: ({cell.x}, {cell.y})");
                }

                Debug.Log($"Template: {currentSelectedIngredient.ShapeData.Template}");
            }
            else
            {
                Debug.Log("No ShapeData available - using rectangle fallback");
            }

            Debug.Log("=== TEST COMPLETE ===");
            Debug.Log("The ASE now displays the grid with Y=0 at the bottom to match this 3D view!");
        }

        /// <summary>
        /// Create a test L-shaped ingredient for orientation testing
        /// </summary>
        [ContextMenu("Create Test L-Shape")]
        public void CreateTestLShape()
        {
            if (currentSelectedIngredient == null)
            {
                Debug.LogWarning("No ingredient selected for L-shape test");
                return;
            }

            // Create a 3x3 L-shape for testing
            bool[,] lShape = new bool[3, 3];

            // L-shape pattern: bottom row (Y=0) and left column
            lShape[0, 0] = true; // bottom-left
            lShape[1, 0] = true; // bottom-center
            lShape[2, 0] = true; // bottom-right
            lShape[0, 1] = true; // middle-left
            lShape[0, 2] = true; // top-left

            currentSelectedIngredient.SetShape(lShape);

            Debug.Log("Created L-shape test pattern:");
            Debug.Log("Y=2: [#][ ][ ]");
            Debug.Log("Y=1: [#][ ][ ]");
            Debug.Log("Y=0: [#][#][#]");
            Debug.Log("This should now appear the same way in both ASE and game!");

            // Update visual highlight
            if (hoveredCell != null)
            {
                visualizer.UpdateHighlight(hoveredCell, currentSelectedIngredient);
            }
        }

        /// <summary>
        /// Test method to demonstrate ingredient effect interactions
        /// </summary>
        [ContextMenu("Test Ingredient Effect Interactions")]
        public void TestIngredientEffectInteractions()
        {
            Debug.Log("🎨 === TESTING INGREDIENT EFFECT INTERACTIONS ===");

            if (availableIngredients.Count < 2)
            {
                Debug.LogError("Need at least 2 ingredients to test interactions");
                return;
            }

            // Clear the grid first
            ClearGrid();

            var ingredient1 = availableIngredients[0];
            var ingredient2 = availableIngredients.Count > 1 ? availableIngredients[1] : availableIngredients[0];

            Debug.Log($"Testing with:");
            Debug.Log($"  Ingredient 1: {ingredient1.ItemName} (Effects: {ingredient1.HasEffects()})");
            Debug.Log($"  Ingredient 2: {ingredient2.ItemName} (Effects: {ingredient2.HasEffects()})");

            // Place ingredients next to each other
            Vector2Int pos1 = new Vector2Int(2, 2);
            Vector2Int pos2 = new Vector2Int(3, 2);

            Debug.Log($"Placing {ingredient1.ItemName} at {pos1}");
            bool success1 = TryPlaceIngredient(ingredient1, pos1);

            if (success1)
            {
                Debug.Log($"Placing {ingredient2.ItemName} at {pos2} (adjacent to first ingredient)");
                bool success2 = TryPlaceIngredient(ingredient2, pos2);

                if (success2)
                {
                    Debug.Log("✅ Both ingredients placed successfully!");

                    if (ingredient1.HasEffects() && ingredient2.HasEffects())
                    {
                        var similarEffects = ingredient1.GetSimilarEffectsTo(ingredient2);
                        if (similarEffects.Count > 0)
                        {
                            Debug.Log($"Effect similarity: SIMILAR ({similarEffects.Count} shared effects)");
                            foreach (var (thisEffect, otherEffect) in similarEffects)
                            {
                                Debug.Log($"  - Shared: {thisEffect.GetType().Name}");
                            }
                        }
                        else
                        {
                            Debug.Log("Effect similarity: DIFFERENT");
                        }

                        Debug.Log("You should see particle effects between the ingredients!");
                    }
                    else if (ingredient1.HasEffects() || ingredient2.HasEffects())
                    {
                        Debug.Log("One ingredient has effects - should see neutral interaction");
                    }
                    else
                    {
                        Debug.Log("Neither ingredient has effects - no particle effects expected");
                    }
                }
                else
                {
                    Debug.LogError("Failed to place second ingredient");
                }
            }
            else
            {
                Debug.LogError("Failed to place first ingredient");
            }

            Debug.Log("🎨 === TEST COMPLETE ===");
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

        [ContextMenu("Test Force Place Ingredient")]
        public void TestForcePlaceIngredient()
        {
            if (currentSelectedIngredient == null)
            {
                Debug.LogError("No ingredient selected for testing!");
                return;
            }

            Vector2Int testPosition = new Vector2Int(2, 2);
            Debug.Log(
                $"🧪 FORCE PLACING {currentSelectedIngredient.ItemName} at {testPosition} (bypassing collision detection)");

            // Force place ignoring collisions
            ingredientPlacer.PlaceIngredient(currentSelectedIngredient, testPosition);
            MarkCellsAsOccupied(currentSelectedIngredient, testPosition);
            visualizer.RefreshGrid();

            Debug.Log("🧪 Force placement complete. Check if collision detection now works!");
        }

        [ContextMenu("Force Clear Grid")]
        public void ForceClearGrid()
        {
            Debug.LogWarning("🧹 ForceClearGrid() called via context menu!");
            ClearGrid();
        }

        /// <summary>
        /// Debug method to check current state of all cell colors
        /// </summary>
        [ContextMenu("Debug All Cell Colors")]
        public void DebugAllCellColors()
        {
            Debug.Log("🎨 === DEBUGGING ALL CELL COLORS ===");

            int totalCells = gridWidth * gridHeight;
            int whiteCells = 0;
            int occupiedCells = 0;
            int highlightedCells = 0;
            int unknownCells = 0;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = gridCells[x, y];

                    switch (cell.VisualState)
                    {
                        case CellVisualState.Empty:
                            if (cell.CellColor == Color.white)
                                whiteCells++;
                            else
                            {
                                Debug.LogWarning($"🚨 Empty cell ({x},{y}) has wrong color: {cell.CellColor}");
                                unknownCells++;
                            }

                            break;

                        case CellVisualState.Occupied:
                            occupiedCells++;
                            if (cell.CellColor == Color.white)
                                whiteCells++; // Occupied cells should also be white now
                            else
                            {
                                Debug.LogWarning(
                                    $"🚨 Occupied cell ({x},{y}) should be white but has color: {cell.CellColor}");
                                unknownCells++;
                            }

                            Debug.Log(
                                $"📦 Occupied cell ({x},{y}): {cell.CellColor} by {cell.OccupiedByIngredient?.ItemName} (should be white)");
                            break;

                        case CellVisualState.ValidHighlight:
                        case CellVisualState.InvalidHighlight:
                            highlightedCells++;
                            Debug.Log($"✨ Highlighted cell ({x},{y}): {cell.CellColor} ({cell.VisualState})");
                            break;

                        default:
                            unknownCells++;
                            Debug.LogWarning(
                                $"❓ Unknown state cell ({x},{y}): {cell.VisualState} with color {cell.CellColor}");
                            break;
                    }
                }
            }

            Debug.Log($"🎨 Cell Color Summary:");
            Debug.Log($"  📊 Total cells: {totalCells}");
            Debug.Log($"  ⬜ White cells (empty + occupied): {whiteCells}");
            Debug.Log($"  📦 Occupied cells: {occupiedCells} (these should be white too)");
            Debug.Log($"  ✨ Highlighted cells: {highlightedCells}");
            Debug.Log($"  ❓ Wrong color cells: {unknownCells}");

            if (unknownCells > 0)
            {
                Debug.LogError("🚨 Some cells have unexpected colors or states!");
            }
            else
            {
                Debug.Log("✅ All cell colors match their expected states");
            }
        }

        /// <summary>
        /// Force all cells to update their visual state and colors
        /// </summary>
        [ContextMenu("Force Update All Cell Colors")]
        public void ForceUpdateAllCellColors()
        {
            Debug.Log("🔄 Force updating all cell colors...");

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = gridCells[x, y];

                    // Force the cell to recalculate its visual state
                    if (cell.IsOccupied)
                    {
                        cell.SetOccupied(cell.OccupiedByIngredient);
                    }
                    else if (cell.IsHighlighted)
                    {
                        cell.SetHighlighted(true, cell.IsValidPlacement);
                    }
                    else
                    {
                        cell.Clear();
                    }
                }
            }

            // Force visual refresh
            if (visualizer != null)
            {
                visualizer.RefreshGrid();
                Debug.Log("✅ All cells forced to update their colors");
            }
        }

        /// <summary>
        /// Test method to verify occupied cells remain white
        /// </summary>
        [ContextMenu("Test Occupied Cell Colors")]
        public void TestOccupiedCellColors()
        {
            Debug.Log("🧪 === TESTING OCCUPIED CELL COLORS ===");

            if (currentSelectedIngredient == null)
            {
                Debug.LogError("No ingredient selected! Please select an ingredient first.");
                return;
            }

            Vector2Int testPos = new Vector2Int(2, 2);

            Debug.Log($"1. Placing {currentSelectedIngredient.ItemName} at {testPos}");
            bool placed = TryPlaceIngredient(currentSelectedIngredient, testPos);

            if (placed)
            {
                Debug.Log("2. Checking cell colors after placement...");
                var occupiedCells = GetIngredientCells(currentSelectedIngredient, testPos);

                bool allCellsWhite = true;
                foreach (var cellPos in occupiedCells)
                {
                    var cell = GetCell(cellPos.x, cellPos.y);
                    if (cell != null)
                    {
                        if (cell.CellColor != Color.white)
                        {
                            Debug.LogError($"❌ Cell ({cellPos.x},{cellPos.y}) is not white: {cell.CellColor}");
                            allCellsWhite = false;
                        }
                        else
                        {
                            Debug.Log($"✅ Cell ({cellPos.x},{cellPos.y}) is correctly white");
                        }
                    }
                }

                if (allCellsWhite)
                {
                    Debug.Log("✅ SUCCESS: All occupied cells are white! Ingredient model should provide the color.");
                }
                else
                {
                    Debug.LogError("❌ FAILURE: Some occupied cells are not white!");
                }
            }
            else
            {
                Debug.LogError("❌ Could not place ingredient for testing");
            }
        }

        /// <summary>
        /// Public method for UI buttons to call - with extra debugging
        /// </summary>
        public void OnClearGridButton()
        {
            Debug.LogWarning("🧹 OnClearGridButton() called from UI!");
            ClearGrid();
        }

        /// <summary>
        /// Enhanced debug that shows ALL cells that would be occupied by the current ingredient's shape
        /// </summary>
        public void DebugIngredientShapeCollision(Ingredient ingredient, Vector2Int position)
        {
            if (ingredient == null) return;

            DebugSystemConfig.LogShapeCollision($"=== SHAPE COLLISION DEBUG: {ingredient.ItemName} at {position} ===");

            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            DebugSystemConfig.LogShapeCollision($"Shape size: {shapeWidth}x{shapeHeight}");

            // Show the shape pattern
            DebugSystemConfig.LogShapeCollision("Shape pattern:");
            for (int y = shapeHeight - 1; y >= 0; y--)
            {
                string row = $"  Y={y}: ";
                for (int x = 0; x < shapeWidth; x++)
                {
                    row += shape[x, y] ? "[#]" : "[ ]";
                }

                DebugSystemConfig.LogShapeCollision(row);
            }

            // Check each cell that would be occupied
            bool hasCollisions = false;
            DebugSystemConfig.LogShapeCollision("Checking all shape cells:");
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    if (shape[x, y])
                    {
                        Vector2Int cellPos = position + new Vector2Int(x, y);

                        // Check bounds
                        if (cellPos.x < 0 || cellPos.x >= gridWidth || cellPos.y < 0 || cellPos.y >= gridHeight)
                        {
                            DebugSystemConfig.LogShapeCollision(
                                $"  ❌ BOUNDS: Shape cell ({x},{y}) → Grid cell ({cellPos.x},{cellPos.y}) is OUT OF BOUNDS");
                            hasCollisions = true;
                            continue;
                        }

                        // Check occupation
                        bool isOccupied = gridCells[cellPos.x, cellPos.y].IsOccupied;
                        string occupant = gridCells[cellPos.x, cellPos.y].OccupiedByIngredient?.ItemName ?? "None";

                        if (isOccupied)
                        {
                            DebugSystemConfig.LogShapeCollision(
                                $"  ❌ COLLISION: Shape cell ({x},{y}) → Grid cell ({cellPos.x},{cellPos.y}) occupied by {occupant}");
                            hasCollisions = true;
                        }
                        else
                        {
                            DebugSystemConfig.LogShapeCollision(
                                $"  ✅ FREE: Shape cell ({x},{y}) → Grid cell ({cellPos.x},{cellPos.y}) is available");
                        }
                    }
                }
            }

            string result = hasCollisions ? "❌ PLACEMENT BLOCKED" : "✅ PLACEMENT ALLOWED";
            DebugSystemConfig.LogShapeCollision($"RESULT: {result}");
        }

        public void ClearGrid()
        {
            DebugSystemConfig.LogTesting(
                "ClearGrid() called! This will clear all placed ingredients but keep the grid visualization.");

            DebugSystemConfig.LogGridState("Grid state BEFORE clearing:");
            DebugGridState();

            // Clear ingredient selection and highlights FIRST
            ClearIngredientSelection();

            // Explicitly clear all highlights from the visualizer as well
            if (visualizer != null)
            {
                visualizer.ClearHighlights();
                DebugSystemConfig.LogTesting("Explicitly cleared all grid highlights");
            }

            // Clear grid data (ingredient occupancy only)
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y].IsOccupied)
                    {
                        DebugSystemConfig.LogGridState(
                            $"Clearing occupied cell ({x},{y}) with {gridCells[x, y].OccupiedByIngredient?.ItemName}");
                        gridCells[x, y].Clear(); // Only clear occupancy, not the visual grid cell
                    }
                }
            }

            // Clear visual ingredients with detailed logging (but preserve grid visualization)
            DebugSystemConfig.LogTesting("Calling IngredientPlacer.ClearAllIngredients()...");
            if (ingredientPlacer != null)
            {
                int ingredientCountBefore = ingredientPlacer.GetAllPlacedIngredients().Count;
                DebugSystemConfig.LogTesting($"Ingredients to clear: {ingredientCountBefore}");

                ingredientPlacer.ClearAllIngredients();

                int ingredientCountAfter = ingredientPlacer.GetAllPlacedIngredients().Count;
                DebugSystemConfig.LogTesting($"Ingredients remaining after clear: {ingredientCountAfter}");
            }
            else
            {
                DebugSystemConfig.LogErrorRecovery("IngredientPlacer is null! Cannot clear visual ingredients.");
            }

            // Refresh grid visualization (this will update colors but preserve the grid)
            DebugSystemConfig.LogTesting("Refreshing grid visualization...");
            if (visualizer != null)
            {
                visualizer.RefreshGrid(); // This should update the grid colors but keep the grid structure

                // Debug: Check if cells are properly reset to white
                DebugSystemConfig.LogGridState("Verifying cell colors after refresh...");
                int nonWhiteCells = 0;
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        var cell = gridCells[x, y];
                        if (cell.CellColor != Color.white)
                        {
                            DebugSystemConfig.LogErrorRecovery(
                                $"Cell ({x},{y}) color is not white: {cell.CellColor}, VisualState: {cell.VisualState}");
                            nonWhiteCells++;
                        }
                    }
                }

                if (nonWhiteCells == 0)
                {
                    DebugSystemConfig.LogGridState("All cells properly reset to white color");
                }
                else
                {
                    DebugSystemConfig.LogErrorRecovery($"{nonWhiteCells} cells failed to reset to white color!");
                }
            }
            else
            {
                DebugSystemConfig.LogErrorRecovery("GridVisualizer is null! Cannot refresh grid.");
            }

            DebugSystemConfig.LogGridState("Grid state AFTER clearing:");
            DebugGridState();

            DebugSystemConfig.LogTesting(
                "ClearGrid() complete! Grid structure preserved, ingredients removed, highlights cleared.");
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

        private void Update()
        {
            // Continuous verification system
            if (enableContinuousVerification && Time.time - lastVerificationTime >= verificationInterval)
            {
                RunContinuousVerification();
                lastVerificationTime = Time.time;
            }
        }

        /// <summary>
        /// Runs continuous verification checks to catch issues that develop over time
        /// </summary>
        private void RunContinuousVerification()
        {
            // Only run if we have placed ingredients
            var placedIngredients = ingredientPlacer?.GetAllPlacedIngredients();
            if (placedIngredients == null || placedIngredients.Count == 0)
                return;

            // Quick verification - just check if any placed ingredients have lost their cell occupancy
            int inconsistentIngredients = 0;

            foreach (var instance in placedIngredients)
            {
                var expectedCells = GetIngredientCells(instance.ingredient, instance.gridPosition);
                bool hasAnyUnoccupiedCell = false;

                foreach (var cellPos in expectedCells)
                {
                    var cell = GetCell(cellPos.x, cellPos.y);
                    if (cell != null && (!cell.IsOccupied || cell.OccupiedByIngredient != instance.ingredient))
                    {
                        hasAnyUnoccupiedCell = true;
                        break;
                    }
                }

                if (hasAnyUnoccupiedCell)
                {
                    inconsistentIngredients++;
                    Debug.LogWarning(
                        $"🔍 CONTINUOUS CHECK: {instance.ingredient.ItemName} at {instance.gridPosition} has lost cell occupancy!");

                    // Auto-repair: use IngredientPlacer's repair method
                    Debug.Log($"🔧 Auto-repairing occupancy for {instance.ingredient.ItemName} using IngredientPlacer");

                    bool repairSuccess =
                        ingredientPlacer.RepairIngredientOccupancy(instance.ingredient, instance.gridPosition);
                    if (repairSuccess)
                    {
                        Debug.Log($"🔧 Successfully repaired grid occupancy for {instance.ingredient.ItemName}");
                    }
                    else
                    {
                        Debug.LogError($"🚨 Failed to repair grid occupancy for {instance.ingredient.ItemName}");
                    }
                }
            }

            if (inconsistentIngredients > 0)
            {
                Debug.LogWarning(
                    $"🔍 CONTINUOUS CHECK: Found and repaired {inconsistentIngredients} ingredients with occupancy issues");
                visualizer?.RefreshGrid();
            }
        }

        #region Obstacle System

        /// <summary>
        /// Initialize the obstacle system
        /// </summary>
        private void InitializeObstacleSystems()
        {
            Debug.Log("🚧 Initializing Obstacle System");
            aspectObstacles.Clear();

            // Normalize grading weights
            if (gradingWeights != null)
            {
                gradingWeights.NormalizeWeights();
            }
        }

        /// <summary>
        /// Spawn initial obstacles when the game starts
        /// </summary>
        public void SpawnInitialObstacles()
        {
            if (!enableObstacles) return;

            DebugSystemConfig.LogObstacleSpawn(
                $"Spawning initial obstacles with {obstacleSpawnChance:P1} chance per cell");

            int obstaclesSpawned = 0;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    // Skip center cells to give player starting room
                    if (x >= 1 && x <= 3 && y >= 1 && y <= 3) continue;

                    Vector2Int position = new Vector2Int(x, y);
                    float randomRoll = Random.Range(0f, 1f);

                    // Log the spawn chance calculation
                    if (obstacleSpawnDebugger != null)
                    {
                        obstacleSpawnDebugger.LogSpawnChanceCalculation(position, obstacleSpawnChance, randomRoll);
                    }

                    if (randomRoll < obstacleSpawnChance)
                    {
                        var obstacleType =
                            (ObstacleType)Random.Range(0, System.Enum.GetValues(typeof(ObstacleType)).Length);
                        var obstacle = new AspectObstacle(obstacleType, position);
                        aspectObstacles.Add(obstacle);
                        obstaclesSpawned++;

                        // Log the obstacle type spawned
                        if (obstacleSpawnDebugger != null)
                        {
                            obstacleSpawnDebugger.LogObstacleTypeSpawned(obstacleType, position);
                        }

                        DebugSystemConfig.LogObstacleSpawn($"Spawned {obstacleType} obstacle at ({x}, {y})");
                    }
                }
            }

            DebugSystemConfig.LogObstacleSpawn($"Spawned {obstaclesSpawned} obstacles total");

            // Update visualizer to show obstacles
            if (visualizer != null)
            {
                visualizer.RefreshGrid();
            }
        }

        /// <summary>
        /// Check if a position has an obstacle
        /// </summary>
        public AspectObstacle GetObstacleAt(Vector2Int position)
        {
            return aspectObstacles.Find(o => o.Position == position);
        }

        /// <summary>
        /// Handle obstacle interactions when placing ingredients
        /// </summary>
        private bool HandleObstacleInteraction(Ingredient ingredient, Vector2Int position)
        {
            var obstacle = GetObstacleAt(position);
            if (obstacle == null) return true; // No obstacle, placement allowed

            bool canPlace = obstacle.CanPlaceIngredient(ingredient);
            if (canPlace)
            {
                bool placementSuccess = obstacle.TryPlaceIngredient(ingredient, this);
                if (placementSuccess)
                {
                    Debug.Log($"✅ Successfully completed {obstacle.ObstacleType} obstacle at {position}");

                    // Handle special obstacle effects
                    ProcessObstacleEffects(obstacle, ingredient);

                    // Check for frigid obstacles that might be melted by adjacency
                    CheckAdjacentFrigidObstacles(position);
                }

                return placementSuccess;
            }

            Debug.Log($"❌ Cannot place {ingredient.ItemName} on {obstacle.ObstacleType} obstacle at {position}");
            return false;
        }

        /// <summary>
        /// Process special effects from obstacle completion
        /// </summary>
        private void ProcessObstacleEffects(AspectObstacle obstacle, Ingredient ingredient)
        {
            switch (obstacle.ObstacleType)
            {
                case ObstacleType.Caustic:
                    Debug.Log($"🧪 Caustic obstacle: {ingredient.ItemName} potency reduced by 20%");
                    break;

                case ObstacleType.Divine:
                    if (ingredient.IngredientAspect == Aspect.Divine && ingredient.IsUnrefined)
                    {
                        Debug.Log($"✨ Divine obstacle: {ingredient.ItemName} potency increased by 10%");
                    }

                    break;
            }
        }

        /// <summary>
        /// Check adjacent cells for frigid obstacles that might be melted
        /// </summary>
        private void CheckAdjacentFrigidObstacles(Vector2Int position)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var direction in directions)
            {
                Vector2Int adjacentPos = position + direction;
                var obstacle = GetObstacleAt(adjacentPos);

                if (obstacle != null && obstacle.ObstacleType == ObstacleType.Frigid && !obstacle.IsCompleted)
                {
                    bool melted = obstacle.TryMeltFrozen(this);
                    if (melted)
                    {
                        Debug.Log($"❄️ Frigid obstacle at {adjacentPos} was melted by adjacency!");
                    }
                }
            }
        }

        /// <summary>
        /// Add an obstacle at runtime (for testing or dynamic gameplay)
        /// </summary>
        [ContextMenu("Add Random Obstacle")]
        public void AddRandomObstacle()
        {
            // Find an empty cell
            var emptyCells = new List<Vector2Int>();
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = GetCell(x, y);
                    if (cell != null && !cell.IsOccupied && GetObstacleAt(new Vector2Int(x, y)) == null)
                    {
                        emptyCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                var randomPos = emptyCells[Random.Range(0, emptyCells.Count)];
                float randomRoll = Random.Range(0f, 1f);

                // Always spawn for manual testing, but still log the roll
                if (obstacleSpawnDebugger != null)
                {
                    obstacleSpawnDebugger.LogSpawnChanceCalculation(randomPos, 1.0f, randomRoll);
                }

                var randomType = (ObstacleType)Random.Range(0, System.Enum.GetValues(typeof(ObstacleType)).Length);
                var obstacle = new AspectObstacle(randomType, randomPos);
                aspectObstacles.Add(obstacle);

                // Log the obstacle type spawned
                if (obstacleSpawnDebugger != null)
                {
                    obstacleSpawnDebugger.LogObstacleTypeSpawned(randomType, randomPos);
                }

                DebugSystemConfig.LogObstacleSpawn($"Added {randomType} obstacle at {randomPos}");

                if (visualizer != null)
                {
                    visualizer.RefreshGrid();
                }
            }
            else
            {
                Debug.LogWarning("No empty cells available for obstacle placement");
            }
        }

        /// <summary>
        /// Test obstacle spawn chance with current settings
        /// </summary>
        [ContextMenu("Test Current Obstacle Spawn Chance")]
        public void TestCurrentObstacleSpawnChance()
        {
            if (obstacleSpawnDebugger != null)
            {
                obstacleSpawnDebugger.TestSpawnProbability();
            }
            else
            {
                Debug.LogWarning("No ObstacleSpawnDebugger available for testing");
            }
        }

        /// <summary>
        /// Show current obstacle spawn statistics
        /// </summary>
        [ContextMenu("Show Obstacle Spawn Statistics")]
        public void ShowObstacleSpawnStatistics()
        {
            if (obstacleSpawnDebugger != null)
            {
                obstacleSpawnDebugger.GenerateSpawnAnalyticsReport();

                Debug.Log("🚧 === CURRENT SPAWN SETTINGS ===");
                Debug.Log($"🚧 Spawn Chance: {obstacleSpawnChance:P2} ({obstacleSpawnChance:F3})");
                Debug.Log($"🚧 Obstacles Enabled: {enableObstacles}");
                Debug.Log($"🚧 Current Obstacles on Grid: {aspectObstacles.Count}");
                Debug.Log($"🚧 Grid Size: {gridWidth}x{gridHeight} = {gridWidth * gridHeight} cells");

                int maxPossibleObstacles = (gridWidth * gridHeight) - 9; // Subtract center area
                float expectedObstacles = maxPossibleObstacles * obstacleSpawnChance;
                Debug.Log($"🚧 Expected Obstacles (theoretical): {expectedObstacles:F1}");

                string summary = obstacleSpawnDebugger.GetSpawnStatsSummary();
                Debug.Log($"🚧 Live Stats: {summary}");
            }
            else
            {
                Debug.LogWarning("No ObstacleSpawnDebugger available for statistics");
            }
        }

        /// <summary>
        /// Respawn all obstacles (for testing different spawn chances)
        /// </summary>
        [ContextMenu("Respawn All Obstacles")]
        public void RespawnAllObstacles()
        {
            // Clear existing obstacles
            aspectObstacles.Clear();

            // Reset spawn debugger stats
            if (obstacleSpawnDebugger != null)
            {
                obstacleSpawnDebugger.ResetSpawnStatistics();
            }

            Debug.Log("🚧 Cleared all obstacles, respawning with current settings...");

            // Spawn new obstacles
            SpawnInitialObstacles();

            Debug.Log($"🚧 Respawn complete! New obstacle count: {aspectObstacles.Count}");
        }

        /// <summary>
        /// Remove an obstacle at a specific position
        /// </summary>
        public bool RemoveObstacleAt(Vector2Int position)
        {
            var obstacle = GetObstacleAt(position);
            if (obstacle != null)
            {
                aspectObstacles.Remove(obstacle);
                Debug.Log($"🚧 Removed {obstacle.ObstacleType} obstacle at {position}");

                if (visualizer != null)
                {
                    visualizer.RefreshGrid();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get obstacle visual color for the visualizer
        /// </summary>
        public Color GetObstacleColorAt(Vector2Int position)
        {
            var obstacle = GetObstacleAt(position);
            return obstacle?.GetObstacleColor() ?? Color.clear;
        }

        /// <summary>
        /// Check if an obstacle is completed
        /// </summary>
        public bool IsObstacleCompletedAt(Vector2Int position)
        {
            var obstacle = GetObstacleAt(position);
            return obstacle?.IsCompleted ?? false;
        }

        #endregion

        #region Proficiency Grading System

        /// <summary>
        /// Initialize the proficiency grading system
        /// </summary>
        private void InitializeProficiencySystem()
        {
            Debug.Log("📊 Initializing Proficiency Grading System");

            if (gradingWeights == null)
            {
                gradingWeights = new ProficiencyWeights();
            }

            gradingWeights.NormalizeWeights();
            Debug.Log(
                $"📊 Grading weights normalized: Coverage={gradingWeights.coverageWeight:F2}, Adjacency={gradingWeights.adjacencyWeight:F2}, etc.");
        }

        /// <summary>
        /// Calculate proficiency grade for current grid state
        /// </summary>
        [ContextMenu("Calculate Proficiency Grade")]
        public ProficiencyGrade CalculateCurrentProficiency()
        {
            if (!enableProficiencyGrading)
            {
                Debug.LogWarning("Proficiency grading is disabled");
                return new ProficiencyGrade();
            }

            // Gather placed ingredients
            var placedIngredientsDict = new Dictionary<Vector2Int, Ingredient>();
            var placedIngredients = ingredientPlacer?.GetAllPlacedIngredients();

            if (placedIngredients != null)
            {
                foreach (var instance in placedIngredients)
                {
                    var cells = GetIngredientCells(instance.ingredient, instance.gridPosition);
                    foreach (var cellPos in cells)
                    {
                        placedIngredientsDict[cellPos] = instance.ingredient;
                    }
                }
            }

            var grade = ProficiencyGrading.CalculateProficiency(this, placedIngredientsDict, aspectObstacles,
                gradingWeights);

            Debug.Log($"📊 === PROFICIENCY GRADE CALCULATED ===");
            Debug.Log($"📊 Overall Score: {grade.overallScore:F1}% (Grade: {grade.gradeLevel})");
            Debug.Log($"📊 Coverage Ratio: {grade.coverageRatio:F1}%");
            Debug.Log($"📊 Adjacency Synergy: {grade.adjacencySynergy:F1}%");
            Debug.Log($"📊 Expansion Utilization: {grade.expansionUtilization:F1}%");
            Debug.Log($"📊 Shape Difficulty: {grade.shapeDifficulty:F1}%");
            Debug.Log($"📊 Orientation Efficiency: {grade.orientationEfficiency:F1}%");
            Debug.Log($"📊 Obstacles Completed: {grade.obstaclesCompleted:F1}%");
            Debug.Log($"📊 Feedback: {grade.feedback}");

            return grade;
        }

        /// <summary>
        /// Create refined version of an ingredient (smaller, more potent)
        /// </summary>
        public Ingredient CreateRefinedIngredient(Ingredient originalIngredient)
        {
            if (originalIngredient == null || !originalIngredient.IsUnrefined)
            {
                Debug.LogWarning("Cannot refine: ingredient is null or already refined");
                return originalIngredient;
            }

            // Create a refined copy (in a real system, this would create a new ScriptableObject instance)
            Debug.Log($"🔬 Creating refined version of {originalIngredient.ItemName}");
            Debug.Log(
                $"🔬 Original: Size {originalIngredient.GridWidth}x{originalIngredient.GridHeight}, Potency {originalIngredient.Potency}, Unrefined: {originalIngredient.IsUnrefined}");
            Debug.Log($"🔬 Refined: Size reduced by 25%, Potency increased by 50%, Obstacle chance reduced to 5%");

            // In a real implementation, you would:
            // 1. Create a new Ingredient ScriptableObject instance
            // 2. Copy properties from original
            // 3. Apply refinement bonuses
            // 4. Set OriginalIngredient reference
            // 5. Mark as refined (IsUnrefined = false)

            return originalIngredient; // Placeholder return
        }

        /// <summary>
        /// Check if refinement is available for an ingredient
        /// </summary>
        public bool CanRefineIngredient(Ingredient ingredient)
        {
            return ingredient != null && ingredient.IsUnrefined;
        }

        /// <summary>
        /// Get all completed obstacles
        /// </summary>
        public List<AspectObstacle> GetCompletedObstacles()
        {
            return aspectObstacles.Where(o => o.IsCompleted).ToList();
        }

        /// <summary>
        /// Get proficiency grade color for UI display
        /// </summary>
        public Color GetGradeColor(GradeLevel grade)
        {
            return ProficiencyGrading.GetGradeColor(grade);
        }

        #endregion

        #region Recipe System

        /// <summary>
        /// Check for recipe matches and execute if found
        /// </summary>
        private void CheckForRecipeMatches()
        {
            Debug.Log("🧪 === CHECKING FOR RECIPE MATCHES ===");
            
            // Get all placed ingredients
            var placedIngredients = ingredientPlacer?.GetAllPlacedIngredients();
            if (placedIngredients == null || placedIngredients.Count < 2)
            {
                Debug.Log("🧪 Not enough ingredients placed for recipes (need at least 2)");
                return;
            }

            // Extract unique ingredients from placed items
            var uniqueIngredients = new HashSet<Ingredient>();
            foreach (var instance in placedIngredients)
            {
                uniqueIngredients.Add(instance.ingredient);
            }

            Debug.Log($"🧪 Found {uniqueIngredients.Count} unique ingredients on grid:");
            foreach (var ingredient in uniqueIngredients)
            {
                Debug.Log($"   - {ingredient.ItemName}");
            }

            // Check against all recipes in database
            var database = AlchemyRecipeDatabase.Instance;
            if (database == null)
            {
                Debug.LogError("🧪 AlchemyRecipeDatabase not found! Cannot check recipes.");
                return;
            }

            Debug.Log($"🧪 Checking against {database.Recipes.Count} recipes in database");

            // Try to find a matching recipe
            AlchemyRecipe matchedRecipe = null;
            foreach (var recipe in database.Recipes)
            {
                if (DoesRecipeMatch(recipe, uniqueIngredients))
                {
                    matchedRecipe = recipe;
                    break;
                }
            }

            if (matchedRecipe != null)
            {
                Debug.Log($"🧪 ✅ RECIPE MATCH FOUND: {matchedRecipe.ItemName}!");
                ExecuteRecipe(matchedRecipe, uniqueIngredients);
            }
            else
            {
                Debug.Log("🧪 ❌ No recipe matches found for current ingredients");
            }
        }

        /// <summary>
        /// Check if a recipe matches the given ingredients
        /// </summary>
        private bool DoesRecipeMatch(AlchemyRecipe recipe, HashSet<Ingredient> placedIngredients)
        {
            var requiredIngredients = new List<Ingredient>();
            
            if (recipe.InputIngredient1 != null) requiredIngredients.Add(recipe.InputIngredient1);
            if (recipe.InputIngredient2 != null) requiredIngredients.Add(recipe.InputIngredient2);
            if (recipe.InputIngredient3 != null) requiredIngredients.Add(recipe.InputIngredient3);

            Debug.Log($"🧪 Checking recipe '{recipe.ItemName}' requiring {requiredIngredients.Count} ingredients:");
            foreach (var ingredient in requiredIngredients)
            {
                Debug.Log($"   - Required: {ingredient.ItemName}");
            }

            // Check if all required ingredients are present
            foreach (var required in requiredIngredients)
            {
                bool found = placedIngredients.Any(placed => placed == required);
                if (!found)
                {
                    Debug.Log($"🧪 Missing required ingredient: {required.ItemName}");
                    return false;
                }
            }

            // Check if we have exactly the right number of ingredients (no extra)
            if (placedIngredients.Count != requiredIngredients.Count)
            {
                Debug.Log($"🧪 Wrong number of ingredients: have {placedIngredients.Count}, need {requiredIngredients.Count}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Execute the matched recipe
        /// </summary>
        private void ExecuteRecipe(AlchemyRecipe recipe, HashSet<Ingredient> usedIngredients)
        {
            Debug.Log($"🧪 ⚗️ EXECUTING RECIPE: {recipe.ItemName}");
            Debug.Log($"🧪 Using ingredients: {string.Join(", ", usedIngredients.Select(i => i.ItemName))}");
            Debug.Log($"🧪 Output: {recipe.OutputPotion?.ItemName ?? "Unknown Potion"} x{recipe.OutputQuantity}");

            // Show immediate success message
            Debug.Log($"🧪 ✅ SUCCESS! Created {recipe.OutputPotion?.ItemName ?? "Unknown Potion"}!");
            
            // Start the recipe completion process with visual feedback
            StartCoroutine(RecipeCompletionSequence(recipe, usedIngredients));
        }

        /// <summary>
        /// Handle the visual sequence of recipe completion
        /// </summary>
        private System.Collections.IEnumerator RecipeCompletionSequence(AlchemyRecipe recipe, HashSet<Ingredient> usedIngredients)
        {
            // Phase 1: Show the successful recipe for a moment
            Debug.Log($"🧪 📋 Recipe Complete! {recipe.ItemName} -> {recipe.OutputPotion?.ItemName}");
            Debug.Log($"🧪 🕐 Showing results for 2 seconds before clearing grid...");
            
            // Wait for 2 seconds to let player see the result
            yield return new WaitForSeconds(2.0f);
            
            // Phase 2: Clear the grid
            Debug.Log($"🧪 🧹 Clearing grid and finalizing recipe...");
            ClearGrid();
            
            // Phase 3: Final completion message
            Debug.Log($"🧪 🎉 Recipe execution complete! Enjoy your new {recipe.OutputPotion?.ItemName ?? "potion"}!");
            
            // Here you could add more functionality like:
            // - Add the output potion to inventory
            // - Show crafting animation
            // - Play success sound
            // - Award experience points
        }

        /// <summary>
        /// Test method for recipe system
        /// </summary>
        [ContextMenu("Test Recipe System")]
        public void TestRecipeSystem()
        {
            Debug.Log("🧪 === TESTING RECIPE SYSTEM ===");

            // Get the database
            var database = AlchemyRecipeDatabase.Instance;
            if (database == null)
            {
                Debug.LogError("🧪 AlchemyRecipeDatabase not found! Cannot test recipes.");
                return;
            }

            Debug.Log($"🧪 Database loaded successfully with {database.Recipes.Count} recipes");

            // List all available recipes
            foreach (var recipe in database.Recipes)
            {
                Debug.Log($"🧪 Recipe: {recipe.ItemName}");
                Debug.Log($"   Inputs: {recipe.InputIngredient1?.ItemName} + {recipe.InputIngredient2?.ItemName}" + 
                         (recipe.InputIngredient3 != null ? $" + {recipe.InputIngredient3.ItemName}" : ""));
                Debug.Log($"   Output: {recipe.OutputPotion?.ItemName} x{recipe.OutputQuantity}");
            }

            // Test with currently placed ingredients
            CheckForRecipeMatches();

            Debug.Log("🧪 === RECIPE SYSTEM TEST COMPLETE ===");
        }

        /// <summary>
        /// Test recipe with specific ingredients (for testing purposes)
        /// </summary>
        [ContextMenu("Test Recipe - Place FireClaw & FireTalon")]
        public void TestRecipeWithFireIngredients()
        {
            Debug.Log("🧪 === TESTING RECIPE WITH FIRE INGREDIENTS ===");

            // Clear grid first
            ClearGrid();

            // Find FireClaw and FireTalon in available ingredients
            Ingredient fireClaw = null;
            Ingredient fireTalon = null;

            foreach (var ingredient in availableIngredients)
            {
                if (ingredient.ItemName.Contains("FireClaw"))
                    fireClaw = ingredient;
                else if (ingredient.ItemName.Contains("FireTalon"))
                    fireTalon = ingredient;
            }

            if (fireClaw == null || fireTalon == null)
            {
                Debug.LogWarning("🧪 Required ingredients (FireClaw & FireTalon) not found in available ingredients");
                Debug.Log("🧪 Available ingredients:");
                foreach (var ingredient in availableIngredients)
                {
                    Debug.Log($"   - {ingredient.ItemName}");
                }
                return;
            }

            // Place the ingredients
            Vector2Int pos1 = new Vector2Int(1, 1);
            Vector2Int pos2 = new Vector2Int(2, 1);

            Debug.Log($"🧪 Placing {fireClaw.ItemName} at {pos1}");
            bool success1 = TryPlaceIngredient(fireClaw, pos1);

            if (success1)
            {
                Debug.Log($"🧪 Placing {fireTalon.ItemName} at {pos2}");
                bool success2 = TryPlaceIngredient(fireTalon, pos2);

                if (success2)
                {
                    Debug.Log("🧪 Both ingredients placed successfully! Checking for recipes...");
                    // The recipe checking should automatically trigger through the placement system
                }
                else
                {
                    Debug.LogError($"🧪 Failed to place {fireTalon.ItemName}");
                }
            }
            else
            {
                Debug.LogError($"🧪 Failed to place {fireClaw.ItemName}");
            }
        }

        #endregion
    }
}