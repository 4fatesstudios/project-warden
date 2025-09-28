using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

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
        
        [Header("Proficiency Grading")] public bool enableProficiencyGrading = true;
        public ProficiencyWeights gradingWeights = new ProficiencyWeights();
        [Header("Gameplay")] public List<Ingredient> availableIngredients = new List<Ingredient>();
        public Transform ingredientContainer;
        public UnityEngine.Camera gameCamera;

        private GridCell[,] gridCells;
        private GridVisualizer visualizer;
        private IngredientPlacer ingredientPlacer;
        private InputActions inputActions;
        
        // Enhanced systems
        private EnhancedGridSystem enhancedGridSystem;
        private SynergySystem synergySystem;
        private FailureSystem failureSystem;
        private AlchemySkillTree skillTree;
        private TemplateSystem templateSystem;

        private Ingredient currentSelectedIngredient;
        private Vector2Int hoveredCell = Vector2Int.one * -1;
        
        // Crafting state management to prevent multiple simultaneous crafting
        private bool isProcessingRecipe = false;

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
                
            // Initialize enhanced systems
            // InitializeEnhancedSystems();
            
            // Add AutomaticSidebarManager to handle all sidebar issues automatically
            var existingComponents = GetComponents<MonoBehaviour>();
            bool hasAutomaticManager = false;
            
            foreach (var component in existingComponents)
            {
                if (component.GetType().Name == "AutomaticSidebarManager")
                {
                    hasAutomaticManager = true;
                    break;
                }
            }
            
            if (!hasAutomaticManager)
            {
                // Use reflection to add the component to avoid direct type reference
                var automaticManagerType = System.Type.GetType("FourFatesStudios.ProjectWarden.GridDemo.UI.AutomaticSidebarManager");
                if (automaticManagerType != null)
                {
                    gameObject.AddComponent(automaticManagerType);
                    Debug.Log("✅ Added AutomaticSidebarManager for automatic UI management");
                }
                else
                {
                    Debug.LogWarning("⚠️ AutomaticSidebarManager type not found - UI may not work properly");
                }
            }

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
            // Note: ObstacleSpawnDebugger temporarily disabled during migration
            // if (obstacleSpawnDebugger == null)
            // {
            //     obstacleSpawnDebugger = GetComponent<ObstacleSpawnDebugger>();
            //     if (obstacleSpawnDebugger == null)
            //     {
            //         obstacleSpawnDebugger = gameObject.AddComponent<ObstacleSpawnDebugger>();
            //         DebugSystemConfig.LogObstacleSpawn("Added ObstacleSpawnDebugger component");
            //     }
            // }
        }

        private void LoadTestIngredients()
        {
            DebugSystemConfig.LogTesting("Loading all available ingredients from Resources...");

            // Load all ingredients from the Items/Ingredients folder
            Ingredient[] allIngredients = Resources.LoadAll<Ingredient>("Items/Ingredients");
            
            DebugSystemConfig.LogTesting($"Found {allIngredients.Length} ingredient assets in Resources/Items/Ingredients");
            
            // Debug: List all found ingredients
            for (int i = 0; i < allIngredients.Length; i++)
            {
                if (allIngredients[i] != null)
                {
                    DebugSystemConfig.LogTesting($"   Found ingredient {i + 1}: '{allIngredients[i].ItemName}' from asset '{allIngredients[i].name}'");
                }
                else
                {
                    DebugSystemConfig.LogTesting($"   Found NULL ingredient at index {i}");
                }
            }
            
            foreach (Ingredient ingredient in allIngredients)
            {
                if (ingredient != null)
                {
                    availableIngredients.Add(ingredient);
                    DebugSystemConfig.LogTesting($"✅ Loaded ingredient: {ingredient.ItemName}");
                }
            }

            // If no ingredients were found in the main folder, try fallback paths
            if (availableIngredients.Count == 0)
            {
                DebugSystemConfig.LogTesting("No ingredients found in main folder, trying fallback paths...");
                
                string[] fallbackPaths = {
                    "Items/Ingredients/TestIngredient1",
                    "TestIngredients/Ice Crystal",
                    "TestIngredients/Life Bloom"
                };

                foreach (string path in fallbackPaths)
                {
                    var ingredient = Resources.Load<Ingredient>(path);
                    if (ingredient != null)
                    {
                        availableIngredients.Add(ingredient);
                        DebugSystemConfig.LogTesting($"✅ Loaded fallback ingredient: {ingredient.ItemName}");
                    }
                    else
                    {
                        DebugSystemConfig.LogTesting($"❌ Could not load ingredient from: {path}");
                    }
                }
            }

            DebugSystemConfig.LogTesting($"🎯 Total ingredients loaded: {availableIngredients.Count}");
            
            // Log all loaded ingredient names for debugging
            for (int i = 0; i < availableIngredients.Count; i++)
            {
                DebugSystemConfig.LogTesting($"   {i + 1}. {availableIngredients[i].ItemName}");
            }
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

                    // Note: Recipe checking should be manual, not automatic on ingredient placement
                    // CheckForRecipeMatches();

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

        /// <summary>
        /// Player-initiated grid clear: preserves obstacle colors, only removes ingredients
        /// </summary>
        public void ClearGrid()
        {
            ClearGridPreserveObstacles();
        }
        
        /// <summary>
        /// Player manual clear: Clear ingredients but preserve obstacle visual appearance
        /// </summary>
        public void ClearGridPreserveObstacles()
        {
            DebugSystemConfig.LogTesting(
                "ClearGridPreserveObstacles() called! Clearing ingredients but preserving obstacle colors.");

            DebugSystemConfig.LogGridState("Grid state BEFORE clearing (preserve obstacles):");
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
                        gridCells[x, y].Clear(); // Only clear occupancy, obstacles remain
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

            // Refresh grid visualization (this will update colors and preserve obstacle colors)
            DebugSystemConfig.LogTesting("Refreshing grid visualization while preserving obstacles...");
            if (visualizer != null)
            {
                visualizer.RefreshGrid();

                // Debug: Check cell states after refresh (obstacles should keep their colors)
                DebugSystemConfig.LogGridState("Verifying cell colors after refresh (obstacles preserved)...");
                int obstacleCount = 0;
                int emptyWhiteCells = 0;
                
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        var cell = gridCells[x, y];
                        if (cell.HasObstacle)
                        {
                            obstacleCount++;
                            Color expectedColor = cell.Obstacle.GetObstacleColor();
                            if (cell.CellColor != expectedColor)
                            {
                                DebugSystemConfig.LogErrorRecovery(
                                    $"Obstacle cell ({x},{y}) has wrong color: {cell.CellColor}, expected: {expectedColor}");
                            }
                        }
                        else if (cell.CellColor == Color.white)
                        {
                            emptyWhiteCells++;
                        }
                    }
                }

                DebugSystemConfig.LogGridState($"Grid cleared with {obstacleCount} obstacles preserved and {emptyWhiteCells} empty white cells");
            }
            else
            {
                DebugSystemConfig.LogErrorRecovery("GridVisualizer is null! Cannot refresh grid.");
            }

            DebugSystemConfig.LogGridState("Grid state AFTER clearing (obstacles preserved):");
            DebugGridState();

            DebugSystemConfig.LogTesting(
                "ClearGridPreserveObstacles() complete! Ingredients removed, obstacle colors preserved.");
        }
        
        /// <summary>
        /// Game-initiated complete clear: Removes everything including obstacles and resets to white
        /// </summary>
        public void ClearGridComplete()
        {
            DebugSystemConfig.LogTesting(
                "ClearGridComplete() called! Clearing all ingredients AND obstacles, resetting to white.");

            DebugSystemConfig.LogGridState("Grid state BEFORE complete clearing:");
            DebugGridState();

            // Clear ingredient selection and highlights FIRST
            ClearIngredientSelection();

            // Explicitly clear all highlights from the visualizer as well
            if (visualizer != null)
            {
                visualizer.ClearHighlights();
                DebugSystemConfig.LogTesting("Explicitly cleared all grid highlights");
            }

            // Clear grid data (ingredient occupancy AND obstacles)
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = gridCells[x, y];
                    
                    if (cell.IsOccupied)
                    {
                        DebugSystemConfig.LogGridState(
                            $"Clearing occupied cell ({x},{y}) with {cell.OccupiedByIngredient?.ItemName}");
                    }
                    
                    if (cell.HasObstacle)
                    {
                        DebugSystemConfig.LogGridState(
                            $"Removing obstacle from cell ({x},{y}): {cell.Obstacle?.ObstacleType}");
                        cell.RemoveObstacle();
                    }
                    
                    cell.Clear(); // Clear occupancy and reset visual state
                }
            }

            // Clear obstacle list
            int obstacleCount = aspectObstacles.Count;
            aspectObstacles.Clear();
            DebugSystemConfig.LogTesting($"Cleared {obstacleCount} obstacles from obstacle list");

            // Clear visual ingredients
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

            // Refresh grid visualization (this will reset everything to white)
            DebugSystemConfig.LogTesting("Refreshing grid visualization for complete reset...");
            if (visualizer != null)
            {
                visualizer.RefreshGrid();

                // Debug: Check if all cells are properly reset to white
                DebugSystemConfig.LogGridState("Verifying all cells reset to white...");
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

            DebugSystemConfig.LogGridState("Grid state AFTER complete clearing:");
            DebugGridState();

            DebugSystemConfig.LogTesting(
                "ClearGridComplete() complete! Everything cleared and reset to white.");
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

                    if (randomRoll < obstacleSpawnChance)
                    {
                        var obstacleType =
                            (ObstacleType)Random.Range(0, System.Enum.GetValues(typeof(ObstacleType)).Length);
                        var obstacle = new AspectObstacle(obstacleType, position);
                        aspectObstacles.Add(obstacle);
                        
                        // Also set the obstacle on the grid cell so it knows it has an obstacle
                        var gridCell = GetCell(x, y);
                        if (gridCell != null)
                        {
                            gridCell.SetObstacle(obstacle);
                        }
                        
                        obstaclesSpawned++;

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
        /// Public method to force grid recreation when size or configuration changes
        /// </summary>
        public void ForceRecreateGrid()
        {
            Debug.Log($"🔄 Force recreating grid cells for size {gridWidth}x{gridHeight}");
            
            // First, force the visualizer to recreate its visual arrays for the new size
            if (visualizer != null)
            {
                visualizer.ForceRecreateGridVisuals();
            }
            else
            {
                Debug.LogWarning("🔄 GridVisualizer not found during grid recreation");
            }
            
            // Then recreate the logical grid cells
            RecreateGridCells();
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

                var randomType = (ObstacleType)Random.Range(0, System.Enum.GetValues(typeof(ObstacleType)).Length);
                var obstacle = new AspectObstacle(randomType, randomPos);
                aspectObstacles.Add(obstacle);

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
                gradingWeights, IsFreeCraftingMode());

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

        #region Enhanced Systems Integration
        
        /// <summary>
        /// Initialize all enhanced alchemy systems
        /// </summary>
        private void InitializeEnhancedSystems()
        {
            Debug.Log("🚀 Initializing Enhanced Alchemy Systems...");
            
            // Enhanced Grid System
            enhancedGridSystem = GetComponent<EnhancedGridSystem>();
            if (enhancedGridSystem == null)
            {
                enhancedGridSystem = gameObject.AddComponent<EnhancedGridSystem>();
                Debug.Log("✅ Added EnhancedGridSystem");
            }
            
            // Synergy System
            synergySystem = GetComponent<SynergySystem>();
            if (synergySystem == null)
            {
                synergySystem = gameObject.AddComponent<SynergySystem>();
                Debug.Log("✅ Added SynergySystem");
            }
            
            // Failure System
            failureSystem = GetComponent<FailureSystem>();
            if (failureSystem == null)
            {
                failureSystem = gameObject.AddComponent<FailureSystem>();
                Debug.Log("✅ Added FailureSystem");
            }
            
            // Skill Tree
            skillTree = GetComponent<AlchemySkillTree>();
            if (skillTree == null)
            {
                skillTree = gameObject.AddComponent<AlchemySkillTree>();
                Debug.Log("✅ Added AlchemySkillTree");
            }
            
            // Template System
            templateSystem = GetComponent<TemplateSystem>();
            if (templateSystem == null)
            {
                templateSystem = gameObject.AddComponent<TemplateSystem>();
                Debug.Log("✅ Added TemplateSystem");
            }
            
            Debug.Log("🚀 Enhanced Alchemy Systems initialized successfully!");
        }
        
        /// <summary>
        /// Enhanced ingredient placement with all new systems
        /// </summary>
        public bool TryPlaceIngredientEnhanced(Ingredient ingredient, Vector2Int position)
        {
            Debug.Log($"🧪 === ENHANCED PLACEMENT: {ingredient.ItemName} at {position} ===");
            
            // Check if ingredient can expand grid
            if (ingredient.UnlocksAdditionalSpace)
            {
                bool expanded = enhancedGridSystem.TryExpandGrid(ingredient, ingredient.AdditionalSpaceCount);
                if (expanded)
                {
                    Debug.Log($"🔄 Grid expanded by {ingredient.ItemName}!");
                    // Recreate grid cells for new size
                    RecreateGridCells();
                }
            }
            
            // Check for layered placement if skill is unlocked
            bool allowOverlap = skillTree.IsSkillUnlocked("Ingredient Overlap");
            if (allowOverlap && enhancedGridSystem.CanOverlapAt(position, ingredient))
            {
                Debug.Log($"📚 Layered placement enabled for {ingredient.ItemName}");
                bool layeredSuccess = enhancedGridSystem.TryPlaceIngredientWithLayers(ingredient, position);
                if (layeredSuccess)
                {
                    // Also place normally for compatibility
                    bool normalSuccess = TryPlaceIngredient(ingredient, position);
                    if (normalSuccess)
                    {
                        PostPlacementEnhancements(ingredient, position);
                        return true;
                    }
                }
            }
            
            // Standard placement
            bool placementSuccess = TryPlaceIngredient(ingredient, position);
            if (placementSuccess)
            {
                PostPlacementEnhancements(ingredient, position);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Post-placement enhancements and checks
        /// </summary>
        private void PostPlacementEnhancements(Ingredient ingredient, Vector2Int position)
        {
            Debug.Log($"🔮 Running post-placement enhancements for {ingredient.ItemName}...");
            
            // Check for synergies
            var synergyResult = synergySystem.CheckSynergiesAt(position);
            if (synergyResult.hasActiveSynergy)
            {
                Debug.Log($"✨ Synergies activated! Potency multiplier: {synergyResult.totalPotencyMultiplier:F1}x");
                foreach (var description in synergyResult.synergyDescriptions)
                {
                    Debug.Log($"  🔮 {description}");
                }
            }
            
            // Check template progress if active
            if (templateSystem.CurrentTemplate != null)
            {
                var progress = templateSystem.CheckTemplateProgress();
                Debug.Log($"🗺️ Template progress: {progress.completionPercentage:P0}");
                
                if (progress.isComplete)
                {
                    Debug.Log($"🏆 Template completed! Bonus: {progress.currentBonus:F1}x");
                    // Award bonus skill points
                    if (templateSystem.CurrentTemplate.bonusSkillPoints > 0)
                    {
                        skillTree.AwardSkillPoints(templateSystem.CurrentTemplate.bonusSkillPoints, "Template completion");
                    }
                }
            }
            
            // Check for ingredient refund (skill-based)
            if (skillTree.IsSkillUnlocked("Ingredient Conservation"))
            {
                float refundChance = skillTree.GetSkillValue("Ingredient Conservation");
                if (Random.Range(0f, 1f) < refundChance)
                {
                    Debug.Log($"💎 Ingredient refund triggered! {ingredient.ItemName} returned to inventory");
                    // In a full implementation, would add ingredient back to inventory
                }
            }
        }
        
        /// <summary>
        /// Enhanced recipe checking with failure system
        /// </summary>
        private void CheckForRecipeMatchesEnhanced()
        {
            Debug.Log("🧪 === ENHANCED RECIPE CHECKING ===");
            
            // Check for conflicts first
            var failureAnalysis = failureSystem.AnalyzeCurrentGrid();
            
            if (failureAnalysis.hasConflicts)
            {
                Debug.Log("⚠️ Recipe conflicts detected!");
                
                bool proceedAnyway = failureSystem.ShowFailureWarning();
                if (!proceedAnyway)
                {
                    Debug.Log("❌ Recipe cancelled due to conflicts");
                    return;
                }
                
                // Roll for failure
                float roll = Random.Range(0f, 1f);
                if (roll < failureAnalysis.totalFailureChance)
                {
                    Debug.Log($"💥 Recipe failed! (Roll: {roll:F2}, Failure chance: {failureAnalysis.totalFailureChance:F2})");
                    
                    var usedIngredients = GetCurrentPlacedIngredients();
                    failureSystem.HandleRecipeFailure(usedIngredients);
                    
                    // Clear grid after failure (complete clear - removes obstacles)
                    ClearGridComplete();
                    return;
                }
                else
                {
                    Debug.Log($"🍀 Recipe succeeded despite conflicts! (Roll: {roll:F2})");
                }
            }
            
            // Proceed with normal recipe checking
            CheckForRecipeMatches();
        }
        
        /// <summary>
        /// Enhanced recipe completion with skill bonuses
        /// </summary>
        private void CompleteRecipeEnhanced(AlchemyRecipe recipe, HashSet<Ingredient> usedIngredients)
        {
            Debug.Log($"🎉 === ENHANCED RECIPE COMPLETION: {recipe.ItemName} ===");
            
            // Calculate skill bonuses
            float potencyBonus = 1f + skillTree.GetSkillValue("Alchemical Expertise");
            float synergyBonus = skillTree.GetSkillValue("Synergy Master");
            
            // Apply template bonuses
            float templateBonus = 1f;
            if (templateSystem.CurrentTemplate != null)
            {
                var progress = templateSystem.CheckTemplateProgress();
                if (progress.isComplete)
                {
                    templateBonus = progress.currentBonus;
                }
            }
            
            float totalBonus = potencyBonus * (1f + synergyBonus) * templateBonus;
            
            Debug.Log($"🔢 Final potency bonus: {totalBonus:F1}x");
            Debug.Log($"  - Skill bonus: {potencyBonus:F1}x");
            Debug.Log($"  - Synergy bonus: +{synergyBonus:P0}");
            Debug.Log($"  - Template bonus: {templateBonus:F1}x");
            
            // Award skill points based on proficiency
            if (enableProficiencyGrading)
            {
                var grade = CalculateCurrentProficiency();
                skillTree.AwardPointsForProficiency(grade);
            }
            
            // Award base skill points for successful crafting
            skillTree.AwardSkillPoints(1, "Successful recipe completion");
        }
        
        /// <summary>
        /// Recreate grid cells when grid size changes
        /// </summary>
        private void RecreateGridCells()
        {
            Debug.Log($"🔄 Recreating grid cells for size {gridWidth}x{gridHeight}");
            
            // Save current occupied cells
            var occupiedCells = new Dictionary<Vector2Int, Ingredient>();
            if (gridCells != null)
            {
                for (int x = 0; x < gridCells.GetLength(0); x++)
                {
                    for (int y = 0; y < gridCells.GetLength(1); y++)
                    {
                        if (gridCells[x, y].IsOccupied)
                        {
                            occupiedCells[new Vector2Int(x, y)] = gridCells[x, y].OccupiedByIngredient;
                        }
                    }
                }
            }
            
            // Create new grid
            gridCells = new GridCell[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    gridCells[x, y] = new GridCell(x, y);
                    
                    // Restore occupancy if within bounds
                    Vector2Int pos = new Vector2Int(x, y);
                    if (occupiedCells.ContainsKey(pos))
                    {
                        gridCells[x, y].SetOccupied(occupiedCells[pos]);
                    }
                }
            }
            
            // Refresh visualization
            if (visualizer != null)
            {
                visualizer.RefreshGrid();
            }
        }
        
        /// <summary>
        /// Use purify skill to remove obstacle
        /// </summary>
        [ContextMenu("Use Purify Skill")]
        public void UsePurifySkill()
        {
            if (!skillTree.CanUsePurify())
            {
                Debug.LogWarning("Purify skill not available or no charges remaining");
                return;
            }
            
            // Find first obstacle to remove (in a real game, player would click on obstacle)
            var firstObstacle = aspectObstacles.FirstOrDefault(o => !o.IsCompleted);
            if (firstObstacle != null)
            {
                bool success = skillTree.UsePurifyCharge();
                if (success)
                {
                    RemoveObstacleAt(firstObstacle.Position);
                    Debug.Log($"✨ Purified obstacle at {firstObstacle.Position}!");
                }
            }
            else
            {
                Debug.Log("No obstacles to purify");
            }
        }
        
        /// <summary>
        /// Test all enhanced systems
        /// </summary>
        [ContextMenu("Test Enhanced Systems")]
        public void TestEnhancedSystems()
        {
            Debug.Log("🧪 === TESTING ALL ENHANCED SYSTEMS ===");
            
            // Test enhanced grid
            if (enhancedGridSystem != null)
            {
                Debug.Log($"Enhanced Grid: Efficiency {enhancedGridSystem.GetGridEfficiencyScore():P0}");
            }
            
            // Test synergy system
            if (synergySystem != null)
            {
                synergySystem.TestSynergySystem();
            }
            
            // Test failure system
            if (failureSystem != null)
            {
                failureSystem.TestFailureSystem();
            }
            
            // Test skill tree
            if (skillTree != null)
            {
                skillTree.TestSkillTree();
            }
            
            // Test template system
            if (templateSystem != null)
            {
                templateSystem.TestTemplateSystem();
            }
        }

        /// <summary>
        /// Get current placed ingredients for failure system
        /// </summary>
        private List<Ingredient> GetCurrentPlacedIngredients()
        {
            var ingredients = new List<Ingredient>();
            if (ingredientPlacer != null)
            {
                var placedIngredients = ingredientPlacer.GetAllPlacedIngredients();
                ingredients.AddRange(placedIngredients.Select(p => p.ingredient).Distinct());
            }
            return ingredients;
        }
        
        #endregion

        /// <summary>
        /// Check for recipe matches and execute if found
        /// </summary>
        private void CheckForRecipeMatches()
        {
            // Prevent multiple simultaneous recipe processing
            if (isProcessingRecipe)
            {
                Debug.Log("🧪 ⚠️ Recipe processing already in progress - ignoring additional calls");
                return;
            }
            
            Debug.Log("🧪 === CHECKING FOR RECIPE MATCHES ===");
            
            try
            {
                // Set processing state immediately
                isProcessingRecipe = true;
                
                // DEFENSIVE COPYING: Create snapshots of current state before processing
                var placedIngredients = ingredientPlacer?.GetAllPlacedIngredients();
                if (placedIngredients == null || placedIngredients.Count < 2)
                {
                    Debug.Log("🧪 Not enough ingredients placed for recipes (need at least 2)");
                    return;
                }

                // Create defensive copy of placed ingredients to prevent modification during processing
                var placedIngredientsSnapshot = new List<IngredientInstance>(placedIngredients);
                Debug.Log($"🧪 📸 Created snapshot of {placedIngredientsSnapshot.Count} placed ingredients");

                // Extract unique ingredients from snapshot (defensive copy)
                var uniqueIngredients = new HashSet<Ingredient>();
                foreach (var instance in placedIngredientsSnapshot)
                {
                    if (instance?.ingredient != null)
                    {
                        uniqueIngredients.Add(instance.ingredient);
                    }
                }

                // Create immutable copy for recipe processing
                var uniqueIngredientsSnapshot = new HashSet<Ingredient>(uniqueIngredients);
                
                Debug.Log($"🧪 Found {uniqueIngredientsSnapshot.Count} unique ingredients on grid:");
                foreach (var ingredient in uniqueIngredientsSnapshot)
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
                    if (DoesRecipeMatch(recipe, uniqueIngredientsSnapshot))
                    {
                        matchedRecipe = recipe;
                        break;
                    }
                }

                if (matchedRecipe != null)
                {
                    Debug.Log($"🧪 ✅ RECIPE MATCH FOUND: {matchedRecipe.ItemName}!");
                    
                    // ATOMIC EXECUTION: Process recipe with snapshots, don't release lock until completely done
                    ExecuteRecipeAtomic(matchedRecipe, uniqueIngredientsSnapshot, placedIngredientsSnapshot);
                }
                else
                {
                    Debug.Log("🧪 ❌ No recipe matches found for current ingredients");
                    
                    // Check for dynamic infusion-based crafting
                    var dynamicPotion = CheckForInfusionBasedCrafting(uniqueIngredientsSnapshot, placedIngredientsSnapshot);
                    if (dynamicPotion != null)
                    {
                        Debug.Log($"🧪 ✨ DYNAMIC POTION CREATED: {dynamicPotion.ItemName}!");
                        ExecuteDynamicPotionCrafting(dynamicPotion, uniqueIngredientsSnapshot, placedIngredientsSnapshot);
                    }
                    else
                    {
                        Debug.Log("🧪 💥 No valid combinations found - destroying ingredients");
                        ExecuteIngredientDestruction(uniqueIngredientsSnapshot, placedIngredientsSnapshot);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during recipe processing: {ex.Message}");
                Debug.LogError($"🧪 Stack trace: {ex.StackTrace}");
            }
            finally
            {
                // Always reset processing state, even if an exception occurred
                isProcessingRecipe = false;
                Debug.Log("🧪 Recipe processing state reset");
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
        /// Execute the matched recipe atomically (synchronously) with defensive copying
        /// This replaces the old ExecuteRecipe method to prevent race conditions
        /// </summary>
        private void ExecuteRecipeAtomic(AlchemyRecipe recipe, HashSet<Ingredient> usedIngredientsSnapshot, List<IngredientInstance> placedIngredientsSnapshot)
        {
            Debug.Log($"🧪 ⚗️ EXECUTING RECIPE ATOMICALLY: {recipe.ItemName}");
            Debug.Log($"🧪 Using ingredients: {string.Join(", ", usedIngredientsSnapshot.Select(i => i.ItemName))}");
            Debug.Log($"🧪 Output: {recipe.OutputPotion?.ItemName ?? "Unknown Potion"} x{recipe.OutputQuantity}");
            
            try
            {
                // Phase 1: Immediate grid clearing (prevents further input)
                Debug.Log($"🧪 🧹 Clearing grid immediately to prevent additional ingredient placement...");
                ClearGridPreserveObstacles();
                
                // Phase 2: Add potion to inventory system (using snapshots)
                if (recipe.OutputPotion != null)
                {
                    var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
                    if (inventory != null)
                    {
                        // Atomic inventory addition
                        inventory.AddItem(recipe.OutputPotion, recipe.OutputQuantity);
                        Debug.Log($"🎒 ✅ Added {recipe.OutputQuantity}x {recipe.OutputPotion.ItemName} to inventory!");
                        
                        // Phase 3: Update UI systems
                        UpdateUISystemsAfterCrafting(recipe.OutputPotion);
                    }
                    else
                    {
                        Debug.LogWarning("🎒 ⚠️ ItemSlotContainerHolder not found - potion not added to inventory");
                    }
                }
                
                // Phase 4: Final success message
                Debug.Log($"🧪 ✅ SUCCESS! Atomically created {recipe.OutputPotion?.ItemName ?? "Unknown Potion"}!");
                Debug.Log($"🧪 🎉 Recipe execution complete! Enjoy your new {recipe.OutputPotion?.ItemName ?? "potion"}!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during atomic recipe execution: {ex.Message}");
                Debug.LogError($"🧪 Stack trace: {ex.StackTrace}");
                
                // In case of error, still clear the grid to prevent stuck state
                ClearGridPreserveObstacles();
            }
        }
        
        /// <summary>
        /// Update UI systems after successful crafting (separated for clarity)
        /// </summary>
        private void UpdateUISystemsAfterCrafting(Potion craftedPotion)
        {
            try
            {
                // Notify the UI system about the new potion
                var completedPotionsUI = FindFirstObjectByType<CompletedPotionsUIDocument>();
                if (completedPotionsUI != null)
                {
                    completedPotionsUI.OnPotionCrafted(craftedPotion);
                    Debug.Log($"📱 ✅ Updated UI with new potion: {craftedPotion.ItemName}");
                }
                else
                {
                    Debug.LogWarning("📱 ⚠️ CompletedPotionsUIDocument not found - UI may not update");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"📱 ⚠️ Error updating UI systems: {ex.Message}");
                // Don't rethrow - UI update failure shouldn't break crafting
            }
        }

        /// <summary>
        /// DEPRECATED: Old Execute recipe method - replaced by ExecuteRecipeAtomic
        /// Kept for reference but should not be used
        /// </summary>
        [System.Obsolete("Use ExecuteRecipeAtomic instead to prevent race conditions")]
        private void ExecuteRecipe(AlchemyRecipe recipe, HashSet<Ingredient> usedIngredients)
        {
            Debug.LogWarning("🧪 ⚠️ DEPRECATED ExecuteRecipe called - use ExecuteRecipeAtomic instead!");
            
            // Redirect to atomic version with defensive copying
            var ingredientsList = usedIngredients.ToList();
            var snapshot = new HashSet<Ingredient>(usedIngredients);
            var placedSnapshot = new List<IngredientInstance>(); // Empty - not used in deprecated path
            
            ExecuteRecipeAtomic(recipe, snapshot, placedSnapshot);
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
            
            // Phase 2: Clear the grid (preserve obstacles for continued play)
            Debug.Log($"🧪 🧹 Clearing grid and finalizing recipe...");
            ClearGridPreserveObstacles();
            
            // Phase 3: Add potion to inventory system
            if (recipe.OutputPotion != null)
            {
                var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
                if (inventory != null)
                {
                    inventory.AddItem(recipe.OutputPotion, recipe.OutputQuantity);
                    Debug.Log($"🎒 ✅ Added {recipe.OutputQuantity}x {recipe.OutputPotion.ItemName} to inventory!");
                    
                    // Notify the UI system about the new potion
                    var completedPotionsUI = FindFirstObjectByType<CompletedPotionsUIDocument>();
                    if (completedPotionsUI != null)
                    {
                        completedPotionsUI.OnPotionCrafted(recipe.OutputPotion);
                        Debug.Log($"📱 ✅ Updated UI with new potion: {recipe.OutputPotion.ItemName}");
                    }
                    else
                    {
                        Debug.LogWarning("📱 ⚠️ CompletedPotionsUIDocument not found - UI may not update");
                    }
                }
                else
                {
                    Debug.LogWarning("🎒 ⚠️ ItemSlotContainerHolder not found - potion not added to inventory");
                }
            }
            
            // Phase 4: Final completion message
            Debug.Log($"🧪 🎉 Recipe execution complete! Enjoy your new {recipe.OutputPotion?.ItemName ?? "potion"}!");
            
            // Additional functionality could be added here:
            // - Show crafting animation
            // - Play success sound
            // - Award experience points
        }

        /// <summary>
        /// Public method to manually reset processing state (for emergency situations)
        /// </summary>
        [ContextMenu("🔧 Force Reset Processing State")]
        public void ForceResetProcessingState()
        {
            bool wasProcessing = isProcessingRecipe;
            isProcessingRecipe = false;
            
            Debug.Log($"🔧 Processing state manually reset. Was processing: {wasProcessing}");
        }
        
        /// <summary>
        /// Public property to check current processing state
        /// </summary>
        public bool IsProcessingRecipe => isProcessingRecipe;
        
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

        #region Dynamic Infusion-Based Crafting System

        /// <summary>
        /// Check if ingredients can create a dynamic potion based on shared infusions
        /// </summary>
        private Potion CheckForInfusionBasedCrafting(HashSet<Ingredient> uniqueIngredients, List<IngredientInstance> placedIngredients)
        {
            Debug.Log($"🧪 ✨ === CHECKING FOR INFUSION-BASED CRAFTING ===");
            
            if (uniqueIngredients.Count < 2)
            {
                Debug.Log("🧪 Need at least 2 ingredients for infusion-based crafting");
                return null;
            }

            // Find shared infusions between ingredients
            var sharedInfusions = FindSharedInfusions(uniqueIngredients);
            
            if (sharedInfusions.Count == 0)
            {
                Debug.Log("🧪 No shared infusions found between ingredients");
                return null;
            }

            Debug.Log($"🧪 ✨ Found {sharedInfusions.Count} shared infusion types:");
            foreach (var infusionType in sharedInfusions.Keys)
            {
                var infusions = sharedInfusions[infusionType];
                Debug.Log($"   - {infusionType.Name}: {infusions.Count} instances from {infusions.Select(i => i.sourceIngredient.ItemName).Distinct().Count()} ingredients");
            }

            // Create or get dynamic potion based on shared infusions
            return CreateDynamicPotion(sharedInfusions, uniqueIngredients);
        }

        /// <summary>
        /// Find infusions that are shared between at least 2 different ingredients
        /// </summary>
        private Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> FindSharedInfusions(HashSet<Ingredient> ingredients)
        {
            var infusionMap = new Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>>();
            
            // Collect all infusions from all ingredients
            foreach (var ingredient in ingredients)
            {
                if (ingredient.InfusionBundle?.Infusions != null)
                {
                    foreach (var infusion in ingredient.InfusionBundle.Infusions)
                    {
                        if (infusion != null)
                        {
                            var infusionType = infusion.GetType();
                            
                            if (!infusionMap.ContainsKey(infusionType))
                            {
                                infusionMap[infusionType] = new List<(object, Ingredient)>();
                            }
                            
                            infusionMap[infusionType].Add((infusion, ingredient));
                        }
                    }
                }
            }
            
            // Filter to only include infusions that appear in at least 2 different ingredients
            var sharedInfusions = new Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>>();
            
            foreach (var kvp in infusionMap)
            {
                var infusionType = kvp.Key;
                var infusionList = kvp.Value;
                
                // Count unique ingredients that have this infusion
                var uniqueIngredients = infusionList.Select(tuple => tuple.sourceIngredient).Distinct().Count();
                
                if (uniqueIngredients >= 2)
                {
                    sharedInfusions[infusionType] = infusionList;
                    Debug.Log($"🧪 🔗 Shared infusion detected: {infusionType.Name} (found in {uniqueIngredients} ingredients)");
                }
            }
            
            return sharedInfusions;
        }

        /// <summary>
        /// Create a dynamic potion based on shared infusions
        /// Options: Runtime creation vs. Asset creation
        /// </summary>
        private Potion CreateDynamicPotion(Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions, HashSet<Ingredient> sourceIngredients)
        {
            Debug.Log($"🧪 🎨 Creating dynamic potion from shared infusions...");
            
            // Generate potion name based on ingredients and infusions
            var potionName = GenerateDynamicPotionName(sharedInfusions, sourceIngredients);
            
            // For now, create runtime potion (you can change this to asset creation if preferred)
            var dynamicPotion = CreateRuntimePotion(potionName, sharedInfusions, sourceIngredients);
            
            return dynamicPotion;
        }

        /// <summary>
        /// Generate a descriptive name for the dynamic potion
        /// </summary>
        private string GenerateDynamicPotionName(Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions, HashSet<Ingredient> sourceIngredients)
        {
            var infusionNames = sharedInfusions.Keys.Select(type => type.Name.Replace("Infusion", "")).ToList();
            var ingredientNames = sourceIngredients.Select(i => i.Adjective ?? i.Noun ?? "Unknown").Where(s => !string.IsNullOrEmpty(s)).ToList();
            
            string infusionPart = string.Join(" & ", infusionNames);
            string ingredientPart = ingredientNames.Count > 0 ? ingredientNames.First() : "Mixed";
            
            return $"{ingredientPart} {infusionPart} Potion";
        }

        /// <summary>
        /// Create a runtime potion (non-persistent) with the shared infusions
        /// </summary>
        private Potion CreateRuntimePotion(string potionName, Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions, HashSet<Ingredient> sourceIngredients)
        {
            Debug.Log($"🧪 ⚡ Creating runtime potion: {potionName}");
            
            // Create a new potion instance (runtime only)
            var dynamicPotion = ScriptableObject.CreateInstance<Potion>();
            
            // Set basic properties using reflection to access private fields
            SetPotionProperty(dynamicPotion, "itemName", potionName);
            SetPotionProperty(dynamicPotion, "itemDescription", $"A potion discovered through alchemical experimentation. Contains {string.Join(", ", sharedInfusions.Keys.Select(t => t.Name.Replace("Infusion", "")))} properties.");
            
            // Calculate potency based on source ingredients
            var averagePotency = sourceIngredients.Average(i => i.Potency);
            SetPotionProperty(dynamicPotion, "potency", Mathf.RoundToInt((float)averagePotency));
            
            // Set dynamic potion properties
            SetPotionProperty(dynamicPotion, "isCustomPotion", true);
            SetPotionProperty(dynamicPotion, "rarity", PotionRarity.Uncommon); // Dynamic potions are at least uncommon
            
            // Create and set infusion bundle for the potion
            var potionInfusionBundle = new InfusionBundle();
            
            // Add the shared infusions to the potion
            foreach (var sharedInfusionType in sharedInfusions.Keys)
            {
                var infusionList = sharedInfusions[sharedInfusionType];
                // Take the first instance of each shared infusion type
                var representativeInfusion = infusionList.First().infusion;
                
                // Add to potion's infusion bundle (casting to proper Infusion type)
                if (representativeInfusion != null)
                {
                    try
                    {
                        // Use reflection to add infusion since we can't cast directly
                        var addInfusionMethod = typeof(InfusionBundle).GetMethod("AddInfusion");
                        if (addInfusionMethod != null)
                        {
                            addInfusionMethod.Invoke(potionInfusionBundle, new object[] { representativeInfusion });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"🧪 ⚠️ Could not add infusion {representativeInfusion.GetType().Name}: {ex.Message}");
                    }
                }
            }
            
            // Set the infusion bundle
            SetPotionProperty(dynamicPotion, "infusionBundle", potionInfusionBundle);
            
            // Set source ingredients for reference
            SetPotionProperty(dynamicPotion, "sourceIngredients", sourceIngredients.ToList());
            
            // Set visual properties based on infusions
            SetDynamicPotionVisuals(dynamicPotion, sharedInfusions);
            
            Debug.Log($"🧪 ✨ Runtime potion created with {potionInfusionBundle.Infusions.Count} infusions");
            
            return dynamicPotion;
        }
        
        /// <summary>
        /// Helper method to set private potion properties using reflection
        /// </summary>
        private void SetPotionProperty(Potion potion, string propertyName, object value)
        {
            try
            {
                var field = typeof(Potion).GetField(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(potion, value);
                }
                else
                {
                    Debug.LogWarning($"🧪 ⚠️ Could not find field '{propertyName}' in Potion class");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"🧪 ⚠️ Error setting potion property '{propertyName}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set visual properties for dynamic potion based on infusion types
        /// </summary>
        private void SetDynamicPotionVisuals(Potion potion, Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions)
        {
            // Default visual properties
            Color primaryColor = Color.blue;
            Color secondaryColor = Color.white;
            bool hasGlow = false;
            bool hasBubbles = false;
            bool hasParticles = false;
            
            // Determine visual effects based on infusion types
            foreach (var infusionType in sharedInfusions.Keys)
            {
                string infusionName = infusionType.Name.ToLower();
                
                if (infusionName.Contains("fire"))
                {
                    primaryColor = Color.red;
                    secondaryColor = new Color(1f,.4f,0f,1f);
                    hasGlow = true;
                    hasBubbles = true;
                }
                else if (infusionName.Contains("ice") || infusionName.Contains("frost"))
                {
                    primaryColor = Color.cyan;
                    secondaryColor = Color.white;
                    hasParticles = true;
                }
                else if (infusionName.Contains("poison") || infusionName.Contains("toxic"))
                {
                    primaryColor = Color.green;
                    secondaryColor = Color.yellow;
                    hasBubbles = true;
                }
                else if (infusionName.Contains("lightning") || infusionName.Contains("shock"))
                {
                    primaryColor = Color.magenta;
                    secondaryColor = Color.white;
                    hasGlow = true;
                    hasParticles = true;
                }
                else if (infusionName.Contains("divine") || infusionName.Contains("holy"))
                {
                    primaryColor = Color.white;
                    secondaryColor = Color.yellow;
                    hasGlow = true;
                    hasParticles = true;
                }
                else
                {
                    primaryColor = new Color(1f,.4f,0f,.3f);
                    secondaryColor = Color.gray;
                    hasGlow = true;
                }
            }
            
            // Apply visual properties
            SetPotionProperty(potion, "primaryColor", primaryColor);
            SetPotionProperty(potion, "secondaryColor", secondaryColor);
            SetPotionProperty(potion, "hasGlow", hasGlow);
            SetPotionProperty(potion, "hasBubbles", hasBubbles);
            SetPotionProperty(potion, "hasParticles", hasParticles);
            
            Debug.Log($"🧪 🎨 Set dynamic potion visuals: Primary={primaryColor}, Glow={hasGlow}, Bubbles={hasBubbles}, Particles={hasParticles}");
        }

        /// <summary>
        /// Execute dynamic potion crafting (similar to recipe execution)
        /// </summary>
        private void ExecuteDynamicPotionCrafting(Potion dynamicPotion, HashSet<Ingredient> usedIngredients, List<IngredientInstance> placedIngredients)
        {
            Debug.Log($"🧪 ✨ === EXECUTING DYNAMIC POTION CRAFTING ===");
            Debug.Log($"🧪 Creating: {dynamicPotion.ItemName}");
            Debug.Log($"🧪 From ingredients: {string.Join(", ", usedIngredients.Select(i => i.ItemName))}");
            
            try
            {
                // Phase 1: Clear grid immediately
                Debug.Log($"🧪 🧹 Clearing grid...");
                ClearGridPreserveObstacles();
                
                // Phase 2: Add dynamic potion to inventory
                var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
                if (inventory != null)
                {
                    inventory.AddItem(dynamicPotion, 1);
                    Debug.Log($"🎒 ✅ Added dynamic potion to inventory: {dynamicPotion.ItemName}");
                    
                    // Update UI if possible
                    UpdateUISystemsAfterCrafting(dynamicPotion);
                }
                else
                {
                    Debug.LogWarning("🎒 ⚠️ ItemSlotContainerHolder not found - dynamic potion not added to inventory");
                }
                
                // Phase 3: Show discovery message
                Debug.Log($"🧪 🎉 DISCOVERY! You created a new potion: {dynamicPotion.ItemName}!");
                
                // Optional: Save discovered potion as asset for future use
                // SaveDiscoveredPotionAsAsset(dynamicPotion, usedIngredients);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during dynamic potion crafting: {ex.Message}");
            }
        }

        /// <summary>
        /// Execute ingredient destruction when no valid combinations are found
        /// </summary>
        private void ExecuteIngredientDestruction(HashSet<Ingredient> usedIngredients, List<IngredientInstance> placedIngredients)
        {
            Debug.Log($"🧪 💥 === EXECUTING INGREDIENT DESTRUCTION ===");
            Debug.Log($"🧪 Destroying ingredients: {string.Join(", ", usedIngredients.Select(i => i.ItemName))}");
            
            try
            {
                // Clear grid (ingredients are lost)
                Debug.Log($"🧪 🗑️ Clearing grid - ingredients destroyed...");
                ClearGridPreserveObstacles();
                
                // Show failure message
                Debug.Log($"🧪 💀 The ingredients react violently and are destroyed! No useful potion was created.");
                
                // Optional: Add failure effects, sounds, particles, etc.
                // PlayFailureEffects();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during ingredient destruction: {ex.Message}");
            }
        }

        /// <summary>
        /// Optional: Save discovered dynamic potion as an asset for future reference
        /// This creates persistent ScriptableObject assets when new combinations are discovered
        /// </summary>
        private void SaveDiscoveredPotionAsAsset(Potion dynamicPotion, HashSet<Ingredient> sourceIngredients)
        {
#if UNITY_EDITOR
            Debug.Log($"🧪 💾 Saving discovered potion as asset: {dynamicPotion.ItemName}");
            
            try
            {
                string assetPath = $"Assets/Resources/DiscoveredPotions/{dynamicPotion.ItemName}.asset";
                
                // Ensure directory exists
                string directory = System.IO.Path.GetDirectoryName(assetPath);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                // Create persistent asset
                UnityEditor.AssetDatabase.CreateAsset(dynamicPotion, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
                
                Debug.Log($"🧪 ✅ Discovered potion saved as asset: {assetPath}");
                
                // Optional: Create a recipe asset for this discovered combination
                // CreateRecipeAssetForDiscoveredPotion(dynamicPotion, sourceIngredients, assetPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error saving discovered potion as asset: {ex.Message}");
            }
#else
            Debug.Log($"🧪 ⚠️ Asset creation only available in editor - discovered potion remains runtime-only");
#endif
        }

        #endregion

        /// <summary>
        /// Test method for the new dynamic infusion-based crafting system
        /// </summary>
        [ContextMenu("🧪 Test Dynamic Infusion Crafting")]
        public void TestDynamicInfusionCrafting()
        {
            Debug.Log("🧪 === TESTING DYNAMIC INFUSION CRAFTING SYSTEM ===");
            
            if (ingredientPlacer == null)
            {
                Debug.LogError("🧪 IngredientPlacer not found!");
                return;
            }
            
            var placedIngredients = ingredientPlacer.GetAllPlacedIngredients();
            if (placedIngredients == null || placedIngredients.Count < 2)
            {
                Debug.Log("🧪 Place at least 2 ingredients on the grid first to test dynamic crafting");
                return;
            }
            
            Debug.Log($"🧪 Testing with {placedIngredients.Count} placed ingredients:");
            foreach (var instance in placedIngredients)
            {
                Debug.Log($"   - {instance.ingredient.ItemName}");
                
                if (instance.ingredient.InfusionBundle?.Infusions != null)
                {
                    foreach (var infusion in instance.ingredient.InfusionBundle.Infusions)
                    {
                        Debug.Log($"     └ Infusion: {infusion?.GetType().Name ?? "NULL"}");
                    }
                }
                else
                {
                    Debug.Log($"     └ No infusions found");
                }
            }
            
            // Test the shared infusion detection directly
            var uniqueIngredients = new HashSet<Ingredient>();
            foreach (var instance in placedIngredients)
            {
                if (instance?.ingredient != null)
                {
                    uniqueIngredients.Add(instance.ingredient);
                }
            }
            
            var sharedInfusions = FindSharedInfusions(uniqueIngredients);
            
            if (sharedInfusions.Count > 0)
            {
                Debug.Log("🧪 ✅ Shared infusions detected! This should create a dynamic potion.");
                var dynamicPotion = CheckForInfusionBasedCrafting(uniqueIngredients, placedIngredients);
                if (dynamicPotion != null)
                {
                    Debug.Log($"🧪 🎉 SUCCESS! Dynamic potion would be created: {dynamicPotion.ItemName}");
                }
            }
            else
            {
                Debug.Log("🧪 ❌ No shared infusions found - ingredients would be destroyed");
            }
            
            Debug.Log("🧪 === DYNAMIC INFUSION CRAFTING TEST COMPLETE ===");
        }

        #region Helper Methods for Testing and Enhanced Systems

        /// <summary>
        /// Get all currently placed ingredients as a dictionary
        /// </summary>
        public Dictionary<Vector2Int, Ingredient> GetCurrentPlacedIngredientsDict()
        {
            var placements = new Dictionary<Vector2Int, Ingredient>();
            
            if (ingredientPlacer != null)
            {
                var placedIngredients = ingredientPlacer.GetAllPlacedIngredients();
                foreach (var instance in placedIngredients)
                {
                    var cells = GetIngredientCells(instance.ingredient, instance.gridPosition);
                    foreach (var cellPos in cells)
                    {
                        placements[cellPos] = instance.ingredient;
                    }
                }
            }
            
            return placements;
        }

        /// <summary>
        /// Check if an ingredient has specific effects
        /// </summary>
        public bool HasIngredientEffects(Ingredient ingredient)
        {
            // Return true if ingredient has any special effects
            // In a full implementation, this would check for special properties
            return ingredient.IngredientAspect != Aspect.Corporeal || ingredient.Potency > 50;
        }

        /// <summary>
        /// Get grid efficiency score from enhanced grid system
        /// </summary>
        public float GetGridEfficiencyScore()
        {
            if (enhancedGridSystem != null)
            {
                return enhancedGridSystem.GetGridEfficiencyScore();
            }
            
            // Calculate basic efficiency as fallback
            int occupiedCells = 0;
            int totalCells = gridWidth * gridHeight;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y].IsOccupied)
                    {
                        occupiedCells++;
                    }
                }
            }
            
            return totalCells > 0 ? (float)occupiedCells / totalCells : 0f;
        }

        /// <summary>
        /// Get ingredient placement statistics for analysis
        /// </summary>
        public PlacementStats GetPlacementStats()
        {
            var stats = new PlacementStats
            {
                totalIngredients = 0,
                uniqueAspects = new HashSet<Aspect>(),
                adjacentPairs = 0,
                completedObstacles = 0
            };

            if (ingredientPlacer != null)
            {
                var placedIngredients = ingredientPlacer.GetAllPlacedIngredients();
                stats.totalIngredients = placedIngredients.Count;
                
                foreach (var instance in placedIngredients)
                {
                    stats.uniqueAspects.Add(instance.ingredient.IngredientAspect);
                }
            }

            stats.completedObstacles = aspectObstacles.Count(o => o.IsCompleted);
            
            return stats;
        }

        /// <summary>
        /// Data structure for placement statistics
        /// </summary>
        [System.Serializable]
        public struct PlacementStats
        {
            public int totalIngredients;
            public HashSet<Aspect> uniqueAspects;
            public int adjacentPairs;
            public int completedObstacles;
        }

        /// <summary>
        /// Check adjacent cells for specific analysis
        /// </summary>
        public List<Vector2Int> GetAdjacentCells(Vector2Int position, bool includeDiagonals = false)
        {
            var adjacentCells = new List<Vector2Int>();
            
            // Cardinal directions
            var directions = new List<Vector2Int>
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
            
            // Add diagonals if requested
            if (includeDiagonals)
            {
                directions.AddRange(new[]
                {
                    new Vector2Int(1, 1),
                    new Vector2Int(1, -1),
                    new Vector2Int(-1, 1),
                    new Vector2Int(-1, -1)
                });
            }
            
            foreach (var direction in directions)
            {
                var adjacentPos = position + direction;
                if (adjacentPos.x >= 0 && adjacentPos.x < gridWidth &&
                    adjacentPos.y >= 0 && adjacentPos.y < gridHeight)
                {
                    adjacentCells.Add(adjacentPos);
                }
            }
            
            return adjacentCells;
        }
        
        /// <summary>
        /// Enhanced placement validation with all new systems
        /// </summary>
        public bool CanPlaceIngredientEnhanced(Ingredient ingredient, Vector2Int position)
        {
            // Basic placement check
            if (!CanPlaceIngredient(ingredient, position))
            {
                return false;
            }
            
            // Check enhanced systems constraints (if layering is enabled)
            if (enhancedGridSystem != null && enhancedGridSystem.CanOverlapAt(position, ingredient))
            {
                // Enhanced placement is allowed, continue
            }
            
            // Check template constraints if active
            if (templateSystem != null && templateSystem.IsTemplateActive)
            {
                // Template is active - basic placement should be allowed
                // Template validation happens during progress updates
            }
            
            return true;
        }

        /// <summary>
        /// Debug helper to show all enhanced system states
        /// </summary>
        [ContextMenu("Debug Enhanced Systems State")]
        public void DebugEnhancedSystemsState()
        {
            Debug.Log("🔍 === ENHANCED SYSTEMS STATE DEBUG ===");
            
            Debug.Log($"🔮 Synergy System: {(synergySystem != null ? "Active" : "Missing")}");
            if (synergySystem != null)
            {
                Debug.Log($"   - Active synergies: {synergySystem.GetActiveSynergies().Count}");
            }
            
            Debug.Log($"⚠️ Failure System: {(failureSystem != null ? "Active" : "Missing")}");
            if (failureSystem != null)
            {
                Debug.Log($"   - Failure system active");
            }
            
            Debug.Log($"🌳 Skill Tree: {(skillTree != null ? "Active" : "Missing")}");
            if (skillTree != null)
            {
                Debug.Log($"   - Available points: {skillTree.GetAvailableSkillPoints()}");
                Debug.Log($"   - Unlocked skills: {skillTree.GetUnlockedSkills().Count}");
            }
            
            Debug.Log($"🗺️ Template System: {(templateSystem != null ? "Active" : "Missing")}");
            if (templateSystem != null)
            {
                Debug.Log($"   - Active template: {(templateSystem.IsTemplateActive ? templateSystem.CurrentTemplate.templateName : "None")}");
                Debug.Log($"   - Discovered templates: {templateSystem.GetDiscoveredTemplates().Count}");
            }
            
            Debug.Log($"🔄 Enhanced Grid: {(enhancedGridSystem != null ? "Active" : "Missing")}");
            if (enhancedGridSystem != null)
            {
                var stats = enhancedGridSystem.GetExpansionStats();
                Debug.Log($"   - Current size: {stats.currentSize.x}x{stats.currentSize.y}");
                Debug.Log($"   - Efficiency: {enhancedGridSystem.GetGridEfficiencyScore():P}");
            }
            
            Debug.Log($"📊 Grid Stats:");
            Debug.Log($"   - Size: {gridWidth}x{gridHeight}");
            Debug.Log($"   - Obstacles: {aspectObstacles.Count}");
            Debug.Log($"   - Efficiency: {GetGridEfficiencyScore():P}");
        }

        /// <summary>
        /// Check if the current session is in free crafting mode
        /// </summary>
        private bool IsFreeCraftingMode()
        {
            // Check if there's a CraftingModeSelector in the scene
            var modeSelector = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
            if (modeSelector != null)
            {
                // Check if it's in Free crafting mode
                return modeSelector.GetCurrentMode() == FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector.CraftingMode.Free;
            }
            
            // Fallback: Detect based on grid characteristics
            // Free crafting typically uses a 3x3 grid with no obstacles enabled
            bool isSmallGrid = (gridWidth == 3 && gridHeight == 3);
            bool hasNoObstacles = !enableObstacles || aspectObstacles.Count == 0;
            
            if (isSmallGrid && hasNoObstacles)
            {
                Debug.Log("🎮 Detected free crafting mode based on grid characteristics (3x3, no obstacles)");
                return true;
            }
            
            return false;
        }
        #endregion
    }
}