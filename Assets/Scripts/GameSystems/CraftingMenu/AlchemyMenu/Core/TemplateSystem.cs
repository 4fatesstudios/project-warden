using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Handles the template system for puzzle-based alchemy challenges
    /// Implements the "Templates" feature from the design document
    /// </summary>
    public class TemplateSystem : MonoBehaviour
    {
        [Header("Template Configuration")]
        [SerializeField] private bool enableTemplateMode = true;
        [SerializeField] private List<AlchemyTemplate> availableTemplates = new List<AlchemyTemplate>();
        [SerializeField] private AlchemyTemplate currentTemplate;

        [Header("Template Discovery")]
        [SerializeField] private bool templatesFoundInWorld = true;
        [SerializeField] private float templateDiscoveryChance = 0.15f;

        [Header("Reward Configuration")]
        [SerializeField] private float baseRewardMultiplier = 1.5f;
        [SerializeField] private int baseSkillPointReward = 2;

        // Current template state
        private Dictionary<Vector2Int, TemplateCell> templateGrid = new Dictionary<Vector2Int, TemplateCell>();
        private Dictionary<Vector2Int, Ingredient> currentPlacements = new Dictionary<Vector2Int, Ingredient>();
        private bool isTemplateActive = false;
        private float currentCompletionPercentage = 0f;

        // Events
        public System.Action<AlchemyTemplate, bool> OnTemplateCompleted;
        public System.Action<AlchemyTemplate> OnTemplateActivated;
        public System.Action<AlchemyTemplate> OnTemplateDiscovered;
        public System.Action<TemplateProgress> OnTemplateProgressUpdated;

        // Template state tracking
        private Dictionary<string, bool> discoveredTemplates = new Dictionary<string, bool>();
        private Dictionary<string, int> templateCompletionCounts = new Dictionary<string, int>();

        public bool IsTemplateActive => isTemplateActive && currentTemplate != null;
        public AlchemyTemplate CurrentTemplate => currentTemplate;

        private void Start()
        {
            InitializeDefaultTemplates();
            LoadTemplateProgress();
        }

        /// <summary>
        /// Initialize default templates based on the design document
        /// </summary>
        private void InitializeDefaultTemplates()
        {
            CreateSampleTemplates();
        }

        /// <summary>
        /// Public method to create sample templates for testing
        /// </summary>
        public void CreateSampleTemplates()
        {
            // Beginner Template - Simple 3x3 with basic obstacles
            var beginnerTemplate = new AlchemyTemplate
            {
                templateId = "beginner_balance",
                templateName = "Balance of Elements",
                description = "A simple template to learn the basics of obstacle interaction",
                difficulty = TemplateDifficulty.Beginner,
                gridSize = new Vector2Int(3, 3),
                bonusMultiplier = 1.2f,
                bonusSkillPoints = 1,
                templateCells = new List<TemplateCellData>
                {
                    // Center obstacle
                    new TemplateCellData { position = new Vector2Int(1, 1), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Frigid },
                    // Corner requirements
                    new TemplateCellData { position = new Vector2Int(0, 0), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Scorch },
                    new TemplateCellData { position = new Vector2Int(2, 2), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Caustic }
                }
            };
            availableTemplates.Add(beginnerTemplate);

            // Intermediate Template - Cascade challenge
            var intermediateTemplate = new AlchemyTemplate
            {
                templateId = "cascade_reaction",
                templateName = "Cascade Reaction",
                description = "Chain multiple obstacle interactions for maximum effect",
                difficulty = TemplateDifficulty.Intermediate,
                gridSize = new Vector2Int(4, 4),
                bonusMultiplier = 1.8f,
                bonusSkillPoints = 3,
                templateCells = new List<TemplateCellData>
                {
                    // Diagonal line of obstacles
                    new TemplateCellData { position = new Vector2Int(0, 0), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Arc },
                    new TemplateCellData { position = new Vector2Int(1, 1), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Scorch },
                    new TemplateCellData { position = new Vector2Int(2, 2), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Frigid },
                    new TemplateCellData { position = new Vector2Int(3, 3), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Divine },
                    // Required placements
                    new TemplateCellData { position = new Vector2Int(0, 3), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Arc },
                    new TemplateCellData { position = new Vector2Int(3, 0), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Divine }
                }
            };
            availableTemplates.Add(intermediateTemplate);

            // Advanced Template - The Grand Cross
            var advancedTemplate = new AlchemyTemplate
            {
                templateId = "grand_cross",
                templateName = "The Grand Cross",
                description = "Master all aspects in perfect harmony",
                difficulty = TemplateDifficulty.Advanced,
                gridSize = new Vector2Int(5, 5),
                bonusMultiplier = 2.5f,
                bonusSkillPoints = 5,
                templateCells = new List<TemplateCellData>
                {
                    // Cross pattern of obstacles
                    new TemplateCellData { position = new Vector2Int(2, 0), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Divine },
                    new TemplateCellData { position = new Vector2Int(2, 1), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Scorch },
                    new TemplateCellData { position = new Vector2Int(2, 2), cellType = TemplateCellType.LockedIngredient }, // Center must be specific ingredient
                    new TemplateCellData { position = new Vector2Int(2, 3), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Frigid },
                    new TemplateCellData { position = new Vector2Int(2, 4), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Caustic },
                    // Horizontal arm
                    new TemplateCellData { position = new Vector2Int(0, 2), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Arc },
                    new TemplateCellData { position = new Vector2Int(1, 2), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Corporeal },
                    new TemplateCellData { position = new Vector2Int(3, 2), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Corporeal },
                    new TemplateCellData { position = new Vector2Int(4, 2), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Arc },
                    // Required aspect placements around the cross
                    new TemplateCellData { position = new Vector2Int(1, 1), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Divine },
                    new TemplateCellData { position = new Vector2Int(3, 1), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Arc },
                    new TemplateCellData { position = new Vector2Int(1, 3), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Corporeal },
                    new TemplateCellData { position = new Vector2Int(3, 3), cellType = TemplateCellType.RequiredIngredient, requiredAspect = Aspect.Caustic }
                }
            };
            availableTemplates.Add(advancedTemplate);

            // Master Template - Chaotic Symphony
            var masterTemplate = new AlchemyTemplate
            {
                templateId = "chaotic_symphony",
                templateName = "Chaotic Symphony",
                description = "Tame the forces of chaos and turn discord into harmony",
                difficulty = TemplateDifficulty.Master,
                gridSize = new Vector2Int(5, 5),
                bonusMultiplier = 3.0f,
                bonusSkillPoints = 8,
                requiresStoryIngredient = true,
                storyIngredientLockPosition = new Vector2Int(2, 2),
                templateCells = new List<TemplateCellData>
                {
                    // Chaotic arrangement of all obstacle types
                    new TemplateCellData { position = new Vector2Int(0, 0), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Arc },
                    new TemplateCellData { position = new Vector2Int(0, 4), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Scorch },
                    new TemplateCellData { position = new Vector2Int(4, 0), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Frigid },
                    new TemplateCellData { position = new Vector2Int(4, 4), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Caustic },
                    // Ring of obstacles around center
                    new TemplateCellData { position = new Vector2Int(1, 1), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Divine },
                    new TemplateCellData { position = new Vector2Int(2, 1), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Corporeal },
                    new TemplateCellData { position = new Vector2Int(3, 1), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Divine },
                    new TemplateCellData { position = new Vector2Int(1, 2), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Corporeal },
                    new TemplateCellData { position = new Vector2Int(3, 2), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Corporeal },
                    new TemplateCellData { position = new Vector2Int(1, 3), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Divine },
                    new TemplateCellData { position = new Vector2Int(2, 3), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Corporeal },
                    new TemplateCellData { position = new Vector2Int(3, 3), cellType = TemplateCellType.Obstacle, obstacleType = ObstacleType.Divine },
                    // Center is for story ingredient
                    new TemplateCellData { position = new Vector2Int(2, 2), cellType = TemplateCellType.LockedIngredient, isStoryLocked = true }
                }
            };
            availableTemplates.Add(masterTemplate);

            Debug.Log($"🗺️ Created {availableTemplates.Count} sample templates");
        }
        

        /// <summary>
        /// Load template discovery progress
        /// </summary>
        private void LoadTemplateProgress()
        {
            // In a full implementation, this would load from persistent storage
            // For now, mark the beginner template as discovered
            discoveredTemplates["beginner_balance"] = true;
            
            Debug.Log($"🗺️ Loaded template progress: {discoveredTemplates.Count} templates discovered");
        }

        /// <summary>
        /// Activate a template for the current crafting session
        /// </summary>
        public bool ActivateTemplate(string templateId)
        {
            if (!enableTemplateMode)
            {
                Debug.LogWarning("🗺️ Template mode is disabled");
                return false;
            }

            var template = availableTemplates.FirstOrDefault(t => t.templateId == templateId);
            if (template == null)
            {
                Debug.LogWarning($"🗺️ Template not found: {templateId}");
                return false;
            }

            if (!IsTemplateDiscovered(templateId))
            {
                Debug.LogWarning($"🗺️ Template not yet discovered: {templateId}");
                return false;
            }

            currentTemplate = template;
            isTemplateActive = true;
            currentCompletionPercentage = 0f;

            // Setup template grid
            SetupTemplateGrid(template);

            // Apply template to GridGameManager
            ApplyTemplateToGrid(template);

            OnTemplateActivated?.Invoke(template);

            Debug.Log($"🗺️ Activated template: {template.templateName}");
            return true;
        }

        /// <summary>
        /// Deactivate the current template
        /// </summary>
        public void DeactivateTemplate()
        {
            if (!isTemplateActive) return;

            currentTemplate = null;
            isTemplateActive = false;
            templateGrid.Clear();
            currentPlacements.Clear();
            currentCompletionPercentage = 0f;

            Debug.Log("🗺️ Template deactivated");
        }

        /// <summary>
        /// Setup the template grid structure
        /// </summary>
        private void SetupTemplateGrid(AlchemyTemplate template)
        {
            templateGrid.Clear();

            // Initialize all cells as empty
            for (int x = 0; x < template.gridSize.x; x++)
            {
                for (int y = 0; y < template.gridSize.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    templateGrid[pos] = new TemplateCell
                    {
                        position = pos,
                        cellType = TemplateCellType.Empty,
                        isCompleted = false
                    };
                }
            }

            // Apply template-specific cells
            foreach (var cellData in template.templateCells)
            {
                if (templateGrid.ContainsKey(cellData.position))
                {
                    var cell = templateGrid[cellData.position];
                    cell.cellType = cellData.cellType;
                    cell.obstacleType = cellData.obstacleType;
                    cell.requiredAspect = cellData.requiredAspect;
                    cell.requiredIngredient = cellData.requiredIngredient;
                    cell.isStoryLocked = cellData.isStoryLocked;
                    cell.isCompleted = false;
                }
            }

            Debug.Log($"🗺️ Setup template grid: {template.gridSize.x}x{template.gridSize.y} with {template.templateCells.Count} special cells");
        }

        /// <summary>
        /// Apply template obstacles and requirements to the GridGameManager
        /// </summary>
        private void ApplyTemplateToGrid(AlchemyTemplate template)
        {
            var gridManager = GridGameManager.Instance;
            if (gridManager == null) return;

            // Clear existing obstacles
            gridManager.aspectObstacles.Clear();

            // Set grid size
            gridManager.gridWidth = template.gridSize.x;
            gridManager.gridHeight = template.gridSize.y;

            // Add template obstacles
            foreach (var cellData in template.templateCells)
            {
                if (cellData.cellType == TemplateCellType.Obstacle)
                {
                    var obstacle = new AspectObstacle(cellData.obstacleType, cellData.position);
                    gridManager.aspectObstacles.Add(obstacle);
                    
                    // Also set the obstacle on the grid cell so it knows it has an obstacle
                    var gridCell = gridManager.GetCell(cellData.position.x, cellData.position.y);
                    if (gridCell != null)
                    {
                        gridCell.SetObstacle(obstacle);
                    }
                }
            }

            // Refresh grid visualization
            if (gridManager.GetComponent<GridVisualizer>() != null)
            {
                gridManager.GetComponent<GridVisualizer>().RefreshGrid();
            }

            Debug.Log($"🗺️ Applied template to grid: {gridManager.aspectObstacles.Count} obstacles placed");
        }

        /// <summary>
        /// Update template progress with current ingredient placements
        /// </summary>
        public TemplateProgress UpdateTemplateProgress(Dictionary<Vector2Int, Ingredient> placements)
        {
            if (!isTemplateActive)
            {
                return new TemplateProgress { isTemplateActive = false };
            }

            currentPlacements = new Dictionary<Vector2Int, Ingredient>(placements);

            // Check each template cell for completion
            int totalRequiredCells = 0;
            int completedCells = 0;

            foreach (var kvp in templateGrid)
            {
                var cell = kvp.Value;
                if (cell.cellType == TemplateCellType.Empty) continue;

                totalRequiredCells++;
                bool wasCompleted = cell.isCompleted;
                cell.isCompleted = IsCellCompleted(cell, placements);

                if (cell.isCompleted)
                {
                    completedCells++;
                    
                    // Fire completion event for new completions
                    if (!wasCompleted)
                    {
                        Debug.Log($"🗺️ Template cell completed: {cell.cellType} at {cell.position}");
                    }
                }
            }

            // Calculate completion percentage
            currentCompletionPercentage = totalRequiredCells > 0 ? (float)completedCells / totalRequiredCells : 0f;

            var progress = new TemplateProgress
            {
                isTemplateActive = true,
                templateName = currentTemplate.templateName,
                completionPercentage = currentCompletionPercentage,
                completedCells = completedCells,
                totalRequiredCells = totalRequiredCells,
                isComplete = currentCompletionPercentage >= 1.0f,
                currentBonus = CalculateCurrentBonus()
            };

            // Check for template completion
            if (progress.isComplete)
            {
                HandleTemplateCompletion(true);
            }

            OnTemplateProgressUpdated?.Invoke(progress);
            return progress;
        }

        /// <summary>
        /// Check if a specific template cell is completed
        /// </summary>
        private bool IsCellCompleted(TemplateCell cell, Dictionary<Vector2Int, Ingredient> placements)
        {
            switch (cell.cellType)
            {
                case TemplateCellType.Empty:
                    return true;

                case TemplateCellType.Obstacle:
                    // Check if obstacle interaction is completed
                    return IsObstacleCompleted(cell.position);

                case TemplateCellType.RequiredIngredient:
                    // Check if correct ingredient is placed
                    if (!placements.ContainsKey(cell.position)) return false;
                    var ingredient = placements[cell.position];
                    
                    if (cell.requiredAspect.HasValue && ingredient.IngredientAspect != cell.requiredAspect.Value)
                        return false;
                    if (cell.requiredIngredient != null && ingredient != cell.requiredIngredient)
                        return false;
                    
                    return true;

                case TemplateCellType.LockedIngredient:
                    // For story-locked ingredients, check if the specific ingredient is placed
                    if (cell.isStoryLocked)
                    {
                        // Would need to check against story ingredient database
                        return placements.ContainsKey(cell.position);
                    }
                    return placements.ContainsKey(cell.position);

                case TemplateCellType.ForbiddenArea:
                    // Must remain empty
                    return !placements.ContainsKey(cell.position);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Check if an obstacle at a position is completed
        /// </summary>
        private bool IsObstacleCompleted(Vector2Int position)
        {
            var gridManager = GridGameManager.Instance;
            if (gridManager == null) return false;

            return gridManager.IsObstacleCompletedAt(position);
        }

        /// <summary>
        /// Calculate the current bonus multiplier based on completion
        /// </summary>
        private float CalculateCurrentBonus()
        {
            if (!isTemplateActive) return 1.0f;

            // Linear interpolation from 1.0 to template bonus based on completion
            float maxBonus = currentTemplate.bonusMultiplier;
            return Mathf.Lerp(1.0f, maxBonus, currentCompletionPercentage);
        }

        /// <summary>
        /// Handle template completion
        /// </summary>
        private void HandleTemplateCompletion(bool successful)
        {
            if (!isTemplateActive) return;

            string templateId = currentTemplate.templateId;

            // Track completion
            if (!templateCompletionCounts.ContainsKey(templateId))
                templateCompletionCounts[templateId] = 0;
            templateCompletionCounts[templateId]++;

            // Award skill points for successful completion
            if (successful && currentTemplate.bonusSkillPoints > 0)
            {
                var skillTree = GetComponent<AlchemySkillTree>();
                if (skillTree != null)
                {
                    skillTree.AwardSkillPoints(currentTemplate.bonusSkillPoints, $"Template completion: {currentTemplate.templateName}");
                }
            }

            OnTemplateCompleted?.Invoke(currentTemplate, successful);

            Debug.Log($"🗺️ Template {(successful ? "completed" : "failed")}: {currentTemplate.templateName}");
            Debug.Log($"🗺️ Completion count: {templateCompletionCounts[templateId]}");

            // Keep template active for result display, but mark as completed
            // The template will be deactivated when the crafting session ends
        }

        /// <summary>
        /// Discover a new template (from world exploration or quest rewards)
        /// </summary>
        public bool DiscoverTemplate(string templateId)
        {
            if (IsTemplateDiscovered(templateId)) return false;

            var template = availableTemplates.FirstOrDefault(t => t.templateId == templateId);
            if (template == null) return false;

            discoveredTemplates[templateId] = true;
            OnTemplateDiscovered?.Invoke(template);

            Debug.Log($"🗺️ New template discovered: {template.templateName}");
            return true;
        }

        /// <summary>
        /// Check if a template has been discovered
        /// </summary>
        public bool IsTemplateDiscovered(string templateId)
        {
            return discoveredTemplates.ContainsKey(templateId) && discoveredTemplates[templateId];
        }

        /// <summary>
        /// Get all discovered templates
        /// </summary>
        public List<AlchemyTemplate> GetDiscoveredTemplates()
        {
            return availableTemplates.Where(t => IsTemplateDiscovered(t.templateId)).ToList();
        }

        /// <summary>
        /// Apply template rewards to crafting result
        /// </summary>
        public void ApplyTemplateRewardsToResult(ref int potency, ref int quantity, ref float efficiency)
        {
            if (!isTemplateActive) return;

            float bonus = CalculateCurrentBonus();
            potency = Mathf.RoundToInt(potency * bonus);
            efficiency += (bonus - 1.0f) * 0.5f; // Convert bonus to efficiency gain

            Debug.Log($"🗺️ Applied template rewards: {bonus:F1}x potency bonus");
        }

        /// <summary>
        /// Check current template progress
        /// </summary>
        public TemplateProgress CheckTemplateProgress()
        {
            if (!isTemplateActive)
            {
                return new TemplateProgress { isTemplateActive = false };
            }

            return new TemplateProgress
            {
                isTemplateActive = true,
                templateName = currentTemplate.templateName,
                completionPercentage = currentCompletionPercentage,
                isComplete = currentCompletionPercentage >= 1.0f,
                currentBonus = CalculateCurrentBonus()
            };
        }

        #region Public API

        /// <summary>
        /// Test the template system
        /// </summary>
        [ContextMenu("Test Template System")]
        public void TestTemplateSystem()
        {
            Debug.Log("🗺️ === TESTING TEMPLATE SYSTEM ===");
            Debug.Log($"🗺️ Template Mode Enabled: {enableTemplateMode}");
            Debug.Log($"🗺️ Available Templates: {availableTemplates.Count}");
            Debug.Log($"🗺️ Discovered Templates: {discoveredTemplates.Count}");
            Debug.Log($"🗺️ Current Template Active: {isTemplateActive}");
            
            if (isTemplateActive)
            {
                Debug.Log($"🗺️ Current Template: {currentTemplate.templateName}");
                Debug.Log($"🗺️ Completion: {currentCompletionPercentage:P}");
                Debug.Log($"🗺️ Current Bonus: {CalculateCurrentBonus():F1}x");
            }
            
            Debug.Log("🗺️ Available Templates:");
            foreach (var template in availableTemplates)
            {
                string status = IsTemplateDiscovered(template.templateId) ? "🗺️" : "❓";
                int completions = templateCompletionCounts.ContainsKey(template.templateId) ? templateCompletionCounts[template.templateId] : 0;
                Debug.Log($"   {status} {template.templateName} ({template.difficulty}) - Completed {completions} times");
            }
            
            Debug.Log("🗺️ === TEMPLATE SYSTEM TEST COMPLETE ===");
        }

        /// <summary>
        /// Discover all templates for testing
        /// </summary>
        [ContextMenu("Discover All Templates (Testing)")]
        public void DiscoverAllTemplatesForTesting()
        {
            foreach (var template in availableTemplates)
            {
                DiscoverTemplate(template.templateId);
            }
            Debug.Log("🗺️ All templates discovered for testing!");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class AlchemyTemplate
    {
        [Header("Basic Info")]
        public string templateId;
        public string templateName;
        [TextArea(2, 4)]
        public string description;
        public TemplateDifficulty difficulty;

        [Header("Grid Configuration")]
        public Vector2Int gridSize = new Vector2Int(3, 3);
        public List<TemplateCellData> templateCells = new List<TemplateCellData>();

        [Header("Story Integration")]
        public bool requiresStoryIngredient = false;
        public Vector2Int storyIngredientLockPosition;
        public Ingredient requiredStoryIngredient;

        [Header("Rewards")]
        [Range(1f, 5f)]
        public float bonusMultiplier = 1.5f;
        public int bonusSkillPoints = 2;

        [Header("Discovery")]
        public bool foundInWorld = true;
        public string discoveryHint = "";
    }

    [System.Serializable]
    public class TemplateCellData
    {
        public Vector2Int position;
        public TemplateCellType cellType;
        public ObstacleType obstacleType;
        public Aspect? requiredAspect;
        public Ingredient requiredIngredient;
        public bool isStoryLocked = false;
    }

    [System.Serializable]
    public class TemplateCell
    {
        public Vector2Int position;
        public TemplateCellType cellType;
        public ObstacleType obstacleType;
        public Aspect? requiredAspect;
        public Ingredient requiredIngredient;
        public bool isStoryLocked;
        public bool isCompleted;
    }

    [System.Serializable]
    public class TemplateProgress
    {
        public bool isTemplateActive;
        public string templateName;
        public float completionPercentage;
        public int completedCells;
        public int totalRequiredCells;
        public bool isComplete;
        public float currentBonus;
    }

    public enum TemplateDifficulty
    {
        Beginner,
        Intermediate,
        Advanced,
        Master
    }

    public enum TemplateCellType
    {
        Empty,
        Obstacle,
        RequiredIngredient,
        LockedIngredient,
        ForbiddenArea
    }

    #endregion
}