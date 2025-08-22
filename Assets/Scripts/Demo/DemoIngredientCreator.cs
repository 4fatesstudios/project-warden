using System.Reflection;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEditor;
using UnityEngine;

namespace Demo
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
                           IngredientArchetype.Organic, Aspect.Scorch, 2, 1, 1);
            
            CreateIngredient("Fire Talon", "A smaller talon with weak fire magic", Rarity.Common, 
                           IngredientArchetype.Organic, Aspect.Scorch, 1, 1, 1);
            
            CreateIngredient("Frozen Dew", "Pure magical frigid essence", Rarity.Common, 
                           IngredientArchetype.Solvent, Aspect.Frigid, 1, 1, 1);
            
            CreateIngredient("Earth Shard", "A crystallized piece of earth", Rarity.Uncommon, 
                           IngredientArchetype.Ore, Aspect.Corporeal, 3, 2, 1);
            
            CreateIngredient("Wind Essence", "Captured essence of Arc", Rarity.Rare, 
                           IngredientArchetype.Synthetic, Aspect.Arc, 4, 1, 2);
            
            CreateIngredient("Shadow Herb", "A mysterious herb that grows in darkness", Rarity.Epic, 
                           IngredientArchetype.Herb, Aspect.Divine, 5, 2, 2);
                           
            Debug.Log("Demo ingredients created! Check Resources/Demo/Ingredients folder");
#endif
        }

#if UNITY_EDITOR
        private void CreateIngredient(string ingName, string description, Rarity rarity, 
                                    IngredientArchetype archetype, Aspect aspect, 
                                    int potency, int gridWidth, int gridHeight)
        {
            var ingredient = ScriptableObject.CreateInstance<Ingredient>();
            
            // Use reflection to set private fields since we don't have public setters
            var itemNameField = typeof(Item).GetField("itemName", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemDescField = typeof(Item).GetField("itemDescription", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemRarityField = typeof(Item).GetField("itemRarity", BindingFlags.NonPublic | BindingFlags.Instance);
            
            var archetypeField = typeof(Ingredient).GetField("ingredientArchetype", BindingFlags.NonPublic | BindingFlags.Instance);
            var aspectField = typeof(Ingredient).GetField("ingredientAspect", BindingFlags.NonPublic | BindingFlags.Instance);
            var potencyField = typeof(Ingredient).GetField("potency", BindingFlags.NonPublic | BindingFlags.Instance);
            var gridWidthField = typeof(Ingredient).GetField("gridWidth", BindingFlags.NonPublic | BindingFlags.Instance);
            var gridHeightField = typeof(Ingredient).GetField("gridHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Set base item properties
            itemNameField?.SetValue(ingredient, ingName);
            itemDescField?.SetValue(ingredient, description);
            itemRarityField?.SetValue(ingredient, rarity);
            
            // Set ingredient-specific properties
            archetypeField?.SetValue(ingredient, archetype);
            aspectField?.SetValue(ingredient, aspect);
            potencyField?.SetValue(ingredient, potency);
            gridWidthField?.SetValue(ingredient, gridWidth);
            gridHeightField?.SetValue(ingredient, gridHeight);
            
            // Create directory if it doesn't exist
            string folderPath = "Assets/Resources/Items/Ingredients/Demo";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }
            
            // Save as asset
            string assetPath = $"{folderPath}/{ingName.Replace(" ", "")}.asset";
            AssetDatabase.CreateAsset(ingredient, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created ingredient: {ingName} at {assetPath}");
        }
#endif
    }
}