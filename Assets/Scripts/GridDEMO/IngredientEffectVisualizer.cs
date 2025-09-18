using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Effects;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Direction of a shared border between two cells
    /// </summary>
    public enum BorderDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }
    
    /// <summary>
    /// Represents a shared border between two ingredient cells
    /// </summary>
    public struct SharedBorder
    {
        public Vector2Int cell1;
        public Vector2Int cell2;
        public BorderDirection direction;
        public Vector3 worldCenter;
        public Vector3 worldStart;
        public Vector3 worldEnd;
    }
    
    public class IngredientEffectVisualizer : MonoBehaviour
    {
        [Header("Particle Effect Prefabs")]
        [SerializeField, Tooltip("Particle effect for ingredients with similar effects")]
        private GameObject similarEffectsParticlePrefab;
        
        [SerializeField, Tooltip("Particle effect for ingredients with different effects")]
        private GameObject differentEffectsParticlePrefab;
        
        [Header("Visual Settings")]
        [SerializeField, Tooltip("Duration for particle effects")]
        private float effectDuration = 3f;
        
        [SerializeField, Tooltip("Distance for checking adjacent ingredients")]
        private float adjacencyDistance = 1.5f;
        
        [SerializeField, Tooltip("Height offset for particle effects")]
        private float effectHeightOffset = 0.5f;
        
        [Header("Effect Colors")]
        [SerializeField, Tooltip("Color for similar effects sparkles")]
        private Color similarEffectsColor = Color.yellow;
        
        [SerializeField, Tooltip("Color for different effects")]
        private Color differentEffectsColor = Color.red;
        
        private GridGameManager gridManager;
        private IngredientPlacer ingredientPlacer;
        private Dictionary<Vector2Int, GameObject> activeEffects = new Dictionary<Vector2Int, GameObject>();
        
        // Track continuous effects by border key
        private Dictionary<string, GameObject> activeBorderEffects = new Dictionary<string, GameObject>();
        
        private void Awake()
        {
            gridManager = GetComponent<GridGameManager>();
            ingredientPlacer = GetComponent<IngredientPlacer>();
        }
        
        private void Start()
        {
            CreateDefaultEffectPrefabs();
        }
        
        private void CreateDefaultEffectPrefabs()
        {
            if (similarEffectsParticlePrefab == null)
            {
                similarEffectsParticlePrefab = CreateSparkleEffect("SimilarEffectsSparkle", similarEffectsColor);
            }
            
            if (differentEffectsParticlePrefab == null)
            {
                differentEffectsParticlePrefab = CreateReactionEffect("DifferentEffectsReaction", differentEffectsColor);
            }
        }
        
        private GameObject CreateSparkleEffect(string name, Color color)
        {
            GameObject prefab = new GameObject(name);
            
            // Move the prefab off-screen to keep it out of view
            prefab.transform.position = new Vector3(1000f, -1000f, 1000f);
            
            ParticleSystem particles = prefab.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startSize = 0.1f;
            main.startColor = color;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true; // Make particles loop continuously
            
            var emission = particles.emission;
            emission.rateOverTime = 8f; // Continuous emission
            emission.SetBursts(new ParticleSystem.Burst[0]); // Remove burst for continuous effect
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;
            
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.5f);
            
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(color, 0.0f), 
                    new GradientColorKey(Color.white, 0.5f),
                    new GradientColorKey(color, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(1.0f, 0.8f),
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            colorOverLifetime.color = gradient;
            
            return prefab;
        }
        
        private GameObject CreateReactionEffect(string name, Color color)
        {
            GameObject prefab = new GameObject(name);
            
            // Move the prefab off-screen to keep it out of view
            prefab.transform.position = new Vector3(1000f, -1000f, 1000f);
            
            ParticleSystem particles = prefab.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 2f;
            main.startSize = 0.15f;
            main.startColor = color;
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = true; // Make particles loop continuously
            
            var emission = particles.emission;
            emission.rateOverTime = 12f; // Continuous emission for reactions
            emission.SetBursts(new ParticleSystem.Burst[0]); // Remove burst for continuous effect
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;
            
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
            
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(color, 0.0f), 
                    new GradientColorKey(Color.yellow, 0.3f),
                    new GradientColorKey(color, 1.0f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.8f, 0.7f),
                    new GradientAlphaKey(0.0f, 1.0f) 
                }
            );
            colorOverLifetime.color = gradient;
            
            return prefab;
        }
        
        /// <summary>
        /// Check for ingredient interactions when a new ingredient is placed
        /// </summary>
        public void CheckIngredientInteractions(Vector2Int placedPosition, Ingredient placedIngredient)
        {
            if (placedIngredient == null || !placedIngredient.HasInfusionEffects())
            {
                Debug.Log($"🎨 No infusions to check for {placedIngredient?.ItemName ?? "null ingredient"}");
                return;
            }
            
            Debug.Log($"🎨 Checking infusion interactions for {placedIngredient.ItemName} (HasInfusionEffects: {placedIngredient.HasInfusionEffects()})");
            
            if (placedIngredient.InfusionBundle?.Infusions != null)
            {
                Debug.Log($"🎨 Ingredient {placedIngredient.ItemName} - Infusions count: {placedIngredient.InfusionBundle.Infusions.Count}");
                foreach (var infusion in placedIngredient.InfusionBundle.Infusions)
                {
                    Debug.Log($"   - Infusion: {infusion.GetType().Name}");
                }
            }
            
            // Get all cells occupied by the placed ingredient
            var placedCells = gridManager.GetIngredientCells(placedIngredient, placedPosition);
            
            // For each cell occupied by the placed ingredient, check adjacent cells
            foreach (var cellPos in placedCells)
            {
                CheckAdjacentIngredients(cellPos, placedIngredient);
            }
        }
        
        /// <summary>
        /// Check all adjacent cells for ingredient interactions
        /// </summary>
        private void CheckAdjacentIngredients(Vector2Int centerPos, Ingredient centerIngredient)
        {
            // Check 4-directional adjacent cells (up, down, left, right)
            Vector2Int[] directions = { 
                Vector2Int.up, 
                Vector2Int.down, 
                Vector2Int.left, 
                Vector2Int.right 
            };
            
            foreach (Vector2Int direction in directions)
            {
                Vector2Int adjacentPos = centerPos + direction;
                
                // Check if there's an ingredient at this position
                var adjacentInstance = ingredientPlacer.GetIngredientAt(adjacentPos);
                if (adjacentInstance != null && adjacentInstance.ingredient != centerIngredient)
                {
                    ProcessIngredientInteraction(centerPos, centerIngredient, adjacentPos, adjacentInstance.ingredient);
                }
            }
        }
        
        /// <summary>
        /// Process the interaction between two ingredients (comparing infusions only)
        /// </summary>
        private void ProcessIngredientInteraction(Vector2Int pos1, Ingredient ingredient1, Vector2Int pos2, Ingredient ingredient2)
        {
            if (ingredient1 == null || ingredient2 == null)
                return;
            
            bool ingredient1HasInfusions = ingredient1.HasInfusionEffects();
            bool ingredient2HasInfusions = ingredient2.HasInfusionEffects();
            
            Debug.Log($"🎨 Processing infusion interaction between {ingredient1.ItemName} and {ingredient2.ItemName}");
            Debug.Log($"   {ingredient1.ItemName} HasInfusionEffects: {ingredient1HasInfusions}");
            Debug.Log($"   {ingredient2.ItemName} HasInfusionEffects: {ingredient2HasInfusions}");
            
            // Debug: Show what infusions each ingredient has
            if (ingredient1HasInfusions && ingredient1.InfusionBundle?.Infusions != null)
            {
                Debug.Log($"   {ingredient1.ItemName} infusions ({ingredient1.InfusionBundle.Infusions.Count}): {string.Join(", ", ingredient1.InfusionBundle.Infusions.Select(i => i.GetType().Name))}");
            }
            
            if (ingredient2HasInfusions && ingredient2.InfusionBundle?.Infusions != null)
            {
                Debug.Log($"   {ingredient2.ItemName} infusions ({ingredient2.InfusionBundle.Infusions.Count}): {string.Join(", ", ingredient2.InfusionBundle.Infusions.Select(i => i.GetType().Name))}");
            }
            
            // If neither has infusions, no interaction
            if (!ingredient1HasInfusions && !ingredient2HasInfusions)
            {
                Debug.Log($"   🚫 No interaction: Neither ingredient has infusions");
                return;
            }
            
            // Find all shared borders between the two ingredients
            var sharedBorders = FindSharedBorders(pos1, ingredient1, pos2, ingredient2);
            
            if (sharedBorders.Count == 0)
            {
                Debug.Log($"   🚫 No interaction: No shared borders found");
                return; // No actual adjacency
            }
            
            Debug.Log($"   🔗 Found {sharedBorders.Count} shared borders");
            
            // Determine interaction type based on infusions only
            if (ingredient1HasInfusions && ingredient2HasInfusions)
            {
                // Both have infusions - check if they share any infusion types
                var similarInfusions = ingredient1.GetSimilarEffectsTo(ingredient2);
                
                Debug.Log($"   🔍 Checking for shared infusion types: {similarInfusions.Count} matches found");
                
                if (similarInfusions.Count > 0)
                {
                    Debug.Log($"✨ {similarInfusions.Count} shared infusion types detected between {ingredient1.ItemName} and {ingredient2.ItemName} along {sharedBorders.Count} shared borders!");
                    
                    // Log details about the shared infusions
                    foreach (var (thisInfusion, otherInfusion) in similarInfusions)
                    {
                        Debug.Log($"   - Shared infusion type: {thisInfusion.GetType().Name}");
                    }
                    
                    CreateSimilarEffectsParticleAlongBorders(sharedBorders, ingredient1, ingredient2, similarInfusions);
                }
                else
                {
                    Debug.Log($"💥 Different infusion types detected between {ingredient1.ItemName} and {ingredient2.ItemName} along {sharedBorders.Count} shared borders!");
                    CreateDifferentEffectsParticleAlongBorders(sharedBorders, ingredient1, ingredient2);
                }
            }
            else
            {
                // Only one has infusions - neutral interaction
                Debug.Log($"🌟 Neutral interaction (one has infusions) between {ingredient1.ItemName} and {ingredient2.ItemName} along {sharedBorders.Count} shared borders");
                CreateNeutralInteractionParticleAlongBorders(sharedBorders, ingredient1HasInfusions ? ingredient1 : ingredient2);
            }
        }
        
        /// <summary>
        /// Find all shared borders between two ingredients
        /// </summary>
        private List<SharedBorder> FindSharedBorders(Vector2Int pos1, Ingredient ingredient1, Vector2Int pos2, Ingredient ingredient2)
        {
            var sharedBorders = new List<SharedBorder>();
            
            // Get all cells occupied by each ingredient
            var cells1 = gridManager.GetIngredientCells(ingredient1, pos1);
            var cells2 = gridManager.GetIngredientCells(ingredient2, pos2);
            
            // For each cell of ingredient1, check if it's adjacent to any cell of ingredient2
            foreach (var cell1 in cells1)
            {
                foreach (var cell2 in cells2)
                {
                    // Check if cells are adjacent (only 4-directional)
                    Vector2Int diff = cell2 - cell1;
                    
                    if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1) // Manhattan distance of 1
                    {
                        // Determine border direction
                        BorderDirection direction;
                        if (diff.x == 1) direction = BorderDirection.Right;
                        else if (diff.x == -1) direction = BorderDirection.Left;
                        else if (diff.y == 1) direction = BorderDirection.Top;
                        else direction = BorderDirection.Bottom;
                        
                        // Calculate border position (edge between the two cells)
                        Vector3 worldPos1 = gridManager.GridToWorldPosition(cell1);
                        Vector3 worldPos2 = gridManager.GridToWorldPosition(cell2);
                        Vector3 borderCenter = (worldPos1 + worldPos2) * 0.5f + Vector3.up * effectHeightOffset;
                        
                        // Calculate border endpoints for a line effect
                        Vector3 borderStart, borderEnd;
                        CalculateBorderEndpoints(worldPos1, worldPos2, direction, out borderStart, out borderEnd);
                        
                        sharedBorders.Add(new SharedBorder
                        {
                            cell1 = cell1,
                            cell2 = cell2,
                            direction = direction,
                            worldCenter = borderCenter,
                            worldStart = borderStart + Vector3.up * effectHeightOffset,
                            worldEnd = borderEnd + Vector3.up * effectHeightOffset
                        });
                        
                        Debug.Log($"🔗 Found shared border: {cell1} → {cell2} ({direction}) at {borderCenter}");
                    }
                }
            }
            
            return sharedBorders;
        }
        
        /// <summary>
        /// Calculate the start and end points of a border between two cells
        /// </summary>
        private void CalculateBorderEndpoints(Vector3 worldPos1, Vector3 worldPos2, BorderDirection direction, out Vector3 start, out Vector3 end)
        {
            Vector3 borderCenter = (worldPos1 + worldPos2) * 0.5f;
            float halfCellSize = gridManager.cellSize * 0.5f;
            
            switch (direction)
            {
                case BorderDirection.Right:
                case BorderDirection.Left:
                    // Vertical border
                    start = new Vector3(borderCenter.x, borderCenter.y, borderCenter.z - halfCellSize);
                    end = new Vector3(borderCenter.x, borderCenter.y, borderCenter.z + halfCellSize);
                    break;
                    
                case BorderDirection.Top:
                case BorderDirection.Bottom:
                    // Horizontal border
                    start = new Vector3(borderCenter.x - halfCellSize, borderCenter.y, borderCenter.z);
                    end = new Vector3(borderCenter.x + halfCellSize, borderCenter.y, borderCenter.z);
                    break;
                    
                default:
                    start = borderCenter;
                    end = borderCenter;
                    break;
            }
        }
        
        /// <summary>
        /// Create sparkle effects along shared borders for ingredients with similar infusions
        /// </summary>
        private void CreateSimilarEffectsParticleAlongBorders(List<SharedBorder> sharedBorders, Ingredient ingredient1, Ingredient ingredient2, List<(object thisInfusion, object otherInfusion)> similarInfusions)
        {
            foreach (var border in sharedBorders)
            {
                // Create a unique key for this border
                string borderKey = $"{border.cell1}_{border.cell2}_similar";
                
                // Remove existing effect if any
                if (activeBorderEffects.ContainsKey(borderKey))
                {
                    if (activeBorderEffects[borderKey] != null)
                    {
                        Destroy(activeBorderEffects[borderKey]);
                    }
                    activeBorderEffects.Remove(borderKey);
                }
                
                // Create new continuous effect
                Vector3 position = border.worldCenter;
                GameObject effect = Instantiate(similarEffectsParticlePrefab, position, Quaternion.identity);
                
                // Customize the effect based on the shared effect types
                ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var main = particles.main;
                    
                    // Blend colors based on ingredient aspects
                    Color color1 = GetAspectColor(ingredient1.IngredientAspect);
                    Color color2 = GetAspectColor(ingredient2.IngredientAspect);
                    Color blendedColor = Color.Lerp(color1, color2, 0.5f);
                    
                    main.startColor = blendedColor;
                    
                    // Adjust particle system for border placement
                    var shape = particles.shape;
                    shape.shapeType = ParticleSystemShapeType.Box;
                    
                    // Orient the shape along the border
                    if (border.direction == BorderDirection.Top || border.direction == BorderDirection.Bottom)
                    {
                        // Horizontal border
                        shape.scale = new Vector3(0.8f, 0.1f, 0.1f);
                    }
                    else
                    {
                        // Vertical border
                        shape.scale = new Vector3(0.1f, 0.1f, 0.8f);
                    }
                    
                    // Increase intensity based on number of shared infusions
                    var sharedInfusionCount = similarInfusions.Count;
                    var emission = particles.emission;
                    emission.rateOverTime = 5f + (sharedInfusionCount * 3f); // Gentle continuous stream
                    
                    // Configure for smooth, gentle continuous effect
                    main.loop = true;
                    main.startLifetime = 2.0f; // Longer lifetime for gentle floating effect
                    main.startSpeed = 0.5f; // Slow, gentle movement
                    
                    // Add gentle size variation over lifetime
                    var sizeOverLifetime = particles.sizeOverLifetime;
                    sizeOverLifetime.enabled = true;
                    AnimationCurve sizeCurve = new AnimationCurve();
                    sizeCurve.AddKey(0f, 0.2f); // Start small
                    sizeCurve.AddKey(0.5f, 1f); // Grow
                    sizeCurve.AddKey(1f, 0.1f); // Fade small
                    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
                }
                
                // Track this effect for later cleanup
                activeBorderEffects[borderKey] = effect;
            }
            
            Debug.Log($"✨ Created continuous sparkle effects along {sharedBorders.Count} shared borders with {similarInfusions.Count} shared infusions");
            
            // Log the specific infusions that are similar
            foreach (var (thisInfusion, otherInfusion) in similarInfusions)
            {
                Debug.Log($"   🌟 Infusion match: {thisInfusion.GetType().Name} from {ingredient1.ItemName} ↔ {otherInfusion.GetType().Name} from {ingredient2.ItemName}");
            }
        }
        
        /// <summary>
        /// Create periodic reaction effects along shared borders for ingredients with different infusions
        /// </summary>
        private void CreateDifferentEffectsParticleAlongBorders(List<SharedBorder> sharedBorders, Ingredient ingredient1, Ingredient ingredient2)
        {
            foreach (var border in sharedBorders)
            {
                // Create a unique key for this border
                string borderKey = $"{border.cell1}_{border.cell2}_different";
                
                // Remove existing effect if any
                if (activeBorderEffects.ContainsKey(borderKey))
                {
                    if (activeBorderEffects[borderKey] != null)
                    {
                        Destroy(activeBorderEffects[borderKey]);
                    }
                    activeBorderEffects.Remove(borderKey);
                }
                
                // Create new periodic reaction effect
                Vector3 position = border.worldCenter;
                GameObject effect = Instantiate(differentEffectsParticlePrefab, position, Quaternion.identity);
                
                ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var main = particles.main;
                    
                    // Create contrasting colors
                    Color color1 = GetAspectColor(ingredient1.IngredientAspect);
                    Color color2 = GetAspectColor(ingredient2.IngredientAspect);
                    
                    // Use a more dramatic color that contrasts with both
                    main.startColor = GetContrastingColor(color1, color2);
                    
                    // Adjust shape for border reactions
                    var shape = particles.shape;
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(0.3f, 0.2f, 0.3f);
                    
                    // Configure for periodic bursts instead of continuous emission
                    var emission = particles.emission;
                    emission.rateOverTime = 0; // No continuous emission
                    
                    // Set up dynamic periodic bursts with varying intensity
                    emission.SetBursts(new ParticleSystem.Burst[]
                    {
                        new ParticleSystem.Burst(0.0f, 15),   // Initial burst
                        new ParticleSystem.Burst(1.0f, 25),  // Strong reaction burst
                        new ParticleSystem.Burst(1.8f, 10),  // Follow-up burst
                        new ParticleSystem.Burst(3.2f, 30),  // Peak reaction burst
                        new ParticleSystem.Burst(4.5f, 8),   // Settling burst
                        new ParticleSystem.Burst(6.0f, 20),  // Secondary reaction
                    });
                    
                    // Configure timing and appearance
                    main.loop = true;
                    main.startLifetime = 1.5f; // Longer particle lifetime for dramatic effect
                    main.startSpeed = 2.0f; // Faster particles for reactions
                    
                    // Add velocity over lifetime for more dynamic movement
                    var velocityOverLifetime = particles.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;
                    velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
                    velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1.0f); // Expand outward
                }
                
                // Track this effect for later cleanup
                activeBorderEffects[borderKey] = effect;
            }
            
            Debug.Log($"💥 Created periodic reaction effects along {sharedBorders.Count} shared borders between different infusions");
        }
        
        /// <summary>
        /// Create neutral interaction effects along shared borders
        /// </summary>
        private void CreateNeutralInteractionParticleAlongBorders(List<SharedBorder> sharedBorders, Ingredient ingredientWithEffects)
        {
            foreach (var border in sharedBorders)
            {
                // Create a unique key for this border
                string borderKey = $"{border.cell1}_{border.cell2}_neutral";
                
                // Remove existing effect if any
                if (activeBorderEffects.ContainsKey(borderKey))
                {
                    if (activeBorderEffects[borderKey] != null)
                    {
                        Destroy(activeBorderEffects[borderKey]);
                    }
                    activeBorderEffects.Remove(borderKey);
                }
                
                // Create new continuous neutral effect
                Vector3 position = border.worldCenter;
                GameObject effect = Instantiate(similarEffectsParticlePrefab, position, Quaternion.identity);
                
                ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var main = particles.main;
                    main.startColor = Color.white;
                    
                    var emission = particles.emission;
                    emission.rateOverTime = 3f; // Subtle continuous emission for neutral interactions
                    
                    // Smaller, more subtle shape
                    var shape = particles.shape;
                    shape.radius = 0.1f;
                }
                
                // Track this effect for later cleanup
                activeBorderEffects[borderKey] = effect;
            }
            
            Debug.Log($"🌟 Created continuous neutral interaction effects along {sharedBorders.Count} shared borders");
        }
        
        /// <summary>
        /// Count how many effect types are shared between two ingredients
        /// </summary>
        private int CountSharedEffectTypes(Ingredient ingredient1, Ingredient ingredient2)
        {
            int sharedCount = 0;
            
            if (!ingredient1.HasEffects() || !ingredient2.HasEffects())
                return 0;
            
            var types1 = ingredient1.GetEffectTypes();
            var types2 = ingredient2.GetEffectTypes();
            
            foreach (var type1 in types1)
            {
                foreach (var type2 in types2)
                {
                    if (type1 == type2)
                    {
                        sharedCount++;
                        break; // Count each type only once
                    }
                }
            }
            
            return sharedCount;
        }
        
        /// <summary>
        /// Get color based on ingredient aspect
        /// </summary>
        private Color GetAspectColor(Aspect aspect)
        {
            return aspect switch
            {
                Aspect.Scorch => Color.red,
                Aspect.Frigid => Color.cyan,
                Aspect.Arc => Color.yellow,
                Aspect.Caustic => new Color(0.5f, 0.2f, 0.8f), // Purple
                Aspect.Corporeal => Color.green,
                Aspect.Divine => Color.white,
                _ => Color.gray
            };
        }
        
        /// <summary>
        /// Get a contrasting color for different effects
        /// </summary>
        private Color GetContrastingColor(Color color1, Color color2)
        {
            // Create a color that contrasts with both input colors
            float avgR = (color1.r + color2.r) * 0.5f;
            float avgG = (color1.g + color2.g) * 0.5f;
            float avgB = (color1.b + color2.b) * 0.5f;
            
            // Invert the average to create contrast
            return new Color(1f - avgR, 1f - avgG, 1f - avgB, 1f);
        }
        
        /// <summary>
        /// Clear all active effects (useful for grid reset)
        /// </summary>
        public void ClearAllEffects()
        {
            // Clear old legacy effects
            foreach (var effect in activeEffects.Values)
            {
                if (effect != null)
                    Destroy(effect);
            }
            activeEffects.Clear();
            
            // Clear new continuous border effects
            foreach (var effect in activeBorderEffects.Values)
            {
                if (effect != null)
                    Destroy(effect);
            }
            activeBorderEffects.Clear();
            
            Debug.Log("🎨 Cleared all ingredient interaction effects");
        }
        
        /// <summary>
        /// Clean up all particle effects when an ingredient is removed
        /// </summary>
        public void CleanupEffectsForIngredient(Vector2Int ingredientPosition, Ingredient ingredient)
        {
            Debug.Log($"🧹 Cleaning up particle effects for {ingredient?.ItemName ?? "unknown ingredient"} at {ingredientPosition}");
            
            // Get all cells occupied by the ingredient
            var ingredientCells = gridManager.GetIngredientCells(ingredient, ingredientPosition);
            
            // Find and remove all effects involving these cells
            List<string> keysToRemove = new List<string>();
            
            foreach (var kvp in activeBorderEffects)
            {
                string borderKey = kvp.Key;
                GameObject effect = kvp.Value;
                
                // Parse the border key to get cell positions
                string[] parts = borderKey.Split('_');
                if (parts.Length >= 4) // Format: "x,y_x,y_effectType"
                {
                    string cell1Str = parts[0] + "_" + parts[1];
                    string cell2Str = parts[2] + "_" + parts[3];
                    
                    Vector2Int cell1 = ParseCellPosition(cell1Str);
                    Vector2Int cell2 = ParseCellPosition(cell2Str);
                    
                    // Check if either cell belongs to the removed ingredient
                    if (ingredientCells.Contains(cell1) || ingredientCells.Contains(cell2))
                    {
                        if (effect != null)
                        {
                            Destroy(effect);
                        }
                        keysToRemove.Add(borderKey);
                    }
                }
            }
            
            // Remove the keys from the dictionary
            foreach (string key in keysToRemove)
            {
                activeBorderEffects.Remove(key);
            }
            
            Debug.Log($"🧹 Removed {keysToRemove.Count} particle effects for ingredient removal");
        }
        
        /// <summary>
        /// Parse a cell position from string format "(x, y)"
        /// </summary>
        private Vector2Int ParseCellPosition(string cellStr)
        {
            // Remove parentheses and split by comma
            cellStr = cellStr.Trim('(', ')');
            string[] coords = cellStr.Split(',');
            
            if (coords.Length == 2 && 
                int.TryParse(coords[0].Trim(), out int x) && 
                int.TryParse(coords[1].Trim(), out int y))
            {
                return new Vector2Int(x, y);
            }
            
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// Update all ingredient interactions when the grid changes
        /// </summary>
        [ContextMenu("Refresh All Ingredient Interactions")]
        public void RefreshAllIngredientInteractions()
        {
            Debug.Log("🎨 Refreshing all ingredient interactions...");
            
            ClearAllEffects();
            
            var allIngredients = ingredientPlacer.GetAllPlacedIngredients();
            
            foreach (var instance in allIngredients)
            {
                CheckIngredientInteractions(instance.gridPosition, instance.ingredient);
            }
            
            Debug.Log($"🎨 Refreshed interactions for {allIngredients.Count} ingredients");
        }
    }
}