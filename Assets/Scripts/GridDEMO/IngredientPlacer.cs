using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine.UI;
// Temporary using to force recompilation in Unity 6
using System;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class IngredientPlacer : MonoBehaviour
    {
        [Header("Ingredient Visualization")] public GameObject ingredientPrefab;
        public Material[] aspectMaterials;

        [Header("Text Labels")] [SerializeField]
        private bool enableIngredientLabels = false; // Disabled by default to avoid multiple planes

        [SerializeField] private bool use3DTextLabels = false; // Option to use 3D text if needed
        [SerializeField] private bool useUITextLabels = true; // Use UI Canvas text instead

        private GridGameManager gridManager;
        private GridVisualizer gridVisualizer;
        private IngredientEffectVisualizer effectVisualizer;

        private Dictionary<Vector2Int, IngredientInstance> placedIngredients =
            new Dictionary<Vector2Int, IngredientInstance>();

        private Canvas ingredientLabelCanvas; // Single canvas for all labels

        private void Awake()
        {
            gridManager = GetComponent<GridGameManager>();
            gridVisualizer = GetComponent<GridVisualizer>();
            effectVisualizer = GetComponent<IngredientEffectVisualizer>();
            
            // Add effect visualizer if it doesn't exist
            if (effectVisualizer == null)
            {
                effectVisualizer = gameObject.AddComponent<IngredientEffectVisualizer>();
                Debug.Log("🎨 Added IngredientEffectVisualizer component");
            }
        }

        private void Start()
        {
            if (ingredientPrefab == null)
            {
                ingredientPrefab = CreateDefaultIngredientPrefab();
            }
        }

        private GameObject CreateDefaultIngredientPrefab()
        {
            GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "IngredientVisual";

            // Add a slight scale to make it stand out from grid cells
            prefab.transform.localScale = Vector3.one * 0.8f;

            // Add basic material
            MeshRenderer renderer = prefab.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.white;
            renderer.material = mat;

            return prefab;
        }

        public void PlaceIngredient(Ingredient ingredient, Vector2Int gridPosition)
        {
            if (placedIngredients.ContainsKey(gridPosition))
            {
                RemoveIngredient(gridPosition);
            }

            // Mark grid cells as occupied first
            bool occupancySuccess = MarkGridCellsOccupied(ingredient, gridPosition);
            if (!occupancySuccess)
            {
                Debug.LogError($"Failed to mark grid cells occupied for {ingredient.ItemName} at {gridPosition}");
                return;
            }

            // Create visual representation 
            Vector3 worldPosition = gridManager.GridToWorldPosition(gridPosition);
            GameObject ingredientObj = CreateIngredientVisual(ingredient, gridPosition, worldPosition);

            if (ingredientObj == null)
            {
                Debug.LogError($"Failed to create visual for {ingredient.ItemName}");
                ClearGridCellsOccupied(ingredient, gridPosition);
                return;
            }

            // Store the ingredient instance
            IngredientInstance instance = new IngredientInstance
            {
                ingredient = ingredient,
                gridPosition = gridPosition,
                visualObject = ingredientObj,
                placementTime = Time.time
            };

            placedIngredients[gridPosition] = instance;

            // Effects and reactions
            gridVisualizer.PlayPlacementEffect(gridPosition, ingredient);
            CheckForReactions(gridPosition, ingredient);
            
            // Check for ingredient effect interactions (new sparkle/reaction system)
            if (effectVisualizer != null)
            {
                effectVisualizer.CheckIngredientInteractions(gridPosition, ingredient);
            }

            // Clear ingredient selection and highlights after successful placement
            Debug.Log("🔄 Clearing ingredient selection after successful placement");
            if (gridManager != null)
            {
                gridManager.ClearIngredientSelection();
            }
            
            // Force grid visual refresh to ensure cells reset to white
            Debug.Log("🔄 Refreshing grid visuals to reset cell colors to white");
            if (gridVisualizer != null)
            {
                gridVisualizer.RefreshGrid();
            }

            // Final verification
            bool verificationPassed = VerifyPlacementIntegrity(ingredient, gridPosition);
            if (!verificationPassed)
            {
                Debug.LogError($"Placement verification failed for {ingredient.ItemName} at {gridPosition}");
            }
        }

        private GameObject CreateIngredientVisual(Ingredient ingredient, Vector2Int gridPosition, Vector3 worldPosition)
        {
            if (ingredient.ShapeData != null)
            {
                return CreateShapeBasedVisual(ingredient, gridPosition, worldPosition);
            }
            else
            {
                return CreateRectangleVisual(ingredient, gridPosition, worldPosition);
            }
        }

        private GameObject CreateShapeBasedVisual(Ingredient ingredient, Vector2Int gridPosition, Vector3 worldPosition)
        {
            GameObject container = new GameObject($"{ingredient.ItemName}_{gridPosition.x}_{gridPosition.y}");
            container.transform.position = worldPosition;
            container.transform.parent = transform;

            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            // Create individual cubes for each active cell in the shape
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    if (shape[x, y])
                    {
                        GameObject cellObj = Instantiate(ingredientPrefab);
                        cellObj.name = $"Cell_{x}_{y}";
                        cellObj.transform.parent = container.transform;

                        // Position relative to grid
                        Vector3 cellWorldPos = gridManager.GridToWorldPosition(gridPosition + new Vector2Int(x, y));
                        cellObj.transform.position = cellWorldPos;

                        // Configure individual cell
                        ConfigureIngredientCell(cellObj, ingredient, x, y);
                    }
                }
            }

            // Configure the container
            if (enableIngredientLabels)
            {
                CreateIngredientLabel(container, ingredient);
            }

            // Add interaction component to container
            IngredientInteraction interaction = container.AddComponent<IngredientInteraction>();
            interaction.Initialize(ingredient, ingredient.ItemName);

            return container;
        }

        private GameObject CreateRectangleVisual(Ingredient ingredient, Vector2Int gridPosition, Vector3 worldPosition)
        {
            // Adjust position for multi-cell ingredients
            if (ingredient.GridWidth > 1 || ingredient.GridHeight > 1)
            {
                Vector3 offset = new Vector3(
                    (ingredient.GridWidth - 1) * gridManager.cellSize * 0.5f,
                    0f,
                    (ingredient.GridHeight - 1) * gridManager.cellSize * 0.5f
                );
                worldPosition += offset;
            }

            // Create ingredient visual
            GameObject ingredientObj = Instantiate(ingredientPrefab, worldPosition, Quaternion.identity, transform);
            ingredientObj.name = $"{ingredient.ItemName}_{gridPosition.x}_{gridPosition.y}";

            // Configure the ingredient visual
            ConfigureIngredientVisual(ingredientObj, ingredient);

            return ingredientObj;
        }

        private void ConfigureIngredientCell(GameObject cellObj, Ingredient ingredient, int cellX, int cellY)
        {
            // Standard cube size for shape-based ingredients
            cellObj.transform.localScale = Vector3.one * gridManager.cellSize * 0.8f;

            // Configure material and color based on aspect
            MeshRenderer renderer = cellObj.GetComponent<MeshRenderer>();
            Material instanceMaterial = new Material(renderer.material);

            // Set color based on aspect with slight variation per cell for visual interest
            Color aspectColor = GetAspectColor(ingredient.IngredientAspect);

            // Add slight variation based on cell position for visual interest
            float variation = (cellX + cellY) * 0.1f;
            aspectColor = Color.Lerp(aspectColor, Color.white, variation * 0.2f);

            instanceMaterial.color = aspectColor;

            // Add emission based on potency
            float emissionIntensity = ingredient.Potency / 5f;
            instanceMaterial.SetColor("_EmissionColor", aspectColor * emissionIntensity);
            instanceMaterial.EnableKeyword("_EMISSION");

            renderer.material = instanceMaterial;
        }

        private void ConfigureIngredientVisual(GameObject ingredientObj, Ingredient ingredient)
        {
            // Scale based on grid size
            Vector3 scale = new Vector3(
                ingredient.GridWidth * gridManager.cellSize * 0.8f,
                0.2f + (ingredient.Potency * 0.1f), // Height based on potency
                ingredient.GridHeight * gridManager.cellSize * 0.8f
            );
            ingredientObj.transform.localScale = scale;

            // Configure material and color based on aspect
            MeshRenderer renderer = ingredientObj.GetComponent<MeshRenderer>();
            Material instanceMaterial = new Material(renderer.material);

            // Set color based on aspect
            Color aspectColor = GetAspectColor(ingredient.IngredientAspect);
            instanceMaterial.color = aspectColor;

            // Add emission based on potency
            float emissionIntensity = ingredient.Potency / 5f;
            instanceMaterial.SetColor("_EmissionColor", aspectColor * emissionIntensity);
            instanceMaterial.EnableKeyword("_EMISSION");

            renderer.material = instanceMaterial;

            // Add ingredient label (optional to avoid multiple planes)
            if (enableIngredientLabels)
            {
                CreateIngredientLabel(ingredientObj, ingredient);
            }

            // Add interaction component
            IngredientInteraction interaction = ingredientObj.AddComponent<IngredientInteraction>();
            interaction.Initialize(ingredient, ingredient.ItemName);
        }

        private void CreateIngredientLabel(GameObject ingredientObj, Ingredient ingredient)
        {
            if (use3DTextLabels)
            {
                // Create traditional 3D TextMesh (creates a plane)
                CreateLegacy3DTextLabel(ingredientObj, ingredient);
            }
            else if (useUITextLabels)
            {
                // Create UI Canvas-based text (no planes, better performance)
                CreateUITextLabel(ingredientObj, ingredient);
            }
        }

        private void CreateLegacy3DTextLabel(GameObject ingredientObj, Ingredient ingredient)
        {
            // Create a simple text label above the ingredient (creates a plane)
            GameObject labelObj = new GameObject("Label_3D");
            labelObj.transform.parent = ingredientObj.transform;
            labelObj.transform.localPosition = Vector3.up * 0.5f;
            labelObj.transform.rotation = Quaternion.LookRotation(UnityEngine.Camera.main.transform.forward);

            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = ingredient.ItemName;
            textMesh.fontSize = 20;
            textMesh.color = Color.white;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;

            // Add outline for better visibility
            MeshRenderer textRenderer = labelObj.GetComponent<MeshRenderer>();
            textRenderer.material = new Material(Shader.Find("GUI/Text Shader"));

            Debug.Log($"IngredientPlacer: Created 3D text label for {ingredient.ItemName}");
        }

        private void CreateUITextLabel(GameObject ingredientObj, Ingredient ingredient)
        {
            // Ensure we have a single canvas for all labels
            if (ingredientLabelCanvas == null)
            {
                CreateLabelCanvas();
            }

            // Create UI text element (no planes, much better performance)
            GameObject labelObj = new GameObject($"Label_UI_{ingredient.ItemName}");
            labelObj.transform.SetParent(ingredientLabelCanvas.transform, false);

            // Add UI Text component
            UnityEngine.UI.Text uiText = labelObj.AddComponent<UnityEngine.UI.Text>();
            uiText.text = ingredient.ItemName;
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = 14;
            uiText.color = Color.white;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.raycastTarget = false; // Don't block clicks

            // Add outline for better visibility
            UnityEngine.UI.Outline textOutline = labelObj.AddComponent<UnityEngine.UI.Outline>();
            textOutline.effectColor = Color.black;
            textOutline.effectDistance = new Vector2(1, 1);

            // Setup RectTransform
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(100, 30);

            // Position the label above the ingredient using world to screen conversion
            StartCoroutine(UpdateUILabelPosition(labelObj, ingredientObj));

            Debug.Log($"IngredientPlacer: Created UI text label for {ingredient.ItemName}");
        }

        private void CreateLabelCanvas()
        {
            GameObject canvasObj = new GameObject("Ingredient Labels Canvas");
            ingredientLabelCanvas = canvasObj.AddComponent<Canvas>();
            ingredientLabelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            ingredientLabelCanvas.sortingOrder = 100; // Above other UI elements

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("IngredientPlacer: Created canvas for ingredient labels");
        }

        private System.Collections.IEnumerator UpdateUILabelPosition(GameObject labelObj, GameObject ingredientObj)
        {
            while (labelObj != null && ingredientObj != null)
            {
                // Convert world position to screen position
                Vector3 worldPos = ingredientObj.transform.position + Vector3.up * 0.5f;
                Vector2 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(worldPos);

                // Update label position
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.position = screenPos;

                yield return new WaitForSeconds(0.1f); // Update 10 times per second
            }
        }

        public void RemoveIngredient(Vector2Int gridPosition)
        {
            if (placedIngredients.TryGetValue(gridPosition, out IngredientInstance instance))
            {
                // Clear grid occupancy first
                ClearGridCellsOccupied(instance.ingredient, gridPosition);

                // Play removal effect
                gridVisualizer.PlayRemovalEffect(gridPosition);

                // Destroy visual object
                if (instance.visualObject != null)
                {
                    Destroy(instance.visualObject);
                }

                placedIngredients.Remove(gridPosition);
            }
        }

        /// <summary>
        /// Marks grid cells as occupied by the ingredient. Handles both shape-based and rectangle ingredients.
        /// </summary>
        private bool MarkGridCellsOccupied(Ingredient ingredient, Vector2Int position)
        {
            try
            {
                var cellsToOccupy = GetIngredientCells(ingredient, position);
                int successfullyMarked = 0;

                foreach (var cellPos in cellsToOccupy)
                {
                    var cell = gridManager.GetCell(cellPos.x, cellPos.y);
                    if (cell != null)
                    {
                        if (cell.IsOccupied)
                        {
                            Debug.LogError(
                                $"Cell ({cellPos.x},{cellPos.y}) already occupied by {cell.OccupiedByIngredient?.ItemName}");
                            RollbackCellMarking(cellsToOccupy, successfullyMarked);
                            return false;
                        }

                        cell.SetOccupied(ingredient);
                        successfullyMarked++;
                    }
                    else
                    {
                        Debug.LogError($"Cell ({cellPos.x},{cellPos.y}) is out of bounds");
                        RollbackCellMarking(cellsToOccupy, successfullyMarked);
                        return false;
                    }
                }

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception in MarkGridCellsOccupied: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears grid cell occupancy for the ingredient
        /// </summary>
        private void ClearGridCellsOccupied(Ingredient ingredient, Vector2Int position)
        {
            var cellsToClear = GetIngredientCells(ingredient, position);

            foreach (var cellPos in cellsToClear)
            {
                var cell = gridManager.GetCell(cellPos.x, cellPos.y);
                if (cell != null && cell.IsOccupied && cell.OccupiedByIngredient == ingredient)
                {
                    cell.Clear();
                }
            }
        }

        /// <summary>
        /// Gets all grid positions that should be occupied by the ingredient
        /// </summary>
        private List<Vector2Int> GetIngredientCells(Ingredient ingredient, Vector2Int position)
        {
            List<Vector2Int> cells = new List<Vector2Int>();

            if (ingredient.ShapeData != null)
            {
                // Shape-based ingredient
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
                            cells.Add(cellPos);
                        }
                    }
                }
            }
            else
            {
                // Rectangle-based ingredient
                for (int x = 0; x < ingredient.GridWidth; x++)
                {
                    for (int y = 0; y < ingredient.GridHeight; y++)
                    {
                        Vector2Int cellPos = position + new Vector2Int(x, y);
                        cells.Add(cellPos);
                    }
                }
            }

            return cells;
        }

        /// <summary>
        /// Rollback cell marking if placement fails partway through
        /// </summary>
        private void RollbackCellMarking(List<Vector2Int> cellsToOccupy, int successfullyMarked)
        {
            for (int i = 0; i < successfullyMarked && i < cellsToOccupy.Count; i++)
            {
                var cellPos = cellsToOccupy[i];
                var cell = gridManager.GetCell(cellPos.x, cellPos.y);
                if (cell != null)
                {
                    cell.Clear();
                }
            }
        }

        /// <summary>
        /// Verifies that visual placement and grid occupancy are properly synchronized
        /// </summary>
        private bool VerifyPlacementIntegrity(Ingredient ingredient, Vector2Int position)
        {
            // Check that we have visual tracking
            if (!placedIngredients.ContainsKey(position))
            {
                Debug.LogError($"No visual record found for {ingredient.ItemName} at {position}");
                return false;
            }

            // Check that all required grid cells are occupied
            var expectedCells = GetIngredientCells(ingredient, position);

            foreach (var cellPos in expectedCells)
            {
                var cell = gridManager.GetCell(cellPos.x, cellPos.y);
                if (cell == null || !cell.IsOccupied || cell.OccupiedByIngredient != ingredient)
                {
                    Debug.LogError($"Cell ({cellPos.x},{cellPos.y}) not properly occupied by {ingredient.ItemName}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Context menu method for easy testing
        /// </summary>
        [ContextMenu("Force Clear All Ingredients")]
        public void ForceClearAllIngredients()
        {
            Debug.LogWarning("🧹 FORCE CLEAR called from context menu!");
            ClearAllIngredients();
        }

        public void ClearAllIngredients()
        {
            Debug.Log(
                $"🧹 IngredientPlacer.ClearAllIngredients() called - clearing {placedIngredients.Count} ingredients");

            int destroyedCount = 0;
            int errorCount = 0;

            foreach (var kvp in placedIngredients)
            {
                var instance = kvp.Value;

                try
                {
                    // Clear grid occupancy
                    ClearGridCellsOccupied(instance.ingredient, instance.gridPosition);

                    // Destroy visual object
                    if (instance.visualObject != null)
                    {
                        Debug.Log(
                            $"🧹 Destroying visual object for {instance.ingredient.ItemName} at {instance.gridPosition}");
                        DestroyImmediate(instance.visualObject); // Use DestroyImmediate for immediate effect
                        destroyedCount++;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"⚠️ Visual object for {instance.ingredient.ItemName} at {instance.gridPosition} is already null");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"🚨 Error clearing ingredient {instance.ingredient.ItemName}: {e.Message}");
                    errorCount++;
                }
            }

            placedIngredients.Clear();
            Debug.Log($"🧹 Cleared placedIngredients dictionary");

            // Clean up the label canvas if it exists
            if (ingredientLabelCanvas != null)
            {
                Debug.Log("🧹 Destroying ingredient label canvas");
                DestroyImmediate(ingredientLabelCanvas.gameObject);
                ingredientLabelCanvas = null;
            }
            
            // Clear all effect visualizations
            if (effectVisualizer != null)
            {
                effectVisualizer.ClearAllEffects();
            }

            // Additional cleanup: Find and destroy any remaining ingredient objects that might have been missed
            GameObject[] allIngredientObjects = GameObject.FindGameObjectsWithTag("Untagged");
            int additionalDestroyed = 0;

            foreach (GameObject obj in allIngredientObjects)
            {
                if (obj.transform.parent == this.transform && obj.name.Contains("_") &&
                    (obj.name.Contains("TestIngredient") || obj.name.Contains("Ingredient")))
                {
                    Debug.Log($"🧹 Found orphaned ingredient object: {obj.name} - destroying");
                    DestroyImmediate(obj);
                    additionalDestroyed++;
                }
            }

            Debug.Log(
                $"✅ ClearAllIngredients complete! Destroyed: {destroyedCount}, Errors: {errorCount}, Additional cleanup: {additionalDestroyed}");

            // Final safety cleanup: destroy any remaining child objects
            ForceDestroyAllChildIngredients();
        }

        /// <summary>
        /// Safety method to destroy ONLY child ingredient objects while preserving grid infrastructure
        /// </summary>
        private void ForceDestroyAllChildIngredients()
        {
            int childrenDestroyed = 0;
            int gridObjectsPreserved = 0;

            // Get all immediate children of this transform
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            // Selectively destroy only ingredient objects, not grid infrastructure
            foreach (Transform child in children)
            {
                if (child != null && child.gameObject != null)
                {
                    string childName = child.name;
                    
                    // Identify grid infrastructure objects that should be preserved
                    bool isGridCell = childName.StartsWith("Cell_") && (childName.Contains("_") && childName.Split('_').Length == 3);
                    bool isGridLine = childName.StartsWith("GridLine_");
                    bool isGridInfrastructure = isGridCell || isGridLine;
                    
                    // Identify ingredient objects that should be destroyed
                    bool isIngredientObject = (childName.Contains("TestIngredient") || 
                                             childName.Contains("Ingredient") || 
                                             childName.Contains("_")) && 
                                             !isGridInfrastructure;
                    
                    // Additional check: ingredients typically have format "IngredientName_x_y"
                    bool hasIngredientFormat = childName.Contains("_") && 
                                              childName.Split('_').Length >= 3 && 
                                              !childName.StartsWith("Cell_") && 
                                              !childName.StartsWith("GridLine_");
                    
                    if ((isIngredientObject || hasIngredientFormat) && !isGridInfrastructure)
                    {
                        Debug.Log($"🧹 Force destroying child ingredient object: {childName}");
                        DestroyImmediate(child.gameObject);
                        childrenDestroyed++;
                    }
                    else if (isGridInfrastructure)
                    {
                        Debug.Log($"🔒 Preserving grid infrastructure: {childName}");
                        gridObjectsPreserved++;
                    }
                    else
                    {
                        Debug.Log($"🤔 Unknown child object (preserving): {childName}");
                        gridObjectsPreserved++;
                    }
                }
            }

            if (childrenDestroyed > 0 || gridObjectsPreserved > 0)
            {
                Debug.Log($"🧹 Force cleanup complete! Destroyed: {childrenDestroyed} ingredient objects, Preserved: {gridObjectsPreserved} grid objects");
            }
            else
            {
                Debug.Log($"🧹 Force cleanup found no objects to destroy or preserve");
            }
        }

        private void CheckForReactions(Vector2Int position, Ingredient ingredient)
        {
            // Check adjacent cells for potential reactions
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int adjacentPos = position + dir;
                if (placedIngredients.TryGetValue(adjacentPos, out IngredientInstance adjacent))
                {
                    ProcessReaction(ingredient, adjacent.ingredient, position, adjacentPos);
                }
            }
        }

        private void ProcessReaction(Ingredient ingredient1, Ingredient ingredient2, Vector2Int pos1, Vector2Int pos2)
        {
            // Simple reaction system - can be expanded
            bool hasReaction = false;

            // Opposing elements react
            if ((ingredient1.IngredientAspect == Aspect.Scorch && ingredient2.IngredientAspect == Aspect.Frigid) ||
                (ingredient1.IngredientAspect == Aspect.Frigid && ingredient2.IngredientAspect == Aspect.Scorch))
            {
                hasReaction = true;
                Debug.Log($"Thermal reaction between {ingredient1.ItemName} and {ingredient2.ItemName}!");
            }

            // Life and Death reaction
            if ((ingredient1.IngredientAspect == Aspect.Corporeal && ingredient2.IngredientAspect == Aspect.Divine) ||
                (ingredient1.IngredientAspect == Aspect.Divine && ingredient2.IngredientAspect == Aspect.Corporeal))
            {
                hasReaction = true;
                Debug.Log($"Life/Death reaction between {ingredient1.ItemName} and {ingredient2.ItemName}!");
            }

            // Order and Chaos reaction
            if ((ingredient1.IngredientAspect == Aspect.Arc && ingredient2.IngredientAspect == Aspect.Caustic) ||
                (ingredient1.IngredientAspect == Aspect.Caustic && ingredient2.IngredientAspect == Aspect.Arc))
            {
                hasReaction = true;
                Debug.Log($"Order/Chaos reaction between {ingredient1.ItemName} and {ingredient2.ItemName}!");
            }

            if (hasReaction)
            {
                // Visual effect for reaction
                Vector3 midpoint = (gridManager.GridToWorldPosition(pos1) + gridManager.GridToWorldPosition(pos2)) *
                                   0.5f;
                CreateReactionEffect(midpoint, ingredient1, ingredient2);
            }
        }

        private void CreateReactionEffect(Vector3 position, Ingredient ingredient1, Ingredient ingredient2)
        {
            // Create a simple particle effect for reactions
            GameObject effect = new GameObject("ReactionEffect");
            effect.transform.position = position;

            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = Color.Lerp(GetAspectColor(ingredient1.IngredientAspect),
                GetAspectColor(ingredient2.IngredientAspect), 0.5f);
            main.maxParticles = 50;

            var emission = particles.emission;
            emission.rateOverTime = 50f;

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            Destroy(effect, 2f);
        }

        private Color GetAspectColor(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Scorch: return Color.red;
                case Aspect.Frigid: return Color.cyan;
                case Aspect.Arc: return Color.yellow;
                case Aspect.Caustic: return new Color(0.5f, 0.3f, 0.1f); // Brown
                case Aspect.Corporeal: return Color.gray;
                case Aspect.Divine: return Color.white;
                default: return Color.gray;
            }
        }

        public IngredientInstance GetIngredientAt(Vector2Int position)
        {
            // First check if there's an ingredient with this exact position as origin
            if (placedIngredients.TryGetValue(position, out IngredientInstance instance))
            {
                return instance;
            }

            // Check if this position is occupied by any ingredient
            foreach (var kvp in placedIngredients)
            {
                var ingredient = kvp.Value.ingredient;
                var originPos = kvp.Value.gridPosition;

                // Check if the position falls within this ingredient's occupied cells
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
                                Vector2Int cellPos = originPos + new Vector2Int(x, y);
                                if (cellPos == position)
                                {
                                    return kvp.Value;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Rectangle check
                    for (int x = 0; x < ingredient.GridWidth; x++)
                    {
                        for (int y = 0; y < ingredient.GridHeight; y++)
                        {
                            Vector2Int cellPos = originPos + new Vector2Int(x, y);
                            if (cellPos == position)
                            {
                                return kvp.Value;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public List<IngredientInstance> GetAllPlacedIngredients()
        {
            return new List<IngredientInstance>(placedIngredients.Values);
        }

        /// <summary>
        /// Public method for GridGameManager to repair occupancy issues during verification
        /// </summary>
        public bool RepairIngredientOccupancy(Ingredient ingredient, Vector2Int position)
        {
            // Clear any existing occupancy first
            ClearGridCellsOccupied(ingredient, position);

            // Re-mark cells as occupied
            bool success = MarkGridCellsOccupied(ingredient, position);

            if (!success)
            {
                Debug.LogError($"Failed to repair occupancy for {ingredient.ItemName} at {position}");
            }

            return success;
        }
    }

    [System.Serializable]
    public class IngredientInstance
    {
        public Ingredient ingredient;
        public Vector2Int gridPosition;
        public GameObject visualObject;
        public float placementTime;
    }
}