using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Simple Navigation Controller for Crafting System
/// Directly manages panel GameObjects without a complex UI manager
/// </summary>
public class CraftingNavigationController : MonoBehaviour
{
    [Header("Panel Management")]
    [SerializeField] private bool enableDebugLogging = true;
    
    // UI Panel GameObjects (auto-discovered)
    private GameObject craftingMenuSystem;
    private GameObject alchemyMenuUI;
    private GameObject alchemyBookUI;
    private GameObject gridMinigameUI;
    private GameObject bulkCraftingUI;
    private GameObject refinementUI;
    private GameObject roastingMinigameUI;
    private GameObject distillingMinigameUI;
    private GameObject grindingMinigameUI;
    
    // Navigation state
    private Stack<string> panelHistory = new Stack<string>();
    private string currentActivePanel = "";
    
    void Start()
    {
        DiscoverPanels();
        ShowPanel("CraftingMenuSystem"); // Start with main menu
    }
    
    /// <summary>
    /// Auto-discover all UI panels in the scene
    /// </summary>
    private void DiscoverPanels()
    {
        craftingMenuSystem = GameObject.Find("CraftingMenuSystem");
        alchemyMenuUI = GameObject.Find("AlchemyMenuUI");
        alchemyBookUI = GameObject.Find("AlchemyBookUI");
        gridMinigameUI = GameObject.Find("GridMinigameUI");
        bulkCraftingUI = GameObject.Find("BulkCraftingUI");
        refinementUI = GameObject.Find("RefinementUI");
        roastingMinigameUI = GameObject.Find("RoastingMinigameUI");
        distillingMinigameUI = GameObject.Find("DistillingMinigameUI");
        grindingMinigameUI = GameObject.Find("GrindingMinigameUI");
        
        if (enableDebugLogging)
        {
            Debug.Log("🔍 Discovered UI Panels:");
            LogPanelStatus("CraftingMenuSystem", craftingMenuSystem);
            LogPanelStatus("AlchemyMenuUI", alchemyMenuUI);
            LogPanelStatus("AlchemyBookUI", alchemyBookUI);
            LogPanelStatus("GridMinigameUI", gridMinigameUI);
            LogPanelStatus("BulkCraftingUI", bulkCraftingUI);
            LogPanelStatus("RefinementUI", refinementUI);
            LogPanelStatus("RoastingMinigameUI", roastingMinigameUI);
            LogPanelStatus("DistillingMinigameUI", distillingMinigameUI);
            LogPanelStatus("GrindingMinigameUI", grindingMinigameUI);
        }
    }
    
    private void LogPanelStatus(string name, GameObject panel)
    {
        string status = panel != null ? "✅ Found" : "❌ Missing";
        Debug.Log($"  • {name}: {status}");
    }
    
