using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Crafting;
using FourFatesStudios.ProjectWarden.Effects;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.RefiningSystem;
using FourFatesStudios.ProjectWarden.Structs;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu
{
    public class ComprehensiveCraftingController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GridMinigameController tetrisAlchemyController;
        [SerializeField] private IngredientRefiningController refiningController;
        
        [Header("Crafting Configuration")]
        [SerializeField] private List<PotionRecipe> availableRecipes = new List<PotionRecipe>();
        [SerializeField] private List<Ingredient> availableIngredients = new List<Ingredient>();
        [SerializeField] private List<BottleType> availableBottles = new List<BottleType>();
        [SerializeField] private bool enableRealTimeBrewing = true;
        [SerializeField] private int maxConcurrentBrewing = 3;
        
        [Header("Player Progress")]
        [SerializeField] private int alchemySkillLevel = 1;
        [SerializeField] private bool hasAdvancedEquipment = false;
        [SerializeField] private int assistantCount = 0;
        [SerializeField] private int brewingStationCount = 1;
        
        [Header("Recipe Discovery")]
        [SerializeField] private bool enableAutomaticDiscovery = true;
        [SerializeField] private List<PotionRecipe> knownRecipes = new List<PotionRecipe>();
        [SerializeField] private Dictionary<PotionRecipe, RecipeMasteryLevel> recipeMastery = new Dictionary<PotionRecipe, RecipeMasteryLevel>();
        [SerializeField] private Dictionary<PotionRecipe, int> recipeAttempts = new Dictionary<PotionRecipe, int>();
        
        // UI Elements
        private VisualElement rootElement;
        private VisualElement recipeSelectionArea;
        private VisualElement ingredientPreparationArea;
        private VisualElement brewingMethodArea;
        private VisualElement ingredientSequenceArea;
        private VisualElement brewingProgressArea;
        private VisualElement bottleSelectionArea;
        private VisualElement accentIngredientArea;
        private VisualElement completedPotionsArea;
        
        // Crafting State
        private PotionRecipe selectedRecipe;
        private BrewMethod selectedBrewMethod = BrewMethod.StandardBrew;
        private BottleType selectedBottle = BottleType.BasicVial;
        private List<Ingredient> selectedIngredients = new List<Ingredient>();
        private List<Ingredient> ingredientSequence = new List<Ingredient>();
        private Ingredient selectedAccentIngredient;
        private List<BrewingProcess> activeBrewingProcesses = new List<BrewingProcess>();
        
        // Events
        public static event Action<PotionRecipe> OnRecipeDiscovered;
        public static event Action<PotionRecipe, RecipeMasteryLevel> OnRecipeMasteryChanged;
        public static event Action<Potion, CraftingResult> OnPotionCompleted;
        public static event Action<BrewingProcess> OnBrewingStarted;
        public static event Action<BrewingProcess> OnBrewingCompleted;
        
        public class BrewingProcess
        {
            public PotionRecipe recipe;
            public BrewMethod brewMethod;
            public BottleType bottleType;
            public List<Ingredient> ingredients;
            public List<Ingredient> sequence;
            public Ingredient accentIngredient;
            public float startTime;
            public float duration;
            public int stationIndex;
            public bool isComplete;
            
            public float Progress => (Time.time - startTime) / duration;
        }
        
        private void Start()
        {
            InitializeUI();
            LoadRecipeData();
            SetupInitialState();
        }
        
        private void Update()
        {
            if (enableRealTimeBrewing)
            {
                UpdateBrewingProcesses();
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
            
            // Find UI areas
            recipeSelectionArea = rootElement.Q<VisualElement>("RecipeSelectionArea");
            ingredientPreparationArea = rootElement.Q<VisualElement>("IngredientPreparationArea");
            brewingMethodArea = rootElement.Q<VisualElement>("BrewingMethodArea");
            ingredientSequenceArea = rootElement.Q<VisualElement>("IngredientSequenceArea");
            brewingProgressArea = rootElement.Q<VisualElement>("BrewingProgressArea");
            bottleSelectionArea = rootElement.Q<VisualElement>("BottleSelectionArea");
            accentIngredientArea = rootElement.Q<VisualElement>("AccentIngredientArea");
            completedPotionsArea = rootElement.Q<VisualElement>("CompletedPotionsArea");
            
            SetupUICallbacks();
            RefreshUI();
        }
        
        private void SetupUICallbacks()
        {
            // Recipe selection callbacks
            var startCraftingButton = rootElement.Q<Button>("StartCraftingButton");
            startCraftingButton?.RegisterCallback<ClickEvent>(evt => StartCraftingProcess());
            
            var useMinigameButton = rootElement.Q<Button>("UseMinigameButton");
            useMinigameButton?.RegisterCallback<ClickEvent>(evt => OpenTetrisMinigame());
            
            var refineIngredientsButton = rootElement.Q<Button>("RefineIngredientsButton");
            refineIngredientsButton?.RegisterCallback<ClickEvent>(evt => OpenRefiningSystem());
            
            var massCraftButton = rootElement.Q<Button>("MassCraftButton");
            massCraftButton?.RegisterCallback<ClickEvent>(evt => StartMassCrafting());
        }
        
        private void LoadRecipeData()
        {
            // Initialize recipe mastery tracking
            foreach (var recipe in availableRecipes)
            {
                if (!recipeMastery.ContainsKey(recipe))
                {
                    recipeMastery[recipe] = recipe.IsKnownFromStart ? RecipeMasteryLevel.Discovered : RecipeMasteryLevel.Unknown;
                }
                
                if (!recipeAttempts.ContainsKey(recipe))
                {
                    recipeAttempts[recipe] = 0;
                }
                
                if (recipe.IsKnownFromStart && !knownRecipes.Contains(recipe))
                {
                    knownRecipes.Add(recipe);
                }
            }
            
            Debug.Log($"🍶 Loaded {availableRecipes.Count} recipes, {knownRecipes.Count} known");
        }
        
        private void SetupInitialState()
        {
            // Connect to other systems
            if (tetrisAlchemyController != null)
            {
                tetrisAlchemyController.OnCraftingCompleted += OnTetrisAlchemyCompleted;
            }
            
            if (refiningController != null)
            {
                IngredientRefiningController.OnRefiningCompleted += OnIngredientRefined;
            }
            
            CheckForRecipeDiscoveries();
        }
        
        private void CheckForRecipeDiscoveries()
        {
            if (!enableAutomaticDiscovery) return;
            
            foreach (var recipe in availableRecipes)
            {
                if (recipeMastery[recipe] == RecipeMasteryLevel.Unknown)
                {
                    if (recipe.CanDiscoverWith(availableIngredients))
                    {
                        DiscoverRecipe(recipe);
                    }
                }
            }
        }
        
        private void DiscoverRecipe(PotionRecipe recipe)
        {
            recipeMastery[recipe] = RecipeMasteryLevel.Discovered;
            knownRecipes.Add(recipe);
            
            Debug.Log($"📜 Discovered recipe: {recipe.RecipeName}!");
            
            if (!string.IsNullOrEmpty(recipe.DiscoveryHint))
            {
                Debug.Log($"💡 Hint: {recipe.DiscoveryHint}");
            }
            
            OnRecipeDiscovered?.Invoke(recipe);
            RefreshRecipeList();
        }
        
        public void SelectRecipe(PotionRecipe recipe)
        {
            if (!knownRecipes.Contains(recipe))
            {
                Debug.LogWarning($"Recipe {recipe.RecipeName} is not known yet!");
                return;
            }
            
            selectedRecipe = recipe;
            selectedBrewMethod = recipe.RecommendedBrewMethod;
            
            // Auto-select compatible bottle
            if (recipe.CompatibleBottles.Count > 0)
            {
                selectedBottle = recipe.CompatibleBottles.First(b => availableBottles.Contains(b));
            }
            
            RefreshUI();
            Debug.Log($"📋 Selected recipe: {recipe.RecipeName}");
        }
        
        public void SetBrewMethod(BrewMethod method)
        {
            selectedBrewMethod = method;
            RefreshBrewingTimeDisplay();
        }
        
        public void SetBottleType(BottleType bottle)
        {
            selectedBottle = bottle;
            RefreshBottleCompatibilityDisplay();
        }
        
        public void AddIngredientToSequence(Ingredient ingredient)
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("No recipe selected!");
                return;
            }
            
            // Check if ingredient is valid for this recipe
            if (!selectedRecipe.HasIngredient(ingredient) && selectedRecipe.AccentIngredient != ingredient)
            {
                Debug.LogWarning($"{ingredient.ItemName} is not valid for {selectedRecipe.RecipeName}");
                return;
            }
            
            ingredientSequence.Add(ingredient);
            RefreshIngredientSequenceDisplay();
            
            Debug.Log($"🧪 Added {ingredient.ItemName} to sequence (position {ingredientSequence.Count})");
        }
        
        public void SetAccentIngredient(Ingredient ingredient)
        {
            selectedAccentIngredient = ingredient;
            RefreshAccentIngredientDisplay();
            Debug.Log($"✨ Set accent ingredient: {ingredient?.ItemName ?? "None"}");
        }
        
        private void StartCraftingProcess()
        {
            if (!ValidateCraftingSetup())
            {
                return;
            }
            
            // Check if recipe requires specific order
            if (selectedRecipe.RequiresSpecificOrder)
            {
                if (!ValidateIngredientOrder())
                {
                    Debug.LogWarning("Ingredient order doesn't match recipe requirements!");
                    return;
                }
            }
            
            // Check if we should use the Tetris minigame
            var masteryLevel = recipeMastery[selectedRecipe];
            bool shouldUseMinigame = masteryLevel < RecipeMasteryLevel.Mastered || 
                                   (masteryLevel == RecipeMasteryLevel.Mastered && UnityEngine.Random.value < 0.3f);
            
            if (shouldUseMinigame)
            {
                OpenTetrisMinigame();
            }
            else
            {
                StartDirectBrewing();
            }
        }
        
        private bool ValidateCraftingSetup()
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("No recipe selected!");
                return false;
            }
            
            if (alchemySkillLevel < selectedRecipe.MinimumAlchemySkill)
            {
                Debug.LogWarning($"Alchemy skill too low! Required: {selectedRecipe.MinimumAlchemySkill}, Current: {alchemySkillLevel}");
                return false;
            }
            
            if (selectedRecipe.RequiresAdvancedEquipment && !hasAdvancedEquipment)
            {
                Debug.LogWarning("This recipe requires advanced equipment!");
                return false;
            }
            
            if (activeBrewingProcesses.Count >= maxConcurrentBrewing)
            {
                Debug.LogWarning("Maximum brewing processes active!");
                return false;
            }
            
            if (!selectedRecipe.ValidBrewMethods.Contains(selectedBrewMethod))
            {
                Debug.LogWarning($"Brew method {selectedBrewMethod} is not valid for this recipe!");
                return false;
            }
            
            if (!selectedRecipe.CompatibleBottles.Contains(selectedBottle))
            {
                Debug.LogWarning($"Bottle type {selectedBottle} is not compatible with this recipe!");
                return false;
            }
            
            return true;
        }
        
        private bool ValidateIngredientOrder()
        {
            if (selectedRecipe.IngredientOrder.Count != ingredientSequence.Count)
                return false;
                
            for (int i = 0; i < selectedRecipe.IngredientOrder.Count; i++)
            {
                int expectedIndex = selectedRecipe.IngredientOrder[i];
                if (expectedIndex >= selectedRecipe.RequiredIngredients.Count)
                    return false;
                    
                var expectedIngredient = selectedRecipe.RequiredIngredients[expectedIndex].ingredient;
                if (ingredientSequence[i] != expectedIngredient)
                    return false;
            }
            
            return true;
        }
        
        private void OpenTetrisMinigame()
        {
            if (tetrisAlchemyController == null)
            {
                Debug.LogWarning("Tetris alchemy controller not found!");
                StartDirectBrewing();
                return;
            }
            
            // Set up the Tetris minigame with current recipe
            // Convert recipe and set it as selected
            var alchemyRecipe = ConvertToAlchemyRecipe(selectedRecipe);
            tetrisAlchemyController.SetRecipeAsKeyRecipe(false); // Not a key recipe by default
            tetrisAlchemyController.SetAvailableIngredients(ingredientSequence);
            tetrisAlchemyController.Show();
            
            Debug.Log($"🎮 Opening Tetris minigame for {selectedRecipe.RecipeName}");
        }
        
        private void StartDirectBrewing()
        {
            float brewingTime = selectedRecipe.CalculateBrewingTime(selectedBrewMethod, alchemySkillLevel);
            
            var brewingProcess = new BrewingProcess
            {
                recipe = selectedRecipe,
                brewMethod = selectedBrewMethod,
                bottleType = selectedBottle,
                ingredients = new List<Ingredient>(ingredientSequence),
                sequence = new List<Ingredient>(ingredientSequence),
                accentIngredient = selectedAccentIngredient,
                startTime = Time.time,
                duration = brewingTime,
                stationIndex = activeBrewingProcesses.Count,
                isComplete = false
            };
            
            activeBrewingProcesses.Add(brewingProcess);
            
            Debug.Log($"⚗️ Started brewing {selectedRecipe.RecipeName} (ETA: {brewingTime:F0}s)");
            OnBrewingStarted?.Invoke(brewingProcess);
            
            RefreshBrewingProgressDisplay();
        }
        
        private void StartMassCrafting()
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("No recipe selected for mass crafting!");
                return;
            }
            
            var masteryLevel = recipeMastery[selectedRecipe];
            if (masteryLevel < RecipeMasteryLevel.Mastered)
            {
                Debug.LogWarning($"Recipe {selectedRecipe.RecipeName} is not mastered yet! Current level: {masteryLevel}");
                return;
            }
            
            if (assistantCount == 0)
            {
                Debug.LogWarning("No assistants available for mass crafting!");
                return;
            }
            
            int batchSize = Mathf.Min(assistantCount, brewingStationCount, 5);
            
            for (int i = 0; i < batchSize; i++)
            {
                if (activeBrewingProcesses.Count < maxConcurrentBrewing)
                {
                    StartDirectBrewing();
                }
            }
            
            Debug.Log($"🏭 Started mass crafting {batchSize} batches of {selectedRecipe.RecipeName}");
        }
        
        private void UpdateBrewingProcesses()
        {
            for (int i = activeBrewingProcesses.Count - 1; i >= 0; i--)
            {
                var process = activeBrewingProcesses[i];
                
                if (process.Progress >= 1f && !process.isComplete)
                {
                    CompleteBrewing(process);
                    activeBrewingProcesses.RemoveAt(i);
                }
            }
            
            RefreshBrewingProgressDisplay();
        }
        
        private void CompleteBrewing(BrewingProcess process)
        {
            process.isComplete = true;
            
            // Calculate success rate
            bool hasOptimalIngredients = ValidateOptimalIngredients(process.ingredients, process.recipe);
            float successRate = process.recipe.CalculateSuccessRate(process.brewMethod, alchemySkillLevel, hasOptimalIngredients);
            
            // Add accent ingredient bonus
            if (process.accentIngredient != null)
            {
                successRate += 0.1f; // 10% bonus for accent ingredients
            }
            
            float roll = UnityEngine.Random.value;
            CraftingResult result = DetermineCraftingResult(roll, successRate);
            
            Potion completedPotion = CreatePotionFromProcess(process, result);
            
            // Update recipe mastery
            UpdateRecipeMastery(process.recipe, result);
            
            Debug.Log($"🍶 Completed brewing: {completedPotion.ItemName} - {result}");
            
            OnBrewingCompleted?.Invoke(process);
            OnPotionCompleted?.Invoke(completedPotion, result);
        }
        
        private bool ValidateOptimalIngredients(List<Ingredient> ingredients, PotionRecipe recipe)
        {
            // Check if all ingredients are the exact ones required (not substitutes)
            var requiredIngredients = recipe.RequiredIngredients.Select(ri => ri.ingredient).ToList();
            return ingredients.All(i => requiredIngredients.Contains(i));
        }
        
        private CraftingResult DetermineCraftingResult(float roll, float successRate)
        {
            if (roll < successRate * 0.1f) // 10% of success rate for critical
                return CraftingResult.CriticalSuccess;
            else if (roll < successRate)
                return CraftingResult.Success;
            else if (roll < successRate + 0.2f) // 20% chance for partial success
                return CraftingResult.PartialSuccess;
            else if (roll < 0.9f) // 90% total chance, remaining is fail
                return CraftingResult.Failed;
            else
                return CraftingResult.CriticalFail;
        }
        
        private Potion CreatePotionFromProcess(BrewingProcess process, CraftingResult result)
        {
            var potion = ScriptableObject.CreateInstance<Potion>();
            
            // Determine quality based on result
            float quality = result switch
            {
                CraftingResult.CriticalSuccess => UnityEngine.Random.Range(1.5f, 2.0f),
                CraftingResult.Success => UnityEngine.Random.Range(1.0f, 1.3f),
                CraftingResult.PartialSuccess => UnityEngine.Random.Range(0.7f, 1.0f),
                CraftingResult.Failed => UnityEngine.Random.Range(0.3f, 0.7f),
                CraftingResult.CriticalFail => UnityEngine.Random.Range(0.1f, 0.3f),
                _ => 1.0f
            };
            
            // Create effects based on recipe and ingredients
            var effectBundle = CreateEffectBundle(process.ingredients, process.accentIngredient, quality);
            
            // Determine rarity based on recipe and quality
            PotionRarity rarity = DeterminePotionRarity(process.recipe, quality, result);
            
            // Initialize the potion
            potion.InitializeFromCrafting(
                rarity,
                process.recipe.ExpectedTone,
                process.bottleType,
                effectBundle,
                quality,
                "Player", // TODO: Get actual player name
                process.ingredients
            );
            
            // Set item name and description
            string potionName = result == CraftingResult.CriticalSuccess ? 
                $"Masterwork {process.recipe.RecipeName}" : process.recipe.RecipeName;
            
            var nameField = typeof(Item).GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField?.SetValue(potion, potionName);
            
            return potion;
        }
        
        private EffectBundle CreateEffectBundle(List<Ingredient> ingredients, Ingredient accentIngredient, float quality)
        {
            var effectBundle = new EffectBundle();
            
            // Combine effects from all ingredients through their infusions
            foreach (var ingredient in ingredients)
            {
                foreach (var infusion in ingredient.InfusionBundle.Infusions)
                {
                    // Add effects from infusion's effect bundle
                    if (infusion.EffectBundle?.Effects != null)
                    {
                        foreach (var effect in infusion.EffectBundle.Effects)
                        {
                            effectBundle.Effects.Add(effect);
                        }
                    }
                }
            }
            
            // Add accent ingredient effects through its infusions
            if (accentIngredient != null)
            {
                foreach (var infusion in accentIngredient.InfusionBundle.Infusions)
                {
                    // Add effects from accent ingredient's infusion effect bundle (with reduced potency)
                    if (infusion.EffectBundle?.Effects != null)
                    {
                        foreach (var effect in infusion.EffectBundle.Effects)
                        {
                            effectBundle.Effects.Add(effect);
                        }
                    }
                }
            }
            
            return effectBundle;
        }
        
        private PotionRarity DeterminePotionRarity(PotionRecipe recipe, float quality, CraftingResult result)
        {
            PotionRarity baseRarity = recipe.Rarity;
            
            // Upgrade rarity based on quality and result
            if (result == CraftingResult.CriticalSuccess && quality > 1.7f)
            {
                return (PotionRarity)Mathf.Min((int)baseRarity + 2, (int)PotionRarity.Artifact);
            }
            else if (result == CraftingResult.CriticalSuccess || quality > 1.4f)
            {
                return (PotionRarity)Mathf.Min((int)baseRarity + 1, (int)PotionRarity.Artifact);
            }
            else if (result == CraftingResult.PartialSuccess || result == CraftingResult.Failed)
            {
                return (PotionRarity)Mathf.Max((int)baseRarity - 1, (int)PotionRarity.Common);
            }
            
            return baseRarity;
        }
        
        private void UpdateRecipeMastery(PotionRecipe recipe, CraftingResult result)
        {
            recipeAttempts[recipe]++;
            
            var currentMastery = recipeMastery[recipe];
            var newMastery = currentMastery;
            
            // Only successful attempts count toward mastery
            if (result == CraftingResult.Success || result == CraftingResult.CriticalSuccess)
            {
                int successfulAttempts = recipeAttempts[recipe];
                int masteryThreshold = recipe.MasteryThreshold;
                
                newMastery = successfulAttempts switch
                {
                    >= 10 => RecipeMasteryLevel.Perfected,
                    var attempts when attempts >= masteryThreshold => RecipeMasteryLevel.Mastered,
                    >= 3 => RecipeMasteryLevel.Competent,
                    >= 1 => RecipeMasteryLevel.Familiar,
                    _ => RecipeMasteryLevel.Discovered
                };
            }
            
            if (newMastery != currentMastery)
            {
                recipeMastery[recipe] = newMastery;
                OnRecipeMasteryChanged?.Invoke(recipe, newMastery);
                Debug.Log($"🎓 Recipe mastery updated: {recipe.RecipeName} is now {newMastery}");
            }
        }
        
        private void OnTetrisAlchemyCompleted(AlchemyRecipe alchemyRecipe, Dictionary<Vector2Int, PlacedIngredient> placement, bool success)
        {
            if (selectedRecipe == null) return;
            
            if (success)
            {
                Debug.Log("✅ Tetris minigame completed successfully! Starting brewing...");
                StartDirectBrewing();
            }
            else
            {
                Debug.Log("❌ Tetris minigame failed. Creating synthetic ingredient instead.");
                CreateSyntheticIngredient();
            }
        }
        
        private void OnIngredientRefined(Ingredient originalIngredient, RefinedIngredient refinedResult, RefiningResult result)
        {
            if (result == RefiningResult.Success || result == RefiningResult.CriticalSuccess)
            {
                // Add refined ingredient to available ingredients
                availableIngredients.Add(refinedResult);
                Debug.Log($"✅ Refined ingredient added to inventory: {refinedResult.ItemName}");
                
                // Check for new recipe discoveries
                CheckForRecipeDiscoveries();
            }
        }
        
        private void CreateSyntheticIngredient()
        {
            var synthetic = SyntheticIngredient.CreateFromFailedCrafting(selectedIngredients, "Failed brewing attempt");
            
            availableIngredients.Add(synthetic);
            Debug.Log($"🧪 Created synthetic ingredient: {synthetic.ItemName}");
        }
        
        private AlchemyRecipe ConvertToAlchemyRecipe(PotionRecipe potionRecipe)
        {
            // Convert PotionRecipe to AlchemyRecipe for Tetris system
            // This is a simplified conversion - you may need to enhance based on your AlchemyRecipe structure
            var alchemyRecipe = ScriptableObject.CreateInstance<AlchemyRecipe>();
            
            // Set basic properties using reflection if needed
            var nameField = typeof(Item).GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField?.SetValue(alchemyRecipe, potionRecipe.RecipeName);
            
            return alchemyRecipe;
        }
        
        #region UI Update Methods
        
        private void RefreshUI()
        {
            RefreshRecipeList();
            RefreshIngredientList();
            RefreshBrewingMethodDisplay();
            RefreshBottleSelectionDisplay();
            RefreshIngredientSequenceDisplay();
            RefreshAccentIngredientDisplay();
            RefreshBrewingProgressDisplay();
        }
        
        private void RefreshRecipeList()
        {
            if (recipeSelectionArea == null) return;
            
            recipeSelectionArea.Clear();
            
            foreach (var recipe in knownRecipes)
            {
                var recipeElement = CreateRecipeElement(recipe);
                recipeSelectionArea.Add(recipeElement);
            }
        }
        
        private VisualElement CreateRecipeElement(PotionRecipe recipe)
        {
            var container = new VisualElement();
            container.AddToClassList("recipe-item");
            
            if (recipe == selectedRecipe)
                container.AddToClassList("selected");
            
            var nameLabel = new Label(recipe.RecipeName);
            nameLabel.AddToClassList("recipe-name");
            
            var masteryLabel = new Label(recipeMastery[recipe].ToString());
            masteryLabel.AddToClassList("recipe-mastery");
            
            var attemptsLabel = new Label($"Attempts: {recipeAttempts[recipe]}");
            attemptsLabel.AddToClassList("recipe-attempts");
            
            container.Add(nameLabel);
            container.Add(masteryLabel);
            container.Add(attemptsLabel);
            
            container.RegisterCallback<ClickEvent>(evt => SelectRecipe(recipe));
            
            return container;
        }
        
        private void RefreshIngredientList()
        {
            if (ingredientPreparationArea == null) return;
            
            // This would populate the ingredient preparation area
            // Including options to refine ingredients before use
        }
        
        private void RefreshBrewingMethodDisplay()
        {
            if (brewingMethodArea == null) return;
            
            // Update brewing method selection UI
            RefreshBrewingTimeDisplay();
        }
        
        private void RefreshBrewingTimeDisplay()
        {
            if (selectedRecipe == null) return;
            
            float brewingTime = selectedRecipe.CalculateBrewingTime(selectedBrewMethod, alchemySkillLevel);
            var timeLabel = brewingMethodArea?.Q<Label>("BrewingTimeLabel");
            if (timeLabel != null)
            {
                timeLabel.text = $"Brewing Time: {brewingTime:F0}s";
            }
        }
        
        private void RefreshBottleSelectionDisplay()
        {
            if (bottleSelectionArea == null) return;
            
            // Update bottle selection UI
            RefreshBottleCompatibilityDisplay();
        }
        
        private void RefreshBottleCompatibilityDisplay()
        {
            // Update UI to show bottle compatibility with selected recipe
        }
        
        private void RefreshIngredientSequenceDisplay()
        {
            if (ingredientSequenceArea == null) return;
            
            ingredientSequenceArea.Clear();
            
            for (int i = 0; i < ingredientSequence.Count; i++)
            {
                var ingredient = ingredientSequence[i];
                var element = new Label($"{i + 1}. {ingredient.ItemName}");
                element.AddToClassList("sequence-item");
                ingredientSequenceArea.Add(element);
            }
        }
        
        private void RefreshAccentIngredientDisplay()
        {
            if (accentIngredientArea == null) return;
            
            var label = accentIngredientArea.Q<Label>("AccentIngredientLabel");
            if (label != null)
            {
                label.text = selectedAccentIngredient?.ItemName ?? "None";
            }
        }
        
        private void RefreshBrewingProgressDisplay()
        {
            if (brewingProgressArea == null) return;
            
            brewingProgressArea.Clear();
            
            foreach (var process in activeBrewingProcesses)
            {
                var element = CreateBrewingProgressElement(process);
                brewingProgressArea.Add(element);
            }
        }
        
        private VisualElement CreateBrewingProgressElement(BrewingProcess process)
        {
            var container = new VisualElement();
            container.AddToClassList("brewing-process");
            
            var nameLabel = new Label(process.recipe.RecipeName);
            nameLabel.AddToClassList("process-name");
            
            var progressBar = new ProgressBar();
            progressBar.value = process.Progress * 100f;
            progressBar.AddToClassList("process-progress");
            
            var timeLabel = new Label($"{(process.duration - (Time.time - process.startTime)):F0}s remaining");
            timeLabel.AddToClassList("process-time");
            
            container.Add(nameLabel);
            container.Add(progressBar);
            container.Add(timeLabel);
            
            return container;
        }
        
        #endregion
        
        #region UI Actions
        
        private void OpenRefiningSystem()
        {
            Debug.Log("Opening refining system...");
            // TODO: Implement refining system UI
        }
        
        #endregion
        
        #region Public API
        
        public void SetAvailableIngredients(List<Ingredient> ingredients)
        {
            availableIngredients = ingredients ?? new List<Ingredient>();
            CheckForRecipeDiscoveries();
            RefreshIngredientList();
        }
        
        public void SetAvailableRecipes(List<PotionRecipe> recipes)
        {
            availableRecipes = recipes ?? new List<PotionRecipe>();
            LoadRecipeData();
            RefreshRecipeList();
        }
        
        public void SetPlayerSkillLevel(int skillLevel)
        {
            alchemySkillLevel = skillLevel;
            RefreshUI();
        }
        
        public void SetEquipmentStatus(bool hasAdvanced)
        {
            hasAdvancedEquipment = hasAdvanced;
        }
        
        public void SetAssistantCount(int count)
        {
            assistantCount = count;
        }
        
        public void SetBrewingStationCount(int count)
        {
            brewingStationCount = count;
            maxConcurrentBrewing = count * 3; // 3 processes per station
        }
        
        public RecipeMasteryLevel GetRecipeMastery(PotionRecipe recipe)
        {
            return recipeMastery.TryGetValue(recipe, out var mastery) ? mastery : RecipeMasteryLevel.Unknown;
        }
        
        public int GetRecipeAttempts(PotionRecipe recipe)
        {
            return recipeAttempts.TryGetValue(recipe, out var attempts) ? attempts : 0;
        }
        
        public List<PotionRecipe> GetKnownRecipes()
        {
            return new List<PotionRecipe>(knownRecipes);
        }
        
        public List<BrewingProcess> GetActiveBrewingProcesses()
        {
            return new List<BrewingProcess>(activeBrewingProcesses);
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (tetrisAlchemyController != null)
            {
                tetrisAlchemyController.OnCraftingCompleted -= OnTetrisAlchemyCompleted;
            }
            
            if (refiningController != null)
            {
                IngredientRefiningController.OnRefiningCompleted -= OnIngredientRefined;
            }
        }
    }
}