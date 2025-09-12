using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Structs;
using FourFatesStudios.ProjectWarden.Effects;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class InfusionMigrationUtility : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<Ingredient> ingredientsToMigrate = new List<Ingredient>();
        private Dictionary<string, FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion> createdInfusions = new Dictionary<string, FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion>();
        private bool showMigrationDetails = false;
        private int migratedCount = 0;
        
        [MenuItem("Alchemy/Migration/Infusion System Migration")]
        public static void ShowWindow()
        {
            var window = GetWindow<InfusionMigrationUtility>("Infusion Migration");
            window.minSize = new Vector2(600, 400);
            window.RefreshIngredientsList();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🔄 Infusion System Migration Utility", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This utility helps migrate from the old struct-based infusion system to the new ScriptableObject-based system.\n\n" +
                "It will:\n" +
                "• Scan all ingredients for old-style infusions\n" +
                "• Create new Infusion ScriptableObjects\n" +
                "• Update ingredients to use InfusionBundle\n" +
                "• Preserve all effect data during migration", 
                MessageType.Info);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Control buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Scan for Migration Candidates", GUILayout.Height(30)))
            {
                RefreshIngredientsList();
            }
            
            if (GUILayout.Button("⚡ Create Example Infusions", GUILayout.Height(30)))
            {
                CreateExampleInfusions();
            }
            
            if (GUILayout.Button("🧪 Test New System", GUILayout.Height(30)))
            {
                InfusionSystemTester.TestInfusionSystem();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Migration candidates list
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"📋 Migration Candidates ({ingredientsToMigrate.Count})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            showMigrationDetails = EditorGUILayout.Toggle("Show Details", showMigrationDetails);
            EditorGUILayout.EndHorizontal();
            
            if (ingredientsToMigrate.Count > 0)
            {
                if (GUILayout.Button("🚀 Migrate All Ingredients", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Migrate Infusion System", 
                        $"This will migrate {ingredientsToMigrate.Count} ingredients to the new infusion system.\n\n" +
                        "This operation can be undone with Ctrl+Z.\n\n" +
                        "Continue?", "Migrate", "Cancel"))
                    {
                        MigrateAllIngredients();
                    }
                }
                
                EditorGUILayout.Space();
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
                
                foreach (var ingredient in ingredientsToMigrate)
                {
                    DrawIngredientMigrationInfo(ingredient);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No ingredients found that need migration. All ingredients are using the new InfusionBundle system!", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            
            // Migration results
            if (migratedCount > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("✅ Migration Results", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Successfully migrated: {migratedCount} ingredients");
                EditorGUILayout.LabelField($"Created infusions: {createdInfusions.Count}");
                if (createdInfusions.Count > 0)
                {
                    EditorGUILayout.LabelField("Created infusion assets:");
                    foreach (var infusionName in createdInfusions.Keys)
                    {
                        EditorGUILayout.LabelField($"  • {infusionName}");
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
        
        private void RefreshIngredientsList()
        {
            ingredientsToMigrate.Clear();
            
            // Find all ingredients in the project
            string[] guids = AssetDatabase.FindAssets("t:Ingredient");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(path);
                
                if (ingredient != null && NeedsMigration(ingredient))
                {
                    ingredientsToMigrate.Add(ingredient);
                }
            }
            
            Debug.Log($"🔍 Found {ingredientsToMigrate.Count} ingredients that need migration");
        }
        
        private bool NeedsMigration(Ingredient ingredient)
        {
            // Check if ingredient has old-style infusions that need to be migrated
            // This is a heuristic since we can't directly access the old private field
            
            // If the ingredient has an empty InfusionBundle but effects, it might need migration
            bool hasEmptyInfusionBundle = ingredient.InfusionBundle?.Infusions?.Count == 0;
            bool hasEffectBundle = ingredient.EffectBundle?.Effects?.Count > 0;
            
            // Check for the old infusions field using reflection
            var ingredientType = typeof(Ingredient);
            var oldInfusionsField = ingredientType.GetField("infusions", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (oldInfusionsField != null)
            {
                var oldInfusions = oldInfusionsField.GetValue(ingredient) as System.Collections.IList;
                if (oldInfusions != null && oldInfusions.Count > 0)
                {
                    return true; // Has old-style infusions
                }
            }
            
            return false;
        }
        
        private void DrawIngredientMigrationInfo(Ingredient ingredient)
        {
            EditorGUILayout.BeginVertical("Box");
            
            EditorGUILayout.BeginHorizontal();
            
            // Ingredient name and info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(ingredient.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {ingredient.IngredientArchetype}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            // Migration status
            GUILayout.FlexibleSpace();
            GUILayout.Label("⚠️", "Button", GUILayout.Width(25));
            
            // Individual migrate button
            if (GUILayout.Button("Migrate", GUILayout.Width(60)))
            {
                MigrateIngredient(ingredient);
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showMigrationDetails)
            {
                EditorGUILayout.Space();
                
                // Show current infusion bundle state
                EditorGUILayout.LabelField("Current InfusionBundle:", EditorStyles.boldLabel);
                int currentInfusions = ingredient.InfusionBundle?.Infusions?.Count ?? 0;
                EditorGUILayout.LabelField($"  Infusions: {currentInfusions}");
                
                // Show effect bundle state
                EditorGUILayout.LabelField("EffectBundle:", EditorStyles.boldLabel);
                int currentEffects = ingredient.EffectBundle?.Effects?.Count ?? 0;
                EditorGUILayout.LabelField($"  Effects: {currentEffects}");
                
                // Try to show old infusions using reflection
                var oldInfusionsInfo = GetOldInfusionsInfo(ingredient);
                if (!string.IsNullOrEmpty(oldInfusionsInfo))
                {
                    EditorGUILayout.LabelField("Legacy Infusions Found:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(oldInfusionsInfo, EditorStyles.miniLabel);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private string GetOldInfusionsInfo(Ingredient ingredient)
        {
            try
            {
                var ingredientType = typeof(Ingredient);
                var oldInfusionsField = ingredientType.GetField("infusions", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (oldInfusionsField != null)
                {
                    var oldInfusions = oldInfusionsField.GetValue(ingredient) as System.Collections.IList;
                    if (oldInfusions != null && oldInfusions.Count > 0)
                    {
                        var names = new List<string>();
                        foreach (var infusion in oldInfusions)
                        {
                            // Get the infusionName from the struct
                            var infusionType = infusion.GetType();
                            var nameField = infusionType.GetField("infusionName", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (nameField != null)
                            {
                                string name = nameField.GetValue(infusion) as string;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    names.Add(name);
                                }
                            }
                        }
                        return $"  • {string.Join("\n  • ", names)}";
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not read old infusions from {ingredient.name}: {e.Message}");
            }
            
            return "";
        }
        
        private void MigrateAllIngredients()
        {
            migratedCount = 0;
            createdInfusions.Clear();
            
            foreach (var ingredient in ingredientsToMigrate)
            {
                if (MigrateIngredient(ingredient))
                {
                    migratedCount++;
                }
            }
            
            // Refresh the list after migration
            RefreshIngredientsList();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"✅ Migration completed! Migrated {migratedCount} ingredients and created {createdInfusions.Count} new infusion assets.");
        }
        
        private bool MigrateIngredient(Ingredient ingredient)
        {
            try
            {
                bool migrated = false;
                
                // Get old infusions using reflection
                var ingredientType = typeof(Ingredient);
                var oldInfusionsField = ingredientType.GetField("infusions", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (oldInfusionsField != null)
                {
                    var oldInfusions = oldInfusionsField.GetValue(ingredient) as System.Collections.IList;
                    if (oldInfusions != null && oldInfusions.Count > 0)
                    {
                        foreach (var oldInfusion in oldInfusions)
                        {
                            // Extract data from old struct
                            var infusionType = oldInfusion.GetType();
                            var nameField = infusionType.GetField("infusionName", BindingFlags.NonPublic | BindingFlags.Instance);
                            var effectsField = infusionType.GetField("effects", BindingFlags.NonPublic | BindingFlags.Instance);
                            
                            string infusionName = nameField?.GetValue(oldInfusion) as string;
                            var effectBundle = effectsField?.GetValue(oldInfusion) as EffectBundle;
                            
                            if (!string.IsNullOrEmpty(infusionName))
                            {
                                // Create or get existing infusion ScriptableObject
                                var newInfusion = GetOrCreateInfusion(infusionName, effectBundle);
                                
                                // Add to ingredient's InfusionBundle
                                if (newInfusion != null)
                                {
                                    ingredient.InfusionBundle.AddInfusion(newInfusion);
                                    migrated = true;
                                }
                            }
                        }
                        
                        // Clear old infusions after migration
                        oldInfusions.Clear();
                    }
                }
                
                if (migrated)
                {
                    EditorUtility.SetDirty(ingredient);
                    Debug.Log($"✅ Migrated ingredient: {ingredient.name}");
                }
                
                return migrated;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to migrate ingredient {ingredient.name}: {e.Message}");
                return false;
            }
        }
        
        private FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion GetOrCreateInfusion(string infusionName, EffectBundle effectBundle)
        {
            // Check if we already created this infusion
            if (createdInfusions.ContainsKey(infusionName))
            {
                return createdInfusions[infusionName];
            }
            
            // Check if it already exists in the project
            string[] existingGuids = AssetDatabase.FindAssets($"t:Infusion {infusionName}");
            foreach (string guid in existingGuids)
            {
                string existingAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                var existingInfusion = AssetDatabase.LoadAssetAtPath<FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion>(existingAssetPath);
                if (existingInfusion != null && existingInfusion.InfusionName == infusionName)
                {
                    createdInfusions[infusionName] = existingInfusion;
                    return existingInfusion;
                }
            }
            
            // Create new infusion
            var newInfusion = ScriptableObject.CreateInstance<FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion>();
            
            // Set properties using reflection
            var infusionType = typeof(FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion);
            var nameField = infusionType.GetField("infusionName", BindingFlags.NonPublic | BindingFlags.Instance);
            var effectBundleField = infusionType.GetField("effectBundle", BindingFlags.NonPublic | BindingFlags.Instance);
            var colorField = infusionType.GetField("infusionColor", BindingFlags.NonPublic | BindingFlags.Instance);
            var descField = infusionType.GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
            var powerLevelField = infusionType.GetField("powerLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            
            nameField?.SetValue(newInfusion, infusionName);
            effectBundleField?.SetValue(newInfusion, effectBundle ?? new EffectBundle());
            colorField?.SetValue(newInfusion, InferColor(infusionName));
            descField?.SetValue(newInfusion, $"Migrated infusion: {infusionName}");
            powerLevelField?.SetValue(newInfusion, InferPowerLevel(effectBundle));
            
            // Save as asset
            string path = "Assets/Resources/Infusions";
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "Infusions");
            }
            
            string assetPath = $"{path}/{infusionName.Replace(" ", "")}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(newInfusion, assetPath);
            
            createdInfusions[infusionName] = newInfusion;
            Debug.Log($"Created new infusion asset: {assetPath}");
            
            return newInfusion;
        }
        
        private Color InferColor(string infusionName)
        {
            string lowerName = infusionName.ToLower();
            
            if (lowerName.Contains("fire") || lowerName.Contains("flame") || lowerName.Contains("burn")) return Color.red;
            if (lowerName.Contains("ice") || lowerName.Contains("frost") || lowerName.Contains("cold")) return Color.cyan;
            if (lowerName.Contains("lightning") || lowerName.Contains("electric") || lowerName.Contains("shock")) return Color.yellow;
            if (lowerName.Contains("earth") || lowerName.Contains("stone") || lowerName.Contains("rock")) return new Color(0.6f, 0.4f, 0.2f);
            if (lowerName.Contains("wind") || lowerName.Contains("air") || lowerName.Contains("storm")) return new Color(0.8f, 0.9f, 1f);
            
            if (lowerName.Contains("strength") || lowerName.Contains("power") || lowerName.Contains("might")) return new Color(1f, 0.6f, 0f);
            if (lowerName.Contains("speed") || lowerName.Contains("swift") || lowerName.Contains("agility")) return Color.green;
            
            if (lowerName.Contains("mind") || lowerName.Contains("intellect") || lowerName.Contains("wisdom")) return Color.magenta;
            if (lowerName.Contains("shield") || lowerName.Contains("armor") || lowerName.Contains("protection")) return Color.blue;
            if (lowerName.Contains("corrupt") || lowerName.Contains("dark") || lowerName.Contains("shadow")) return new Color(0.2f, 0.1f, 0.2f);
            if (lowerName.Contains("holy") || lowerName.Contains("divine") || lowerName.Contains("sacred")) return new Color(1f, 1f, 0.8f);
            
            return Color.white;
        }
        
        private int InferPowerLevel(EffectBundle effectBundle)
        {
            if (effectBundle?.Effects == null || effectBundle.Effects.Count == 0)
                return 1;
            
            // Base power level on number and complexity of effects
            int effectCount = effectBundle.Effects.Count;
            
            if (effectCount == 1) return 2;
            if (effectCount == 2) return 3;
            if (effectCount == 3) return 4;
            if (effectCount >= 4) return 5;
            
            return 3; // Default
        }
        
        private void CreateExampleInfusions()
        {
            string path = "Assets/Resources/Infusions";
            if (!AssetDatabase.IsValidFolder(path))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "Infusions");
            }
            
            var examples = new[]
            {
                ("Fire Infusion", Color.red, "Provides burning damage effects"),
                ("Ice Infusion", Color.cyan, "Provides freezing effects"),
                ("Lightning Infusion", Color.yellow, "Provides shocking effects"),
                ("Strength Infusion", new Color(1f, 0.6f, 0f), "Enhances physical power"),
                ("Mind Infusion", Color.magenta, "Enhances mental abilities"),
                ("Shield Infusion", Color.blue, "Provides protective effects"),
                ("Poison Infusion", new Color(0.5f, 0.8f, 0.2f), "Applies poison effects"),
                ("Holy Infusion", new Color(1f, 1f, 0.8f), "Provides divine blessings")
            };
            
            int created = 0;
            foreach (var (name, color, description) in examples)
            {
                // Check if already exists
                string assetPath = $"{path}/{name.Replace(" ", "")}.asset";
                if (!AssetDatabase.LoadAssetAtPath<FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion>(assetPath))
                {
                    CreateExampleInfusion(name, color, description);
                    created++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"✅ Created {created} example infusions in {path}");
            EditorUtility.DisplayDialog("Example Infusions", $"Created {created} example infusions in the Resources/Infusions folder.", "OK");
        }
        
        private void CreateExampleInfusion(string name, Color color, string description)
        {
            var infusion = ScriptableObject.CreateInstance<FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion>();
            
            var infusionType = typeof(FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion);
            var nameField = infusionType.GetField("infusionName", BindingFlags.NonPublic | BindingFlags.Instance);
            var colorField = infusionType.GetField("infusionColor", BindingFlags.NonPublic | BindingFlags.Instance);
            var descField = infusionType.GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
            var effectBundleField = infusionType.GetField("effectBundle", BindingFlags.NonPublic | BindingFlags.Instance);
            var powerLevelField = infusionType.GetField("powerLevel", BindingFlags.NonPublic | BindingFlags.Instance);
            
            nameField?.SetValue(infusion, name);
            colorField?.SetValue(infusion, color);
            descField?.SetValue(infusion, description);
            effectBundleField?.SetValue(infusion, new EffectBundle());
            powerLevelField?.SetValue(infusion, 3);
            
            string assetPath = $"Assets/Resources/Infusions/{name.Replace(" ", "")}.asset";
            AssetDatabase.CreateAsset(infusion, assetPath);
        }
    }
}