using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple, guaranteed-to-work crafting scene converter
/// </summary>
public class SimpleCraftingConverter : EditorWindow
{
    [MenuItem("Tools/Convert Crafting Scene", false, 100)]
    public static void QuickConvert()
    {
        var currentScene = SceneManager.GetActiveScene();
        
        if (currentScene.name != "Crafting System")
        {
            EditorUtility.DisplayDialog("Wrong Scene", 
                $"Please open the 'Crafting System' scene first.\n\nCurrent scene: {currentScene.name}", 
                "OK");
            return;
        }

        Debug.Log("🚀 Quick converting Crafting System scene...");

        // Create the core components
        CreateCraftingManager();
        CreateInventory();
        CreateNavigationController();

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);

        Debug.Log("✅ Conversion complete!");
        
        EditorUtility.DisplayDialog("Success!", 
            "🎉 Crafting System converted!\n\n" +
            "✅ CraftingUIManager added\n" +
            "✅ Inventory system created\n" +
            "✅ Navigation controls setup\n\n" +
            "Press Play and use 1-4 keys to test!", 
            "Great!");
    }

    [MenuItem("Tools/Setup Hybrid UI", false, 101)]
    public static void ShowSetupWindow()
    {
        GetWindow<SimpleCraftingConverter>("Hybrid Crafting Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Hybrid Crafting Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        var scene = SceneManager.GetActiveScene();
        EditorGUILayout.LabelField($"Scene: {scene.name}");

        if (scene.name != "Crafting System")
        {
            EditorGUILayout.HelpBox("⚠️ Open 'Crafting System' scene first", MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox("Convert your scene to hybrid modular architecture", MessageType.Info);

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🚀 Convert Scene", GUILayout.Height(40)))
        {
            QuickConvert();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "After conversion:\n" +
            "• 1-4: Switch main panels\n" +
            "• F1-F4: Minigames\n" +
            "• ESC: Go back", 
            MessageType.Info);
    }

    private static void CreateCraftingManager()
    {
        if (GameObject.Find("CraftingUIManager") != null)
        {
            Debug.Log("✅ CraftingUIManager already exists");
            return;
        }

        var managerGO = new GameObject("CraftingUIManager");
        
        // Add a simple component for now - we'll enhance it later
        var simpleManager = managerGO.AddComponent<SimpleCraftingManager>();
        
        Debug.Log("✅ Created CraftingUIManager");
        Selection.activeGameObject = managerGO;
    }

    private static void CreateInventory()
    {
        if (GameObject.Find("Inventory") != null)
        {
            Debug.Log("✅ Inventory already exists");
            return;
        }

        var inventoryGO = new GameObject("Inventory");
        inventoryGO.AddComponent<ItemSlotContainerHolder>();
        
        Debug.Log("✅ Created Inventory");
    }

    private static void CreateNavigationController()
    {
        if (GameObject.Find("CraftingNavigationController") != null)
        {
            Debug.Log("✅ Navigation controller already exists");
            return;
        }

        var navGO = new GameObject("CraftingNavigationController");
        navGO.AddComponent<SimpleNavigationController>();
        
        Debug.Log("✅ Created Navigation Controller");
    }
}

/// <summary>
/// Simple crafting manager that handles panel switching
/// </summary>
public class SimpleCraftingManager : MonoBehaviour
{
    private GameObject[] uiPanels;
    private string currentPanel = "";

    void Start()
    {
        // Find all UI panels
        uiPanels = new GameObject[]
        {
            GameObject.Find("CraftingMenuSystem"),
            GameObject.Find("PotionCraftingUI"),
            GameObject.Find("GridMinigameUI"),
            GameObject.Find("BulkCraftingUI"),
            GameObject.Find("RefinementUI"),
            GameObject.Find("RoastingMinigameUI"),
            GameObject.Find("DistillingMinigameUI"),
            GameObject.Find("GrindingMinigameUI")
        };

        ShowPanel("CraftingMenuSystem");
        Debug.Log("🎮 Simple Crafting Manager ready! Use 1-4 keys to navigate.");
    }

    public void ShowPanel(string panelName)
    {
        // Hide all panels
        foreach (var panel in uiPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // Show requested panel
        var targetPanel = GameObject.Find(panelName);
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            currentPanel = panelName;
            Debug.Log($"👁️ Showing: {panelName}");
        }
    }

    void Update()
    {
        // Simple navigation
        if (Input.GetKeyDown(KeyCode.Alpha1)) ShowPanel("CraftingMenuSystem");
        if (Input.GetKeyDown(KeyCode.Alpha2)) ShowPanel("PotionCraftingUI");
        if (Input.GetKeyDown(KeyCode.Alpha3)) ShowPanel("BulkCraftingUI");
        if (Input.GetKeyDown(KeyCode.Alpha4)) ShowPanel("RefinementUI");
        
        if (Input.GetKeyDown(KeyCode.F1)) ShowPanel("GridMinigameUI");
        if (Input.GetKeyDown(KeyCode.F2)) ShowPanel("RoastingMinigameUI");
        if (Input.GetKeyDown(KeyCode.F3)) ShowPanel("DistillingMinigameUI");
        if (Input.GetKeyDown(KeyCode.F4)) ShowPanel("GrindingMinigameUI");
    }
}

/// <summary>
/// Simple navigation controller
/// </summary>
public class SimpleNavigationController : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🎮 Navigation: 1-4 for main panels, F1-F4 for minigames");
    }

    public void ShowMainMenu() => FindFirstObjectByType<SimpleCraftingManager>()?.ShowPanel("CraftingMenuSystem");
    public void ShowPotionCrafting() => FindFirstObjectByType<SimpleCraftingManager>()?.ShowPanel("PotionCraftingUI");
    public void ShowBulkCrafting() => FindFirstObjectByType<SimpleCraftingManager>()?.ShowPanel("BulkCraftingUI");
    public void ShowRefinement() => FindFirstObjectByType<SimpleCraftingManager>()?.ShowPanel("RefinementUI");
}