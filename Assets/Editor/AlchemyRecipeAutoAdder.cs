#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;

/// <summary>
/// Automatically adds newly created AlchemyRecipe assets to the AlchemyRecipeDatabase
/// </summary>
public class AlchemyRecipeAutoAdder : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool foundNewRecipe = false;
        
        // Check imported assets for new AlchemyRecipe assets
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".asset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<AlchemyRecipe>(assetPath);
                if (asset != null)
                {
                    AddRecipeToDatabase(asset);
                    foundNewRecipe = true;
                }
            }
        }
        
        // Check moved assets for AlchemyRecipe assets
        for (int i = 0; i < movedAssets.Length; i++)
        {
            if (movedAssets[i].EndsWith(".asset"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<AlchemyRecipe>(movedAssets[i]);
                if (asset != null)
                {
                    AddRecipeToDatabase(asset);
                    foundNewRecipe = true;
                }
            }
        }
        
        if (foundNewRecipe)
        {
            // Refresh the database
            AssetDatabase.SaveAssets();
        }
    }
    
    private static void AddRecipeToDatabase(AlchemyRecipe recipe)
    {
        if (recipe == null) return;
        
        // Load the database
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
            Debug.LogWarning($"❌ No AlchemyRecipeDatabase found! Cannot auto-add recipe '{recipe.name}'.");
            return;
        }
        
        // Check if recipe is already in database
        if (database.Recipes.Any(r => r == recipe))
        {
            return; // Already in database, no need to add
        }
        
        // Add the recipe to database
        var serializedDb = new SerializedObject(database);
        var recipesProperty = serializedDb.FindProperty("_recipes");
        
        recipesProperty.arraySize++;
        var newElement = recipesProperty.GetArrayElementAtIndex(recipesProperty.arraySize - 1);
        newElement.objectReferenceValue = recipe;
        
        serializedDb.ApplyModifiedProperties();
        EditorUtility.SetDirty(database);
        
        Debug.Log($"🔄 Auto-added recipe '{recipe.name}' to AlchemyRecipeDatabase!");
    }
}
#endif