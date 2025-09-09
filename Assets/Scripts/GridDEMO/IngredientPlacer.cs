using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class IngredientPlacer : MonoBehaviour
    {
        [Header("Ingredient Visualization")]
        public GameObject ingredientPrefab;
        public Material[] aspectMaterials;
        
        [Header("Text Labels")]
        [SerializeField] private bool enableIngredientLabels = false; // Disabled by default to avoid multiple planes
        [SerializeField] private bool use3DTextLabels = false; // Option to use 3D text if needed
        [SerializeField] private bool useUITextLabels = true; // Use UI Canvas text instead
        
        private GridGameManager gridManager;
        private GridVisualizer gridVisualizer;
        private Dictionary<Vector2Int, IngredientInstance> placedIngredients = new Dictionary<Vector2Int, IngredientInstance>();
        private Canvas ingredientLabelCanvas; // Single canvas for all labels
        
        private void Awake()
        {
            gridManager = GetComponent<GridGameManager>();
            gridVisualizer = GetComponent<GridVisualizer>();
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
            
            // Calculate world position
            Vector3 worldPosition = gridManager.GridToWorldPosition(gridPosition);
            
            // Create ingredient visual based on shape data
            GameObject ingredientObj = CreateIngredientVisual(ingredient, gridPosition, worldPosition);
            
            // Store the ingredient instance
            IngredientInstance instance = new IngredientInstance
            {
                ingredient = ingredient,
                gridPosition = gridPosition,
                visualObject = ingredientObj,
                placementTime = Time.time
            };
            
            placedIngredients[gridPosition] = instance;
            
            // Play placement effect
            gridVisualizer.PlayPlacementEffect(gridPosition, ingredient);
            
            // Trigger any reactions with neighboring ingredients
            CheckForReactions(gridPosition, ingredient);
            
            Debug.Log($"Placed {ingredient.ItemName} at {gridPosition}");
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
            
            Debug.Log($"IngredientPlacer: Created 3D text label for {ingredient.ItemName} (creates a plane)");
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
            
            Debug.Log($"IngredientPlacer: Created UI text label for {ingredient.ItemName} (no planes)");
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
            
            Debug.Log("IngredientPlacer: Created single canvas for all ingredient labels");
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
                // Play removal effect
                gridVisualizer.PlayRemovalEffect(gridPosition);
                
                // Destroy visual object
                if (instance.visualObject != null)
                {
                    Destroy(instance.visualObject);
                }
                
                placedIngredients.Remove(gridPosition);
                
                Debug.Log($"Removed ingredient from {gridPosition}");
            }
        }
        
        public void ClearAllIngredients()
        {
            foreach (var kvp in placedIngredients)
            {
                if (kvp.Value.visualObject != null)
                {
                    Destroy(kvp.Value.visualObject);
                }
            }
            
            placedIngredients.Clear();
            
            // Clean up the label canvas if it exists
            if (ingredientLabelCanvas != null)
            {
                Destroy(ingredientLabelCanvas.gameObject);
                ingredientLabelCanvas = null;
                Debug.Log("IngredientPlacer: Cleaned up label canvas");
            }
            
            Debug.Log("Cleared all ingredients from grid");
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
                Vector3 midpoint = (gridManager.GridToWorldPosition(pos1) + gridManager.GridToWorldPosition(pos2)) * 0.5f;
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
            main.startColor = Color.Lerp(GetAspectColor(ingredient1.IngredientAspect), GetAspectColor(ingredient2.IngredientAspect), 0.5f);
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
            placedIngredients.TryGetValue(position, out IngredientInstance instance);
            return instance;
        }
        
        public List<IngredientInstance> GetAllPlacedIngredients()
        {
            return new List<IngredientInstance>(placedIngredients.Values);
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