    /// <summary>
    /// Show a specific panel by name
    /// </summary>
    public void ShowPanel(string panelName)
    {
        // Store current panel in history (if not the same)
        if (!string.IsNullOrEmpty(currentActivePanel) && currentActivePanel != panelName)
        {
            panelHistory.Push(currentActivePanel);
        }
        
        // Hide all panels first
        HideAllPanels();
        
        // Show the requested panel
        GameObject targetPanel = GetPanelByName(panelName);
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            currentActivePanel = panelName;
            
            if (enableDebugLogging)
                Debug.Log($"📱 Showing panel: {panelName}");
        }
        else
        {
            Debug.LogError($"❌ Panel not found: {panelName}");
        }
    }
    
    /// <summary>
    /// Hide all panels
    /// </summary>
    private void HideAllPanels()
    {
        var allPanels = new[] { craftingMenuSystem, alchemyMenuUI, alchemyBookUI, gridMinigameUI, 
                               bulkCraftingUI, refinementUI, roastingMinigameUI, distillingMinigameUI, grindingMinigameUI };
        
        foreach (var panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Get panel GameObject by name
    /// </summary>
    private GameObject GetPanelByName(string panelName)
    {
        return panelName switch
        {
            "CraftingMenuSystem" => craftingMenuSystem,
            "AlchemyMenuUI" => alchemyMenuUI,
            "AlchemyBookUI" => alchemyBookUI,
            "GridMinigameUI" => gridMinigameUI,
            "BulkCraftingUI" => bulkCraftingUI,
            "RefinementUI" => refinementUI,
            "RoastingMinigameUI" => roastingMinigameUI,
            "DistillingMinigameUI" => distillingMinigameUI,
            "GrindingMinigameUI" => grindingMinigameUI,
            _ => null
        };
    }
    
    /// <summary>
    /// Go back to previous panel
    /// </summary>
    public void GoBack()
    {
        if (panelHistory.Count > 0)
        {
            string previousPanel = panelHistory.Pop();
            ShowPanel(previousPanel);
            
            if (enableDebugLogging)
                Debug.Log($"⬅️ Going back to: {previousPanel}");
        }
        else
        {
            ShowMainMenu(); // Default to main menu
        }
    }
    
    // Public navigation methods for UI buttons and hotkeys
    public void ShowMainMenu() => ShowPanel("CraftingMenuSystem");
    public void ShowAlchemyMenu() => ShowPanel("AlchemyMenuUI"); // Potion Brewing Menu
    public void ShowAlchemyBook() => ShowPanel("AlchemyBookUI"); // Potion Brewing Guide
    public void ShowBulkCrafting() => ShowPanel("BulkCraftingUI");
    public void ShowRefinement() => ShowPanel("RefinementUI");
    public void ShowGridMinigame() => ShowPanel("GridMinigameUI");
    public void ShowRoastingMinigame() => ShowPanel("RoastingMinigameUI");
    public void ShowDistillingMinigame() => ShowPanel("DistillingMinigameUI");
    public void ShowGrindingMinigame() => ShowPanel("GrindingMinigameUI");
    
    // Keyboard input handling
    void Update()
    {
        if (!enableDebugLogging) return;
        
        // Main navigation (Number keys)
        if (Input.GetKeyDown(KeyCode.Alpha1)) ShowMainMenu(); // Main Menu
        if (Input.GetKeyDown(KeyCode.Alpha2)) ShowAlchemyMenu(); // Potion Brewing Menu
        if (Input.GetKeyDown(KeyCode.Alpha3)) ShowBulkCrafting(); // Bulk Crafting
        if (Input.GetKeyDown(KeyCode.Alpha4)) ShowRefinement(); // Refinement
        if (Input.GetKeyDown(KeyCode.Alpha5)) ShowAlchemyBook(); // Potion Brewing Guide
        
        // Minigame navigation (F keys)
        if (Input.GetKeyDown(KeyCode.F1)) ShowGridMinigame();
        if (Input.GetKeyDown(KeyCode.F2)) ShowRoastingMinigame();
        if (Input.GetKeyDown(KeyCode.F3)) ShowDistillingMinigame();
        if (Input.GetKeyDown(KeyCode.F4)) ShowGrindingMinigame();
        
        // Back navigation
        if (Input.GetKeyDown(KeyCode.Escape)) GoBack();
    }
    
    // Context menu methods for debugging
    [ContextMenu("Test Panel Navigation")]
    private void TestPanelNavigation()
    {
        Debug.Log("🧪 Testing all panels...");
        var panelNames = new[] { "CraftingMenuSystem", "AlchemyMenuUI", "AlchemyBookUI", "GridMinigameUI", 
                                "BulkCraftingUI", "RefinementUI", "RoastingMinigameUI", "DistillingMinigameUI", "GrindingMinigameUI" };
        
        foreach (string panelName in panelNames)
        {
            GameObject panel = GetPanelByName(panelName);
            string status = panel != null ? "✅ Available" : "❌ Missing";
            Debug.Log($"  • {panelName}: {status}");
        }
    }
    
    [ContextMenu("Show All Panels")]
    private void ShowAllPanels()
    {
        var allPanels = new[] { craftingMenuSystem, alchemyMenuUI, alchemyBookUI, gridMinigameUI, 
                               bulkCraftingUI, refinementUI, roastingMinigameUI, distillingMinigameUI, grindingMinigameUI };
        
        foreach (var panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(true);
        }
        Debug.Log("👁️ Showing all panels for testing");
    }
    
    // Get all panel names for debugging
    public string[] GetAllPanelNames()
    {
        return new[] { "CraftingMenuSystem", "AlchemyMenuUI", "AlchemyBookUI", "GridMinigameUI", 
                      "BulkCraftingUI", "RefinementUI", "RoastingMinigameUI", "DistillingMinigameUI", "GrindingMinigameUI" };
    }
}