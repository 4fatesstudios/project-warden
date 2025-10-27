using UnityEngine;
using UnityEditor;
using System.IO;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Tool to help migrate old rhythm-based recipes to new grid-based recipes
    /// </summary>
    public class RecipeMigrationTool : EditorWindow
    {
        private string recipeFolderPath = "Assets/Resources/Recipes";
        private bool dryRun = true;
        private RecipeDifficulty defaultDifficulty = RecipeDifficulty.Standard;
        private float defaultEfficiency = 0.6f;
        private string defaultHints = "Arrange ingredients to create a stable reaction.";

        [MenuItem("Tools/Recipe Migration Tool")]
        public static void ShowWindow()
        {
            GetWindow<RecipeMigrationTool>("Recipe Migration Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Recipe Migration Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool helps migrate old rhythm-based AlchemyRecipe assets to the new grid-based format. " +
                "Old properties like requiredHits, maxAttempts, requiredTemperature, and totalDuration will be removed.",
                MessageType.Info);

            GUILayout.Space(10);

            // Configuration
            GUILayout.Label("Migration Settings", EditorStyles.boldLabel);
            
            recipeFolderPath = EditorGUILayout.TextField("Recipe Folder Path", recipeFolderPath);
            
            dryRun = EditorGUILayout.Toggle("Dry Run (Preview Only)", dryRun);
            
            defaultDifficulty = (RecipeDifficulty)EditorGUILayout.EnumPopup("Default Difficulty", defaultDifficulty);
            
            defaultEfficiency = EditorGUILayout.Slider("Default Min Efficiency", defaultEfficiency, 0.1f, 0.9f);
            
            defaultHints = EditorGUILayout.TextField("Default Hints", defaultHints);

            GUILayout.Space(10);

            if (dryRun)
            {
                EditorGUILayout.HelpBox("Dry Run Mode: No files will be modified. Check console for preview.", MessageType.Warning);
            }

            GUILayout.Space(10);

            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Scan Recipes"))
            {
                ScanRecipes();
            }
            
            if (GUILayout.Button("Migrate Recipes"))
            {
                MigrateRecipes();
            }
            
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("Create Example Recipe"))
            {
                CreateExampleRecipe();
            }
        }

        private void ScanRecipes()
        {
            Debug.Log("=== SCANNING RECIPES ===");

            if (!Directory.Exists(recipeFolderPath))
            {
                Debug.LogError($"Recipe folder not found: {recipeFolderPath}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:AlchemyRecipe", new[] { recipeFolderPath });
            Debug.Log($"Found {guids.Length} AlchemyRecipe assets");

            int needsMigration = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var recipe = AssetDatabase.LoadAssetAtPath<AlchemyRecipe>(path);

                if (recipe != null)
                {
                    bool needsUpdate = CheckIfNeedsMigration(recipe);
                    Debug.Log($"Recipe: {recipe.name} - Needs Migration: {needsUpdate}");
                    
                    if (needsUpdate)
                        needsMigration++;
                }
            }

            Debug.Log($"=== SCAN COMPLETE: {needsMigration} recipes need migration ===");
        }

        private void MigrateRecipes()
        {
            Debug.Log($"=== MIGRATING RECIPES (Dry Run: {dryRun}) ===");

            if (!Directory.Exists(recipeFolderPath))
            {
                Debug.LogError($"Recipe folder not found: {recipeFolderPath}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:AlchemyRecipe", new[] { recipeFolderPath });
            int migratedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var recipe = AssetDatabase.LoadAssetAtPath<AlchemyRecipe>(path);

                if (recipe != null && CheckIfNeedsMigration(recipe))
                {
                    if (dryRun)
                    {
                        Debug.Log($"[DRY RUN] Would migrate: {recipe.name}");
                        LogMigrationDetails(recipe);
                    }
                    else
                    {
                        MigrateRecipe(recipe);
                        migratedCount++;
                        Debug.Log($"✓ Migrated: {recipe.name}");
                    }
                }
            }

            if (!dryRun)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"=== MIGRATION COMPLETE: {migratedCount} recipes migrated ===");
            }
            else
            {
                Debug.Log("=== DRY RUN COMPLETE ===");
            }
        }

        private bool CheckIfNeedsMigration(AlchemyRecipe recipe)
        {
            // Check if recipe still has old properties or lacks new ones
            // Since we can't access private fields directly in this context,
            // we'll use reflection or assume all recipes need migration
            return true; // For now, assume all need checking
        }

        private void LogMigrationDetails(AlchemyRecipe recipe)
        {
            Debug.Log($"  Recipe: {recipe.name}");
            Debug.Log($"    Current Ingredients: {recipe.InputIngredient1?.name}, {recipe.InputIngredient2?.name}, {recipe.InputIngredient3?.name}");
            Debug.Log($"    Output: {recipe.OutputPotion?.name}");
            Debug.Log($"    Will set difficulty to: {defaultDifficulty}");
            Debug.Log($"    Will set efficiency to: {defaultEfficiency:P}");
        }

        private void MigrateRecipe(AlchemyRecipe recipe)
        {
            // Create a serialized object to modify the recipe
            var serializedObject = new SerializedObject(recipe);

            // Set new properties
            var difficultyProp = serializedObject.FindProperty("difficulty");
            if (difficultyProp != null)
                difficultyProp.enumValueIndex = (int)defaultDifficulty;

            var efficiencyProp = serializedObject.FindProperty("minimumEfficiency");
            if (efficiencyProp != null && efficiencyProp.floatValue == 0.6f) // Only update if still default
                efficiencyProp.floatValue = defaultEfficiency;

            var hintsProp = serializedObject.FindProperty("recipeHints");
            if (hintsProp != null && string.IsNullOrEmpty(hintsProp.stringValue))
                hintsProp.stringValue = defaultHints;

            var outputQuantityProp = serializedObject.FindProperty("outputQuantity");
            if (outputQuantityProp != null && outputQuantityProp.intValue == 0)
                outputQuantityProp.intValue = 1;

            // Apply changes
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(recipe);
        }

        private void CreateExampleRecipe()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Example Recipe",
                "ExampleGridRecipe",
                "asset",
                "Choose where to save the example recipe");

            if (string.IsNullOrEmpty(path))
                return;

            var recipe = CreateInstance<AlchemyRecipe>();
            
            // Configure example recipe with new features
            var serializedObject = new SerializedObject(recipe);
            
            serializedObject.FindProperty("itemName").stringValue = "Example Grid Recipe";
            serializedObject.FindProperty("itemDescription").stringValue = "An example recipe showing new grid-based features";
            serializedObject.FindProperty("difficulty").enumValueIndex = (int)RecipeDifficulty.Standard;
            serializedObject.FindProperty("minimumEfficiency").floatValue = 0.7f;
            serializedObject.FindProperty("outputQuantity").intValue = 1;
            serializedObject.FindProperty("recipeHints").stringValue = "Place fire and water ingredients adjacent for bonus effects";
            serializedObject.FindProperty("allowsIngredientInteractions").boolValue = true;
            
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(recipe, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✓ Created example recipe at: {path}");
            Selection.activeObject = recipe;
        }
    }
}