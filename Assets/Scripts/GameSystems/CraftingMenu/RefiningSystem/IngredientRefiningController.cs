using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Structs;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.RefiningSystem
{
    public class IngredientRefiningController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private StyleSheet refiningStyles;
        
        [Header("Refining Configuration")]
        [SerializeField] private List<Ingredient> availableIngredients = new List<Ingredient>();
        [SerializeField] private bool enableRealTimeProcessing = true;
        [SerializeField] private bool allowBatchRefining = true;
        [SerializeField] private int maxBatchSize = 5;
        
        [Header("Skill Integration")]
        [SerializeField] private int baseGrindingSkill = 1;
        [SerializeField] private int baseDistillingSkill = 1;
        [SerializeField] private int baseRoastingSkill = 1;
        [SerializeField] private float equipmentQualityBonus = 0f;
        
        // UI Elements
        private VisualElement rootElement;
        private VisualElement ingredientList;
        private VisualElement refiningArea;
        private VisualElement processQueue;
        private Label statusLabel;
        private Button startRefiningButton;
        private Button clearQueueButton;
        
        // Refining State
        private List<RefiningProcess> activeProcesses = new List<RefiningProcess>();
        private Dictionary<Ingredient, RefiningData> refiningDatabase = new Dictionary<Ingredient, RefiningData>();
        private Queue<RefiningProcess> refiningQueue = new Queue<RefiningProcess>();
        
        // Events
        public static event Action<Ingredient, RefinedIngredient, RefiningResult> OnRefiningCompleted;
        public static event Action<Ingredient, RefiningType> OnRefiningStarted;
        public static event Action<List<RefiningProcess>> OnQueueUpdated;
        
        public class RefiningProcess
        {
            public Ingredient sourceIngredient;
            public RefiningType refiningType;
            public RefiningData refiningData;
            public float startTime;
            public float duration;
            public int playerSkillLevel;
            public bool isBatchProcess;
            public int batchCount;
            
            public float Progress => (Time.time - startTime) / duration;
            public bool IsComplete => Progress >= 1f;
        }
        
        private void Start()
        {
            InitializeUI();
            LoadRefiningDatabase();
            SetupAvailableIngredients();
        }
        
        private void Update()
        {
            if (enableRealTimeProcessing)
            {
                UpdateActiveProcesses();
            }
        }
        
        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument is not assigned!");
                return;
            }
            
            rootElement = uiDocument.rootVisualElement;
            
            // Find UI elements
            ingredientList = rootElement.Q<VisualElement>("IngredientList");
            refiningArea = rootElement.Q<VisualElement>("RefiningArea");
            processQueue = rootElement.Q<VisualElement>("ProcessQueue");
            statusLabel = rootElement.Q<Label>("StatusLabel");
            startRefiningButton = rootElement.Q<Button>("StartRefiningButton");
            clearQueueButton = rootElement.Q<Button>("ClearQueueButton");
            
            // Setup button events
            startRefiningButton?.RegisterCallback<ClickEvent>(evt => StartQueuedProcesses());
            clearQueueButton?.RegisterCallback<ClickEvent>(evt => ClearRefiningQueue());
            
            UpdateStatusDisplay();
        }
        
        private void LoadRefiningDatabase()
        {
            // Initialize refining data for different ingredient types
            refiningDatabase.Clear();
            
            foreach (var ingredient in availableIngredients)
            {
                if (ingredient == null) continue;
                
                // Create refining data based on ingredient archetype
                var validRefiningTypes = GetValidRefiningTypes(ingredient.IngredientArchetype);
                
                foreach (var refiningType in validRefiningTypes)
                {
                    var data = CreateRefiningDataForIngredient(ingredient, refiningType);
                    refiningDatabase[ingredient] = data;
                }
            }
            
            Debug.Log($"📚 Loaded {refiningDatabase.Count} refining recipes");
        }
        
        private List<RefiningType> GetValidRefiningTypes(IngredientArchetype archetype)
        {
            return archetype switch
            {
                IngredientArchetype.Ore => new List<RefiningType> { RefiningType.Grinding },
                IngredientArchetype.Herb => new List<RefiningType> { RefiningType.Distilling, RefiningType.Roasting },
                IngredientArchetype.Organic => new List<RefiningType> { RefiningType.Distilling, RefiningType.Roasting },
                IngredientArchetype.Solvent => new List<RefiningType> { RefiningType.Distilling },
                _ => new List<RefiningType>()
            };
        }
        
        private RefiningData CreateRefiningDataForIngredient(Ingredient ingredient, RefiningType refiningType)
        {
            var data = RefiningData.CreateDefault(refiningType);
            
            // Customize based on ingredient properties
            data.difficulty = ingredient.Potency switch
            {
                1 => RefiningDifficulty.Easy,
                2 => RefiningDifficulty.Moderate, 
                3 => RefiningDifficulty.Hard,
                4 => RefiningDifficulty.Expert,
                5 => RefiningDifficulty.Expert,
                _ => RefiningDifficulty.Moderate
            };
            
            // Adjust success rates based on difficulty
            data.baseSuccessRate = data.difficulty switch
            {
                RefiningDifficulty.Trivial => 0.95f,
                RefiningDifficulty.Easy => 0.8f,
                RefiningDifficulty.Moderate => 0.65f,
                RefiningDifficulty.Hard => 0.5f,
                RefiningDifficulty.Expert => 0.35f,
                RefiningDifficulty.Impossible => 0.1f,
                _ => 0.7f
            };
            
            data.criticalSuccessRate = Mathf.Max(0.05f, data.baseSuccessRate * 0.2f);
            data.lossRate = 1f - data.baseSuccessRate;
            
            // Set processing time based on complexity
            data.processingTime = refiningType switch
            {
                RefiningType.Grinding => 15f + (ingredient.Potency * 5f),
                RefiningType.Distilling => 30f + (ingredient.Potency * 10f),
                RefiningType.Roasting => 20f + (ingredient.Potency * 7f),
                _ => 30f
            };
            
            data.minimumSkillLevel = ingredient.Potency * 10;
            
            return data;
        }
        
        private void SetupAvailableIngredients()
        {
            if (ingredientList == null) return;
            
            ingredientList.Clear();
            
            foreach (var ingredient in availableIngredients)
            {
                if (ingredient == null) continue;
                
                var ingredientElement = CreateIngredientListItem(ingredient);
                ingredientList.Add(ingredientElement);
            }
        }
        
        private VisualElement CreateIngredientListItem(Ingredient ingredient)
        {
            var container = new VisualElement();
            container.AddToClassList("ingredient-item");
            
            var nameLabel = new Label(ingredient.ItemName);
            nameLabel.AddToClassList("ingredient-name");
            
            var archetypeLabel = new Label($"({ingredient.IngredientArchetype})");
            archetypeLabel.AddToClassList("ingredient-archetype");
            
            var potencyLabel = new Label($"Potency: {ingredient.Potency}");
            potencyLabel.AddToClassList("ingredient-potency");
            
            // Add refining options
            var refiningOptions = new VisualElement();
            refiningOptions.AddToClassList("refining-options");
            
            var validTypes = GetValidRefiningTypes(ingredient.IngredientArchetype);
            foreach (var refiningType in validTypes)
            {
                var button = new Button(() => QueueRefining(ingredient, refiningType));
                button.text = GetRefiningTypeIcon(refiningType) + " " + refiningType.ToString();
                button.AddToClassList("refining-button");
                button.AddToClassList($"refining-{refiningType.ToString().ToLower()}");
                
                // Check if player has required skill level
                int requiredSkill = GetRequiredSkillLevel(ingredient, refiningType);
                int playerSkill = GetPlayerSkillLevel(refiningType);
                
                if (playerSkill < requiredSkill)
                {
                    button.SetEnabled(false);
                    button.tooltip = $"Requires {refiningType} skill level {requiredSkill} (current: {playerSkill})";
                }
                else
                {
                    var successRate = CalculateSuccessRate(ingredient, refiningType);
                    button.tooltip = $"Success rate: {successRate:P0}";
                }
                
                refiningOptions.Add(button);
            }
            
            container.Add(nameLabel);
            container.Add(archetypeLabel);
            container.Add(potencyLabel);
            container.Add(refiningOptions);
            
            return container;
        }
        
        private string GetRefiningTypeIcon(RefiningType type)
        {
            return type switch
            {
                RefiningType.Grinding => "⚒️",
                RefiningType.Distilling => "🧪",
                RefiningType.Roasting => "🔥",
                _ => "🔧"
            };
        }
        
        public void QueueRefining(Ingredient ingredient, RefiningType refiningType)
        {
            if (!CanRefineIngredient(ingredient, refiningType))
            {
                Debug.LogWarning($"Cannot refine {ingredient.ItemName} with {refiningType}");
                return;
            }
            
            var refiningData = GetRefiningData(ingredient, refiningType);
            var playerSkill = GetPlayerSkillLevel(refiningType);
            
            var process = new RefiningProcess
            {
                sourceIngredient = ingredient,
                refiningType = refiningType,
                refiningData = refiningData,
                playerSkillLevel = playerSkill,
                isBatchProcess = false,
                batchCount = 1
            };
            
            refiningQueue.Enqueue(process);
            UpdateQueueDisplay();
            
            Debug.Log($"🔄 Queued {refiningType} of {ingredient.ItemName}");
            OnRefiningStarted?.Invoke(ingredient, refiningType);
        }
        
        public void QueueBatchRefining(Ingredient ingredient, RefiningType refiningType, int count)
        {
            if (!allowBatchRefining || count > maxBatchSize)
            {
                Debug.LogWarning($"Batch refining not allowed or count too high: {count}");
                return;
            }
            
            for (int i = 0; i < count; i++)
            {
                QueueRefining(ingredient, refiningType);
            }
        }
        
        private void StartQueuedProcesses()
        {
            while (refiningQueue.Count > 0 && activeProcesses.Count < 3) // Max 3 concurrent processes
            {
                var process = refiningQueue.Dequeue();
                process.startTime = Time.time;
                process.duration = process.refiningData.processingTime;
                
                activeProcesses.Add(process);
                
                Debug.Log($"▶️ Started refining {process.sourceIngredient.ItemName} (ETA: {process.duration:F0}s)");
            }
            
            UpdateQueueDisplay();
            UpdateStatusDisplay();
        }
        
        private void UpdateActiveProcesses()
        {
            for (int i = activeProcesses.Count - 1; i >= 0; i--)
            {
                var process = activeProcesses[i];
                
                if (process.IsComplete)
                {
                    CompleteRefining(process);
                    activeProcesses.RemoveAt(i);
                }
            }
            
            UpdateStatusDisplay();
        }
        
        private void CompleteRefining(RefiningProcess process)
        {
            var result = DetermineRefiningResult(process);
            
            switch (result)
            {
                case RefiningResult.Success:
                    CreateRefinedIngredient(process, false);
                    Debug.Log($"✅ Successfully refined {process.sourceIngredient.ItemName}");
                    break;
                    
                case RefiningResult.CriticalSuccess:
                    CreateRefinedIngredient(process, true);
                    Debug.Log($"🌟 CRITICAL SUCCESS! Perfectly refined {process.sourceIngredient.ItemName}");
                    break;
                    
                case RefiningResult.Failed:
                    Debug.Log($"❌ Failed to refine {process.sourceIngredient.ItemName} (ingredient retained)");
                    break;
                    
                case RefiningResult.Lost:
                    Debug.Log($"💥 Lost {process.sourceIngredient.ItemName} during refining process");
                    break;
            }
            
            OnRefiningCompleted?.Invoke(process.sourceIngredient, null, result);
        }
        
        private RefiningResult DetermineRefiningResult(RefiningProcess process)
        {
            var data = process.refiningData;
            var successRate = data.CalculateSuccessRate(process.playerSkillLevel, equipmentQualityBonus);
            
            float roll = UnityEngine.Random.Range(0f, 1f);
            
            if (roll < data.criticalSuccessRate)
                return RefiningResult.CriticalSuccess;
            else if (roll < successRate)
                return RefiningResult.Success;
            else if (roll < successRate + data.lossRate)
                return RefiningResult.Lost;
            else
                return RefiningResult.Failed;
        }
        
        private void CreateRefinedIngredient(RefiningProcess process, bool isCritical)
        {
            var refinedIngredient = ScriptableObject.CreateInstance<RefinedIngredient>();
            
            float purity = isCritical ? UnityEngine.Random.Range(1.5f, 2.0f) : UnityEngine.Random.Range(0.8f, 1.2f);
            refinedIngredient.Initialize(process.sourceIngredient, process.refiningType, isCritical, purity);
            
            // Here you would add the refined ingredient to inventory
            Debug.Log($"📦 Created: {refinedIngredient.ItemName} (Potency: {refinedIngredient.Potency})");
        }
        
        private bool CanRefineIngredient(Ingredient ingredient, RefiningType refiningType)
        {
            var validTypes = GetValidRefiningTypes(ingredient.IngredientArchetype);
            if (!validTypes.Contains(refiningType))
                return false;
                
            int requiredSkill = GetRequiredSkillLevel(ingredient, refiningType);
            int playerSkill = GetPlayerSkillLevel(refiningType);
            
            return playerSkill >= requiredSkill;
        }
        
        private RefiningData GetRefiningData(Ingredient ingredient, RefiningType refiningType)
        {
            if (refiningDatabase.TryGetValue(ingredient, out var data))
                return data;
                
            return CreateRefiningDataForIngredient(ingredient, refiningType);
        }
        
        private int GetRequiredSkillLevel(Ingredient ingredient, RefiningType refiningType)
        {
            var data = GetRefiningData(ingredient, refiningType);
            return data.minimumSkillLevel;
        }
        
        private int GetPlayerSkillLevel(RefiningType refiningType)
        {
            return refiningType switch
            {
                RefiningType.Grinding => baseGrindingSkill,
                RefiningType.Distilling => baseDistillingSkill,
                RefiningType.Roasting => baseRoastingSkill,
                _ => 1
            };
        }
        
        private float CalculateSuccessRate(Ingredient ingredient, RefiningType refiningType)
        {
            var data = GetRefiningData(ingredient, refiningType);
            var playerSkill = GetPlayerSkillLevel(refiningType);
            return data.CalculateSuccessRate(playerSkill, equipmentQualityBonus);
        }
        
        private void UpdateQueueDisplay()
        {
            if (processQueue == null) return;
            
            processQueue.Clear();
            
            foreach (var process in refiningQueue)
            {
                var queueItem = new VisualElement();
                queueItem.AddToClassList("queue-item");
                
                var label = new Label($"{GetRefiningTypeIcon(process.refiningType)} {process.sourceIngredient.ItemName}");
                queueItem.Add(label);
                
                processQueue.Add(queueItem);
            }
            
            OnQueueUpdated?.Invoke(activeProcesses);
        }
        
        private void UpdateStatusDisplay()
        {
            if (statusLabel == null) return;
            
            string status = $"Active: {activeProcesses.Count} | Queued: {refiningQueue.Count}";
            
            if (activeProcesses.Count > 0)
            {
                var nextCompletion = activeProcesses.Min(p => p.startTime + p.duration - Time.time);
                status += $" | Next completion: {nextCompletion:F0}s";
            }
            
            statusLabel.text = status;
        }
        
        private void ClearRefiningQueue()
        {
            refiningQueue.Clear();
            UpdateQueueDisplay();
            Debug.Log("🗑️ Cleared refining queue");
        }
        
        public void SetAvailableIngredients(List<Ingredient> ingredients)
        {
            availableIngredients = ingredients ?? new List<Ingredient>();
            LoadRefiningDatabase();
            SetupAvailableIngredients();
        }
        
        public void SetPlayerSkillLevels(int grinding, int distilling, int roasting)
        {
            baseGrindingSkill = grinding;
            baseDistillingSkill = distilling;
            baseRoastingSkill = roasting;
            
            SetupAvailableIngredients(); // Refresh UI with new skill levels
        }
        
        public void SetEquipmentBonus(float bonus)
        {
            equipmentQualityBonus = bonus;
        }
        
        public List<RefiningProcess> GetActiveProcesses()
        {
            return new List<RefiningProcess>(activeProcesses);
        }
        
        public int GetQueueLength()
        {
            return refiningQueue.Count;
        }
        
        #region Public API for External Systems
        
        public static bool CanRefine(Ingredient ingredient, RefiningType refiningType)
        {
            var data = RefiningData.CreateDefault(refiningType);
            return data.IsValidForArchetype(ingredient.IngredientArchetype);
        }
        
        public static List<RefiningType> GetAvailableRefiningTypes(Ingredient ingredient)
        {
            var controller = FindFirstObjectByType<IngredientRefiningController>();
            return controller?.GetValidRefiningTypes(ingredient.IngredientArchetype) ?? new List<RefiningType>();
        }
        
        #endregion
    }
}