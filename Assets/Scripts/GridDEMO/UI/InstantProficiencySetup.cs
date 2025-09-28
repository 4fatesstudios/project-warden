using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Instant setup for proficiency display - attach to any GameObject and it will create the UI
    /// </summary>
    public class InstantProficiencySetup : MonoBehaviour
    {
        [Header("Quick Setup")]
        [SerializeField] private bool setupOnAwake = true;
        [SerializeField] private bool destroyAfterSetup = true;
        
        private void Awake()
        {
            if (setupOnAwake)
            {
                SetupProficiencyUI();
            }
        }
        
        [ContextMenu("Setup Proficiency UI Now")]
        public void SetupProficiencyUI()
        {
            Debug.Log("🚀 Starting instant proficiency UI setup...");
            
            // Find or create UI integration
            GridDemoUIIntegration integration = FindFirstObjectByType<GridDemoUIIntegration>();
            if (integration == null)
            {
                GameObject integrationObj = new GameObject("UI Integration Manager");
                integration = integrationObj.AddComponent<GridDemoUIIntegration>();
                Debug.Log("✅ Created GridDemoUIIntegration");
            }
            
            // Force setup integration which will create ProficiencyDisplayManager
            integration.ForceSetupIntegration();
            
            // Find the proficiency manager and force initialization
            ProficiencyDisplayManager proficiencyManager = FindFirstObjectByType<ProficiencyDisplayManager>();
            if (proficiencyManager != null)
            {
                proficiencyManager.ForceRefreshProficiency();
                Debug.Log("✅ Proficiency display is now active!");
            }
            else
            {
                Debug.LogWarning("⚠️ ProficiencyDisplayManager not found after setup");
            }
            
            if (destroyAfterSetup)
            {
                Destroy(gameObject);
            }
        }
        
        [ContextMenu("Test Proficiency System")]
        public void TestProficiencySystem()
        {
            ProficiencyDisplayManager proficiencyManager = FindFirstObjectByType<ProficiencyDisplayManager>();
            if (proficiencyManager != null)
            {
                proficiencyManager.ShowProficiencyBreakdown();
                Debug.Log("🎯 Check console for proficiency breakdown");
            }
            else
            {
                Debug.LogWarning("⚠️ ProficiencyDisplayManager not found - run setup first");
            }
        }
    }
}