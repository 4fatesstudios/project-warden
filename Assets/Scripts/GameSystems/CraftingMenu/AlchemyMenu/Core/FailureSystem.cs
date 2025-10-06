using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Handles recipe failures and synthetic ingredient creation from failed attempts
    /// Implements the failure and synthetic component crafting from the design document
    /// </summary>
    public partial class FailureSystem : MonoBehaviour
    {
        [Header("Failure Configuration")]
        [SerializeField] private bool enableFailureWarnings = true;
        [SerializeField] private bool allowFailureOverride = true;
        [SerializeField] private float baseFailureChance = 0.2f;

        [Header("Conflict Detection")]
        [SerializeField] private List<ConflictRule> conflictRules = new List<ConflictRule>();
        [SerializeField] private bool autoDetectAspectConflicts = true;

        [Header("Synthetic Creation")]
        [SerializeField] private bool enableSyntheticCreation = true;
        [SerializeField] private float syntheticCreationChance = 0.8f;
        [SerializeField] private SyntheticIngredientTemplate defaultSyntheticTemplate;

        // Events
        public System.Action<FailureAnalysis> OnConflictDetected;
        public System.Action<List<Ingredient>, Ingredient> OnSyntheticCreated;
        public System.Action<string> OnFailureMessageShown;

        private void Start()
        {
            InitializeDefaultConflicts();
        }

        /// <summary>
        /// Initialize common ingredient conflicts
        /// </summary>
        private void InitializeDefaultConflicts()
        {
            // Aspect conflicts from the design document
            conflictRules.Add(new ConflictRule
            {
                conflictName = "Fire and Ice Opposition",
                description = "Scorch and Frigid aspects are naturally opposed",
                conflictingAspects = new List<Aspect> { Aspect.Scorch, Aspect.Frigid },
                failureChanceIncrease = 0.3f,
                severity = ConflictSeverity.High,
                canCreateSynthetic = true
            });

            conflictRules.Add(new ConflictRule
            {
                conflictName = "Divine Corruption",
                description = "Divine aspects conflict with corrupted ingredients",
                requiresDivineAspect = true,
                requiresCorruptedIngredient = true,
                failureChanceIncrease = 0.4f,
                severity = ConflictSeverity.Critical,
                canCreateSynthetic = false
            });

            conflictRules.Add(new ConflictRule
            {
                conflictName = "Caustic Instability",
                description = "Multiple caustic ingredients create unstable reactions",
                requiredAspectCount = new Dictionary<Aspect, int> { { Aspect.Caustic, 3 } },
                failureChanceIncrease = 0.25f,
                severity = ConflictSeverity.Medium,
                canCreateSynthetic = true
            });

            conflictRules.Add(new ConflictRule
            {
                conflictName = "Arc Chaos Overload",
                description = "Too many Arc aspects create uncontrollable chaos",
                requiredAspectCount = new Dictionary<Aspect, int> { { Aspect.Arc, 2 } },
                failureChanceIncrease = 0.35f,
                severity = ConflictSeverity.High,
                chaosEffect = true,
                canCreateSynthetic = true
            });

            Debug.Log($"💥 Initialized {conflictRules.Count} conflict rules");
        }

        /// <summary>
        /// Analyze current grid state for conflicts
        /// </summary>
        public FailureAnalysis AnalyzeCurrentGrid()
        {
            // Get current placed ingredients from GridGameManager
            var gridManager = GridGameManager.Instance;
            if (gridManager == null)
            {
                Debug.LogWarning("💥 No GridGameManager found for failure analysis");
                return new FailureAnalysis { hasConflicts = false };
            }

            var placedIngredients = GetCurrentPlacedIngredients(gridManager);
            return AnalyzeIngredientConflicts(placedIngredients);
        }

        /// <summary>
        /// Analyze a specific set of ingredients for conflicts
        /// </summary>
        public FailureAnalysis AnalyzeIngredientConflicts(List<Ingredient> ingredients)
        {
            var analysis = new FailureAnalysis
            {
                analyzedIngredients = ingredients,
                baseFailureChance = baseFailureChance,
                detectedConflicts = new List<DetectedConflict>()
            };

            if (ingredients.Count < 2)
            {
                analysis.hasConflicts = false;
                return analysis;
            }

            // Check each conflict rule
            foreach (var rule in conflictRules)
            {
                var conflict = CheckConflictRule(rule, ingredients);
                if (conflict != null)
                {
                    analysis.detectedConflicts.Add(conflict);
                    analysis.totalFailureChance += rule.failureChanceIncrease;
                }
            }

            // Auto-detect aspect conflicts if enabled
            if (autoDetectAspectConflicts)
            {
                var aspectConflicts = DetectAutomaticAspectConflicts(ingredients);
                analysis.detectedConflicts.AddRange(aspectConflicts);
                analysis.totalFailureChance += aspectConflicts.Sum(c => c.failureIncrease);
            }

            analysis.hasConflicts = analysis.detectedConflicts.Count > 0;
            analysis.totalFailureChance = Mathf.Clamp01(analysis.totalFailureChance);

            // Fire event if conflicts found
            if (analysis.hasConflicts)
            {
                OnConflictDetected?.Invoke(analysis);
            }

            Debug.Log($"💥 Conflict Analysis: {analysis.detectedConflicts.Count} conflicts, {analysis.totalFailureChance:P} failure chance");

            return analysis;
        }

        /// <summary>
        /// Check a specific conflict rule against ingredients
        /// </summary>
        private DetectedConflict CheckConflictRule(ConflictRule rule, List<Ingredient> ingredients)
        {
            // Check aspect conflicts
            if (rule.conflictingAspects.Count > 0)
            {
                var foundAspects = ingredients.Select(i => i.IngredientAspect).Distinct().ToList();
                var conflictingFound = rule.conflictingAspects.Intersect(foundAspects);
                
                if (conflictingFound.Count() >= 2)
                {
                    return new DetectedConflict
                    {
                        conflictRule = rule,
                        conflictingIngredients = ingredients.Where(i => rule.conflictingAspects.Contains(i.IngredientAspect)).ToList(),
                        conflictDescription = $"{rule.conflictName}: {string.Join(" vs ", conflictingFound)}",
                        failureIncrease = rule.failureChanceIncrease,
                        severity = rule.severity
                    };
                }
            }

            // Check aspect count requirements
            if (rule.requiredAspectCount.Count > 0)
            {
                foreach (var kvp in rule.requiredAspectCount)
                {
                    var aspect = kvp.Key;
                    var requiredCount = kvp.Value;
                    var actualCount = ingredients.Count(i => i.IngredientAspect == aspect);

                    if (actualCount >= requiredCount)
                    {
                        return new DetectedConflict
                        {
                            conflictRule = rule,
                            conflictingIngredients = ingredients.Where(i => i.IngredientAspect == aspect).ToList(),
                            conflictDescription = $"{rule.conflictName}: {actualCount} {aspect} ingredients",
                            failureIncrease = rule.failureChanceIncrease,
                            severity = rule.severity
                        };
                    }
                }
            }

            // Check Divine vs Corrupted
            if (rule.requiresDivineAspect && rule.requiresCorruptedIngredient)
            {
                bool hasDivine = ingredients.Any(i => i.IngredientAspect == Aspect.Divine);
                bool hasCorrupted = ingredients.Any(i => i.IsCorrupted);

                if (hasDivine && hasCorrupted)
                {
                    return new DetectedConflict
                    {
                        conflictRule = rule,
                        conflictingIngredients = ingredients.Where(i => i.IngredientAspect == Aspect.Divine || i.IsCorrupted).ToList(),
                        conflictDescription = $"{rule.conflictName}: Divine purity conflicts with corruption",
                        failureIncrease = rule.failureChanceIncrease,
                        severity = rule.severity
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// Automatically detect obvious aspect conflicts
        /// </summary>
        private List<DetectedConflict> DetectAutomaticAspectConflicts(List<Ingredient> ingredients)
        {
            var conflicts = new List<DetectedConflict>();

            // Check for too many different aspects (chaos)
            var uniqueAspects = ingredients.Select(i => i.IngredientAspect).Distinct().Count();
            if (uniqueAspects >= 5)
            {
                conflicts.Add(new DetectedConflict
                {
                    conflictRule = new ConflictRule
                    {
                        conflictName = "Aspect Chaos",
                        description = "Too many different aspects create instability",
                        failureChanceIncrease = 0.15f
                    },
                    conflictingIngredients = ingredients,
                    conflictDescription = $"Aspect Chaos: {uniqueAspects} different aspects present",
                    failureIncrease = 0.15f,
                    severity = ConflictSeverity.Medium
                });
            }

            // Check for archetype mismatches
            var archetypes = ingredients.Select(i => i.IngredientArchetype).Distinct().ToList();
            if (archetypes.Contains(IngredientArchetype.Synthetic) && archetypes.Count > 2)
            {
                conflicts.Add(new DetectedConflict
                {
                    conflictRule = new ConflictRule
                    {
                        conflictName = "Synthetic Incompatibility",
                        description = "Synthetic ingredients don't mix well with natural ones",
                        failureChanceIncrease = 0.1f
                    },
                    conflictingIngredients = ingredients.Where(i => i.IngredientArchetype == IngredientArchetype.Synthetic).ToList(),
                    conflictDescription = "Synthetic Incompatibility: Mixed synthetic and natural ingredients",
                    failureIncrease = 0.1f,
                    severity = ConflictSeverity.Low
                });
            }

            return conflicts;
        }

        /// <summary>
        /// Show failure warning dialog to player
        /// </summary>
        public bool ShowFailureWarning()
        {
            if (!enableFailureWarnings)
                return true; // Proceed without warning

            var analysis = AnalyzeCurrentGrid();
            if (!analysis.hasConflicts)
                return true; // No conflicts, proceed

            string warningMessage = GenerateFailureWarningMessage(analysis);
            OnFailureMessageShown?.Invoke(warningMessage);

            // In a real implementation, this would show a dialog and return the player's choice
            // For now, return based on configuration
            bool proceedAnyway = allowFailureOverride;

            Debug.Log($"💥 Failure Warning Shown: {warningMessage}");
            Debug.Log($"💥 Player Choice: {(proceedAnyway ? "Proceed Anyway" : "Cancel")}");

            return proceedAnyway;
        }

        /// <summary>
        /// Generate warning message for conflicts
        /// </summary>
        private string GenerateFailureWarningMessage(FailureAnalysis analysis)
        {
            var message = $"⚠️ RECIPE CONFLICTS DETECTED ⚠️\n\n";
            message += $"Failure Chance: {analysis.totalFailureChance:P}\n\n";
            message += "Detected Issues:\n";

            foreach (var conflict in analysis.detectedConflicts)
            {
                string severityIcon = conflict.severity switch
                {
                    ConflictSeverity.Low => "⚡",
                    ConflictSeverity.Medium => "⚠️",
                    ConflictSeverity.High => "🔥",
                    ConflictSeverity.Critical => "💀",
                    _ => "❓"
                };

                message += $"{severityIcon} {conflict.conflictDescription}\n";
            }

            message += "\nIf you proceed and the recipe fails, you may create a synthetic ingredient instead.\n";
            message += "Do you want to continue anyway?";

            return message;
        }

        /// <summary>
        /// Handle recipe failure and attempt synthetic creation
        /// </summary>
        public void HandleRecipeFailure(List<Ingredient> usedIngredients)
        {
            Debug.Log($"💥 === RECIPE FAILURE HANDLING ===");
            Debug.Log($"💥 Used Ingredients: {string.Join(", ", usedIngredients.Select(i => i.ItemName))}");

            if (!enableSyntheticCreation)
            {
                Debug.Log($"💥 Synthetic creation disabled - ingredients lost");
                return;
            }

            // Roll for synthetic creation
            float roll = Random.Range(0f, 1f);
            if (roll < syntheticCreationChance)
            {
                var synthetic = CreateSyntheticIngredient(usedIngredients);
                if (synthetic != null)
                {
                    OnSyntheticCreated?.Invoke(usedIngredients, synthetic);
                    Debug.Log($"💥 ✅ Synthetic ingredient created: {synthetic.ItemName}");
                }
                else
                {
                    Debug.Log($"💥 ❌ Failed to create synthetic ingredient");
                }
            }
            else
            {
                Debug.Log($"💥 💀 No synthetic created - ingredients lost (roll: {roll:F2}, needed: < {syntheticCreationChance:F2})");
            }
        }

        /// <summary>
        /// Create a synthetic ingredient from failed recipe components
        /// </summary>
        private Ingredient CreateSyntheticIngredient(List<Ingredient> sourceIngredients)
        {
            if (sourceIngredients.Count == 0) return null;

            // For now, return a placeholder since we can't create ScriptableObject instances at runtime
            // In a full implementation, this would either:
            // 1. Use a pool of pre-created synthetic ingredients
            // 2. Generate synthetic data structures that can be serialized
            // 3. Use a synthetic ingredient factory system

            Debug.Log($"🧬 Creating synthetic from: {string.Join(", ", sourceIngredients.Select(i => i.ItemName))}");

            var syntheticData = new SyntheticIngredientData
            {
                generatedName = GenerateSyntheticName(sourceIngredients),
                sourceIngredients = sourceIngredients,
                combinedAspects = sourceIngredients.Select(i => i.IngredientAspect).Distinct().ToList(),
                averagePotency = Mathf.RoundToInt((float)sourceIngredients.Average(i => i.Potency)),
                isStable = DetermineStability(sourceIngredients),
                creationDate = System.DateTime.Now
            };

            Debug.Log($"🧬 Synthetic Data: {syntheticData.generatedName} (Potency: {syntheticData.averagePotency}, Stable: {syntheticData.isStable})");

            // In a real implementation, would create an actual Ingredient instance
            // For now, return the first source ingredient as a placeholder
            return sourceIngredients[0];
        }

        /// <summary>
        /// Generate a name for the synthetic ingredient
        /// </summary>
        private string GenerateSyntheticName(List<Ingredient> sourceIngredients)
        {
            var adjectives = new List<string> { "Unstable", "Volatile", "Synthetic", "Artificial", "Hybrid", "Mutated", "Experimental" };
            var baseNames = sourceIngredients.Select(i => i.ItemName.Split(' ').Last()).ToList();
            
            var randomAdjective = adjectives[Random.Range(0, adjectives.Count)];
            var baseName = baseNames[Random.Range(0, baseNames.Count)];
            
            return $"{randomAdjective} {baseName} Compound";
        }

        /// <summary>
        /// Determine if the synthetic ingredient is stable
        /// </summary>
        private bool DetermineStability(List<Ingredient> sourceIngredients)
        {
            // Base stability on conflict severity
            var analysis = AnalyzeIngredientConflicts(sourceIngredients);
            
            if (!analysis.hasConflicts) return true;
            
            var criticalConflicts = analysis.detectedConflicts.Count(c => c.severity == ConflictSeverity.Critical);
            var highConflicts = analysis.detectedConflicts.Count(c => c.severity == ConflictSeverity.High);
            
            if (criticalConflicts > 0) return false;
            if (highConflicts > 1) return false;
            
            return Random.Range(0f, 1f) > 0.3f; // 70% chance for stable if no critical conflicts
        }

        /// <summary>
        /// Get currently placed ingredients from grid manager
        /// </summary>
        private List<Ingredient> GetCurrentPlacedIngredients(FourFatesStudios.ProjectWarden.GridDemo.GridGameManager gridManager)
        {
            var ingredients = new List<Ingredient>();
            
            if (gridManager.GetComponent<IngredientPlacer>() != null)
            {
                var placer = gridManager.GetComponent<IngredientPlacer>();
                var placedIngredients = placer.GetAllPlacedIngredients();
                ingredients.AddRange(placedIngredients.Select(p => p.ingredient).Distinct());
            }
            
            return ingredients;
        }

        #region Public API

        /// <summary>
        /// Add a custom conflict rule
        /// </summary>
        public void AddConflictRule(ConflictRule rule)
        {
            conflictRules.Add(rule);
            Debug.Log($"💥 Added conflict rule: {rule.conflictName}");
        }

        /// <summary>
        /// Test the failure system
        /// </summary>
        [ContextMenu("Test Failure System")]
        public void TestFailureSystem()
        {
            Debug.Log("💥 === TESTING FAILURE SYSTEM ===");
            Debug.Log($"💥 Failure Warnings Enabled: {enableFailureWarnings}");
            Debug.Log($"💥 Allow Override: {allowFailureOverride}");
            Debug.Log($"💥 Base Failure Chance: {baseFailureChance:P}");
            Debug.Log($"💥 Synthetic Creation: {enableSyntheticCreation} ({syntheticCreationChance:P})");
            Debug.Log($"💥 Conflict Rules: {conflictRules.Count}");
            
            foreach (var rule in conflictRules)
            {
                Debug.Log($"   - {rule.conflictName}: {rule.description}");
            }
            
            // Test current grid
            var analysis = AnalyzeCurrentGrid();
            Debug.Log($"💥 Current Grid Analysis: {analysis.detectedConflicts.Count} conflicts, {analysis.totalFailureChance:P} failure chance");
            
            Debug.Log("💥 === FAILURE SYSTEM TEST COMPLETE ===");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class ConflictRule
    {
        [Header("Basic Info")]
        public string conflictName;
        [TextArea(2, 3)]
        public string description;

        [Header("Conflict Conditions")]
        public List<Aspect> conflictingAspects = new List<Aspect>();
        public Dictionary<Aspect, int> requiredAspectCount = new Dictionary<Aspect, int>();
        public bool requiresDivineAspect = false;
        public bool requiresCorruptedIngredient = false;

        [Header("Effects")]
        [Range(0f, 1f)]
        public float failureChanceIncrease = 0.2f;
        public ConflictSeverity severity = ConflictSeverity.Medium;
        public bool canCreateSynthetic = true;
        public bool chaosEffect = false;
    }

    [System.Serializable]
    public class DetectedConflict
    {
        public ConflictRule conflictRule;
        public List<Ingredient> conflictingIngredients;
        public string conflictDescription;
        public float failureIncrease;
        public ConflictSeverity severity;
    }

    [System.Serializable]
    public class FailureAnalysis
    {
        public bool hasConflicts;
        public List<Ingredient> analyzedIngredients;
        public List<DetectedConflict> detectedConflicts;
        public float baseFailureChance;
        public float totalFailureChance;
    }

    [System.Serializable]
    public class SyntheticIngredientData
    {
        public string generatedName;
        public List<Ingredient> sourceIngredients;
        public List<Aspect> combinedAspects;
        public int averagePotency;
        public bool isStable;
        public System.DateTime creationDate;
    }

    [System.Serializable]
    public class SyntheticIngredientTemplate
    {
        public string templateName;
        public IngredientArchetype archetype;
        public Aspect defaultAspect;
        public int basePotency;
        public bool isCorrupted;
    }

    public enum ConflictSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    #endregion
}