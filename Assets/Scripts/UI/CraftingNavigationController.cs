using UnityEngine;
using FourFatesStudios.ProjectWarden.UI;

/// <summary>
/// Navigation controls for the Crafting System
/// </summary>
public class CraftingNavigationController : MonoBehaviour
{
    void Update()
    {
        if (CraftingUIManager.Instance == null) return;
        
        // Main navigation
        if (Input.GetKeyDown(KeyCode.Alpha1)) CraftingUIManager.Instance.ShowMainMenu();
        if (Input.GetKeyDown(KeyCode.Alpha2)) CraftingUIManager.Instance.ShowPotionCrafting();
        if (Input.GetKeyDown(KeyCode.Alpha3)) CraftingUIManager.Instance.ShowBulkCrafting();
        if (Input.GetKeyDown(KeyCode.Alpha4)) CraftingUIManager.Instance.ShowRefinement();
        
        // Minigame navigation (F keys)
        if (Input.GetKeyDown(KeyCode.F1)) CraftingUIManager.Instance.ShowGridMinigame();
        if (Input.GetKeyDown(KeyCode.F2)) CraftingUIManager.Instance.ShowRoastingMinigame();
        if (Input.GetKeyDown(KeyCode.F3)) CraftingUIManager.Instance.ShowDistillingMinigame();
        if (Input.GetKeyDown(KeyCode.F4)) CraftingUIManager.Instance.ShowGrindingMinigame();
        
        // Back navigation
        if (Input.GetKeyDown(KeyCode.Escape)) CraftingUIManager.Instance.GoBack();
    }
    
    // Public methods for UI buttons
    public void ShowMainMenu() => CraftingUIManager.Instance?.ShowMainMenu();
    public void ShowPotionCrafting() => CraftingUIManager.Instance?.ShowPotionCrafting();
    public void ShowBulkCrafting() => CraftingUIManager.Instance?.ShowBulkCrafting();
    public void ShowRefinement() => CraftingUIManager.Instance?.ShowRefinement();
    public void GoBack() => CraftingUIManager.Instance?.GoBack();
}