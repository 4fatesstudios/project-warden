using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Simple working UI demo that creates UI programmatically
/// This bypasses UXML file dependencies and shows immediate results
/// Now with proper single-instance management and no stacking
/// </summary>
public class SimpleWorkingUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private bool disableOtherUIs = true;
    [SerializeField] private bool showDebugInfo = false;
    
    private UIDocument uiDocument;
    private VisualElement root;
    private int currentMenu = 0;
    private string[] menuNames = { "Potion Crafting", "Refinement", "Bulk Crafting", "Alchemy Book" };
    
    private static SimpleWorkingUI activeInstance;

    void Awake()
    {
        // Ensure only one instance is active
        if (activeInstance != null && activeInstance != this)
        {
            Debug.Log($"🔄 Disabling duplicate SimpleWorkingUI on {gameObject.name}");
            enabled = false;
            return;
        }
        
        activeInstance = this;
    }

    void Start()
    {
        // Disable other UI Documents if requested
        if (disableOtherUIs)
        {
            DisableOtherUIDocuments();
        }
        
        CreateUIDemo();
    }
    
    void OnDestroy()
    {
        if (activeInstance == this)
        {
            activeInstance = null;
        }
    }
    
    private void DisableOtherUIDocuments()
    {
        var allUIDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        int disabledCount = 0;
        
        foreach (var doc in allUIDocuments)
        {
            if (doc.gameObject != gameObject && doc.enabled)
            {
                doc.enabled = false;
                disabledCount++;
                if (showDebugInfo)
                {
                    Debug.Log($"🚫 Disabled UIDocument on {doc.gameObject.name}");
                }
            }
        }
        
        if (disabledCount > 0)
        {
            Debug.Log($"🧹 Disabled {disabledCount} other UI documents to prevent stacking");
        }
    }
    
        private void SetPadding(VisualElement element, float value)
        {
            element.style.paddingLeft = value;
            element.style.paddingRight = value;
            element.style.paddingTop = value;
            element.style.paddingBottom = value;
        }

    void CreateUIDemo()
    {
        Debug.Log("🚀 Creating Simple Working UI Demo...");
    
        // Get or add UIDocument component
        uiDocument = gameObject.GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            uiDocument = gameObject.AddComponent<UIDocument>();
        }

        // Ensure this UIDocument is the only active one
        uiDocument.enabled = true;
        
        // Clear any existing content completely
        if (uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.Clear();
        }

        // Create the UI
        SetupUI();
    
        Debug.Log($"✅ UI Demo Created! Currently showing: {menuNames[currentMenu]}");
        Debug.Log("🎮 Press 1-4 to switch menus!");
    }

        void SetupUI()
        {
            // Create root element
            root = new VisualElement();
            root.style.flexGrow = 1;
            root.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.2f, 0.95f));
            SetPadding(root, 20);        
            // Create main container
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.backgroundColor = new StyleColor(new Color(0.2f, 0.15f, 0.3f, 0.9f));
            container.style.borderTopLeftRadius = 15;
            container.style.borderTopRightRadius = 15;
            container.style.borderBottomLeftRadius = 15;
            container.style.borderBottomRightRadius = 15;
            SetPadding(container, 20);
            container.style.marginTop = 50;
            container.style.marginBottom = 50;
            container.style.marginLeft = 100;
            container.style.marginRight = 100;
        
            // Add title
            var title = new Label($"🧪 {menuNames[currentMenu]} Demo");
            title.style.fontSize = 32;
            title.style.color = new StyleColor(Color.white);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.marginBottom = 30;
            container.Add(title);
        
            // Add content based on current menu
            AddMenuContent(container);
        
            // Add navigation buttons
            AddNavigation(container);
        
            // Add instructions
            var instructions = new Label("🎮 Use number keys 1-4 to switch menus!\n✨ All buttons show console messages when clicked.");
            instructions.style.fontSize = 14;
            instructions.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            instructions.style.unityTextAlign = TextAnchor.MiddleCenter;
            instructions.style.marginTop = 20;
            instructions.style.whiteSpace = WhiteSpace.Normal;
            container.Add(instructions);
        
            root.Add(container);
        
            // Set as the UI Document's visual tree - clear first, then add fresh content
            if (uiDocument != null)
            {
                uiDocument.rootVisualElement.Clear();
                uiDocument.rootVisualElement.Add(root);
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"🔄 UI refreshed - showing {menuNames[currentMenu]}");
            }
        }

        void AddMenuContent(VisualElement parent)
        {
            var content = new VisualElement();
            content.style.alignItems = Align.Center;
            content.style.marginBottom = 20;
        
            switch (currentMenu)
            {
                case 0: // Potion Crafting
                    AddPotionCraftingContent(content);
                    break;
                case 1: // Refinement
                    AddRefinementContent(content);
                    break;
                case 2: // Bulk Crafting
                    AddBulkCraftingContent(content);
                    break;
                case 3: // Alchemy Book
                    AddAlchemyBookContent(content);
                    break;
            }
        
            parent.Add(content);
        }

        void AddPotionCraftingContent(VisualElement parent)
        {
            var description = new Label("⚗️ Select ingredients and create powerful potions!");
            description.style.fontSize = 16;
            description.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f));
            description.style.marginBottom = 20;
            parent.Add(description);
        
            // Ingredient slots
            var slotContainer = new VisualElement();
            slotContainer.style.flexDirection = FlexDirection.Row;
            slotContainer.style.justifyContent = Justify.Center;
            slotContainer.style.marginBottom = 20;
        
            for (int i = 0; i < 3; i++)
            {
                var slot = CreateButton("+", new Color(0.4f, 0.3f, 0.6f));
                slot.style.width = 80;
                slot.style.height = 80;
                slot.style.fontSize = 24;
                int slotIndex = i;
                slot.clicked += () => Debug.Log($"Ingredient slot {slotIndex + 1} clicked!");
                slotContainer.Add(slot);
            
                if (i < 2)
                {
                    var plus = new Label(" + ");
                    plus.style.fontSize = 20;
                    plus.style.color = new StyleColor(Color.gray);
                    slotContainer.Add(plus);
                }
            }
            parent.Add(slotContainer);
        
            // Crafting buttons
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.SpaceAround;
        
            var manualBtn = CreateButton("🎮 Manual Craft", new Color(0.4f, 0.6f, 0.4f));
            manualBtn.clicked += () => Debug.Log("Manual crafting - launching grid minigame!");
            buttonContainer.Add(manualBtn);
        
            var autoBtn = CreateButton("⚡ Auto Craft", new Color(0.4f, 0.4f, 0.6f));
            autoBtn.clicked += () => Debug.Log("Auto crafting - requires S-rank unlock!");
            buttonContainer.Add(autoBtn);
        
            var bulkBtn = CreateButton("📦 Bulk Craft", new Color(0.6f, 0.4f, 0.4f));
            bulkBtn.clicked += () => Debug.Log("Opening bulk crafting menu!");
            buttonContainer.Add(bulkBtn);
        
            parent.Add(buttonContainer);
        }

        void AddRefinementContent(VisualElement parent)
        {
            var description = new Label("🔥 Transform ingredients using specialized techniques!");
            description.style.fontSize = 16;
            description.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f));
            description.style.marginBottom = 20;
            parent.Add(description);
        
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.SpaceAround;
        
            var roastBtn = CreateButton("🍳 Roasting", new Color(0.8f, 0.4f, 0.2f));
            roastBtn.clicked += () => Debug.Log("Starting roasting minigame - fire animation!");
            buttonContainer.Add(roastBtn);
        
            var distillBtn = CreateButton("⚗️ Distillation", new Color(0.2f, 0.6f, 0.8f));
            distillBtn.clicked += () => Debug.Log("Starting distillation - bubbling effects!");
            buttonContainer.Add(distillBtn);
        
            var grindBtn = CreateButton("🥄 Grinding", new Color(0.6f, 0.5f, 0.3f));
            grindBtn.clicked += () => Debug.Log("Starting grinding - rhythm gameplay!");
            buttonContainer.Add(grindBtn);
        
            parent.Add(buttonContainer);
        }

        void AddBulkCraftingContent(VisualElement parent)
        {
            var description = new Label("📦 Mass produce potions efficiently!");
            description.style.fontSize = 16;
            description.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f));
            description.style.marginBottom = 20;
            parent.Add(description);
        
            var recipes = new [] { "Health Potion", "Mana Potion", "Stamina Elixir" };
            foreach (var recipe in recipes)
            {
                var btn = CreateButton($"🧪 {recipe} (x10)", new Color(0.3f, 0.4f, 0.5f));
                btn.clicked += () => Debug.Log($"Bulk crafting {recipe}!");
                btn.style.marginBottom = 5;
                parent.Add(btn);
            }
        }

        void AddAlchemyBookContent(VisualElement parent)
        {
            var description = new Label("📚 Recipe collection and crafting guides\n\n🔮 Coming soon!");
            description.style.fontSize = 16;
            description.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f));
            description.style.unityTextAlign = TextAnchor.MiddleCenter;
            description.style.whiteSpace = WhiteSpace.Normal;
            parent.Add(description);
        }

        void AddNavigation(VisualElement parent)
        {
            var navContainer = new VisualElement();
            navContainer.style.flexDirection = FlexDirection.Row;
            navContainer.style.justifyContent = Justify.SpaceAround;
            navContainer.style.marginTop = 20;
        
            for (int i = 0; i < menuNames.Length; i++)
            {
                int menuIndex = i;
                var isActive = i == currentMenu;
            
                var navBtn = CreateButton($"{i + 1}. {menuNames[i]}", 
                    isActive ? new Color(0.6f, 0.4f, 0.8f) : new Color(0.3f, 0.3f, 0.4f));
                navBtn.clicked += () => SwitchMenu(menuIndex);
                navContainer.Add(navBtn);
            }
        
            parent.Add(navContainer);
        }

        Button CreateButton(string text, Color bgColor)
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
            button.style.minWidth = 100;
            button.style.height = 40;
        
            return button;
        }

    void SwitchMenu(int menuIndex)
    {
        if (menuIndex < 0 || menuIndex >= menuNames.Length) return;
        
        var previousMenu = currentMenu;
        currentMenu = menuIndex;
        
        // Clear everything and rebuild the UI completely
        SetupUI();
        
        Debug.Log($"🔄 Switched from '{menuNames[previousMenu]}' to '{menuNames[currentMenu]}'");
    }

    void Update()
    {
        // Keyboard controls
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchMenu(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchMenu(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchMenu(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchMenu(3);
        
        // Emergency reset key
        else if (Input.GetKeyDown(KeyCode.R) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            DisableOtherUIDocuments();
            SetupUI();
            Debug.Log("🔄 UI Reset! Disabled other UI documents and refreshed.");
        }
    }
    
    // Context menu for easy access
    [ContextMenu("Reset UI")]
    void ResetUI()
    {
        DisableOtherUIDocuments();
        SetupUI();
        Debug.Log("🔄 UI manually reset via context menu");
    }
    
    [ContextMenu("Show Debug Info")]
    void ShowDebugInfo()
    {
        showDebugInfo = !showDebugInfo;
        Debug.Log($"🔍 Debug info {(showDebugInfo ? "enabled" : "disabled")}");
    }
}
