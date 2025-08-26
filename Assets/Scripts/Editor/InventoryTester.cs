using UnityEngine;
using UnityEditor;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Tool to add test ingredients to the inventory for crafting system testing
    /// </summary>
    public class InventoryTester : EditorWindow
    {
        [MenuItem("Tools/Add Test Ingredients")]
        public static void ShowWindow()
        {
            GetWindow<InventoryTester>("Inventory Tester");
        }

        private void OnGUI()
        {
            GUILayout.Label("Inventory Testing Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to add test ingredients to inventory", MessageType.Info);
                return;
            }

            var inventoryHolder = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
            
            if (inventoryHolder == null)
            {
                EditorGUILayout.HelpBox("❌ No ItemSlotContainerHolder found!\nUse Tools → Connect Crafting System Components to create one.", MessageType.Error);
                return;
            }

            EditorGUILayout.HelpBox($"✅ Found inventory: {inventoryHolder.gameObject.name}", MessageType.None);

            // Show current inventory contents
            if (inventoryHolder.Container != null)
            {
                var allItems = inventoryHolder.Container.GetAllItems();
                EditorGUILayout.LabelField($"Current inventory items: {allItems.ToList().Count}");
                
                foreach (var item in allItems)
                {
                    var quantity = inventoryHolder.Container.GetItemCount(item);
                    EditorGUILayout.LabelField($"  • {item.ItemName}: {quantity}");
                }
            }

            GUILayout.Space(10);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🧪 Add Basic Test Ingredients", GUILayout.Height(40)))
            {
                AddBasicTestIngredients(inventoryHolder);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(5);
            
            if (GUILayout.Button("🌿 Add More Herb Ingredients"))
            {
                AddHerbIngredients(inventoryHolder);
            }

            if (GUILayout.Button("💧 Add Solvent Ingredients"))
            {
                AddSolventIngredients(inventoryHolder);
            }

            if (GUILayout.Button("🧹 Clear All Inventory"))
            {
                ClearInventory(inventoryHolder);
            }

            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("💡 Tip: Solvents are required for potion crafting. Without solvents, you'll only create components!", MessageType.Info);
        }

        private void AddBasicTestIngredients(ItemSlotContainerHolder inventory)
        {
            // Create test ingredients programmatically - using Herb as fallback
            var herb1 = CreateTestIngredient("Healing Herb", FourFatesStudios.ProjectWarden.Enums.IngredientArchetype.Herb);
            var herb2 = CreateTestIngredient("Mana Flower", FourFatesStudios.ProjectWarden.Enums.IngredientArchetype.Herb);
            var herb3 = CreateTestIngredient("Strength Root", FourFatesStudios.ProjectWarden.Enums.IngredientArchetype.Herb);
            var solvent = CreateTestIngredient("Pure Water", FourFatesStudios.ProjectWarden.Enums.IngredientArchetype.Solvent);

            inventory.AddItem(herb1, 5);
            inventory.AddItem(herb2, 5);
            inventory.AddItem(herb3, 5);
            inventory.AddItem(solvent, 10);

            Debug.Log("✅ Added basic test ingredients to inventory");
            Debug.Log("🧪 You can now test potion crafting!");
        }

        private void AddHerbIngredients(ItemSlotContainerHolder inventory)
        {
            var herbs = new string[] { "Sage Leaf", "Dragon's Breath", "Moonstone Moss", "Fire Thistle", "Ice Mint" };
            
            foreach (var herbName in herbs)
            {
                var herb = CreateTestIngredient(herbName, FourFatesStudios.ProjectWarden.Enums.IngredientArchetype.Herb);
                inventory.AddItem(herb, 3);
            }

            Debug.Log($"✅ Added {herbs.Length} herb ingredients to inventory");
        }

        private void AddSolventIngredients(ItemSlotContainerHolder inventory)
        {
            var solvents = new string[] { "Pure Water", "Blessed Oil", "Spirit Essence", "Crystal Dew" };
            
            foreach (var solventName in solvents)
            {
                var solvent = CreateTestIngredient(solventName, FourFatesStudios.ProjectWarden.Enums.IngredientArchetype.Solvent);
                inventory.AddItem(solvent, 5);
            }

            Debug.Log($"✅ Added {solvents.Length} solvent ingredients to inventory");
        }

        private void ClearInventory(ItemSlotContainerHolder inventory)
        {
            if (inventory.Container != null)
            {
                var allItems = inventory.Container.GetAllItems();
                foreach (var item in allItems)
                {
                    // Use reflection to get count safely
                    var countMethod = inventory.Container.GetType().GetMethod("Count");
                    if (countMethod != null)
                    {
                        var count = (int)countMethod.Invoke(inventory.Container, new object[] { item });
                        inventory.RemoveItem(item, count);
                    }
                }
                
                Debug.Log("🧹 Cleared all items from inventory");
            }
        }

        private Ingredient CreateTestIngredient(string name, FourFatesStudios.ProjectWarden.Enums.IngredientArchetype archetype)
        {
            var ingredient = ScriptableObject.CreateInstance<Ingredient>();
            
            // Set name via reflection (since it might be private)
            var nameField = typeof(Item).GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField?.SetValue(ingredient, name);
            
            // Set archetype via reflection
            var archetypeField = typeof(Ingredient).GetField("ingredientArchetype", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            archetypeField?.SetValue(ingredient, archetype);
            
            // Create some basic potion effects
            var effects = new System.Collections.Generic.List<FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects.PotionEffect>();
            
            // Create a test effect
            var effect = ScriptableObject.CreateInstance<FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects.PotionEffect>();
            var effectNameField = typeof(FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects.PotionEffect).GetField("suffix", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            effectNameField?.SetValue(effect, name.Split(' ')[0]); // Use first word as effect name
            
            effects.Add(effect);
            
            // Set effects via reflection
            var effectsField = typeof(Ingredient).GetField("potionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            effectsField?.SetValue(ingredient, effects);
            
            ingredient.name = name;
            return ingredient;
        }

        [MenuItem("Tools/Quick Add Test Ingredients", false, 160)]
        public static void QuickAddIngredients()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Not in Play Mode", "Enter Play Mode first to add test ingredients.", "OK");
                return;
            }

            var inventoryHolder = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
            
            if (inventoryHolder == null)
            {
                EditorUtility.DisplayDialog("No Inventory Found", "No ItemSlotContainerHolder found. Use Tools → Convert Crafting Scene to Hybrid to create one.", "OK");
                return;
            }

            // Create and add basic ingredients
            var window = CreateInstance<InventoryTester>();
            window.AddBasicTestIngredients(inventoryHolder);
            
            EditorUtility.DisplayDialog("Test Ingredients Added!", 
                "Added basic test ingredients to your inventory:\n\n• 5x Healing Herb\n• 5x Mana Flower\n• 5x Strength Root\n• 10x Pure Water (Solvent)\n\nYou can now test the crafting system!", 
                "OK");
        }
    }
}