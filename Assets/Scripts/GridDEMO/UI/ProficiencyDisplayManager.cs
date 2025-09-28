using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Manages the proficiency display at the top of the UI and tracks completed potions
    /// </summary>
    public class ProficiencyDisplayManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI proficiencyNumberText;
        [SerializeField] private Image proficiencyProgressBar;
        [SerializeField] private ScrollRect potionsScrollView;
        [SerializeField] private Transform potionsContentParent;
        [SerializeField] private GameObject potionEntryPrefab;
        
        [Header("UI Settings")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private Color proficiencyTextColor = Color.white;
        [SerializeField] private int proficiencyFontSize = 12;
        
        [Header("Proficiency Formula Settings")]
        [SerializeField] private bool useAdvancedFormula = true;
        [SerializeField] private ProficiencyWeights proficiencyWeights = new ProficiencyWeights();
        [SerializeField] private float gridSizeWeight = 0.3f;  // Legacy - kept for compatibility
        [SerializeField] private float sRankWeight = 0.4f;     // Legacy - kept for compatibility  
        [SerializeField] private float diversityWeight = 0.2f; // Legacy - kept for compatibility
        [SerializeField] private float complexityWeight = 0.1f; // Legacy - kept for compatibility
        [SerializeField] private int maxDisplayValue = 100;    // Max percentage to display
        
        [Header("Potion List Settings")]
        [SerializeField] private Color potionListBackgroundColor = new Color(0.2f, 0.3f, 0.4f, 1f); // Much more visible blue-gray
        [SerializeField] private float potionEntryHeight = 50f;  // Increased from 30f
        [SerializeField] private Color craftedPotionColor = new Color(0.3f, 0.7f, 0.3f, 1f); // Brighter green
        [SerializeField] private Color defaultPotionColor = new Color(0.4f, 0.4f, 0.6f, 1f); // Brighter blue-gray
        
        // Internal state
        private AlchemySkillSystem skillSystem;
        private Dictionary<Potion,int> completedPotions = new Dictionary<Potion, int>();
        private bool isInitialized = false;
        private ProficiencyGrade currentGrade; // Store current proficiency grade
        
        public static ProficiencyDisplayManager Instance { get; private set; }
        
        /// <summary>
        /// Check if the ProficiencyDisplayManager has been initialized
        /// </summary>
        public bool IsInitialized => isInitialized;
        
        [System.Serializable]
        public class CompletedPotionData
        {
            public string potionName;
            public string recipeKey;
            public int timesCrafted;
            public int sRankCount;
            public bool isAutoCraftUnlocked;
            
            public CompletedPotionData(string name, string key)
            {
                potionName = name;
                recipeKey = key;
                timesCrafted = 0;
                sRankCount = 0;
                isAutoCraftUnlocked = false;
            }
        }
        
        /// <summary>
        /// Ensure TextMeshPro resources are available, and import essentials if needed
        /// </summary>
        private bool EnsureTextMeshProResources()
        {
            // Check if we have any TextMeshPro fonts available
            var tmpFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (tmpFonts != null && tmpFonts.Length > 0)
            {
                Debug.Log($"🧪 Found {tmpFonts.Length} TextMeshPro font assets");
                return true;
            }
            
            Debug.LogWarning("🧪 No TextMeshPro fonts found! You may need to import TextMeshPro Essentials via Window > TextMeshPro > Import TMP Essential Resources");
            return false;
        }
        
        private void Awake()
        {
            Debug.Log($"🧪 ProficiencyDisplayManager: Awake() called on GameObject '{gameObject.name}'!");
            
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("🧪 ProficiencyDisplayManager: Set as singleton instance");
                // Note: Not using DontDestroyOnLoad to avoid scene management conflicts
            }
            else
            {
                Debug.LogWarning($"🧪 ProficiencyDisplayManager: Duplicate instance found on '{gameObject.name}', destroying...");
                Debug.LogWarning($"🧪 ProficiencyDisplayManager: Existing instance GameObject: {Instance.gameObject.name}");
                Debug.LogWarning($"🧪 ProficiencyDisplayManager: This instance GameObject: {this.gameObject.name}");
                
                // Immediately destroy this component to prevent any events from firing
                DestroyImmediate(this);
                return;
            }
        }
        
        private void Start()
        {
            Debug.Log("🧪 ProficiencyDisplayManager: Start() called!");
            
            // Check TextMeshPro resources availability
            EnsureTextMeshProResources();
            
            if (autoSetupOnStart)
            {
                Debug.Log("🧪 ProficiencyDisplayManager: Starting initialization...");
                InitializeProficiencyDisplay();
            }
        }
        
        private void Update()
        {
            // Refresh display periodically (every 2 seconds)
            if (Time.time % 2f < Time.deltaTime && isInitialized)
            {
                RefreshProficiencyDisplay();
            }
        }
        
        private void InitializeProficiencyDisplay()
        {
            Debug.Log("🧪 ProficiencyDisplayManager: InitializeProficiencyDisplay() starting...");
            
            skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.LogWarning("ProficiencyDisplayManager: AlchemySkillSystem not found! Continuing with basic proficiency calculation...");
                // Continue without AlchemySkillSystem - use basic grid-based proficiency
            }
            else
            {
                Debug.Log("🧪 ProficiencyDisplayManager: Found AlchemySkillSystem");
            }
            
            Debug.Log("🧪 ProficiencyDisplayManager: Setting up proficiency display...");
            SetupProficiencyDisplay();
            Debug.Log("🧪 ProficiencyDisplayManager: Setting up potion scroll view...");
            SetupPotionScrollView();
            Debug.Log("🧪 ProficiencyDisplayManager: Refreshing displays...");
            RefreshProficiencyDisplay();
            
            isInitialized = true;
            Debug.Log("ProficiencyDisplayManager: Initialized successfully" + (skillSystem == null ? " (basic mode)" : " (full mode)"));
        }
        
        private void SetupProficiencyDisplay()
        {
            // Note: This sets up a separate proficiency display for potion tracking
            // The main colored proficiency display is handled elsewhere
            Canvas canvas = FindMainCanvas();
            if (canvas != null && proficiencyNumberText == null)
            {
                // Create proficiency display container
                GameObject proficiencyContainer = new GameObject("Proficiency Display Container");
                proficiencyContainer.transform.SetParent(canvas.transform, false);
                
                RectTransform containerRect = proficiencyContainer.AddComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0.5f, 1f);
                containerRect.anchorMax = new Vector2(0.5f, 1f);
                containerRect.pivot = new Vector2(0.5f, 1f);
                containerRect.sizeDelta = new Vector2(200f, 50f);
                containerRect.anchoredPosition = new Vector2(0f, -10f);
                
                // Add background
                Image backgroundImage = proficiencyContainer.AddComponent<Image>();
                backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
                // Create text
                GameObject textObj = new GameObject("Proficiency Text");
                textObj.transform.SetParent(proficiencyContainer.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                proficiencyNumberText = textObj.AddComponent<TextMeshProUGUI>();
                proficiencyNumberText.text = "Proficiency: Calculating..."; // Temporary text while calculating
                proficiencyNumberText.fontSize = proficiencyFontSize;
                proficiencyNumberText.color = proficiencyTextColor;
                proficiencyNumberText.alignment = TextAlignmentOptions.Center;
                proficiencyNumberText.fontStyle = FontStyles.Bold;
            }
        }
        
        private void SetupPotionScrollView()
        {
            // Find the right panel or create it
            RightPanelManager rightPanel = FindFirstObjectByType<RightPanelManager>();
            if (rightPanel == null)
            {
                Debug.LogWarning("ProficiencyDisplayManager: RightPanelManager not found, creating static potion list separately");
                CreateStandalonePotionList();
                return;
            }
            
            // Modify the right panel to include our static potion list
            ModifyRightPanelForPotionList(rightPanel);
        }
        
        private void CreateStandalonePotionList()
        {
            Debug.Log("🧪 CreateStandalonePotionList: Starting...");
            
            Canvas canvas = FindMainCanvas();
            if (canvas == null) 
            {
                Debug.LogError("🧪 CreateStandalonePotionList: FindMainCanvas returned null! Cannot create UI.");
                return;
            }
            
            Debug.Log($"🧪 CreateStandalonePotionList: Found canvas '{canvas.name}' - RenderMode: {canvas.renderMode}");
            
            // Create standalone potion list panel
            GameObject potionListPanel = new GameObject("Static Potion List Panel");
            potionListPanel.transform.SetParent(canvas.transform, false);
            Debug.Log($"🧪 CreateStandalonePotionList: Created panel, parent set to '{canvas.name}'");
            
            RectTransform panelRect = potionListPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 0.5f);
            panelRect.sizeDelta = new Vector2(200f, 0f);  // Increased from 125f
            panelRect.anchoredPosition = new Vector2(-10f, 0f);
            Debug.Log($"🧪 CreateStandalonePotionList: RectTransform configured - Size: {panelRect.sizeDelta}, Position: {panelRect.anchoredPosition}");
            
            // Add background
            Image panelBackground = potionListPanel.AddComponent<Image>();
            panelBackground.color = potionListBackgroundColor;
            panelBackground.type = Image.Type.Simple;  // Simple solid color, no sprite needed
            
            Debug.Log($"🧪 CreateStandalonePotionList: Panel background configured - Color: {panelBackground.color}, Type: {panelBackground.type}");
            
            // Create header
            CreatePotionListHeader(potionListPanel);
            Debug.Log("🧪 CreateStandalonePotionList: Header created");
            
            // Create scroll view
            CreatePotionScrollView(potionListPanel);
            Debug.Log("🧪 CreateStandalonePotionList: Scroll view created");
            
            // Make sure panel is active
            potionListPanel.SetActive(true);
            Debug.Log("✅ CreateStandalonePotionList: Panel creation complete and active");
        }
        
        private void ModifyRightPanelForPotionList(RightPanelManager rightPanel)
        {
            // This would integrate with the existing right panel
            // For now, we'll create a separate static panel that's always visible
            CreateStandalonePotionList();
        }
        
        private void CreatePotionListHeader(GameObject parent)
        {
            GameObject header = new GameObject("Potion List Header");
            header.transform.SetParent(parent.transform, false);
            
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 25f);
            headerRect.anchoredPosition = Vector2.zero;
            
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            
            // Header text
            GameObject headerText = new GameObject("Header Text");
            headerText.transform.SetParent(header.transform, false);
            
            RectTransform headerTextRect = headerText.AddComponent<RectTransform>();
            headerTextRect.anchorMin = Vector2.zero;
            headerTextRect.anchorMax = Vector2.one;
            headerTextRect.offsetMin = new Vector2(5f, 0f);
            headerTextRect.offsetMax = new Vector2(-5f, 0f);
            
            TextMeshProUGUI headerTextComp = headerText.AddComponent<TextMeshProUGUI>();
            headerTextComp.text = "COMPLETED POTIONS";
            headerTextComp.fontSize = 12f;  // Increased from 7f
            headerTextComp.fontStyle = FontStyles.Bold;
            headerTextComp.color = Color.white;
            headerTextComp.alignment = TextAlignmentOptions.Center;
            
            // Set the font asset to ensure visibility - Unity 6 compatible
            TMP_FontAsset defaultFont = null;
            
            // Try Unity 6 default font first
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (defaultFont == null)
            {
                // Try the built-in resource (older Unity versions)
                defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("LiberationSans SDF");
            }
            if (defaultFont == null)
            {
                // Try to find any available TMP font
                defaultFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault();
            }
            
            if (defaultFont != null)
            {
                headerTextComp.font = defaultFont;
                Debug.Log($"🧪 Set font for header text: {defaultFont.name}");
            }
            else
            {
                Debug.LogWarning("🧪 Could not find any TextMeshPro font asset for header!");
            }
        }
        
        private void CreatePotionScrollView(GameObject parent)
        {
            Debug.Log("🧪 CreatePotionScrollView: Creating simple scroll view...");
            
            // Main scroll view container
            GameObject scrollViewObj = new GameObject("Potion ScrollView");
            scrollViewObj.transform.SetParent(parent.transform, false);
            
            RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(5f, 5f);
            scrollRect.offsetMax = new Vector2(-5f, -30f); // Leave space for header
            
            // Add ScrollRect component
            potionsScrollView = scrollViewObj.AddComponent<ScrollRect>();
            potionsScrollView.horizontal = false;
            potionsScrollView.vertical = true;
            
            // Create viewport (clipping area)
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollViewObj.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            
            // Add Mask component for clipping
            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            
            // Viewport needs an Image component for masking
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            
            // Create content area (this will hold all the potion entries)
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f); // Top-left anchor
            contentRect.anchorMax = new Vector2(1f, 1f); // Top-right anchor  
            contentRect.pivot = new Vector2(0.5f, 1f);   // Pivot at top-center
            contentRect.sizeDelta = new Vector2(0f, 100f); // Start with some height
            contentRect.anchoredPosition = Vector2.zero;
            
            // Add vertical layout to arrange entries
            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 5f;
            contentLayout.padding = new RectOffset(5, 5, 5, 5);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            
            // Auto-resize content based on children
            ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Connect scroll view components
            potionsScrollView.viewport = viewportRect;
            potionsScrollView.content = contentRect;
            potionsContentParent = content.transform;
            
            Debug.Log("✅ CreatePotionScrollView: Scroll view setup complete");
        }
        
        
        /// <summary>
        /// Public method to force refresh proficiency display when grid changes
        /// Call this from GridGameManager when ingredients are placed/removed
        /// </summary>
        public void ForceRefreshProficiency()
        {
            if (isInitialized)
            {
                RefreshProficiencyDisplay();
            }
        }
        
        public void RefreshProficiencyDisplay()
        {
            if (!isInitialized) 
                return;

            float proficiencyPercentage = 0f;
            
            if (useAdvancedFormula)
            {
                currentGrade = CalculateCurrentProficiencyGrade();
                if (currentGrade != null)
                {
                    proficiencyPercentage = currentGrade.proficiencyPercentage;
                }
                else
                {
                    // Grid not ready yet, show 0 instead of error
                    proficiencyPercentage = 0f;
                }
            }
            else if (skillSystem != null)
            {
                int totalSRanks = skillSystem.SkillData.totalSRanks;
                proficiencyPercentage = Mathf.Min(totalSRanks * 10f, maxDisplayValue);
                Debug.Log("Simple proficiency calculated: " + proficiencyPercentage.ToString("F1") + "%");
            }
            else
            {
                // Fallback: Basic grid utilization when no skill system
                proficiencyPercentage = CalculateBasicGridProficiency();
                Debug.Log("Basic grid proficiency calculated: " + proficiencyPercentage.ToString("F1") + "%");
            }
            
            proficiencyPercentage = Mathf.Min(proficiencyPercentage, maxDisplayValue);
            
            // Only update text display if it exists (user disabled text-based display)
            if (proficiencyNumberText != null)
            {
                string proficiencyText = "Proficiency: " + proficiencyPercentage.ToString("F1") + "%";
                
                if (useAdvancedFormula && currentGrade != null)
                {
                    string gradeText = " [" + currentGrade.gradeLevel.ToString() + "]";
                    proficiencyText += gradeText;
                    proficiencyNumberText.color = ProficiencyGrading.GetGradeColor(currentGrade.gradeLevel);
                }
                else
                {
                    proficiencyNumberText.color = Color.white;
                }
                
                proficiencyNumberText.text = proficiencyText;
            }
            
            // Update progress bar if it exists
            if (proficiencyProgressBar != null)
            {
                float normalizedProgress = proficiencyPercentage / 100f;
                proficiencyProgressBar.fillAmount = normalizedProgress;
            }
        }
        
        /// <summary>
        /// Create a visual progress bar string
        /// </summary>
        private string CreateProgressBar(float percentage, int barLength = 10)
        {
            int filledLength = Mathf.RoundToInt(percentage * barLength);
            string filled = new string('█', filledLength);
            string empty = new string('░', barLength - filledLength);
            return $"[{filled}{empty}]";
        }
        
        /// <summary>
        /// Simple test method to verify compilation
        /// </summary>
        public void TestMethod()
        {
            Debug.Log("Test method works");
        }
        
        /// <summary>
        /// Calculate basic proficiency based on current grid utilization when AlchemySkillSystem is not available
        /// </summary>
        private float CalculateBasicGridProficiency()
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                return 0f;
            }

            var ingredientPlacer = gridManager.GetComponent<IngredientPlacer>();
            if (ingredientPlacer == null)
            {
                return 0f;
            }

            // Get placed ingredients
            var ingredientInstances = ingredientPlacer.GetAllPlacedIngredients();
            if (ingredientInstances.Count == 0)
            {
                return 0f; // No ingredients placed
            }

            // Calculate basic metrics
            int totalCells = gridManager.gridWidth * gridManager.gridHeight;
            int placedCells = ingredientInstances.Count;
            
            // Calculate coverage ratio (0-1)
            float coverageRatio = (float)placedCells / totalCells;
            
            // Basic proficiency = coverage * 100, capped at maxDisplayValue
            float baseProficiency = coverageRatio * 100f;
            
            // Add small bonus for ingredient diversity
            var uniqueIngredients = ingredientInstances.Select(i => i.ingredient.ItemName).Distinct().Count();
            float diversityBonus = uniqueIngredients * 5f; // 5% per unique ingredient type
            
            float totalProficiency = baseProficiency + diversityBonus;
            return Mathf.Min(totalProficiency, maxDisplayValue);
        }
        
        /// <summary>
        /// Calculate current proficiency grade using the proper grading system
        /// </summary>
        private ProficiencyGrade CalculateCurrentProficiencyGrade()
        {
            // Get grid manager and current ingredient placement
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogWarning("🧮 GridManager not found for proficiency calculation");
                return CreateDefaultGrade();
            }

            var ingredientPlacer = gridManager.GetComponent<IngredientPlacer>();
            if (ingredientPlacer == null)
            {
                Debug.LogWarning("🧮 IngredientPlacer not found for proficiency calculation");
                return CreateDefaultGrade();
            }

            // Convert placed ingredients to format expected by grading system
            var placedIngredients = new Dictionary<Vector2Int, Ingredient>();
            var ingredientInstances = ingredientPlacer.GetAllPlacedIngredients();
            
            foreach (var instance in ingredientInstances)
            {
                placedIngredients[instance.gridPosition] = instance.ingredient;
            }

            // Get obstacles
            var obstacles = gridManager.aspectObstacles ?? new List<AspectObstacle>();

            // Ensure weights are normalized
            proficiencyWeights.NormalizeWeights();

            // Calculate using the proper grading system
            bool isFreeCrafting = IsFreeCraftingMode(gridManager);
            var grade = ProficiencyGrading.CalculateProficiency(gridManager, placedIngredients, obstacles, proficiencyWeights, isFreeCrafting);
            
            Debug.Log($"🧮 Proficiency breakdown - Coverage: {grade.coverageRatio:F2}, Adjacency: {grade.adjacencySynergy:F2}, " +
                     $"Expansion: {grade.expansionUtilization:F2}, Shape: {grade.shapeDifficulty:F2}, " +
                     $"Orientation: {grade.orientationEfficiency:F2}, Obstacles: {grade.obstaclesCompleted:F2}");
            
            return grade;
        }
        
        /// <summary>
        /// Check if the current session is in free crafting mode
        /// </summary>
        private bool IsFreeCraftingMode(GridGameManager gridManager)
        {
            // Check if there's a CraftingModeSelector in the scene
            var modeSelector = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
            if (modeSelector != null)
            {
                // Check if it's in Free crafting mode
                return modeSelector.GetCurrentMode() == FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector.CraftingMode.Free;
            }
            
            // Fallback: Detect based on grid characteristics
            // Free crafting typically uses a 3x3 grid with no obstacles enabled
            bool isSmallGrid = (gridManager.gridWidth == 3 && gridManager.gridHeight == 3);
            bool hasNoObstacles = !gridManager.enableObstacles || gridManager.aspectObstacles.Count == 0;
            
            return isSmallGrid && hasNoObstacles;
        }
        
        /// <summary>
        /// Create a default grade for when calculation fails
        /// </summary>
        private ProficiencyGrade CreateDefaultGrade()
        {
            return new ProficiencyGrade
            {
                overallScore = 0f,
                proficiencyPercentage = 0f,
                gradeLevel = GradeLevel.F,
                feedback = "Unable to calculate proficiency"
            };
        }
        
        /// <summary>
        /// Calculate score based on grid size utilization efficiency
        /// </summary>
        private float CalculateGridUtilizationScore()
        {
            // Try to get grid game manager for current grid metrics
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null) 
            {
                Debug.LogWarning("🧮 GridManager not found for utilization score");
                return 0f;
            }
            
            // Base grid size (starting size)
            float baseGridSize = 3f * 3f; // 3x3 base grid
            float currentGridSize = gridManager.gridWidth * gridManager.gridHeight;
            
            // Grid expansion bonus - reward using larger grids efficiently
            float expansionRatio = currentGridSize / baseGridSize;
            float expansionBonus = Mathf.Pow(expansionRatio, 1.5f) * 50f; // Exponential bonus for larger grids
            
            // Efficiency bonus - check if grid space is being used well
            float efficiency = gridManager.GetComponent<CraftingGrid>()?.CalculateGridEfficiency() ?? 0.5f;
            float efficiencyBonus = efficiency * 100f;
            
            float totalScore = expansionBonus + efficiencyBonus;
            Debug.Log($"🧮 Grid Utilization: Size={currentGridSize}, Expansion={expansionBonus:F1}, Efficiency={efficiency:F2} ({efficiencyBonus:F1}), Total={totalScore:F1}");
            
            return totalScore;
        }
        
        /// <summary>
        /// Calculate score based on recipe diversity
        /// </summary>
        private float CalculateRecipeDiversityScore()
        {
            var skillData = skillSystem.SkillData;
            int uniqueRecipes = skillData.recipeSkills.Count;
            
            // Bonus for having many different recipes mastered
            float diversityBonus = uniqueRecipes * 25f; // 25 points per unique recipe
            
            // Additional bonus for auto-craft unlocks
            int autoCraftCount = skillData.recipeSkills.Count(r => r.autoCraftingUnlocked);
            float autoCraftBonus = autoCraftCount * 15f; // 15 extra points per auto-craft unlock
            
            return diversityBonus + autoCraftBonus;
        }
        
        /// <summary>
        /// Calculate score based on crafting complexity achievements
        /// </summary>
        private float CalculateComplexityScore()
        {
            var skillData = skillSystem.SkillData;
            
            // Bonus for skill tree unlocks
            float skillBonus = 0f;
            if (skillData.hasIngredientRefund) skillBonus += 50f;
            if (skillData.hasOverlapPlacement) skillBonus += 75f;
            if (skillData.hasEnhancedGridSize) skillBonus += 100f;
            
            // Auto-craft success chance bonus
            skillBonus += skillData.autoCraftSuccessChance * 2f; // 2 points per 1% chance
            
            // High S-rank concentration bonus (for recipes with many S-ranks)
            float concentrationBonus = 0f;
            foreach (var recipe in skillData.recipeSkills)
            {
                if (recipe.sRankCount >= 5)
                {
                    concentrationBonus += (recipe.sRankCount - 4) * 10f; // 10 points for each S-rank beyond 5
                }
            }
            
            return skillBonus + concentrationBonus;
        }
        
        public void OnPotionCrafted(Potion potion)
        {
            // Check if this is the singleton instance
            if (Instance != this)
            {
                Debug.LogWarning("🧪 OnPotionCrafted(Potion) called on non-singleton instance, ignoring");
                return;
            }
            
            if (potion == null)
            {
                Debug.LogError("🧪 OnPotionCrafted: Potion parameter is null!");
                return;
            }
            
            if (completedPotions.ContainsKey(potion))
            {
                completedPotions[potion] = completedPotions[potion] + 1;
                Debug.Log($"🧪 Updated existing potion: {potion.ItemName}, crafted {completedPotions[potion]} times");
            }
            else
            {
                //add to completed potions
                completedPotions.Add(potion,1);
                Debug.Log($"🧪 Added new potion: {potion.ItemName}, crafted {completedPotions[potion]} times");

            }
            RefreshProficiencyDisplay();
        }

        private Canvas CreateUICanvas()
        {
            GameObject canvasObj = new GameObject("Proficiency UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("📱 ProficiencyDisplayManager: Created UI Canvas");
            return canvas;
        }
        
        /// <summary>
        /// Force initialization of the ProficiencyDisplayManager (used when Start() is not called)
        /// </summary>
        public void ForceInitialize()
        {
            Debug.Log("🧪 ProficiencyDisplayManager: ForceInitialize() called");
            if (!isInitialized)
            {
                InitializeProficiencyDisplay();
            }
            else
            {
                Debug.Log("🧪 ProficiencyDisplayManager: Already initialized");
            }
        }
        
        [ContextMenu("Show Proficiency Breakdown")]
        public void ShowProficiencyBreakdown()
        {
            if (!isInitialized || skillSystem == null) return;
            
            if (useAdvancedFormula)
            {
                var skillData = skillSystem.SkillData;
                
                float sRankScore = skillData.totalSRanks * 100f * sRankWeight;
                float gridScore = CalculateGridUtilizationScore() * gridSizeWeight;
                float diversityScore = CalculateRecipeDiversityScore() * diversityWeight;
                float complexityScore = CalculateComplexityScore() * complexityWeight;
                
                Debug.Log($"=== PROFICIENCY BREAKDOWN ===\n" +
                         $"S-Rank Score: {sRankScore:F1} (Weight: {sRankWeight:P0})\n" +
                         $"Grid Utilization: {gridScore:F1} (Weight: {gridSizeWeight:P0})\n" +
                         $"Recipe Diversity: {diversityScore:F1} (Weight: {diversityWeight:P0})\n" +
                         $"Complexity Bonus: {complexityScore:F1} (Weight: {complexityWeight:P0})\n" +
                         $"TOTAL: {sRankScore + gridScore + diversityScore + complexityScore:F1}");
            }
            else
            {
                Debug.Log($"Simple Proficiency: {skillSystem.SkillData.totalSRanks} (Total S-Ranks)");
            }
        }
        
        /// <summary>
        /// Get current proficiency as a percentage of max possible
        /// </summary>
        public float GetProficiencyPercentage()
        {
            if (!isInitialized) return 0f;
            
            if (useAdvancedFormula)
            {
                var grade = CalculateCurrentProficiencyGrade();
                if (grade != null)
                {
                    return grade.proficiencyPercentage / 100f; // Convert percentage to 0-1 range
                }
            }
            else if (skillSystem != null)
            {
                int totalSRanks = skillSystem.SkillData.totalSRanks;
                float percentage = Mathf.Min(totalSRanks * 10f, maxDisplayValue); // Each S-rank = 10%
                return percentage / 100f; // Convert to 0-1 range
            }
            else
            {
                // Fallback to basic grid proficiency
                float percentage = CalculateBasicGridProficiency();
                return percentage / 100f; // Convert to 0-1 range
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Get grid efficiency estimate for a specific recipe (placeholder implementation)
        /// </summary>
        private float GetRecipeGridEfficiency(string recipeKey)
        {
            // This is a simplified estimation - in a full implementation, 
            // you'd track actual grid usage data per recipe
            if (skillSystem == null) return 0f;
            
            var recipeSkill = skillSystem.GetRecipeSkill(recipeKey);
            if (recipeSkill == null) return 0f;
            
            // Estimate efficiency based on S-rank count (more S-ranks = better efficiency)
            float baseEfficiency = 0.6f; // Base efficiency assumption
            float sRankBonus = Mathf.Min(recipeSkill.sRankCount * 0.05f, 0.3f); // Up to 30% bonus
            
            return baseEfficiency + sRankBonus;
        }
        
        /// <summary>
        /// Called when the grid is cleared to reset the proficiency display
        /// </summary>
        public void OnGridCleared()
        {
            // Check if this is the singleton instance
            if (Instance != this)
            {
                Debug.LogWarning("🧪 OnGridCleared called on non-singleton instance, ignoring");
                return;
            }
            
            Debug.Log("🔄 Proficiency system notified of grid clear - refreshing display");
            
            // Immediately refresh the display to show updated values
            if (isInitialized)
            {
                RefreshProficiencyDisplay();
            }
            
            // If using advanced formula, the proficiency should reflect current grid state
            if (useAdvancedFormula && proficiencyNumberText != null)
            {
                // The advanced formula considers current grid utilization, so clearing the grid
                // will automatically reduce the proficiency score
                var newGrade = CalculateCurrentProficiencyGrade();
                if (newGrade != null)
                {
                    float newProficiency = newGrade.proficiencyPercentage;
                    
                    // Add a brief visual feedback for the reset
                    if (proficiencyNumberText != null)
                    {
                        StartCoroutine(AnimateProficiencyReset(newProficiency));
                    }
                }
            }
        }
        
        /// <summary>
        /// Animate the proficiency display when grid is cleared
        /// </summary>
        private System.Collections.IEnumerator AnimateProficiencyReset(float newValue)
        {
            if (proficiencyNumberText == null) yield break;
            
            // Briefly flash the display to indicate reset
            Color originalColor = proficiencyNumberText.color;
            proficiencyNumberText.color = Color.red;
            
            yield return new WaitForSeconds(0.2f);
            
            proficiencyNumberText.color = originalColor;
            proficiencyNumberText.text = $"Proficiency: {newValue:F1}%";
            
            // Update progress bar if it exists
            if (proficiencyProgressBar != null)
            {
                float progress = newValue / 100f; // Convert percentage to 0-1 range
                proficiencyProgressBar.fillAmount = progress;
            }
        }
        
        /// <summary>
        /// Find the main Canvas in the scene for UI creation
        /// </summary>
        private Canvas FindMainCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return canvas;
                }
            }
            return canvases.Length > 0 ? canvases[0] : null;
        }
        
        private void OnDestroy()
        {
            Debug.LogWarning("🧪 ProficiencyDisplayManager: OnDestroy() called!");
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}