using UnityEngine;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Handles integration between GridDemo UI and the proficiency/potion tracking system
    /// </summary>
    public class GridDemoUIIntegration : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool debugMode = true;
        
        private ProficiencyDisplayManager proficiencyManager;
        private AlchemySkillSystem skillSystem;
        private bool isSetupComplete = false;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupIntegration();
            }
        }
        
        private void SetupIntegration()
        {
            if (debugMode)
                Debug.Log("🔧 GridDemoUIIntegration: Setting up proficiency display integration...");
            
            // Find or create proficiency manager
            proficiencyManager = FindFirstObjectByType<ProficiencyDisplayManager>();
            if (proficiencyManager == null)
            {
                GameObject proficiencyObj = new GameObject("Proficiency Display Manager");
                proficiencyManager = proficiencyObj.AddComponent<ProficiencyDisplayManager>();
                
                if (debugMode)
                    Debug.Log("✅ Created ProficiencyDisplayManager");
            }
            
            // Ensure skill system exists
            skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null)
            {
                if (debugMode)
                    Debug.LogWarning("⚠️ AlchemySkillSystem not found - proficiency tracking may not work correctly");
            }
            
            // Mark as complete
            isSetupComplete = true;
            
            if (debugMode)
                Debug.Log("✅ GridDemoUIIntegration: Setup complete!");
        }
        
        public void OnPotionCrafted(string recipeKey, string potionName)
        {
            if (!isSetupComplete || proficiencyManager == null)
                return;
            
            // Try to find the potion by name to pass to proficiency manager
            var potions = Resources.LoadAll<Potion>("Potions");
            var matchedPotion = potions.FirstOrDefault(p => p.ItemName == potionName);
            if (matchedPotion != null)
            {
                proficiencyManager.OnPotionCrafted(matchedPotion);
            }
            else
            {
                Debug.LogWarning($"Could not find potion '{potionName}' for proficiency tracking");
            }
            
            if (debugMode)
                Debug.Log($"📝 Recorded potion craft: {potionName} ({recipeKey})");
        }
        
        public void RefreshProficiencyDisplay()
        {
            if (proficiencyManager != null)
            {
                proficiencyManager.ForceRefreshProficiency();
            }
        }
        
        [ContextMenu("Force Setup Integration")]
        public void ForceSetupIntegration()
        {
            SetupIntegration();
        }
        
        [ContextMenu("Test Advanced Proficiency")]
        public void TestAdvancedProficiency()
        {
            if (proficiencyManager != null)
            {
                proficiencyManager.ShowProficiencyBreakdown();
                proficiencyManager.ForceRefreshProficiency();
                
                if (debugMode)
                {
                    float percentage = proficiencyManager.GetProficiencyPercentage();
                    Debug.Log($"🎯 Current proficiency percentage: {percentage:P1}");
                }
            }
        }
        
        [ContextMenu("Simulate Grid Size Test")]
        public void SimulateGridSizeTest()
        {
            // This simulates what happens with different grid sizes
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null && debugMode)
            {
                Debug.Log($"📏 Current Grid Size: {gridManager.gridWidth}x{gridManager.gridHeight} " +
                         $"(Total: {gridManager.gridWidth * gridManager.gridHeight} cells)");
                
                // Show how grid expansion affects proficiency
                Debug.Log($"🔍 Base grid (3x3 = 9 cells) vs Current grid = {(float)(gridManager.gridWidth * gridManager.gridHeight) / 9f:F2}x expansion");
            }
        }
    }
}