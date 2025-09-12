using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.Effects;
using System.Reflection;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class InfusionSystemTester : EditorWindow
    {
        [MenuItem("Alchemy/Test Infusion System")]
        public static void TestInfusionSystem()
        {
            CreateTestInfusions();
            TestInfusionBundle();
            Debug.Log("✅ Infusion system test completed successfully!");
        }
        
        private static void CreateTestInfusions()
        {
            // Create Resources/Infusions directory if it doesn't exist
            string path = "Assets/Resources/Infusions";
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "Infusions");
            }
            
            // Create Fire Infusion
            var fireInfusion = CreateTestInfusion("Fire Infusion", Color.red, "Provides burning damage effects");
            
            // Create Ice Infusion
            var iceInfusion = CreateTestInfusion("Ice Infusion", Color.cyan, "Provides freezing effects");
            
            // Create Strength Infusion
            var strengthInfusion = CreateTestInfusion("Strength Infusion", new Color(1f, 0.6f, 0f), "Enhances physical capabilities");
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"✅ Created test infusions: {fireInfusion.InfusionName}, {iceInfusion.InfusionName}, {strengthInfusion.InfusionName}");
        }
        
        private static Infusion CreateTestInfusion(string name, Color color, string description)
        {
            var infusion = ScriptableObject.CreateInstance<Infusion>();
            
            // Use reflection to set private fields
            var infusionType = typeof(Infusion);
            
            var nameField = infusionType.GetField("infusionName", BindingFlags.NonPublic | BindingFlags.Instance);
            var colorField = infusionType.GetField("infusionColor", BindingFlags.NonPublic | BindingFlags.Instance);
            var descField = infusionType.GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
            var effectBundleField = infusionType.GetField("effectBundle", BindingFlags.NonPublic | BindingFlags.Instance);
            var powerLevelField = infusionType.GetField("powerLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            
            nameField?.SetValue(infusion, name);
            colorField?.SetValue(infusion, color);
            descField?.SetValue(infusion, description);
            effectBundleField?.SetValue(infusion, new EffectBundle());
            powerLevelField?.SetValue(infusion, 3); // Medium power level
            
            string assetPath = $"Assets/Resources/Infusions/{name.Replace(" ", "")}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(infusion, assetPath);
            
            return infusion;
        }
        
        private static void TestInfusionBundle()
        {
            var bundle = new InfusionBundle();
            
            // Load test infusions
            var fireInfusion = AssetDatabase.LoadAssetAtPath<Infusion>("Assets/Resources/Infusions/FireInfusion.asset");
            var iceInfusion = AssetDatabase.LoadAssetAtPath<Infusion>("Assets/Resources/Infusions/IceInfusion.asset");
            var strengthInfusion = AssetDatabase.LoadAssetAtPath<Infusion>("Assets/Resources/Infusions/StrengthInfusion.asset");
            
            if (fireInfusion != null) bundle.AddInfusion(fireInfusion);
            if (iceInfusion != null) bundle.AddInfusion(iceInfusion);
            if (strengthInfusion != null) bundle.AddInfusion(strengthInfusion);
            
            // Test bundle functionality
            Debug.Log($"🧪 InfusionBundle Test Results:");
            Debug.Log($"  • Bundle contains {bundle.Infusions.Count} infusions");
            Debug.Log($"  • Total power level: {bundle.GetTotalPowerLevel()}");
            Debug.Log($"  • Effect categories: {string.Join(", ", bundle.GetAllEffectCategories())}");
            Debug.Log($"  • Has effects: {bundle.HasEffects()}");
            Debug.Log($"  • Validation: {bundle.IsValid(out string validationMessage)} - {validationMessage}");
            
            // Test individual infusion properties
            if (fireInfusion != null)
            {
                Debug.Log($"🔥 Fire Infusion Test:");
                Debug.Log($"  • Name: {fireInfusion.InfusionName}");
                Debug.Log($"  • Category: {fireInfusion.Category}");
                Debug.Log($"  • Color: {fireInfusion.InfusionColor}");
                Debug.Log($"  • Power Level: {fireInfusion.PowerLevel}");
                Debug.Log($"  • Can Stack: {fireInfusion.CanStack}");
                Debug.Log($"  • Effect Types: {string.Join(", ", fireInfusion.GetEffectTypes().Select(t => t.Name))}");
                Debug.Log($"  • Validation: {fireInfusion.IsValid(out string msg)} - {msg}");
            }
        }
    }
}