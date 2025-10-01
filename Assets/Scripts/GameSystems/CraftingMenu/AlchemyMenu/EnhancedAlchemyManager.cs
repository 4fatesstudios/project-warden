using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Enhanced Alchemy Manager that integrates all the advanced systems from the design document
    /// This is the main orchestrator for the complex alchemy system
    /// </summary>
    public class EnhancedAlchemyManager : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private GridMinigameController gridController;
        [SerializeField] private SynergySystem synergySystem;
        [SerializeField] private TemplateSystem templateSystem;
        // private SyntheticIngredientCreator syntheticCreator; // TODO: Implement SyntheticIngredientCreator

        [Header("System Configuration")]
        [SerializeField] private bool enableAdvancedFeatures = true;
        [SerializeField] private bool enableTemplateMode = true;
        [SerializeField] private bool enableSynergyDiscovery = true;
        [SerializeField] private bool enableSyntheticCreation = true;
        [SerializeField] private bool enableObstacleSystem = true;

        [Header("Skill Integration")]
        [SerializeField] private bool enableSkillEffects = true;
        [SerializeField] private int maxOverlapTiles = 3;
        [SerializeField] private bool enableIngredientRefund = false;
        [SerializeField] private float refundChance = 0.15f;

        // Current crafting session state
        private AlchemyRecipe currentRecipe;
        private Dictionary<Vector2Int, Ingredient> currentPlacements = new Dictionary<Vector2Int, Ingredient>();
        private List<SynergyInstance> activeSynergies = new List<SynergyInstance>();
        private bool isCraftingInProgress = false;

        // Statistics tracking
        private int synergiesDiscoveredThisSession = 0;
        private int syntheticsCreatedThisSession = 0;
        private int templatesCompletedThisSession = 0;

        // Events for UI integration
        public System.Action<AlchemyRecipe, Dictionary<Vector2Int, Ingredient>, EnhancedCraftingResult> OnAdvancedCraftingCompleted;
        public System.Action<SynergyInstance> OnSynergyDiscovered;
        public System.Action<SyntheticIngredientData> OnSyntheticCreated;
        public System.Action<string> OnSystemStatusChanged;

        private void Awake()
        {
            InitializeSystemIntegration();
        }

        private void Start()
        {
            ConnectEventHandlers();
            InitializeSkillEffects();
        }

        /// <summary>
        /// Initialize integration between all systems
        /// </summary>
        private void InitializeSystemIntegration()
        {
            // Find components if not assigned
            if (gridController == null)
                gridController = GetComponent<GridMinigameController>();
            if (synergySystem == null)
                synergySystem = GetComponentInChildren<SynergySystem>();
            // if (syntheticCreator == null)
            //     syntheticCreator = GetComponentInChildren<SyntheticIngredientCreator>();
            if (templateSystem == null)
                templateSystem = GetComponentInChildren<TemplateSystem>();

            // Also try to find components in parent or siblings
            if (gridController == null)
                gridController = GetComponentInParent<GridMinigameController>();
            if (gridController == null)
                gridController = Object.FindFirstObjectByType<GridMinigameController>();

            // Validate core components
            if (gridController == null)
            {
                Debug.LogError("EnhancedAlchemyManager requires GridMinigameController component!");
                enabled = false;
                return;
            }

            Debug.Log("🧪 Enhanced Alchemy Manager initialized with all systems");
        }

        /// <summary>
        /// Connect event handlers between systems
        /// </summary>
        private void ConnectEventHandlers()
        {
            // Grid controller events
            if (gridController != null)
            {
                gridController.OnCraftingCompleted += HandleCraftingAttempt;
            }

            // Synergy system events
            if (synergySystem != null)
            {
                synergySystem.OnSynergyDiscovered += HandleSynergyDiscovered;
                synergySystem.OnSynergyActivated += HandleSynergyActivated;
            }

            // Synthetic creator events
            // if (syntheticCreator != null)
            // {
            //     syntheticCreator.OnSyntheticIngredientCreated += HandleSyntheticCreated;
            // }

            // Template system events
            if (templateSystem != null)
            {
                templateSystem.OnTemplateCompleted += HandleTemplateCompleted;
                templateSystem.OnTemplateActivated += HandleTemplateActivated;
            }

            // Obstacle system events - using GridGameManager directly
            // if (obstacleSystem != null)
            // {
            //     obstacleSystem.OnObstacleStateChanged += HandleObstacleStateChanged;
            // }
        }

        /// <summary>
        /// Initialize skill system effects
        /// </summary>
        private void InitializeSkillEffects()
        {
            if (!enableSkillEffects) return;

            // Apply skill-based enhancements
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem != null)
            {
                // Enhanced overlap from skills
                if (skillSystem.HasSkill("advanced_overlap"))
                {
                    maxOverlapTiles = 5;
                    Debug.Log("🎯 Advanced overlap skill activated");
                }

                // Ingredient refund skill
                if (skillSystem.HasSkill("ingredient_conservation"))
                {
                    enableIngredientRefund = true;
                    refundChance = 0.25f;
                    Debug.Log("♻️ Ingredient conservation skill activated");
                }

                // Template mastery
                if (skillSystem.HasSkill("template_mastery"))
                {
                    enableTemplateMode = true;
                    Debug.Log("📋 Template mastery skill activated");
                }
            }
        }

        /// <summary>
        /// Handle crafting attempts with enhanced processing
        /// </summary>
        public void HandleCraftingAttempt(AlchemyRecipe recipe, Dictionary<Vector2Int, PlacedIngredient> placedIngredients, bool basicSuccess)
        {
            if (isCraftingInProgress) return;

            isCraftingInProgress = true;
            currentRecipe = recipe;

            // Convert placed ingredients to dictionary
            currentPlacements = placedIngredients.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ingredient
            );

            Debug.Log($"🧪 Processing enhanced crafting for recipe: {recipe.ItemName}");

            // Process with all enhanced systems
            var result = ProcessEnhancedCrafting(recipe, currentPlacements, basicSuccess);

            // Notify systems and UI
            OnAdvancedCraftingCompleted?.Invoke(recipe, currentPlacements, result);

            isCraftingInProgress = false;
        }

        /// <summary>
        /// Process crafting with all enhanced systems
        /// </summary>
        private EnhancedCraftingResult ProcessEnhancedCrafting(AlchemyRecipe recipe, Dictionary<Vector2Int, Ingredient> placements, bool basicSuccess)
        {
            var result = new EnhancedCraftingResult
            {
                recipe = recipe,
                wasSuccessful = basicSuccess,
                originalPotency = recipe.OutputPotion?.Potency ?? 100,
                originalQuantity = recipe.OutputQuantity,
                finalPotency = recipe.OutputPotion?.Potency ?? 100,
                finalQuantity = recipe.OutputQuantity,
                efficiencyBonus = 0f
            };

            // Update all systems with current placement
            UpdateSystemsWithPlacements(placements);

            // 1. Calculate synergy effects
            if (enableSynergyDiscovery && synergySystem != null)
            {
                synergySystem.AnalyzeSynergies(placements);
                activeSynergies = synergySystem.GetActiveSynergies();
                
                ApplySynergyEffects(result);
            }

            // 2. Apply obstacle effects using GridGameManager
            if (enableObstacleSystem)
            {
                ApplyObstacleEffects(result, placements);
            }

            // 3. Apply template rewards if applicable
            if (enableTemplateMode && templateSystem != null && templateSystem.IsTemplateActive)
            {
                ApplyTemplateEffects(result);
            }

            // 4. Apply skill bonuses
            if (enableSkillEffects)
            {
                ApplySkillEffects(result);
            }

            // 5. Handle ingredient refunds
            if (enableIngredientRefund)
            {
                HandleIngredientRefunds(placements);
            }

            // 6. Handle synthetic creation if crafting failed (disabled until SyntheticIngredientCreator is implemented)
            // if (!result.wasSuccessful && enableSyntheticCreation && syntheticCreator != null)
            // {
            //     HandleSyntheticCreation(placements, result);
            // }

            // 7. Final calculations
            CalculateFinalResults(result);

            LogCraftingResults(result);
            return result;
        }

        /// <summary>
        /// Update all systems with current ingredient placements
        /// </summary>
        private void UpdateSystemsWithPlacements(Dictionary<Vector2Int, Ingredient> placements)
        {
            // Update obstacle system for adjacency checks - using GridGameManager directly
            // if (obstacleSystem != null)
            // {
            //     obstacleSystem.UpdateGridState(placements);
            // }

            // Update template system progress
            if (templateSystem != null && templateSystem.IsTemplateActive)
            {
                templateSystem.UpdateTemplateProgress(placements);
            }
        }

        /// <summary>
        /// Apply synergy effects to crafting result
        /// </summary>
        private void ApplySynergyEffects(EnhancedCraftingResult result)
        {
            if (activeSynergies.Count == 0) return;

            float totalPotencyMultiplier = 1.0f;
            float totalEfficiencyBonus = 0f;
            var synergyDescriptions = new List<string>();

            foreach (var synergy in activeSynergies)
            {
                totalPotencyMultiplier *= synergy.effectivePotencyMultiplier;
                totalEfficiencyBonus += synergy.synergyData.efficiencyBonus;
                synergyDescriptions.Add(synergy.synergyData.synergyName);
                
                if (synergy.synergyData.unlocksNewEffect)
                {
                    result.specialEffects.Add(synergy.synergyData.newEffectDescription);
                }
            }

            result.synergyMultiplier = totalPotencyMultiplier;
            result.efficiencyBonus += totalEfficiencyBonus;
            result.activeSynergies = synergyDescriptions;

            Debug.Log($"✨ Applied synergy effects: {totalPotencyMultiplier:F2}x potency, +{totalEfficiencyBonus:P} efficiency");
        }

        /// <summary>
        /// Apply obstacle effects to crafting result
        /// </summary>
        private void ApplyObstacleEffects(EnhancedCraftingResult result, Dictionary<Vector2Int, Ingredient> placements)
        {
            // Calculate potency modifications from obstacles using GridGameManager
            var gridManager = GridGameManager.Instance;
            if (gridManager == null) return;

            float obstacleMultiplier = 1.0f;
            var obstacleEffects = new List<string>();

            foreach (var placement in placements)
            {
                var position = placement.Key;
                var ingredient = placement.Value;
                
                // Get obstacle at position using GridGameManager
                var obstacle = gridManager.GetObstacleAt(position);
                if (obstacle != null)
                {
                    // Calculate effective potency using obstacle
                    int effectivePotency = obstacle.GetEffectivePotency(ingredient);
                    float baselinePotency = ingredient.Potency;
                    
                    if (effectivePotency != baselinePotency)
                    {
                        float positionMultiplier = effectivePotency / baselinePotency;
                        obstacleMultiplier *= positionMultiplier;
                        
                        obstacleEffects.Add($"{obstacle.ObstacleType} at {position}: {positionMultiplier:F2}x");
                    }
                }
            }

            result.obstacleMultiplier = obstacleMultiplier;
            result.obstacleEffects = obstacleEffects;

            Debug.Log($"🎯 Applied obstacle effects: {obstacleMultiplier:F2}x total modifier");
        }

        /// <summary>
        /// Apply template effects to crafting result
        /// </summary>
        private void ApplyTemplateEffects(EnhancedCraftingResult result)
        {
            if (!templateSystem.IsTemplateActive) return;

            // Apply template rewards to the result
            int potency = result.finalPotency;
            int quantity = result.finalQuantity;
            float efficiency = result.efficiencyBonus;

            templateSystem.ApplyTemplateRewardsToResult(ref potency, ref quantity, ref efficiency);

            result.finalPotency = potency;
            result.finalQuantity = quantity;
            result.efficiencyBonus = efficiency;
            result.templateRewardsApplied = true;

            Debug.Log("📋 Applied template rewards to crafting result");
        }

        /// <summary>
        /// Apply skill-based effects to crafting result
        /// </summary>
        private void ApplySkillEffects(EnhancedCraftingResult result)
        {
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null) return;

            // Potency boost skills
            if (skillSystem.HasSkill("master_alchemist"))
            {
                result.skillMultiplier *= 1.15f; // 15% bonus
                result.skillEffects.Add("Master Alchemist: +15% potency");
            }

            // Efficiency skills
            if (skillSystem.HasSkill("efficient_crafting"))
            {
                result.efficiencyBonus += 0.1f; // 10% efficiency bonus
                result.skillEffects.Add("Efficient Crafting: +10% efficiency");
            }

            // Bonus quantity skills
            if (skillSystem.HasSkill("abundant_harvest") && Random.value < 0.3f)
            {
                result.bonusQuantity += 1;
                result.skillEffects.Add("Abundant Harvest: +1 bonus potion");
            }

            Debug.Log($"🎯 Applied skill effects: {result.skillMultiplier:F2}x multiplier, {result.skillEffects.Count} effects");
        }

        /// <summary>
        /// Handle potential ingredient refunds based on skills
        /// </summary>
        private void HandleIngredientRefunds(Dictionary<Vector2Int, Ingredient> placements)
        {
            if (!enableIngredientRefund) return;

            var refundedIngredients = new List<Ingredient>();
            
            foreach (var ingredient in placements.Values.Distinct())
            {
                if (Random.value < refundChance)
                {
                    refundedIngredients.Add(ingredient);
                    
                    // Add back to inventory if using inventory integration
                    if (gridController.UseInventoryIngredients && gridController.InventoryHolder != null)
                    {
                        gridController.InventoryHolder.AddItem(ingredient, 1);
                    }
                }
            }

            if (refundedIngredients.Count > 0)
            {
                Debug.Log($"♻️ Refunded {refundedIngredients.Count} ingredients: {string.Join(", ", refundedIngredients.Select(i => i.ItemName))}");
            }
        }

        /// <summary>
        /// Handle synthetic ingredient creation from failed crafting
        /// </summary>
        private void HandleSyntheticCreation(Dictionary<Vector2Int, Ingredient> placements, EnhancedCraftingResult result)
        {
            // TODO: Implement SyntheticIngredientCreator
            // var sourceIngredients = placements.Values.Distinct().ToList();
            // 
            // if (syntheticCreator.CanCreateSynthetic(sourceIngredients))
            // {
            //     var syntheticIngredient = syntheticCreator.CreateSyntheticIngredient(sourceIngredients, placements);
            //     
            //     if (syntheticIngredient != null)
            //     {
            //         result.syntheticCreated = syntheticIngredient;
            //         result.wasSyntheticCreated = true;
            //         syntheticsCreatedThisSession++;
            //         
            //         // Add to inventory if using inventory integration
            //         if (gridController.AddResultsToInventory && gridController.InventoryHolder != null)
            //         {
            //             gridController.InventoryHolder.AddItem(syntheticIngredient, 1);
            //         }
            //     }
            // }
        }

        /// <summary>
        /// Calculate final results with all modifiers applied
        /// </summary>
        private void CalculateFinalResults(EnhancedCraftingResult result)
        {
            // Apply all multipliers to potency
            float totalMultiplier = result.synergyMultiplier * result.obstacleMultiplier * result.skillMultiplier;
            result.finalPotency = Mathf.RoundToInt(result.originalPotency * totalMultiplier);

            // Add bonus quantities
            result.finalQuantity = result.originalQuantity + result.bonusQuantity;

            // Calculate overall success rating
            result.successRating = CalculateSuccessRating(result);

            Debug.Log($"🎉 Final results: {result.finalPotency} potency ({totalMultiplier:F2}x), {result.finalQuantity} quantity, {result.successRating:F1} rating");
        }

        /// <summary>
        /// Calculate overall success rating for the crafting attempt
        /// </summary>
        private float CalculateSuccessRating(EnhancedCraftingResult result)
        {
            float rating = result.wasSuccessful ? 1.0f : 0.3f; // Base rating

            // Bonus for synergies
            rating += activeSynergies.Count * 0.2f;

            // Bonus for efficiency
            rating += result.efficiencyBonus;

            // Bonus for template completion
            if (result.templateRewardsApplied)
                rating += 0.3f;

            // Bonus for skill effects
            rating += result.skillEffects.Count * 0.1f;

            return Mathf.Clamp(rating, 0f, 3.0f); // 0-3 star rating
        }

        /// <summary>
        /// Log detailed crafting results
        /// </summary>
        private void LogCraftingResults(EnhancedCraftingResult result)
        {
            var log = new System.Text.StringBuilder();
            log.AppendLine($"🧪 CRAFTING COMPLETE: {result.recipe.ItemName}");
            log.AppendLine($"   Success: {result.wasSuccessful}");
            log.AppendLine($"   Potency: {result.originalPotency} → {result.finalPotency} ({result.finalPotency / (float)result.originalPotency:F2}x)");
            log.AppendLine($"   Quantity: {result.originalQuantity} → {result.finalQuantity}");
            log.AppendLine($"   Rating: {result.successRating:F1}/3.0 stars");
            
            if (result.activeSynergies.Count > 0)
                log.AppendLine($"   Synergies: {string.Join(", ", result.activeSynergies)}");
            
            if (result.obstacleEffects.Count > 0)
                log.AppendLine($"   Obstacles: {result.obstacleEffects.Count} effects applied");
            
            if (result.skillEffects.Count > 0)
                log.AppendLine($"   Skills: {string.Join(", ", result.skillEffects)}");

            Debug.Log(log.ToString());
        }

        #region Event Handlers

        private void HandleSynergyDiscovered(SynergyInstance synergy)
        {
            synergiesDiscoveredThisSession++;
            OnSynergyDiscovered?.Invoke(synergy);
            OnSystemStatusChanged?.Invoke($"New synergy discovered: {synergy.synergyData.synergyName}!");
        }

        private void HandleSynergyActivated(SynergyInstance synergy)
        {
            OnSystemStatusChanged?.Invoke($"Synergy activated: {synergy.synergyData.synergyName}");
        }

        private void HandleSyntheticCreated(SyntheticIngredientData syntheticData)
        {
            // TODO: Implement when SyntheticIngredientCreator is available
            // OnSyntheticCreated?.Invoke(syntheticData);
            // OnSystemStatusChanged?.Invoke($"Synthetic ingredient created: {syntheticData.generatedName}");
        }
        
        private void HandleTemplateCompleted(AlchemyTemplate template, bool successful)
        {
            if (successful)
            {
                templatesCompletedThisSession++;
                OnSystemStatusChanged?.Invoke($"Template completed: {template.templateName}!");
            }
            else
            {
                OnSystemStatusChanged?.Invoke($"Template failed: {template.templateName}");
            }
        }

        private void HandleTemplateActivated(AlchemyTemplate template)
        {
            OnSystemStatusChanged?.Invoke($"Template activated: {template.templateName}");
        }

        // TODO: Implement when EnhancedObstacleSystem is available
        // private void HandleObstacleStateChanged(AspectObstacle obstacle, string description)
        // {
        //     OnSystemStatusChanged?.Invoke($"Obstacle effect: {description}");
        // }
        #endregion

        #region Public API

        /// <summary>
        /// Get current session statistics
        /// </summary>
        public SessionStatistics GetSessionStatistics()
        {
            return new SessionStatistics
            {
                templatesCompleted = templatesCompletedThisSession,
                activeSynergiesCount = activeSynergies.Count,
                isTemplateActive = templateSystem?.IsTemplateActive ?? false,
                currentTemplateName = templateSystem?.CurrentTemplate?.templateName ?? ""
            };
        }

        /// <summary>
        /// Reset session statistics
        /// </summary>
        public void ResetSessionStatistics()
        {
            synergiesDiscoveredThisSession = 0;
            syntheticsCreatedThisSession = 0;
            templatesCompletedThisSession = 0;
            
            OnSystemStatusChanged?.Invoke("Session statistics reset");
        }

        /// <summary>
        /// Enable or disable specific features
        /// </summary>
        public void SetFeatureEnabled(string featureName, bool enabled)
        {
            switch (featureName.ToLower())
            {
                case "synergies":
                    enableSynergyDiscovery = enabled;
                    break;
                case "obstacles":
                    enableObstacleSystem = enabled;
                    break;
                case "templates":
                    enableTemplateMode = enabled;
                    break;
                case "synthetics":
                    enableSyntheticCreation = enabled;
                    break;
                case "skills":
                    enableSkillEffects = enabled;
                    break;
            }
            
            OnSystemStatusChanged?.Invoke($"Feature '{featureName}' {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Get current system status for UI display
        /// </summary>
        public string GetSystemStatus()
        {
            var status = new List<string>();
            
            if (enableSynergyDiscovery && activeSynergies.Count > 0)
                status.Add($"{activeSynergies.Count} synergies active");
            
            if (enableTemplateMode && templateSystem?.IsTemplateActive == true)
                status.Add($"Template: {templateSystem.CurrentTemplate.templateName}");
            
            // TODO: Add obstacle status when EnhancedObstacleSystem is implemented
            // if (enableObstacleSystem && obstacleSystem?.ActiveObstacles.Count > 0)
            //     status.Add($"{obstacleSystem.ActiveObstacles.Count} obstacles");
            
            return status.Count > 0 ? string.Join(" • ", status) : "Standard crafting mode";
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class EnhancedCraftingResult
    {
        public AlchemyRecipe recipe;
        public bool wasSuccessful;
        public int originalPotency;
        public int originalQuantity;
        public int finalPotency;
        public int finalQuantity;
        public int bonusQuantity;
        
        public float synergyMultiplier = 1.0f;
        public float obstacleMultiplier = 1.0f;
        public float skillMultiplier = 1.0f;
        public float efficiencyBonus = 0f;
        public float successRating = 0f;
        
        public List<string> activeSynergies = new List<string>();
        public List<string> obstacleEffects = new List<string>();
        public List<string> skillEffects = new List<string>();
        public List<string> specialEffects = new List<string>();
        
        public bool templateRewardsApplied = false;
    }

    [System.Serializable]
    public class SessionStatistics
    {
        public int templatesCompleted;
        public int activeSynergiesCount;
        public bool isTemplateActive;
        public string currentTemplateName;
    }

    #endregion
}