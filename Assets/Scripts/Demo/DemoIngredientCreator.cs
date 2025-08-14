using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.Demo
{
    /// <summary>
    /// Helper script to create demo ingredients for testing the crafting system
    /// </summary>
    public class DemoIngredientCreator : MonoBehaviour
    {
        [ContextMenu("Create Demo Ingredients")]
        public void CreateDemoIngredients()
        {
#if UNITY_EDITOR
            // Create demo ingredients for testing
            CreateIngredient("Fire Claw", "A sharp claw infused with fire magic", Rarity.Common, 
                           IngredientArchetype.AnimalPart, Aspect.Fire, 2, 1, 1);
            
            CreateIngredient("Fire Talon", "A smaller talon with weak fire magic", Rarity.Common, 
                           IngredientArchetype.AnimalPart, Aspect.Fire, 1, 1, 1);
            
            CreateIngredient("Water Droplet", "Pure magical water essence", Rarity.Common, 
                           IngredientArchetype.Liquid, Aspect.Water, 1, 1, 1);
            
            CreateIngredient("Earth Shard", "A crystallized piece of earth magic", Rarity.Uncommon, 
                           IngredientArchetype.Crystal, Aspect.Earth, 3, 2, 1);
            
            CreateIngredient("Wind Essence", "Captured essence of the wind", Rarity.Rare, 
                           IngredientArchetype.Essence, Aspect.Air, 4, 1, 2);
            
            CreateIngredient("Shadow Herb", "A mysterious herb that grows in darkness", Rarity.Epic, 
                           IngredientArchetype.Herb, Aspect.Dark, 5, 2, 2);
                           
            Debug.Log("Demo ingredients created! Check Resources/Demo/Ingredients folder");
#endif
        }

#if UNITY_EDITOR
        private void CreateIngredient(string name, string description, Rarity rarity, 
                                    IngredientArchetype archetype, Aspect aspect, 
                                    int potency, int gridWidth, int gridHeight)
        {
            var ingredient = ScriptableObject.CreateInstance<Ingredient>();
            
            // Use reflection to set private fields since we don't have public setters
            var itemNameField = typeof(Item).GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var itemDescField = typeof(Item).GetField("itemDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var itemRarityField = typeof(Item).GetField("itemRarity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var archetypeField = typeof(Ingredient).GetField("ingredientArchetype", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var aspectField = typeof(Ingredient).GetField("ingredientAspect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var potencyField = typeof(Ingredient).GetField("potency", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gridWidthField = typeof(Ingredient).GetField("gridWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gridHeightField = typeof(Ingredient).GetField("gridHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Set base item properties
            itemNameField?.SetValue(ingredient, name);
            itemDescField?.SetValue(ingredient, description);
            itemRarityField?.SetValue(ingredient, rarity);
            
            // Set ingredient-specific properties
            archetypeField?.SetValue(ingredient, archetype);
            aspectField?.SetValue(ingredient, aspect);
            potencyField?.SetValue(ingredient, potency);
            gridWidthField?.SetValue(ingredient, gridWidth);
            gridHeightField?.SetValue(ingredient, gridHeight);
            
            // Create directory if it doesn't exist
            string folderPath = "Assets/Resources/Demo/Ingredients";
            if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
            {
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!UnityEditor.AssetDatabase.IsValidFolder(newPath))
                    {
                        UnityEditor.AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            // Save as asset
            string assetPath = $"{folderPath}/{name.Replace(" ", "")}.asset";
            UnityEditor.AssetDatabase.CreateAsset(ingredient, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"Created ingredient: {name} at {assetPath}");
        }
#endif
    }
}