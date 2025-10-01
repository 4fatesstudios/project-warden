using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Handles synergy detection and effects between ingredients based on aspect types and placement patterns
    /// Implements the synergy system from the design document
    /// </summary>
    public partial class SynergySystem : MonoBehaviour
    {
        [Header("Synergy Configuration")]
        [SerializeField] private bool enableSynergyEffects = true;
        [SerializeField] private bool showSynergyVisuals = true;
        [SerializeField] private float synergyDetectionRadius = 1.5f;
        [SerializeField] private ParticleSystem synergyParticleEffect;

        [Header("Synergy Database")]
        [SerializeField] private List<SynergyDefinition> definedSynergies = new List<SynergyDefinition>();

        // Current active synergies
        private List<SynergyInstance> activeSynergies = new List<SynergyInstance>();
        private Dictionary<Vector2Int, List<SynergyInstance>> positionSynergies = new Dictionary<Vector2Int, List<SynergyInstance>>();

        // Events
        public System.Action<SynergyInstance> OnSynergyDiscovered;
        public System.Action<SynergyInstance> OnSynergyActivated;
        public System.Action<SynergyInstance> OnSynergyRemoved;

        private void Start()
        {
            InitializeDefaultSynergies();
        }

        /// <summary>
        /// Initialize common synergy combinations based on the design document
        /// </summary>
        private void InitializeDefaultSynergies()
        {
            // Common Claw Synergy (example from design doc)
            definedSynergies.Add(new SynergyDefinition
            {
                synergyName = "Triple Claw Mastery",
                description = "Common claw + uncommon claw + rare claw gives effect as strong as 3 rare claws",
                requiredAspects = new List<Aspect> { Aspect.Corporeal, Aspect.Corporeal, Aspect.Corporeal },
                requiredArchetypes = new List<IngredientArchetype> { IngredientArchetype.Organic, IngredientArchetype.Organic, IngredientArchetype.Organic },
                potencyMultiplier = 2.5f,
                efficiencyBonus = 0.15f,
                requiresAdjacency = true,
                unlocksNewEffect = true,
                newEffectDescription = "Grants enhanced physical prowess and durability"
            });

            // Elemental Opposites
            definedSynergies.Add(new SynergyDefinition
            {
                synergyName = "Fire and Ice",
                description = "Frigid and Scorch aspects create temperature differential effects",
                requiredAspects = new List<Aspect> { Aspect.Frigid, Aspect.Scorch },
                potencyMultiplier = 1.8f,
                efficiencyBonus = 0.2f,
                requiresAdjacency = true,
                unlocksNewEffect = true,
                newEffectDescription = "Creates temperature shock effects"
            });

            // Divine Purification
            definedSynergies.Add(new SynergyDefinition
            {
                synergyName = "Divine Purification",
                description = "Divine aspects purify and enhance nearby ingredients",
                requiredAspects = new List<Aspect> { Aspect.Divine },
                potencyMultiplier = 1.3f,
                efficiencyBonus = 0.1f,
                requiresAdjacency = false,
                affectsNearbyIngredients = true,
                purificationRadius = 2.0f
            });

            // Caustic Amplification
            definedSynergies.Add(new SynergyDefinition
            {
                synergyName = "Caustic Chain Reaction",
                description = "Multiple caustic ingredients create cascading effects",
                requiredAspects = new List<Aspect> { Aspect.Caustic, Aspect.Caustic },
                potencyMultiplier = 1.6f,
                efficiencyBonus = 0.1f,
                requiresAdjacency = true,
                spreadEffect = true
            });

            // Arc Chaos Synergy
            definedSynergies.Add(new SynergyDefinition
            {
                synergyName = "Lightning Storm",
                description = "Arc aspects create unpredictable but powerful effects",
                requiredAspects = new List<Aspect> { Aspect.Arc, Aspect.Arc },
                potencyMultiplier = 2.0f,
                efficiencyBonus = 0.05f,
                requiresAdjacency = false,
                chaosEffect = true,
                variablePotency = true
            });

            Debug.Log($"🔮 Initialized {definedSynergies.Count} default synergy definitions");
        }

        /// <summary>
        /// Analyze current ingredient placements for synergies
        /// </summary>
        public SynergyAnalysisResult AnalyzeSynergies(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (!enableSynergyEffects || placedIngredients.Count < 2)
            {
                return new SynergyAnalysisResult { hasActiveSynergy = false };
            }

            // Clear previous synergies
            ClearActiveSynergies();

            // Check each synergy definition
            foreach (var synergyDef in definedSynergies)
            {
                var foundSynergies = DetectSynergy(synergyDef, placedIngredients);
                activeSynergies.AddRange(foundSynergies);
            }

            // Calculate overall results
            var result = CalculateSynergyResults();

            // Show visual effects
            if (showSynergyVisuals && activeSynergies.Count > 0)
            {
                ShowSynergyVisualEffects();
            }

            Debug.Log($"🔮 Synergy Analysis: Found {activeSynergies.Count} active synergies, Total multiplier: {result.totalPotencyMultiplier:F2}x");

            return result;
        }

        /// <summary>
        /// Detect instances of a specific synergy in the current placement
        /// </summary>
        private List<SynergyInstance> DetectSynergy(SynergyDefinition synergyDef, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            var instances = new List<SynergyInstance>();

            // Group ingredients by aspect and archetype
            var aspectGroups = placedIngredients.GroupBy(kvp => kvp.Value.IngredientAspect)
                                               .ToDictionary(g => g.Key, g => g.ToList());

            var archetypeGroups = placedIngredients.GroupBy(kvp => kvp.Value.IngredientArchetype)
                                                  .ToDictionary(g => g.Key, g => g.ToList());

            // Check if we have the required aspects
            if (!HasRequiredAspects(synergyDef, aspectGroups))
                return instances;

            // Check if we have the required archetypes (if specified)
            if (synergyDef.requiredArchetypes.Count > 0 && !HasRequiredArchetypes(synergyDef, archetypeGroups))
                return instances;

            // Find combinations that satisfy the synergy
            var combinations = FindSynergyCombinations(synergyDef, placedIngredients);

            foreach (var combination in combinations)
            {
                // Check adjacency requirements
                if (synergyDef.requiresAdjacency && !AreIngredientsAdjacent(combination))
                    continue;

                // Create synergy instance
                var instance = new SynergyInstance
                {
                    synergyData = synergyDef,
                    participatingIngredients = combination.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    centerPosition = CalculateCenterPosition(combination.Select(kvp => kvp.Key).ToList()),
                    effectivePotencyMultiplier = CalculateEffectivePotency(synergyDef, combination),
                    discoveredThisSession = !IsKnownSynergy(synergyDef)
                };

                instances.Add(instance);

                // Mark positions for this synergy
                foreach (var pos in combination.Select(kvp => kvp.Key))
                {
                    if (!positionSynergies.ContainsKey(pos))
                        positionSynergies[pos] = new List<SynergyInstance>();
                    positionSynergies[pos].Add(instance);
                }

                // Fire discovery event if new
                if (instance.discoveredThisSession)
                {
                    OnSynergyDiscovered?.Invoke(instance);
                }

                OnSynergyActivated?.Invoke(instance);
            }

            return instances;
        }

        /// <summary>
        /// Check if current placements have the required aspects for a synergy
        /// </summary>
        private bool HasRequiredAspects(SynergyDefinition synergy, Dictionary<Aspect, List<KeyValuePair<Vector2Int, Ingredient>>> aspectGroups)
        {
            var aspectCounts = synergy.requiredAspects.GroupBy(a => a)
                                                    .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in aspectCounts)
            {
                var aspect = kvp.Key;
                var requiredCount = kvp.Value;
                var availableCount = aspectGroups.ContainsKey(aspect) ? aspectGroups[aspect].Count : 0;

                if (availableCount < requiredCount)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if current placements have the required archetypes for a synergy
        /// </summary>
        private bool HasRequiredArchetypes(SynergyDefinition synergy, Dictionary<IngredientArchetype, List<KeyValuePair<Vector2Int, Ingredient>>> archetypeGroups)
        {
            var archetypeCounts = synergy.requiredArchetypes.GroupBy(a => a)
                                                           .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in archetypeCounts)
            {
                var archetype = kvp.Key;
                var requiredCount = kvp.Value;
                var availableCount = archetypeGroups.ContainsKey(archetype) ? archetypeGroups[archetype].Count : 0;

                if (availableCount < requiredCount)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Find all possible combinations that satisfy the synergy requirements
        /// </summary>
        private List<List<KeyValuePair<Vector2Int, Ingredient>>> FindSynergyCombinations(SynergyDefinition synergy, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            var combinations = new List<List<KeyValuePair<Vector2Int, Ingredient>>>();

            // For simple cases (2-3 ingredients), use brute force
            if (synergy.requiredAspects.Count <= 3)
            {
                combinations.AddRange(FindSmallCombinations(synergy, placedIngredients));
            }
            else
            {
                // For larger combinations, use more efficient algorithm
                combinations.AddRange(FindLargeCombinations(synergy, placedIngredients));
            }

            return combinations;
        }

        /// <summary>
        /// Find combinations for small synergies (2-3 ingredients)
        /// </summary>
        private List<List<KeyValuePair<Vector2Int, Ingredient>>> FindSmallCombinations(SynergyDefinition synergy, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            var combinations = new List<List<KeyValuePair<Vector2Int, Ingredient>>>();
            var ingredientList = placedIngredients.ToList();

            if (synergy.requiredAspects.Count == 1)
            {
                // Single ingredient synergies
                foreach (var kvp in ingredientList)
                {
                    if (kvp.Value.IngredientAspect == synergy.requiredAspects[0])
                    {
                        combinations.Add(new List<KeyValuePair<Vector2Int, Ingredient>> { kvp });
                    }
                }
            }
            else if (synergy.requiredAspects.Count == 2)
            {
                // Two ingredient synergies
                for (int i = 0; i < ingredientList.Count; i++)
                {
                    for (int j = i + 1; j < ingredientList.Count; j++)
                    {
                        var combo = new List<KeyValuePair<Vector2Int, Ingredient>> { ingredientList[i], ingredientList[j] };
                        if (MatchesAspectRequirements(synergy, combo))
                        {
                            combinations.Add(combo);
                        }
                    }
                }
            }
            else if (synergy.requiredAspects.Count == 3)
            {
                // Three ingredient synergies
                for (int i = 0; i < ingredientList.Count; i++)
                {
                    for (int j = i + 1; j < ingredientList.Count; j++)
                    {
                        for (int k = j + 1; k < ingredientList.Count; k++)
                        {
                            var combo = new List<KeyValuePair<Vector2Int, Ingredient>> { ingredientList[i], ingredientList[j], ingredientList[k] };
                            if (MatchesAspectRequirements(synergy, combo))
                            {
                                combinations.Add(combo);
                            }
                        }
                    }
                }
            }

            return combinations;
        }

        /// <summary>
        /// Find combinations for larger synergies (more than 3 ingredients)
        /// </summary>
        private List<List<KeyValuePair<Vector2Int, Ingredient>>> FindLargeCombinations(SynergyDefinition synergy, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            // For now, implement a simple recursive approach
            // In a more complex system, you'd use sophisticated combination algorithms
            var combinations = new List<List<KeyValuePair<Vector2Int, Ingredient>>>();
            var ingredientList = placedIngredients.ToList();

            // Generate all combinations of the required size
            var requiredCount = synergy.requiredAspects.Count;
            var combos = GetCombinations(ingredientList, requiredCount);

            foreach (var combo in combos)
            {
                if (MatchesAspectRequirements(synergy, combo))
                {
                    combinations.Add(combo);
                }
            }

            return combinations;
        }

        /// <summary>
        /// Check if a combination matches the aspect requirements
        /// </summary>
        private bool MatchesAspectRequirements(SynergyDefinition synergy, List<KeyValuePair<Vector2Int, Ingredient>> combination)
        {
            var comboAspects = combination.Select(kvp => kvp.Value.IngredientAspect).ToList();
            var requiredAspects = synergy.requiredAspects.ToList();

            // Sort both lists for comparison
            comboAspects.Sort();
            requiredAspects.Sort();

            return comboAspects.SequenceEqual(requiredAspects);
        }

        /// <summary>
        /// Check if ingredients in a combination are adjacent
        /// </summary>
        private bool AreIngredientsAdjacent(List<KeyValuePair<Vector2Int, Ingredient>> combination)
        {
            if (combination.Count < 2) return true;

            var positions = combination.Select(kvp => kvp.Key).ToList();

            // For each position, check if at least one other position is adjacent
            foreach (var pos in positions)
            {
                bool hasAdjacentNeighbor = false;
                foreach (var otherPos in positions)
                {
                    if (pos != otherPos && Vector2Int.Distance(pos, otherPos) <= synergyDetectionRadius)
                    {
                        hasAdjacentNeighbor = true;
                        break;
                    }
                }
                if (!hasAdjacentNeighbor)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate effective potency for a synergy instance
        /// </summary>
        private float CalculateEffectivePotency(SynergyDefinition synergy, List<KeyValuePair<Vector2Int, Ingredient>> combination)
        {
            float basePotency = synergy.potencyMultiplier;

            // Apply variable potency for chaos effects
            if (synergy.variablePotency)
            {
                float variance = Random.Range(0.8f, 1.4f);
                basePotency *= variance;
            }

            // Bonus for high-potency ingredients
            float avgIngredientPotency = (float)combination.Average(kvp => kvp.Value.Potency);
            if (avgIngredientPotency > 3)
            {
                basePotency *= 1.1f;
            }

            return basePotency;
        }

        /// <summary>
        /// Calculate center position of a synergy
        /// </summary>
        private Vector2 CalculateCenterPosition(List<Vector2Int> positions)
        {
            if (positions.Count == 0) return Vector2.zero;

            float avgX = (float)positions.Average(p => p.x);
            float avgY = (float)positions.Average(p => p.y);
            return new Vector2(avgX, avgY);
        }

        /// <summary>
        /// Calculate overall synergy results
        /// </summary>
        private SynergyAnalysisResult CalculateSynergyResults()
        {
            var result = new SynergyAnalysisResult
            {
                hasActiveSynergy = activeSynergies.Count > 0,
                activeSynergyCount = activeSynergies.Count,
                totalPotencyMultiplier = 1.0f,
                totalEfficiencyBonus = 0f,
                synergyDescriptions = new List<string>()
            };

            if (activeSynergies.Count == 0)
                return result;

            // Calculate cumulative effects
            foreach (var synergy in activeSynergies)
            {
                result.totalPotencyMultiplier *= synergy.effectivePotencyMultiplier;
                result.totalEfficiencyBonus += synergy.synergyData.efficiencyBonus;
                result.synergyDescriptions.Add(synergy.synergyData.synergyName);

                if (synergy.synergyData.unlocksNewEffect)
                {
                    result.unlockedEffects.Add(synergy.synergyData.newEffectDescription);
                }

                if (synergy.discoveredThisSession)
                {
                    result.newSynergiesDiscovered++;
                }
            }

            return result;
        }

        /// <summary>
        /// Show visual effects for active synergies
        /// </summary>
        private void ShowSynergyVisualEffects()
        {
            foreach (var synergy in activeSynergies)
            {
                // Create particle effects at synergy center
                if (synergyParticleEffect != null)
                {
                    var effect = Instantiate(synergyParticleEffect, new Vector3(synergy.centerPosition.x, 0.5f, synergy.centerPosition.y), Quaternion.identity);
                    
                    // Customize effect based on synergy type
                    var main = effect.main;
                    main.startColor = GetSynergyColor(synergy.synergyData);
                    
                    // Auto-destroy after duration
                    Destroy(effect.gameObject, 3f);
                }

                Debug.Log($"✨ Synergy Visual: {synergy.synergyData.synergyName} at {synergy.centerPosition}");
            }
        }

        /// <summary>
        /// Get color for synergy visual effects
        /// </summary>
        private Color GetSynergyColor(SynergyDefinition synergy)
        {
            if (synergy.requiredAspects.Contains(Aspect.Divine))
                return Color.magenta;
            if (synergy.requiredAspects.Contains(Aspect.Scorch))
                return Color.red;
            if (synergy.requiredAspects.Contains(Aspect.Frigid))
                return Color.cyan;
            if (synergy.requiredAspects.Contains(Aspect.Arc))
                return Color.yellow;
            if (synergy.requiredAspects.Contains(Aspect.Caustic))
                return Color.green;
            
            return Color.gray; // Default synergy color
        }

        /// <summary>
        /// Clear all active synergies
        /// </summary>
        private void ClearActiveSynergies()
        {
            foreach (var synergy in activeSynergies)
            {
                OnSynergyRemoved?.Invoke(synergy);
            }

            activeSynergies.Clear();
            positionSynergies.Clear();
        }

        /// <summary>
        /// Check if a synergy has been discovered before
        /// </summary>
        private bool IsKnownSynergy(SynergyDefinition synergy)
        {
            // In a full implementation, this would check against a persistent discovery database
            // For now, assume all synergies are new discoveries
            return false;
        }

        /// <summary>
        /// Get combinations of items from a list
        /// </summary>
        private List<List<T>> GetCombinations<T>(List<T> list, int length)
        {
            if (length == 1) return list.Select(t => new List<T> { t }).ToList();

            var combinations = new List<List<T>>();
            for (int i = 0; i <= list.Count - length; i++)
            {
                var head = list[i];
                var tail = list.Skip(i + 1).ToList();
                foreach (var combination in GetCombinations(tail, length - 1))
                {
                    combination.Insert(0, head);
                    combinations.Add(combination);
                }
            }
            return combinations;
        }

        #region Public API

        /// <summary>
        /// Check synergies at a specific position
        /// </summary>
        public SynergyAnalysisResult CheckSynergiesAt(Vector2Int position)
        {
            if (positionSynergies.ContainsKey(position))
            {
                var localSynergies = positionSynergies[position];
                return new SynergyAnalysisResult
                {
                    hasActiveSynergy = true,
                    activeSynergyCount = localSynergies.Count,
                    totalPotencyMultiplier = localSynergies.Aggregate(1f, (acc, s) => acc * s.effectivePotencyMultiplier),
                    synergyDescriptions = localSynergies.Select(s => s.synergyData.synergyName).ToList()
                };
            }

            return new SynergyAnalysisResult { hasActiveSynergy = false };
        }

        /// <summary>
        /// Get all active synergies
        /// </summary>
        public List<SynergyInstance> GetActiveSynergies()
        {
            return new List<SynergyInstance>(activeSynergies);
        }

        /// <summary>
        /// Add a custom synergy definition
        /// </summary>
        public void AddSynergyDefinition(SynergyDefinition synergy)
        {
            definedSynergies.Add(synergy);
            Debug.Log($"🔮 Added custom synergy: {synergy.synergyName}");
        }

        /// <summary>
        /// Test the synergy system with debug output
        /// </summary>
        [ContextMenu("Test Synergy System")]
        public void TestSynergySystem()
        {
            Debug.Log("🔮 === TESTING SYNERGY SYSTEM ===");
            Debug.Log($"🔮 Synergy Effects Enabled: {enableSynergyEffects}");
            Debug.Log($"🔮 Show Visual Effects: {showSynergyVisuals}");
            Debug.Log($"🔮 Detection Radius: {synergyDetectionRadius}");
            Debug.Log($"🔮 Defined Synergies: {definedSynergies.Count}");
            
            foreach (var synergy in definedSynergies)
            {
                Debug.Log($"   - {synergy.synergyName}: {synergy.description}");
            }
            
            Debug.Log($"🔮 Active Synergies: {activeSynergies.Count}");
            Debug.Log("🔮 === SYNERGY SYSTEM TEST COMPLETE ===");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class SynergyDefinition
    {
        [Header("Basic Info")]
        public string synergyName;
        [TextArea(2, 3)]
        public string description;

        [Header("Requirements")]
        public List<Aspect> requiredAspects = new List<Aspect>();
        public List<IngredientArchetype> requiredArchetypes = new List<IngredientArchetype>();
        public bool requiresAdjacency = true;

        [Header("Effects")]
        [Range(0.1f, 5.0f)]
        public float potencyMultiplier = 1.5f;
        [Range(0f, 1f)]
        public float efficiencyBonus = 0.1f;
        public bool unlocksNewEffect = false;
        public string newEffectDescription = "";

        [Header("Special Properties")]
        public bool affectsNearbyIngredients = false;
        public float purificationRadius = 1.0f;
        public bool spreadEffect = false;
        public bool chaosEffect = false;
        public bool variablePotency = false;
    }

    [System.Serializable]
    public class SynergyInstance
    {
        public SynergyDefinition synergyData;
        public Dictionary<Vector2Int, Ingredient> participatingIngredients;
        public Vector2 centerPosition;
        public float effectivePotencyMultiplier;
        public bool discoveredThisSession;
    }

    [System.Serializable]
    public class SynergyAnalysisResult
    {
        public bool hasActiveSynergy;
        public int activeSynergyCount;
        public float totalPotencyMultiplier;
        public float totalEfficiencyBonus;
        public List<string> synergyDescriptions = new List<string>();
        public List<string> unlockedEffects = new List<string>();
        public int newSynergiesDiscovered;
    }

    #endregion
}