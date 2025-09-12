using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Effects;
using System.Collections.Generic;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.Examples
{
    /// <summary>
    /// Example demonstrating how to use the new Infusion Management System
    /// </summary>
    public class InfusionSystemExample : MonoBehaviour
    {
        [Header("Test Assets")]
        [SerializeField] private Ingredient testIngredient;
        [SerializeField] private Potion testPotion;
        [SerializeField] private List<Infusion> availableInfusions = new List<Infusion>();
        
        [Header("Runtime Testing")]
        [SerializeField] private bool testOnStart = true;
        [SerializeField] private bool showDetailedLogs = true;
        
        private void Start()
        {
            if (testOnStart)
            {
                TestInfusionSystem();
            }
        }
        
        [ContextMenu("Test Infusion System")]
        public void TestInfusionSystem()
        {
            Debug.Log("🌟 === INFUSION SYSTEM EXAMPLE ===");
            
            // Load infusions from Resources if not assigned
            LoadAvailableInfusions();
            
            // Test InfusionBundle operations
            TestInfusionBundle();
            
            // Test Ingredient integration
            TestIngredientIntegration();
            
            // Test Potion integration
            TestPotionIntegration();
            
            // Test advanced features
            TestAdvancedFeatures();
            
            Debug.Log("✅ === INFUSION SYSTEM EXAMPLE COMPLETE ===");
        }
        
        private void LoadAvailableInfusions()
        {
            if (availableInfusions.Count == 0)
            {
                var loadedInfusions = Resources.LoadAll<Infusion>("Infusions");
                availableInfusions.AddRange(loadedInfusions);
                Debug.Log($"🔍 Loaded {availableInfusions.Count} infusions from Resources/Infusions");
            }
        }
        
        private void TestInfusionBundle()
        {
            Debug.Log("\\n🧪 Testing InfusionBundle System:");
            
            var bundle = new InfusionBundle();
            
            // Test adding infusions
            foreach (var infusion in availableInfusions)
            {
                if (infusion != null)
                {
                    bundle.AddInfusion(infusion);
                    Debug.Log($"  ➕ Added: {infusion.InfusionName} (Category: {infusion.Category}, Power: {infusion.PowerLevel})");
                }
            }
            
            // Test bundle properties
            Debug.Log($"\\n📊 Bundle Analysis:");
            Debug.Log($"  • Total Infusions: {bundle.Infusions.Count}");
            Debug.Log($"  • Unique Infusions: {bundle.GetUniqueInfusions().Count}");
            Debug.Log($"  • Total Power Level: {bundle.GetTotalPowerLevel()}");
            Debug.Log($"  • Effect Categories: {string.Join(", ", bundle.GetAllEffectCategories())}");
            Debug.Log($"  • Has Effects: {bundle.HasEffects()}");
            Debug.Log($"  • Validation: {bundle.IsValid(out string validationMessage)} - {validationMessage}");
            
            // Test stacking (if any infusions support it)
            var stackableInfusions = availableInfusions.FindAll(i => i.CanStack);
            if (stackableInfusions.Count > 0)
            {
                var stackableInfusion = stackableInfusions[0];
                Debug.Log($"\\n🔄 Testing Stacking with {stackableInfusion.InfusionName}:");
                
                int originalCount = bundle.GetStackCount(stackableInfusion);
                bundle.AddInfusion(stackableInfusion); // Try to stack
                int newCount = bundle.GetStackCount(stackableInfusion);
                
                Debug.Log($"  • Original stacks: {originalCount}");
                Debug.Log($"  • After adding again: {newCount}");
                Debug.Log($"  • Max stacks allowed: {stackableInfusion.MaxStacks}");
            }
        }
        
        private void TestIngredientIntegration()
        {
            if (testIngredient == null)
            {
                Debug.LogWarning("⚠️ No test ingredient assigned. Skipping ingredient integration test.");
                return;
            }
            
            Debug.Log($"\\n🌿 Testing Ingredient Integration with {testIngredient.name}:");
            
            // Show current state
            Debug.Log($"  • Current infusions: {testIngredient.InfusionBundle.Infusions.Count}");
            Debug.Log($"  • Has effects: {testIngredient.HasEffects()}");
            
            // Add some infusions
            if (availableInfusions.Count > 0)
            {
                var infusionToAdd = availableInfusions[0];
                testIngredient.InfusionBundle.AddInfusion(infusionToAdd);
                Debug.Log($"  ➕ Added infusion: {infusionToAdd.InfusionName}");
                
                // Test ingredient methods
                Debug.Log($"  • Updated infusion count: {testIngredient.InfusionBundle.Infusions.Count}");
                Debug.Log($"  • Total power level: {testIngredient.InfusionBundle.GetTotalPowerLevel()}");
                Debug.Log($"  • Has effects after adding: {testIngredient.HasEffects()}");
                
                // Test effect type checking
                var effectTypes = testIngredient.GetEffectTypes();
                Debug.Log($"  • Effect types: {string.Join(", ", System.Array.ConvertAll(effectTypes, t => t.Name))}");
            }
        }
        
        private void TestPotionIntegration()
        {
            if (testPotion == null)
            {
                Debug.LogWarning("⚠️ No test potion assigned. Skipping potion integration test.");
                return;
            }
            
            Debug.Log($"\\n🍶 Testing Potion Integration with {testPotion.name}:");
            
            // Show current state
            Debug.Log($"  • Current infusions: {testPotion.InfusionBundle.Infusions.Count}");
            Debug.Log($"  • Has infusions: {testPotion.HasInfusions()}");
            Debug.Log($"  • Infusion power level: {testPotion.GetInfusionPowerLevel()}");
            
            // Add infusions to potion
            if (availableInfusions.Count >= 2)
            {
                var infusion1 = availableInfusions[0];
                var infusion2 = availableInfusions[1];
                
                testPotion.AddInfusion(infusion1);
                testPotion.AddInfusion(infusion2);
                
                Debug.Log($"  ➕ Added infusions: {infusion1.InfusionName}, {infusion2.InfusionName}");
                
                // Test potion methods
                Debug.Log($"  • Updated infusion count: {testPotion.InfusionBundle.Infusions.Count}");
                Debug.Log($"  • Total infusion power: {testPotion.GetInfusionPowerLevel()}");
                Debug.Log($"  • All effects count: {testPotion.GetAllEffects().Count}");
                
                // Test enhanced description
                if (showDetailedLogs)
                {
                    Debug.Log($"  📝 Enhanced Description:\\n{testPotion.GetEnhancedDescription()}");
                }
            }
        }
        
        private void TestAdvancedFeatures()
        {
            Debug.Log("\\n⚡ Testing Advanced Features:");
            
            // Test effect categorization
            TestEffectCategorization();
            
            // Test infusion validation
            TestInfusionValidation();
            
            // Test bundle operations
            TestBundleOperations();
        }
        
        private void TestEffectCategorization()
        {
            Debug.Log("\\n🏷️ Effect Categorization Test:");
            
            foreach (var infusion in availableInfusions)
            {
                if (infusion != null)
                {
                    var categories = infusion.GetEffectCategories();
                    var effectTypes = infusion.GetEffectTypes();
                    
                    Debug.Log($"  • {infusion.InfusionName}:");
                    Debug.Log($"    - Categories: {string.Join(", ", categories)}");
                    Debug.Log($"    - Effect Types: {string.Join(", ", effectTypes.Select(t => t.Name))}");
                }
            }
        }
        
        private void TestInfusionValidation()
        {
            Debug.Log("\\n✅ Infusion Validation Test:");
            
            foreach (var infusion in availableInfusions)
            {
                if (infusion != null)
                {
                    bool isValid = infusion.IsValid(out string validationMessage);
                    string status = isValid ? "✅ Valid" : "❌ Invalid";
                    Debug.Log($"  • {infusion.InfusionName}: {status} - {validationMessage}");
                }
            }
        }
        
        private void TestBundleOperations()
        {
            Debug.Log("\\n🔧 Bundle Operations Test:");
            
            if (availableInfusions.Count >= 3)
            {
                var bundle = new InfusionBundle();
                var infusion1 = availableInfusions[0];
                var infusion2 = availableInfusions[1];
                var infusion3 = availableInfusions[2];
                
                // Add infusions
                bundle.AddInfusion(infusion1);
                bundle.AddInfusion(infusion2);
                bundle.AddInfusion(infusion3);
                Debug.Log($"  ➕ Added 3 infusions");
                
                // Test contains
                Debug.Log($"  🔍 Contains {infusion1.InfusionName}: {bundle.ContainsInfusion(infusion1)}");
                
                // Test removal
                bool removed = bundle.RemoveInfusion(infusion2);
                Debug.Log($"  ➖ Removed {infusion2.InfusionName}: {removed}");
                Debug.Log($"  📊 Remaining infusions: {bundle.Infusions.Count}");
                
                // Test clear
                bundle.Clear();
                Debug.Log($"  🗑️ Cleared bundle. Remaining: {bundle.Infusions.Count}");
            }
        }
        
        [ContextMenu("Create Test Setup")]
        public void CreateTestSetup()
        {
            Debug.Log("🔧 Creating test setup...");
            
            // This could create test ingredients and potions
            // For now, just log instructions
            Debug.Log("To set up testing:");
            Debug.Log("1. Create some test infusions using the Alchemy System Editor");
            Debug.Log("2. Assign test ingredients and potions in this component");
            Debug.Log("3. Click 'Test Infusion System' to run the example");
        }
        
        private void OnValidate()
        {
            // Refresh available infusions when values change in inspector
            if (Application.isPlaying)
            {
                LoadAvailableInfusions();
            }
        }
    }
}