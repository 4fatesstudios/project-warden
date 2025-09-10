using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
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
            Debug.Log("🔄 InitializeComponents called - this will recreate the grid!");
            Debug.Log($"Stack trace: {System.Environment.StackTrace}");

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
                Debug.Log("🧪 Added PlacementTester component for runtime debugging");
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
                    Debug.Log($"Auto-assigned camera: {gameCamera.name}");
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
        }
        
        private void LoadTestIngredients()
        {
            Debug.Log("🧪 Loading test ingredients from Resources...");
            
            // Try to load from different possible locations
            string[] possiblePaths = {
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
                    Debug.Log($"✅ Loaded ingredient: {ingredient.ItemName}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Could not load ingredient from: {path}");
                }
            }
            
            Debug.Log($"🧪 Total ingredients loaded: {availableIngredients.Count}");
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
            Debug.Log($"🎯 === TryPlaceIngredient: {ingredient.ItemName} at {position} ===");

            bool canPlace = CanPlaceIngredient(ingredient, position);
            Debug.Log($"🎯 CanPlaceIngredient result: {canPlace}");

            if (canPlace)
            {
                Debug.Log($"✅ Placement approved! Delegating to IngredientPlacer for unified placement");
                
                // Let IngredientPlacer handle both visual placement AND grid occupancy as a unified operation
                ingredientPlacer.PlaceIngredient(ingredient, position);
                
                // Simple verification that IngredientPlacer did its job correctly
                bool placementSuccess = VerifyIngredientPlacement(ingredient, position);
                if (placementSuccess)
                {
                    Debug.Log($"✅ TryPlaceIngredient: SUCCESS - {ingredient.ItemName} placed and verified at {position}");
                    visualizer.RefreshGrid();
                    DebugGridStateAfterPlacement(ingredient, position);
                    return true;
                }
                else
                {
                    Debug.LogError($"🚨 TryPlaceIngredient: FAILED - IngredientPlacer could not complete placement for {ingredient.ItemName} at {position}");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"❌ Cannot place {ingredient.ItemName} at {position} - collision detected!");
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
            if (enableCollisionDebugLogging)
                Debug.Log($"CanPlaceIngredientWithShape: {ingredient.ItemName} at {position}");

            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            if (enableCollisionDebugLogging)
                Debug.Log($"Shape dimensions: {shapeWidth}x{shapeHeight}");

            // Check bounds
            if (position.x + shapeWidth > gridWidth || position.y + shapeHeight > gridHeight)
            {
                if (enableCollisionDebugLogging)
                    Debug.Log(
                        $"Out of bounds: position {position} + shape ({shapeWidth},{shapeHeight}) exceeds grid ({gridWidth},{gridHeight})");
                return false;
            }

            // Debug: Print shape pattern
            if (enableCollisionDebugLogging)
            {
                Debug.Log("Shape pattern:");
                for (int y = shapeHeight - 1; y >= 0; y--)
                {
                    string row = $"Y={y}: ";
                    for (int x = 0; x < shapeWidth; x++)
                    {
                        row += shape[x, y] ? "[#]" : "[ ]";
                    }

                    Debug.Log(row);
                }
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
                            if (enableCollisionDebugLogging)
                                Debug.Log(
                                    $"Shape cell ({x},{y}) -> Grid cell ({cellPos.x},{cellPos.y}) is out of bounds");
                            return false;
                        }

                        // Check if occupied
                        bool isOccupied = gridCells[cellPos.x, cellPos.y].IsOccupied;
                        string occupant = gridCells[cellPos.x, cellPos.y].OccupiedByIngredient?.ItemName ?? "None";

                        if (enableCollisionDebugLogging)
                            Debug.Log(
                                $"Shape cell ({x},{y}) -> Grid cell ({cellPos.x},{cellPos.y}): Occupied = {isOccupied}, By = {occupant}");

                        if (isOccupied)
                        {
                            if (enableCollisionDebugLogging)
                                Debug.Log($"COLLISION: Cell ({cellPos.x},{cellPos.y}) is occupied by {occupant}");
                            return false;
                        }
                    }
                }
            }

            if (enableCollisionDebugLogging)
                Debug.Log("No collisions detected - placement allowed");
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
            if (enableCollisionDebugLogging)
                Debug.Log($"MarkCellsAsOccupied: {ingredient.ItemName} at {position}");

            // Use shape data if available, fallback to rectangle
            if (ingredient.ShapeData != null)
            {
                if (enableCollisionDebugLogging)
                    Debug.Log($"Using shape-based marking for {ingredient.ItemName}");
                MarkCellsWithShape(ingredient, position);
            }
            else
            {
                if (enableCollisionDebugLogging)
                    Debug.Log($"Using rectangle-based marking for {ingredient.ItemName}");
                MarkCellsRectangle(ingredient, position);
            }

            // Verify that cells were actually marked
            if (enableCollisionDebugLogging)
            {
                Debug.Log($"Verification: Checking if cells were properly marked for {ingredient.ItemName}");
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
                                Debug.Log(
                                    $"  VERIFY: Cell ({cellPos.x},{cellPos.y}) -> Occupied = {isOccupied}, By = {occupant}");

                                if (!isOccupied)
                                {
                                    Debug.LogError(
                                        $"  ❌ MARKING FAILED: Cell ({cellPos.x},{cellPos.y}) should be occupied but isn't!");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MarkCellsWithShape(Ingredient ingredient, Vector2Int position)
        {
            if (enableCollisionDebugLogging)
                Debug.Log($"MarkCellsWithShape: {ingredient.ItemName} at {position}");

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
                            if (enableCollisionDebugLogging)
                                Debug.Log(
                                    $"  Marking cell ({cellPos.x},{cellPos.y}) as occupied by {ingredient.ItemName}");
                            gridCells[cellPos.x, cellPos.y].SetOccupied(ingredient);
                        }
                        else
                        {
                            if (enableCollisionDebugLogging)
                                Debug.LogError($"  Trying to mark out-of-bounds cell ({cellPos.x},{cellPos.y})!");
                        }
                    }
                }
            }
        }

        private void MarkCellsRectangle(Ingredient ingredient, Vector2Int position)
        {
            if (enableCollisionDebugLogging)
                Debug.Log(
                    $"MarkCellsRectangle: {ingredient.ItemName} at {position}, size {ingredient.GridWidth}x{ingredient.GridHeight}");

            // Fallback to original rectangle-based marking
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    Vector2Int cellPos = position + new Vector2Int(x, y);
                    if (enableCollisionDebugLogging)
                        Debug.Log(
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
                Debug.Log($"🔍 Verifying grid state on ingredient selection: {ingredient?.ItemName ?? "None"}");
                VerifyAllPlacedIngredientsQuiet();
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
                    
                    Debug.Log($"🔍 Cell ({cellPos.x},{cellPos.y}): Occupied={isOccupied}, By={occupantName}, CorrectIngredient={occupiedByCorrectIngredient}");
                    
                    if (isOccupied && occupiedByCorrectIngredient)
                    {
                        properlyOccupiedCells++;
                    }
                    else if (!isOccupied)
                    {
                        Debug.LogError($"🚨 VERIFICATION ERROR: Cell ({cellPos.x},{cellPos.y}) should be occupied by {ingredient.ItemName} but is empty!");
                    }
                    else if (isOccupied && !occupiedByCorrectIngredient)
                    {
                        Debug.LogError($"🚨 VERIFICATION ERROR: Cell ({cellPos.x},{cellPos.y}) occupied by wrong ingredient: {occupantName} instead of {ingredient.ItemName}!");
                    }
                }
                else
                {
                    Debug.LogError($"🚨 VERIFICATION ERROR: Could not get cell at ({cellPos.x},{cellPos.y}) - out of bounds?");
                }
            }
            
            bool verificationPassed = (properlyOccupiedCells == totalExpectedCells);
            
            Debug.Log($"🔍 Verification result: {properlyOccupiedCells}/{totalExpectedCells} cells properly occupied = {(verificationPassed ? "PASS" : "FAIL")}");
            
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
            
            Debug.Log($"🔍 Verification complete: {verifiedIngredients}/{totalIngredients} ingredients properly occupy their cells");
            
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
                    Debug.LogWarning($"🔍 QUIET CHECK: {instance.ingredient.ItemName} at {instance.gridPosition} verification FAILED");
                }
            }
            
            if (failedIngredients > 0)
            {
                Debug.LogWarning($"🔍 QUIET CHECK: {failedIngredients}/{placedIngredients.Count} ingredients failed verification");
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
                        Debug.LogWarning($"🔍 Cell ({cellPos.x},{cellPos.y}) should be occupied by {ingredient.ItemName} but is empty!");
                    }
                    else if (isOccupied && !occupiedByCorrectIngredient)
                    {
                        Debug.LogWarning($"🔍 Cell ({cellPos.x},{cellPos.y}) occupied by wrong ingredient: {cell.OccupiedByIngredient?.ItemName} instead of {ingredient.ItemName}!");
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
                                        var expectedCells = GetIngredientCells(instance.ingredient, instance.gridPosition);
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
                                    Debug.LogError($"🚨 INCONSISTENT: Cell ({x},{y}) claims to be occupied by {cell.OccupiedByIngredient.ItemName} but no matching placed ingredient found!");
                                }
                            }
                            else
                            {
                                inconsistentCells++;
                                Debug.LogError($"🚨 INCONSISTENT: Cell ({x},{y}) is marked occupied but has no OccupiedByIngredient!");
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
        /// Toggle debug logging on/off
        /// </summary>
        [ContextMenu("Toggle Debug Logging")]
        public void ToggleDebugLogging()
        {
            enableCollisionDebugLogging = !enableCollisionDebugLogging;
            Debug.Log($"Collision debug logging: {(enableCollisionDebugLogging ? "ENABLED" : "DISABLED")}");
        }

        /// <summary>
        /// Toggle continuous verification on/off
        /// </summary>
        [ContextMenu("Toggle Continuous Verification")]
        public void ToggleContinuousVerification()
        {
            enableContinuousVerification = !enableContinuousVerification;
            Debug.Log($"Continuous verification: {(enableContinuousVerification ? "ENABLED" : "DISABLED")} (Interval: {verificationInterval}s)");
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
            Debug.LogWarning("🔍 FRAME-BY-FRAME VERIFICATION ENABLED - This is very intensive! Only use for debugging.");
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

        [Header("Debug Settings")]
        private bool enableCollisionDebugLogging = true;
        
        [Header("Continuous Verification")]
        [SerializeField] private bool enableContinuousVerification = true;
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
            Debug.Log($"🧪 FORCE PLACING {currentSelectedIngredient.ItemName} at {testPosition} (bypassing collision detection)");
            
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
            
            Debug.Log($"🔍 === SHAPE COLLISION DEBUG: {ingredient.ItemName} at {position} ===");
            
            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);
            
            Debug.Log($"📐 Shape size: {shapeWidth}x{shapeHeight}");
            
            // Show the shape pattern
            Debug.Log("📋 Shape pattern:");
            for (int y = shapeHeight - 1; y >= 0; y--)
            {
                string row = $"  Y={y}: ";
                for (int x = 0; x < shapeWidth; x++)
                {
                    row += shape[x, y] ? "[#]" : "[ ]";
                }
                Debug.Log(row);
            }
            
            // Check each cell that would be occupied
            bool hasCollisions = false;
            Debug.Log("🔍 Checking all shape cells:");
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
                            Debug.Log($"  ❌ BOUNDS: Shape cell ({x},{y}) → Grid cell ({cellPos.x},{cellPos.y}) is OUT OF BOUNDS");
                            hasCollisions = true;
                            continue;
                        }
                        
                        // Check occupation
                        bool isOccupied = gridCells[cellPos.x, cellPos.y].IsOccupied;
                        string occupant = gridCells[cellPos.x, cellPos.y].OccupiedByIngredient?.ItemName ?? "None";
                        
                        if (isOccupied)
                        {
                            Debug.Log($"  ❌ COLLISION: Shape cell ({x},{y}) → Grid cell ({cellPos.x},{cellPos.y}) occupied by {occupant}");
                            hasCollisions = true;
                        }
                        else
                        {
                            Debug.Log($"  ✅ FREE: Shape cell ({x},{y}) → Grid cell ({cellPos.x},{cellPos.y}) is available");
                        }
                    }
                }
            }
            
            string result = hasCollisions ? "❌ PLACEMENT BLOCKED" : "✅ PLACEMENT ALLOWED";
            Debug.Log($"🎯 RESULT: {result}");
        }
        
        public void ClearGrid()
        {
            Debug.LogWarning("🧹 ClearGrid() called! This will clear all placed ingredients but keep the grid visualization.");
            
            Debug.Log("🧹 Grid state BEFORE clearing:");
            DebugGridState();
            
            // Clear grid data (ingredient occupancy only)
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridCells[x, y].IsOccupied)
                    {
                        Debug.Log($"🧹 Clearing occupied cell ({x},{y}) with {gridCells[x, y].OccupiedByIngredient?.ItemName}");
                        gridCells[x, y].Clear(); // Only clear occupancy, not the visual grid cell
                    }
                }
            }
            
            // Clear visual ingredients with detailed logging (but preserve grid visualization)
            Debug.Log("🧹 Calling IngredientPlacer.ClearAllIngredients()...");
            if (ingredientPlacer != null)
            {
                int ingredientCountBefore = ingredientPlacer.GetAllPlacedIngredients().Count;
                Debug.Log($"🧹 Ingredients to clear: {ingredientCountBefore}");
                
                ingredientPlacer.ClearAllIngredients();
                
                int ingredientCountAfter = ingredientPlacer.GetAllPlacedIngredients().Count;
                Debug.Log($"🧹 Ingredients remaining after clear: {ingredientCountAfter}");
            }
            else
            {
                Debug.LogError("🚨 IngredientPlacer is null! Cannot clear visual ingredients.");
            }
            
            // Refresh grid visualization (this will update colors but preserve the grid)
            Debug.Log("🧹 Refreshing grid visualization...");
            if (visualizer != null)
            {
                visualizer.RefreshGrid(); // This should update the grid colors but keep the grid structure
            }
            else
            {
                Debug.LogError("🚨 GridVisualizer is null! Cannot refresh grid.");
            }
            
            Debug.Log("🧹 Grid state AFTER clearing:");
            DebugGridState();
            
            Debug.Log("✅ ClearGrid() complete! Grid structure preserved, ingredients removed.");
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
                    Debug.LogWarning($"🔍 CONTINUOUS CHECK: {instance.ingredient.ItemName} at {instance.gridPosition} has lost cell occupancy!");
                    
                    // Auto-repair: use IngredientPlacer's repair method
                    Debug.Log($"🔧 Auto-repairing occupancy for {instance.ingredient.ItemName} using IngredientPlacer");
                    
                    bool repairSuccess = ingredientPlacer.RepairIngredientOccupancy(instance.ingredient, instance.gridPosition);
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
                Debug.LogWarning($"🔍 CONTINUOUS CHECK: Found and repaired {inconsistentIngredients} ingredients with occupancy issues");
                visualizer?.RefreshGrid();
            }
        }
    }
}