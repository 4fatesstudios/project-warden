using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.Demo
{
    /// <summary>
    /// Quick working UI demo that creates UI programmatically
    /// This bypasses UXML file dependencies and shows immediate results
    /// </summary>
    public class QuickUIDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool showUIOnStart = true;
        [SerializeField] private Ingredient[] testIngredients;
        
        private UIDocument uiDocument;
        private VisualElement root;
        private int currentMenu;
        private string[] menuNames = { "Potion Crafting", "Refinement", "Bulk Crafting", "Alchemy Book" };

        private void Awake()
        {
            CreateUIDocument();
            if (showUIOnStart)
            {
                CreateDemoUI();
            }
        }
        
        private void SetPadding(VisualElement element, float value)
        {
            element.style.paddingLeft = value;
            element.style.paddingRight = value;
            element.style.paddingTop = value;
            element.style.paddingBottom = value;
        }

        private void CreateUIDocument()
        {
            // Create UIDocument component on this GameObject
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }
            
            // Create the UI programmatically instead of using UXML
            CreateProgrammaticUI();
        }

        private void CreateProgrammaticUI()
        {
            // Create the visual tree programmatically
            var visualTreeAsset = ScriptableObject.CreateInstance<VisualTreeAsset>();
            uiDocument.visualTreeAsset = visualTreeAsset;
            
            // Get the root and create our UI
            root = uiDocument.rootVisualElement;
            root.Clear();
            
            // Apply basic styling
            root.style.flexGrow = 1;
            root.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.15f, 0.95f));
            SetPadding(root, 20);
        }

        private void CreateDemoUI()
        {
            Debug.Log("Creating programmatic UI demo...");
            
            // Clear existing content
            root.Clear();
            
            // Create main container
            var mainContainer = new VisualElement();
            mainContainer.style.flexGrow = 1;
            mainContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.1f, 0.2f, 0.9f));
            mainContainer.style.borderTopLeftRadius = 15;
            mainContainer.style.borderTopRightRadius = 15;
            mainContainer.style.borderBottomLeftRadius = 15;
            mainContainer.style.borderBottomRightRadius = 15;
            SetPadding(mainContainer, 20);
            mainContainer.style.marginTop = 50;
            mainContainer.style.marginBottom = 50;
            mainContainer.style.marginLeft = 100;
            mainContainer.style.marginRight = 100;
            
            root.Add(mainContainer);
            
            // Create header
            CreateHeader(mainContainer);
            
            // Create menu content based on current selection
            CreateMenuContent(mainContainer);
            
            // Create navigation
            CreateNavigation(mainContainer);
            
            // Create instructions
            CreateInstructions(mainContainer);
            
            Debug.Log("✓ Programmatic UI created successfully!");
        }

        private void CreateHeader(VisualElement parent)
        {
            var header = new VisualElement();
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 20;
            header.style.paddingTop = 15;
            header.style.paddingBottom = 15;
            header.style.backgroundColor = new StyleColor(new Color(0.3f, 0.2f, 0.4f, 0.8f));
            header.style.borderTopLeftRadius = 10;
            header.style.borderTopRightRadius = 10;
            header.style.borderBottomLeftRadius = 10;
            header.style.borderBottomRightRadius = 10;
            
            var title = new Label($"🧪 Crafting System Demo - {menuNames[currentMenu]}");
            title.style.fontSize = 28;
            title.style.color = new StyleColor(new Color(1f, 0.9f, 0.6f));
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            var subtitle = new Label("Working UI Demo - No external files required!");
            subtitle.style.fontSize = 14;
            subtitle.style.color = new StyleColor(new Color(0.8f, 0.7f, 0.5f));
            subtitle.style.unityTextAlign = TextAnchor.MiddleCenter;
            subtitle.style.marginTop = 5;
            
            header.Add(title);
            header.Add(subtitle);
            parent.Add(header);
        }

        private void CreateMenuContent(VisualElement parent)
        {
            var content = new VisualElement();
            content.style.flexGrow = 1;
            content.style.backgroundColor = new StyleColor(new Color(0.2f, 0.15f, 0.3f, 0.9f));
            content.style.borderTopLeftRadius = 10;
            content.style.borderTopRightRadius = 10;
            content.style.borderBottomLeftRadius = 10;
            content.style.borderBottomRightRadius = 10;
            SetPadding(content, 20);
            content.style.marginBottom = 20;
            
            switch (currentMenu)
            {
                case 0: CreatePotionCraftingContent(content); break;
                case 1: CreateRefinementContent(content); break;
                case 2: CreateBulkCraftingContent(content); break;
                case 3: CreateAlchemyBookContent(content); break;
            }
            
            parent.Add(content);
        }

        private void CreatePotionCraftingContent(VisualElement parent)
        {
            var title = new Label("⚗️ Potion Crafting Station");
            title.style.fontSize = 20;
            title.style.color = new StyleColor(Color.white);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 15;
            parent.Add(title);
            
            // Ingredient selection area
            var ingredientArea = new VisualElement();
            ingredientArea.style.flexDirection = FlexDirection.Row;
            ingredientArea.style.justifyContent = Justify.Center;
            ingredientArea.style.alignItems = Align.Center;
            ingredientArea.style.marginBottom = 20;
            
            for (int i = 0; i < 3; i++)
            {
                var slot = new Button();
                slot.text = "+";
                slot.style.width = 80;
                slot.style.height = 80;
                slot.style.backgroundColor = new StyleColor(new Color(0.4f, 0.3f, 0.5f));
                slot.style.borderTopLeftRadius = 10;
                slot.style.borderTopRightRadius = 10;
                slot.style.borderBottomLeftRadius = 10;
                slot.style.borderBottomRightRadius = 10;
                slot.style.fontSize = 24;
                slot.style.color = new StyleColor(Color.white);
                slot.style.marginLeft = 10;
                slot.style.marginRight = 10;
                
                int slotIndex = i;
                slot.clicked += () => OnIngredientSlotClicked(slotIndex);
                
                ingredientArea.Add(slot);
                
                if (i < 2)
                {
                    var plus = new Label("+");
                    plus.style.fontSize = 20;
                    plus.style.color = new StyleColor(Color.gray);
                    plus.style.unityFontStyleAndWeight = FontStyle.Bold;
                    ingredientArea.Add(plus);
                }
            }
            
            parent.Add(ingredientArea);
            
            // Crafting buttons
            var buttonArea = new VisualElement();
            buttonArea.style.flexDirection = FlexDirection.Row;
            buttonArea.style.justifyContent = Justify.SpaceAround;
            buttonArea.style.marginTop = 20;
            
            var manualCraftBtn = CreateStyledButton("🎮 Manual Craft", new Color(0.4f, 0.6f, 0.4f));
            manualCraftBtn.clicked += () => Debug.Log("Manual crafting started - would launch grid minigame!");
            
            var autoCraftBtn = CreateStyledButton("⚡ Auto Craft", new Color(0.4f, 0.4f, 0.6f));
            autoCraftBtn.clicked += () => Debug.Log("Auto crafting started - requires S-rank unlock!");
            
            var bulkCraftBtn = CreateStyledButton("📦 Bulk Craft", new Color(0.6f, 0.4f, 0.4f));
            bulkCraftBtn.clicked += () => Debug.Log("Opening bulk crafting interface!");
            
            buttonArea.Add(manualCraftBtn);
            buttonArea.Add(autoCraftBtn);
            buttonArea.Add(bulkCraftBtn);
            parent.Add(buttonArea);
        }

        private void CreateRefinementContent(VisualElement parent)
        {
            var title = new Label("🔥 Refinement Station");
            title.style.fontSize = 20;
            title.style.color = new StyleColor(Color.white);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 15;
            parent.Add(title);
            
            var description = new Label("Transform raw ingredients into refined materials using specialized techniques!");
            description.style.fontSize = 14;
            description.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            description.style.unityTextAlign = TextAnchor.MiddleCenter;
            description.style.marginBottom = 20;
            description.style.whiteSpace = WhiteSpace.Normal;
            parent.Add(description);
            
            // Refinement options
            var refinementArea = new VisualElement();
            refinementArea.style.flexDirection = FlexDirection.Row;
            refinementArea.style.justifyContent = Justify.SpaceAround;
            
            var roastingBtn = CreateStyledButton("🍳 Roasting\n(Frying Pan)", new Color(0.8f, 0.4f, 0.2f));
            roastingBtn.clicked += () => Debug.Log("Starting roasting minigame - fire animation, temperature control!");
            
            var distillationBtn = CreateStyledButton("⚗️ Distillation\n(Laboratory)", new Color(0.2f, 0.6f, 0.8f));
            distillationBtn.clicked += () => Debug.Log("Starting distillation minigame - bubbling, steam effects!");
            
            var grindingBtn = CreateStyledButton("🥄 Grinding\n(Mortar & Pestle)", new Color(0.6f, 0.5f, 0.3f));
            grindingBtn.clicked += () => Debug.Log("Starting grinding minigame - rhythm-based gameplay!");
            
            refinementArea.Add(roastingBtn);
            refinementArea.Add(distillationBtn);
            refinementArea.Add(grindingBtn);
            parent.Add(refinementArea);
        }

        private void CreateBulkCraftingContent(VisualElement parent)
        {
            var title = new Label("📦 Bulk Crafting Station");
            title.style.fontSize = 20;
            title.style.color = new StyleColor(Color.white);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 15;
            parent.Add(title);
            
            var description = new Label("Mass produce potions efficiently using unlocked recipes!");
            description.style.fontSize = 14;
            description.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            description.style.unityTextAlign = TextAnchor.MiddleCenter;
            description.style.marginBottom = 20;
            parent.Add(description);
            
            // Mock recipe list
            var recipeArea = new VisualElement();
            recipeArea.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.2f, 0.8f));
            recipeArea.style.borderTopLeftRadius = 8;
            recipeArea.style.borderTopRightRadius = 8;
            recipeArea.style.borderBottomLeftRadius = 8;
            recipeArea.style.borderBottomRightRadius = 8;
            SetPadding(recipeArea, 15);
            
            var recipes = new [] { "Health Potion", "Mana Potion", "Stamina Elixir", "Fire Resistance" };
            foreach (var recipe in recipes)
            {
                var recipeBtn = CreateStyledButton($"🧪 {recipe} (x10)", new Color(0.3f, 0.3f, 0.5f));
                recipeBtn.clicked += () => Debug.Log($"Bulk crafting {recipe}!");
                recipeBtn.style.marginBottom = 5;
                recipeArea.Add(recipeBtn);
            }
            
            parent.Add(recipeArea);
        }

        private void CreateAlchemyBookContent(VisualElement parent)
        {
            var title = new Label("📚 Alchemy Knowledge Book");
            title.style.fontSize = 20;
            title.style.color = new StyleColor(Color.white);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 15;
            parent.Add(title);
            
            var description = new Label("📖 Recipe collection, ingredient database, and crafting guides\n\n🔮 Coming soon in full implementation!");
            description.style.fontSize = 14;
            description.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            description.style.unityTextAlign = TextAnchor.MiddleCenter;
            description.style.whiteSpace = WhiteSpace.Normal;
            parent.Add(description);
        }

        private Button CreateStyledButton(string text, Color bgColor)
        {
            var button = new Button();
            button.text = text;
            button.style.backgroundColor = new StyleColor(bgColor);
            button.style.color = new StyleColor(Color.white);
            button.style.borderTopLeftRadius = 8;
            button.style.borderTopRightRadius = 8;
            button.style.borderBottomLeftRadius = 8;
            button.style.borderBottomRightRadius = 8;
            SetPadding(button, 10);
            button.style.marginLeft = 5;
            button.style.marginRight = 5;
            button.style.fontSize = 14;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.whiteSpace = WhiteSpace.Normal;
            button.style.height = 60;
            button.style.minWidth = 120;
            
            return button;
        }

        private void CreateNavigation(VisualElement parent)
        {
            var navArea = new VisualElement();
            navArea.style.flexDirection = FlexDirection.Row;
            navArea.style.justifyContent = Justify.SpaceAround;
            navArea.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.2f, 0.8f));
            navArea.style.borderTopLeftRadius = 8;
            navArea.style.borderTopRightRadius = 8;
            navArea.style.borderBottomLeftRadius = 8;
            navArea.style.borderBottomRightRadius = 8;
            SetPadding(navArea, 15);
            navArea.style.marginBottom = 15;
            
            for (int i = 0; i < menuNames.Length; i++)
            {
                int menuIndex = i;
                var isActive = i == currentMenu;
                
                var navBtn = CreateStyledButton($"{i + 1}. {menuNames[i]}", 
                    isActive ? new Color(0.6f, 0.4f, 0.8f) : new Color(0.3f, 0.3f, 0.4f));
                navBtn.clicked += () => SwitchMenu(menuIndex);
                
                if (isActive)
                {
                    navBtn.style.borderLeftWidth = 3;
                    navBtn.style.borderRightWidth = 3;
                    navBtn.style.borderTopWidth = 3;
                    navBtn.style.borderBottomWidth = 3;
                    navBtn.style.borderLeftColor = new StyleColor(Color.white);
                    navBtn.style.borderRightColor = new StyleColor(Color.white);
                    navBtn.style.borderTopColor = new StyleColor(Color.white);
                    navBtn.style.borderBottomColor = new StyleColor(Color.white);
                }
                
                navArea.Add(navBtn);
            }
            
            parent.Add(navArea);
        }

        private void CreateInstructions(VisualElement parent)
        {
            var instructions = new Label("🎮 Controls: Use number keys 1-4 to switch menus or click the navigation buttons above!\n\n✨ This is a working demo of the crafting system UI. All buttons show debug messages in console.");
            instructions.style.fontSize = 12;
            instructions.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            instructions.style.unityTextAlign = TextAnchor.MiddleCenter;
            instructions.style.whiteSpace = WhiteSpace.Normal;
            instructions.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 0.6f));
            instructions.style.borderTopLeftRadius = 5;
            instructions.style.borderTopRightRadius = 5;
            instructions.style.borderBottomLeftRadius = 5;
            instructions.style.borderBottomRightRadius = 5;
            SetPadding(instructions, 20);
            
            parent.Add(instructions);
        }

        private void SwitchMenu(int menuIndex)
        {
            if (menuIndex >= 0 && menuIndex < menuNames.Length)
            {
                currentMenu = menuIndex;
                CreateDemoUI(); // Refresh the UI
                Debug.Log($"Switched to {menuNames[currentMenu]} menu");
            }
        }

        private void OnIngredientSlotClicked(int slotIndex)
        {
            Debug.Log($"Ingredient slot {slotIndex + 1} clicked - would show ingredient selection popup!");
            
            if (testIngredients != null && testIngredients.Length > 0)
            {
                var randomIngredient = testIngredients[Random.Range(0, testIngredients.Length)];
                Debug.Log($"Selected random test ingredient: {randomIngredient?.ItemName ?? "None"}");
            }
            else
            {
                Debug.Log("No test ingredients assigned - create some Ingredient ScriptableObjects and assign them!");
            }
        }

        // Keyboard controls for quick testing
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchMenu(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchMenu(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchMenu(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchMenu(3);
            else if (Input.GetKeyDown(KeyCode.R)) CreateDemoUI(); // Refresh UI
        }

        // Context menu helpers for testing
        [ContextMenu("Refresh UI")]
        public void RefreshUI()
        {
            CreateDemoUI();
        }

        [ContextMenu("Test All Menus")]
        public void TestAllMenus()
        {
            for (int i = 0; i < menuNames.Length; i++)
            {
                Debug.Log($"Testing menu {i + 1}: {menuNames[i]}");
            }
        }
    }
}