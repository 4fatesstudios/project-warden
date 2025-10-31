#if UNITY_EDITOR
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AlchemyRecipe))]
public class AlchemyRecipeEditor : UnityEditor.Editor // Fixed namespace conflict
{
    [MenuItem("Tools/Alchemy/Auto-Add All Recipes to Database")]
    public static void AutoAddAllRecipesToDatabase()
    {
        var database = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
        if (database == null)
        {
            // Try to find database anywhere in the project
            var guids = AssetDatabase.FindAssets("t:AlchemyRecipeDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                database = AssetDatabase.LoadAssetAtPath<AlchemyRecipeDatabase>(path);
            }
        }
        
        if (database == null)
        {
            Debug.LogError("❌ No AlchemyRecipeDatabase found! Please create one first.");
            return;
        }
        
        // Find all AlchemyRecipe assets
        var recipeGuids = AssetDatabase.FindAssets("t:AlchemyRecipe");
        int addedCount = 0;
        
        var serializedDb = new SerializedObject(database);
        var recipesProperty = serializedDb.FindProperty("_recipes");
        
        foreach (var guid in recipeGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var recipe = AssetDatabase.LoadAssetAtPath<AlchemyRecipe>(path);
            
            if (recipe != null && !database.Recipes.Any(r => r == recipe))
            {
                recipesProperty.arraySize++;
                var newElement = recipesProperty.GetArrayElementAtIndex(recipesProperty.arraySize - 1);
                newElement.objectReferenceValue = recipe;
                addedCount++;
            }
        }
        
        serializedDb.ApplyModifiedProperties();
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"🔄 Auto-added {addedCount} recipes to the database. Total recipes: {database.Recipes.Count}");
    }
    
    [MenuItem("Tools/Alchemy/Show Database Status")]
    public static void ShowDatabaseStatus()
    {
        var database = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
        if (database == null)
        {
            var guids = AssetDatabase.FindAssets("t:AlchemyRecipeDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                database = AssetDatabase.LoadAssetAtPath<AlchemyRecipeDatabase>(path);
            }
        }
        
        if (database == null)
        {
            Debug.LogError("❌ No AlchemyRecipeDatabase found!");
            return;
        }
        
        Debug.Log($"📊 AlchemyRecipeDatabase Status:");
        Debug.Log($"   Database location: {AssetDatabase.GetAssetPath(database)}");
        Debug.Log($"   Total recipes: {database.Recipes.Count}");
        
        foreach (var recipe in database.Recipes)
        {
            if (recipe != null)
            {
                Debug.Log($"   ✅ {recipe.name}");
            }
            else
            {
                Debug.Log($"   ❌ [NULL RECIPE ENTRY]");
            }
        }
    }
    // cached styles
    private GUIStyle _warningStyle;
    private GUIStyle _keyStyle;
    private GUIStyle _successStyle;
    
    private AlchemyRecipeDatabase _database;
    private bool _isDatabaseLoaded = false;

    private void OnEnable()
    {
        _warningStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { textColor = Color.yellow },
            fontStyle = FontStyle.Italic
        };

        _keyStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { textColor = Color.cyan },
            alignment = TextAnchor.MiddleCenter
        };
        
        _successStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { textColor = Color.green },
            fontStyle = FontStyle.Bold
        };
        
        // Load the database
        LoadDatabase();
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector but cache changes
        serializedObject.Update();

        // Ingredients 
        EditorGUILayout.LabelField("Input Ingredients", EditorStyles.boldLabel);
        DrawProperty("inputIngredient1", "Ingredient 1");
        DrawProperty("inputIngredient2", "Ingredient 2");
        DrawProperty("inputIngredient3", "Ingredient 3");

        // Rhythm Data 
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Rhythm Requirements", EditorStyles.boldLabel);
        DrawHitsAndAttempts();

        // Output Item
        EditorGUILayout.Space(4);
        
        var outputItemProp = serializedObject.FindProperty("outputItem");
        if (outputItemProp != null)
        {
            var currentItem = outputItemProp.objectReferenceValue as Item;
            var newItem = DrawPotionOrIngredientField("Output Item", currentItem);
            
            if (newItem != currentItem)
            {
                outputItemProp.objectReferenceValue = newItem;
            }
        }
        else
        {
            // Fallback for old recipes still using outputPotion
            DrawProperty("outputPotion", "Output Potion");
        }

        // Temperature and Duration 
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Process Thresholds", EditorStyles.boldLabel);
        DrawProperty("requiredTemperature", "Required Temperature (s)");
        DrawProperty("totalDuration", "Total Duration (s)");

        // Utilities
        EditorGUILayout.Space(6);
        if (GUILayout.Button("Auto-Sort Ingredients"))
            SortIngredientsAlphabetically();

        ShowRecipeKeyPreview();
        ShowMissingWarnings();
        
        // Database Management
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Database Management", EditorStyles.boldLabel);
        ShowCurrentRecipeStatus();
        DrawDatabaseButtons();

        // commit changes
        serializedObject.ApplyModifiedProperties();
    }

    // Helper to draw a property with optional custom label

    private void DrawProperty(string propName, string label = null)
    {
        var prop = serializedObject.FindProperty(propName);
        if (prop == null) return;
        EditorGUILayout.PropertyField(prop, new GUIContent(label ?? prop.displayName));
    }

    private int selectedOutputType = 0; // 0 = Potion, 1 = Synthetic Ingredient
    
    private Item DrawPotionOrIngredientField(string label, Item currentValue)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        
        // Update selected type based on current value only if not null
        if (currentValue != null)
        {
            selectedOutputType = currentValue is Potion ? 0 : 1;
        }
        
        // Type selection toolbar
        int newType = GUILayout.Toolbar(selectedOutputType, new string[] { "Potion", "Synthetic" }, GUILayout.Width(150));
        
        // Update if changed
        if (newType != selectedOutputType)
        {
            selectedOutputType = newType;
            currentValue = null; // Clear when switching types
        }
        
        Item newValue = currentValue;
        
        // Show appropriate object field
        if (selectedOutputType == 0)
        {
            // Potion picker
            newValue = EditorGUILayout.ObjectField(newValue as Potion, typeof(Potion), false) as Item;
        }
        else
        {
            // Ingredient picker with Synthetic validation
            var currentIngredient = newValue as Ingredient;
            var selectedIngredient = EditorGUILayout.ObjectField(currentIngredient, typeof(Ingredient), false) as Ingredient;
            
            // Validate that it's a Synthetic ingredient
            if (selectedIngredient != null && selectedIngredient.IngredientArchetype != IngredientArchetype.Synthetic)
            {
                EditorUtility.DisplayDialog(
                    "Invalid Ingredient Type",
                    $"Only Synthetic ingredients can be used as recipe outputs.\n\n" +
                    $"Selected: {selectedIngredient.ItemName} ({selectedIngredient.IngredientArchetype})\n\n" +
                    $"💡 Tip: Use the search bar in the picker and type 'Synthetic' or the ingredient name.",
                    "OK"
                );
                selectedIngredient = currentIngredient; // Revert
            }
            
            newValue = selectedIngredient;
        }
        
        EditorGUILayout.EndHorizontal();
        
        return newValue;
    }

    private void DrawHitsAndAttempts()
    {
        // AlchemyRecipe uses different properties than the old system
        var difficultyProp = serializedObject.FindProperty("difficulty");
        var efficiencyProp = serializedObject.FindProperty("minimumEfficiency");
        
        if (difficultyProp != null)
            EditorGUILayout.PropertyField(difficultyProp, new GUIContent("Recipe Difficulty"));
        else
            EditorGUILayout.HelpBox("Difficulty property not found. This may be an old recipe format.", MessageType.Warning);
        
        if (efficiencyProp != null)
            EditorGUILayout.PropertyField(efficiencyProp, new GUIContent("Minimum Efficiency"));
        else
            EditorGUILayout.HelpBox("Minimum Efficiency property not found. This may be an old recipe format.", MessageType.Warning);

        var isKeyRecipeProp = serializedObject.FindProperty("isKeyRecipe");
        if (isKeyRecipeProp != null)
            EditorGUILayout.PropertyField(isKeyRecipeProp, new GUIContent("Is Key Recipe"));
    }

    private void SortIngredientsAlphabetically()
    {
        var ing1 = serializedObject.FindProperty("inputIngredient1");
        var ing2 = serializedObject.FindProperty("inputIngredient2");
        var ing3 = serializedObject.FindProperty("inputIngredient3");

        var list = new[] { ing1, ing2, ing3 }
                   .Select(p => p.objectReferenceValue)
                   .OfType<UnityEngine.Object>()
                   .OrderBy(o => o.name)
                   .ToArray();

        if (list.Length == 0) return;

        ing1.objectReferenceValue = list.Length > 0 ? list[0] : null;
        ing2.objectReferenceValue = list.Length > 1 ? list[1] : null;
        ing3.objectReferenceValue = list.Length > 2 ? list[2] : null;
    }

    private void ShowRecipeKeyPreview()
    {
        var ingNames = new[]
        {
            serializedObject.FindProperty("inputIngredient1").objectReferenceValue,
            serializedObject.FindProperty("inputIngredient2").objectReferenceValue,
            serializedObject.FindProperty("inputIngredient3").objectReferenceValue
        }
        .OfType<UnityEngine.Object>()
        .OrderBy(o => o.name)
        .Select(o => o.name)
        .ToArray();

        if (ingNames.Length >= 2)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Recipe Key: {string.Join(", ", ingNames)}", _keyStyle);
        }
    }

    private void ShowMissingWarnings()
    {
        var potion = serializedObject.FindProperty("outputPotion").objectReferenceValue;
        if (potion == null)
            EditorGUILayout.LabelField("Output potion is not assigned.", _warningStyle);

        var ing1 = serializedObject.FindProperty("inputIngredient1").objectReferenceValue;
        var ing2 = serializedObject.FindProperty("inputIngredient2").objectReferenceValue;
        if (ing1 == null || ing2 == null)
            EditorGUILayout.LabelField("At least two ingredients are required.", _warningStyle);
    }
    
    #region Database Management
    
    private void LoadDatabase()
    {
        _database = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
        _isDatabaseLoaded = _database != null;
        
        if (!_isDatabaseLoaded)
        {
            // Try alternative paths
            var guids = AssetDatabase.FindAssets("t:AlchemyRecipeDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _database = AssetDatabase.LoadAssetAtPath<AlchemyRecipeDatabase>(path);
                _isDatabaseLoaded = _database != null;
            }
        }
    }
    
    private void ShowCurrentRecipeStatus()
    {
        if (!_isDatabaseLoaded)
        {
            EditorGUILayout.HelpBox("❌ AlchemyRecipeDatabase not found! Please create one in Resources/Databases/", MessageType.Error);
            if (GUILayout.Button("Create Database"))
            {
                CreateDatabase();
            }
            return;
        }
        
        var currentRecipe = target as AlchemyRecipe;
        if (currentRecipe == null) return;
        
        bool isInDatabase = IsRecipeInDatabase(currentRecipe);
        
        if (isInDatabase)
        {
            EditorGUILayout.LabelField("✅ This recipe is in the database", _successStyle);
        }
        else
        {
            EditorGUILayout.LabelField("⚠️ This recipe is NOT in the database", _warningStyle);
        }
        
        EditorGUILayout.LabelField($"Database contains {_database.Recipes.Count} recipes");
    }
    
    private void DrawDatabaseButtons()
    {
        if (!_isDatabaseLoaded) return;
        
        var currentRecipe = target as AlchemyRecipe;
        if (currentRecipe == null) return;
        
        bool isInDatabase = IsRecipeInDatabase(currentRecipe);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = !isInDatabase;
        if (GUILayout.Button("Add to Database"))
        {
            AddRecipeToDatabase(currentRecipe);
        }
        
        GUI.enabled = isInDatabase;
        if (GUILayout.Button("Remove from Database"))
        {
            RemoveRecipeFromDatabase(currentRecipe);
        }
        
        GUI.enabled = true;
        if (GUILayout.Button("Refresh Database"))
        {
            LoadDatabase();
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Auto-Add All Recipes in Project"))
        {
            AutoAddAllRecipes();
        }
    }
    
    private bool IsRecipeInDatabase(AlchemyRecipe recipe)
    {
        if (!_isDatabaseLoaded || recipe == null) return false;
        return _database.Recipes.Any(r => r == recipe);
    }
    
    private void AddRecipeToDatabase(AlchemyRecipe recipe)
    {
        if (!_isDatabaseLoaded || recipe == null) return;
        
        if (IsRecipeInDatabase(recipe))
        {
            Debug.LogWarning($"Recipe '{recipe.name}' is already in the database!");
            return;
        }
        
        // Access the private _recipes field through serialization
        var serializedDb = new SerializedObject(_database);
        var recipesProperty = serializedDb.FindProperty("_recipes");
        
        // Add the new recipe
        recipesProperty.arraySize++;
        var newElement = recipesProperty.GetArrayElementAtIndex(recipesProperty.arraySize - 1);
        newElement.objectReferenceValue = recipe;
        
        // Apply changes
        serializedDb.ApplyModifiedProperties();
        
        // Force the database to rebuild its lookup
        _database.GetType().GetMethod("BuildLookup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(_database, null);
        
        EditorUtility.SetDirty(_database);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"✅ Added recipe '{recipe.name}' to the database!");
    }
    
    private void RemoveRecipeFromDatabase(AlchemyRecipe recipe)
    {
        if (!_isDatabaseLoaded || recipe == null) return;
        
        var serializedDb = new SerializedObject(_database);
        var recipesProperty = serializedDb.FindProperty("_recipes");
        
        // Find and remove the recipe
        for (int i = 0; i < recipesProperty.arraySize; i++)
        {
            var element = recipesProperty.GetArrayElementAtIndex(i);
            if (element.objectReferenceValue == recipe)
            {
                recipesProperty.DeleteArrayElementAtIndex(i);
                break;
            }
        }
        
        serializedDb.ApplyModifiedProperties();
        EditorUtility.SetDirty(_database);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"❌ Removed recipe '{recipe.name}' from the database!");
    }
    
    private void AutoAddAllRecipes()
    {
        if (!_isDatabaseLoaded) return;
        
        // Find all AlchemyRecipe assets in the project
        var guids = AssetDatabase.FindAssets("t:AlchemyRecipe");
        int addedCount = 0;
        
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var recipe = AssetDatabase.LoadAssetAtPath<AlchemyRecipe>(path);
            
            if (recipe != null && !IsRecipeInDatabase(recipe))
            {
                AddRecipeToDatabase(recipe);
                addedCount++;
            }
        }
        
        Debug.Log($"🔄 Auto-added {addedCount} recipes to the database. Total recipes: {_database.Recipes.Count}");
    }
    
    private void CreateDatabase()
    {
        // Create the database asset
        var database = ScriptableObject.CreateInstance<AlchemyRecipeDatabase>();
        
        // Ensure the directory exists
        string folderPath = "Assets/Resources/Databases";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Resources", "Databases");
        }
        
        string assetPath = "Assets/Resources/Databases/AlchemyRecipeDatabase.asset";
        AssetDatabase.CreateAsset(database, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        LoadDatabase();
        
        Debug.Log($"✅ Created AlchemyRecipeDatabase at {assetPath}");
    }
    
    #endregion
}
#endif
