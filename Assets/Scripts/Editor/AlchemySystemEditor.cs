using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyBook;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class AlchemySystemEditor : EditorWindow
    {
        private int toolbarSelection;
        private readonly string[] toolbarOptions = { "🧪 Ingredients", "📋 Recipes", "🍯 Potions", "⚗️ Refinements", "📚 Book Pages", "🎮 Grid Designer", "📖 Book Editor", "🗄️ Database" };
        
        private Vector2 scrollPosition;
        private Ingredient selectedIngredient;
        private AlchemyRecipe selectedRecipe;
        private Potion selectedPotion;
        private PotionEffect selectedEffect;
        
        // Search and filter
        private string searchQuery = "";
        private bool showOnlyModified;
        
        // Creation wizards
        private bool showIngredientWizard;
        private bool showRecipeWizard;
        
        // Visual grid editor
        private IngredientGridEditor gridEditor = new IngredientGridEditor();
        
        [MenuItem("Tools/Alchemy System Editor")]
        public static void ShowWindow()
        {
            GetWindow<AlchemySystemEditor>("Alchemy System Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Header
            DrawHeader();
            
            // Toolbar
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarOptions);
            
            // Search bar
            DrawSearchBar();
            
            // Content area
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (toolbarSelection)
            {
                case 0: DrawIngredientsTab(); break;
                case 1: DrawRecipesTab(); break;
                case 2: DrawPotionsTab(); break;
                case 3: DrawRefinementsTab(); break;
                case 4: DrawBookPagesTab(); break;
                case 5: DrawGridDesignerTab(); break;
                case 6: DrawBookEditorTab(); break;
                case 7: DrawDatabaseTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal("Box");
            GUILayout.Label("Alchemy System Editor", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Refresh All", GUILayout.Width(100)))
            {
                AssetDatabase.Refresh();
            }
            
            if (GUILayout.Button("Help", GUILayout.Width(50)))
            {
                Application.OpenURL("https://github.com/your-repo/wiki/alchemy-system");
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal("Toolbar");
            GUILayout.Label("Search:", GUILayout.Width(50));
            searchQuery = GUILayout.TextField(searchQuery, "ToolbarSeachTextField");
            
            if (GUILayout.Button("", "ToolbarSeachCancelButton"))
            {
                searchQuery = "";
                GUI.FocusControl(null);
            }
            
            showOnlyModified = GUILayout.Toggle(showOnlyModified, "Modified Only", "ToolbarButton");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawIngredientsTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - ingredient list
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(300));
            GUILayout.Label("Ingredients", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create New Ingredient", GUILayout.Height(30)))
            {
                showIngredientWizard = true;
            }
            
            DrawIngredientList();
            EditorGUILayout.EndVertical();
            
            // Right panel - ingredient editor
            EditorGUILayout.BeginVertical("Box");
            if (selectedIngredient != null)
            {
                DrawIngredientEditor();
            }
            else
            {
                EditorGUILayout.HelpBox("Select an ingredient to edit its properties", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            if (showIngredientWizard)
            {
                DrawIngredientWizard();
            }
        }

        private void DrawIngredientList()
        {
            var ingredients = FindAssetsByType<Ingredient>();
            
            foreach (var ingredient in ingredients)
            {
                if (!string.IsNullOrEmpty(searchQuery) && 
                    !ingredient.name.ToLower().Contains(searchQuery.ToLower()))
                    continue;
                
                EditorGUILayout.BeginHorizontal();
                
                bool isSelected = selectedIngredient == ingredient;
                if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                {
                    selectedIngredient = ingredient;
                }
                
                EditorGUILayout.ObjectField(ingredient, typeof(Ingredient), false);
                
                if (GUILayout.Button("⚡", GUILayout.Width(25)))
                {
                    ShowIngredientQuickActions(ingredient);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawIngredientEditor()
        {
            if (selectedIngredient == null) return;
            
            GUILayout.Label($"Editing: {selectedIngredient.name}", EditorStyles.boldLabel);
            
            var serializedObject = new SerializedObject(selectedIngredient);
            serializedObject.Update();
            
            // Create a custom inspector-like interface
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDescription"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemRarity"));
            
            EditorGUILayout.Space();
            GUILayout.Label("Alchemy Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("potency"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unlocksAdditionalSpace"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("additionalSpaceCount"));
            
            // Visual Grid Editor
            EditorGUILayout.Space();
            gridEditor.DrawGridEditor(selectedIngredient);
            
            EditorGUILayout.Space();
            GUILayout.Label("Refinement Options", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canGrind"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("grindingResult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canDistill"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("distillingResult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canRoast"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("roastingResult"));
            
            serializedObject.ApplyModifiedProperties();
            
            // Action buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Variant"))
            {
                CreateIngredientVariant(selectedIngredient);
            }
            
            if (GUILayout.Button("Auto-Setup Refinements"))
            {
                AutoSetupRefinements(selectedIngredient);
            }
            
            if (GUILayout.Button("Test in Game"))
            {
                TestIngredientInGame(selectedIngredient);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRecipesTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - recipe list
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(300));
            GUILayout.Label("Recipes", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create New Recipe", GUILayout.Height(30)))
            {
                showRecipeWizard = true;
            }
            
            DrawRecipeList();
            EditorGUILayout.EndVertical();
            
            // Right panel - recipe editor
            EditorGUILayout.BeginVertical("Box");
            if (selectedRecipe != null)
            {
                DrawRecipeEditor();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a recipe to edit its properties", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            if (showRecipeWizard)
            {
                DrawRecipeWizard();
            }
        }

        private void DrawRecipeList()
        {
            var recipes = FindAssetsByType<AlchemyRecipe>();
            
            foreach (var recipe in recipes)
            {
                if (!string.IsNullOrEmpty(searchQuery) && 
                    !recipe.name.ToLower().Contains(searchQuery.ToLower()))
                    continue;
                
                EditorGUILayout.BeginHorizontal();
                
                bool isSelected = selectedRecipe == recipe;
                if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                {
                    selectedRecipe = recipe;
                }
                
                EditorGUILayout.ObjectField(recipe, typeof(AlchemyRecipe), false);
                
                if (GUILayout.Button("⚡", GUILayout.Width(25)))
                {
                    ShowRecipeQuickActions(recipe);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawRecipeEditor()
        {
            if (selectedRecipe == null) return;
            
            GUILayout.Label($"Editing Recipe: {selectedRecipe.name}", EditorStyles.boldLabel);
            
            var serializedObject = new SerializedObject(selectedRecipe);
            serializedObject.Update();
            
            // Visual recipe display
            DrawRecipeVisual(selectedRecipe);
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inputIngredient1"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inputIngredient2"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inputIngredient3"));
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("outputPotion"));
            
            serializedObject.ApplyModifiedProperties();
            
            // Recipe testing
            EditorGUILayout.Space();
            if (GUILayout.Button("Test Recipe", GUILayout.Height(30)))
            {
                TestRecipe(selectedRecipe);
            }
        }

        private void DrawRecipeVisual(AlchemyRecipe recipe)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Recipe Preview", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // Input ingredients
            DrawIngredientSlot(recipe.InputIngredient1, "Ingredient 1");
            GUILayout.Label("+", GUILayout.Width(20));
            DrawIngredientSlot(recipe.InputIngredient2, "Ingredient 2");
            GUILayout.Label("+", GUILayout.Width(20));
            DrawIngredientSlot(recipe.InputIngredient3, "Ingredient 3");
            
            GUILayout.Label("=", GUILayout.Width(20));
            
            // Output potion
            DrawPotionSlot(recipe.OutputPotion, "Result");
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawIngredientSlot(Ingredient ingredient, string label)
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(80));
            GUILayout.Label(label, EditorStyles.centeredGreyMiniLabel);
            
            if (ingredient != null)
            {
                if (ingredient.ItemIcon != null)
                {
                    GUILayout.Label(ingredient.ItemIcon.texture, GUILayout.Width(60), GUILayout.Height(60));
                }
                else
                {
                    GUILayout.Box("No Icon", GUILayout.Width(60), GUILayout.Height(60));
                }
                GUILayout.Label(ingredient.ItemName, EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                GUILayout.Box("Empty", GUILayout.Width(60), GUILayout.Height(60));
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawPotionSlot(Potion potion, string label)
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(80));
            GUILayout.Label(label, EditorStyles.centeredGreyMiniLabel);
            
            if (potion != null)
            {
                if (potion.ItemIcon != null)
                {
                    GUILayout.Label(potion.ItemIcon.texture, GUILayout.Width(60), GUILayout.Height(60));
                }
                else
                {
                    GUILayout.Box("No Icon", GUILayout.Width(60), GUILayout.Height(60));
                }
                GUILayout.Label(potion.ItemName, EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                GUILayout.Box("No Result", GUILayout.Width(60), GUILayout.Height(60));
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawPotionsTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - potions list
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(300));
            GUILayout.Label("Potions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create New Potion", GUILayout.Height(30)))
            {
                CreateNewPotion();
            }
            
            if (GUILayout.Button("Create Serpent's Dew", GUILayout.Height(30)))
            {
                CreateSerpentsDew();
            }
            
            DrawPotionsList();
            EditorGUILayout.EndVertical();
            
            // Right panel - potion editor
            EditorGUILayout.BeginVertical("Box");
            if (selectedPotion != null)
            {
                DrawPotionEditor();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a potion to edit its properties", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPotionsList()
        {
            var potions = FindAssetsByType<Potion>();
            
            foreach (var potion in potions)
            {
                if (!string.IsNullOrEmpty(searchQuery) && 
                    !potion.name.ToLower().Contains(searchQuery.ToLower()))
                    continue;
                
                EditorGUILayout.BeginHorizontal();
                
                bool isSelected = selectedPotion == potion;
                if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                {
                    selectedPotion = potion;
                }
                
                EditorGUILayout.ObjectField(potion, typeof(Potion), false);
                
                if (GUILayout.Button("⚡", GUILayout.Width(25)))
                {
                    ShowPotionQuickActions(potion);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPotionEditor()
        {
            if (selectedPotion == null) return;
            
            GUILayout.Label($"Editing Potion: {selectedPotion.name}", EditorStyles.boldLabel);
            
            var serializedObject = new SerializedObject(selectedPotion);
            serializedObject.Update();
            
            // Visual potion display
            DrawPotionVisual(selectedPotion);
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDescription"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemRarity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemPotionType"));
            
            EditorGUILayout.Space();
            GUILayout.Label("Potion Effects", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("potionEffects"));
            
            serializedObject.ApplyModifiedProperties();
            
            // Potion testing
            EditorGUILayout.Space();
            if (GUILayout.Button("Test Potion Effects", GUILayout.Height(30)))
            {
                TestPotionEffects(selectedPotion);
            }
        }

        private void DrawPotionVisual(Potion potion)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Potion Preview", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            // Potion icon and info
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(120));
            if (potion.ItemIcon != null)
            {
                GUILayout.Label(potion.ItemIcon.texture, GUILayout.Width(100), GUILayout.Height(100));
            }
            else
            {
                GUILayout.Box("No Icon", GUILayout.Width(100), GUILayout.Height(100));
            }
            GUILayout.Label(potion.ItemName, EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label($"Type: {potion.ItemPotionType}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
            
            // Effects list
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Effects:", EditorStyles.boldLabel);
            if (potion.PotionEffects != null && potion.PotionEffects.Count > 0)
            {
                foreach (var effect in potion.PotionEffects)
                {
                    if (effect != null)
                    {
                        GUILayout.Label($"• {effect.name}", EditorStyles.label);
                    }
                }
            }
            else
            {
                GUILayout.Label("No effects assigned", EditorStyles.centeredGreyMiniLabel);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void CreateNewPotion()
        {
            // Create a new potion asset
            var potion = ScriptableObject.CreateInstance<Potion>();
            potion.name = "New Potion";
            
            // Ensure the directory exists
            string path = "Assets/Resources/Potions";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Potions");
            }
            
            // Create the asset
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewPotion.asset");
            AssetDatabase.CreateAsset(potion, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            selectedPotion = potion;
            Debug.Log($"Created new potion: {assetPath}");
        }

        private void CreateSerpentsDew()
        {
            // Check if Serpent's Dew already exists
            var existingPotions = FindAssetsByType<Potion>();
            var existingSerpentsDew = existingPotions.FirstOrDefault(p => p.name.Contains("Serpent") && p.name.Contains("Dew"));
            
            if (existingSerpentsDew != null)
            {
                EditorUtility.DisplayDialog("Potion Exists", 
                    "Serpent's Dew potion already exists!\nSelecting existing potion.", 
                    "OK");
                selectedPotion = existingSerpentsDew;
                return;
            }
            
            // Create Serpent's Dew potion
            var serpentsDew = ScriptableObject.CreateInstance<Potion>();
            
            // Set up the potion using reflection to access private fields
            var potionType = typeof(Potion);
            var itemType = typeof(Item);
            
            // Set basic item properties
            var itemNameField = itemType.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var itemDescField = itemType.GetField("itemDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var itemRarityField = itemType.GetField("itemRarity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            itemNameField?.SetValue(serpentsDew, "Serpent's Dew");
            itemDescField?.SetValue(serpentsDew, "A mystical potion distilled from rare serpent scales and morning dew. Grants enhanced agility and poison resistance.");
            itemRarityField?.SetValue(serpentsDew, Rarity.Rare);
            
            // Set potion-specific properties
            var potionTypeField = potionType.GetField("itemPotionType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var potionEffectsField = potionType.GetField("potionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            potionTypeField?.SetValue(serpentsDew, ItemPotionType.Buffing);
            
            // Try to find and assign poison resistance effect
            var potionEffects = FindAssetsByType<PotionEffect>();
            var effects = new List<PotionEffect>();
            
            // Look for relevant effects
            var poisonResistance = potionEffects.FirstOrDefault(e => e.name.ToLower().Contains("poison") || e.name.ToLower().Contains("resist"));
            if (poisonResistance != null)
            {
                effects.Add(poisonResistance);
            }
            
            var agilityEffect = potionEffects.FirstOrDefault(e => e.name.ToLower().Contains("agility") || e.name.ToLower().Contains("speed"));
            if (agilityEffect != null)
            {
                effects.Add(agilityEffect);
            }
            
            potionEffectsField?.SetValue(serpentsDew, effects);
            
            // Ensure the directory exists
            string path = "Assets/Resources/Potions";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Potions");
            }
            
            // Create the asset
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/SerpentsDew.asset");
            AssetDatabase.CreateAsset(serpentsDew, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            selectedPotion = serpentsDew;
            
            Debug.Log($"✅ Created Serpent's Dew potion: {assetPath}");
            
            EditorUtility.DisplayDialog("Potion Created!", 
                "🐍 Serpent's Dew potion has been created!\n\n" +
                "Properties:\n" +
                "• Name: Serpent's Dew\n" +
                "• Rarity: Rare\n" +
                "• Type: Buff\n" +
                "• Effects: Poison resistance and enhanced agility\n\n" +
                "You can now use this potion in recipes!", 
                "Awesome!");
        }

        private void ShowPotionQuickActions(Potion potion)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicatePotion(potion));
            menu.AddItem(new GUIContent("Create Recipe"), false, () => CreateRecipeFromPotion(potion));
            menu.AddItem(new GUIContent("Test Effects"), false, () => TestPotionEffects(potion));
            menu.AddItem(new GUIContent("Add to Book"), false, () => AddPotionToBook(potion));
            menu.ShowAsContext();
        }

        private void TestPotionEffects(Potion potion)
        {
            Debug.Log($"🧪 Testing effects for {potion.ItemName}:");
            if (potion.PotionEffects != null)
            {
                foreach (var effect in potion.PotionEffects)
                {
                    if (effect != null)
                    {
                        Debug.Log($"  • {effect.name}: {string.Join(", ", effect.EffectTypes)}");
                    }
                }
            }
            else
            {
                Debug.Log("  No effects assigned to this potion.");
            }
        }

        private void DrawRefinementsTab()
        {
            EditorGUILayout.HelpBox("Refinement editor for setting up grinding, distillation, and roasting processes", MessageType.Info);
            
            GUILayout.Label("Refinement Processes", EditorStyles.boldLabel);
            
            if (selectedIngredient != null)
            {
                DrawRefinementOptions(selectedIngredient);
            }
            else
            {
                EditorGUILayout.HelpBox("Select an ingredient from the Ingredients tab to edit its refinement options", MessageType.Info);
            }
        }

        private void DrawRefinementOptions(Ingredient ingredient)
        {
            GUILayout.Label($"Refinement for: {ingredient.name}", EditorStyles.boldLabel);
            
            var serializedObject = new SerializedObject(ingredient);
            serializedObject.Update();
            
            // Grinding
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Grinding (Mortar & Pestle)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canGrind"));
            if (ingredient.CanGrind)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("grindingResult"));
                if (GUILayout.Button("Auto-Create Ground Version"))
                {
                    CreateGroundVersion(ingredient);
                }
            }
            EditorGUILayout.EndVertical();
            
            // Distillation
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Distillation (Distillation Column)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canDistill"));
            if (ingredient.CanDistill)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("distillingResult"));
                if (GUILayout.Button("Auto-Create Distilled Version"))
                {
                    CreateDistilledVersion(ingredient);
                }
            }
            EditorGUILayout.EndVertical();
            
            // Roasting
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Roasting (Frying Pan)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canRoast"));
            if (ingredient.CanRoast)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("roastingResult"));
                if (GUILayout.Button("Auto-Create Roasted Version"))
                {
                    CreateRoastedVersion(ingredient);
                }
            }
            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBookPagesTab()
        {
            EditorGUILayout.HelpBox("Alchemy book page editor - coming soon!", MessageType.Info);
            // Implementation for book page editing
        }

        private void DrawDatabaseTab()
        {
            GUILayout.Label("Database Management", EditorStyles.boldLabel);
            
            var database = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
            if (database != null)
            {
                EditorGUILayout.ObjectField("Recipe Database", database, typeof(AlchemyRecipeDatabase), false);
                
                if (GUILayout.Button("Rebuild Database"))
                {
                    RebuildDatabase(database);
                }
                
                if (GUILayout.Button("Validate All Recipes"))
                {
                    ValidateAllRecipes(database);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No AlchemyRecipeDatabase found in Resources/Databases/", MessageType.Warning);
                if (GUILayout.Button("Create Database"))
                {
                    CreateAlchemyDatabase();
                }
            }
        }

        private void DrawIngredientWizard()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Create New Ingredient", EditorStyles.boldLabel);
            
            // Wizard implementation
            if (GUILayout.Button("Close"))
            {
                showIngredientWizard = false;
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawRecipeWizard()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Create New Recipe", EditorStyles.boldLabel);
            
            // Wizard implementation
            if (GUILayout.Button("Close"))
            {
                showRecipeWizard = false;
            }
            
            EditorGUILayout.EndVertical();
        }

        // Helper methods
        private List<T> FindAssetsByType<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)))
                       .Where(asset => asset != null)
                       .ToList();
        }

        private void ShowIngredientQuickActions(Ingredient ingredient)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateIngredient(ingredient));
            menu.AddItem(new GUIContent("Create Recipe"), false, () => CreateRecipeFromIngredient(ingredient));
            menu.AddItem(new GUIContent("Find References"), false, () => FindIngredientReferences(ingredient));
            menu.ShowAsContext();
        }

        private void ShowRecipeQuickActions(AlchemyRecipe recipe)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateRecipe(recipe));
            menu.AddItem(new GUIContent("Test Recipe"), false, () => TestRecipe(recipe));
            menu.AddItem(new GUIContent("Export Recipe"), false, () => ExportRecipe(recipe));
            menu.ShowAsContext();
        }

        private void DrawGridDesignerTab()
        {
            GUILayout.Label("🎮 Grid Pattern Designer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Design grid patterns for alchemy recipes using a visual editor", MessageType.Info);
            
            GUILayout.Space(10);
            
            if (selectedRecipe != null)
            {
                GUILayout.Label($"Editing Pattern for: {selectedRecipe.ItemName}", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Pattern Properties", EditorStyles.boldLabel);
                
                var serializedObject = new SerializedObject(selectedRecipe);
                serializedObject.Update();
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridWidth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridHeight"));
                
                GUILayout.Space(10);
                
                // Grid designer would go here
                DrawRecipeGridDesigner(selectedRecipe);
                
                if (GUILayout.Button("Apply Pattern"))
                {
                    ApplyGridPattern(selectedRecipe);
                }
                
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a recipe from the Recipes tab to design its grid pattern", MessageType.Info);
                
                if (GUILayout.Button("Go to Recipes Tab"))
                {
                    toolbarSelection = 1; // Recipes tab
                }
            }
            
            GUILayout.Space(20);
            
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Pattern Templates", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cross Pattern"))
                CreateCrossPattern();
            if (GUILayout.Button("L-Shape Pattern"))
                CreateLShapePattern();
            if (GUILayout.Button("Diamond Pattern"))
                CreateDiamondPattern();
            if (GUILayout.Button("Custom Pattern"))
                CreateCustomPattern();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawBookEditorTab()
        {
            GUILayout.Label("📖 Alchemy Book Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Create and edit alchemy book entries with templates", MessageType.Info);
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Book Entry Templates", EditorStyles.boldLabel);
            
            var templates = FindAssetsByType<AlchemyBookEntryTemplate>();
            if (templates.Count > 0)
            {
                foreach (var template in templates)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(template, typeof(AlchemyBookEntryTemplate), false);
                    
                    if (GUILayout.Button("Use Template", GUILayout.Width(100)))
                    {
                        CreateEntryFromTemplate(template);
                    }
                    
                    if (GUILayout.Button("Edit", GUILayout.Width(50)))
                    {
                        Selection.activeObject = template;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No entry templates found. Create one first.", MessageType.Info);
            }
            
            if (GUILayout.Button("Create New Template"))
            {
                CreateNewBookTemplate();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(20);
            
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Book Management", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Open Alchemy Book Setup"))
            {
                AlchemyBookSetup.ShowWindow();
            }
            
            if (GUILayout.Button("Validate All Book Entries"))
            {
                ValidateBookEntries();
            }
            
            if (GUILayout.Button("Export Book to JSON"))
            {
                ExportBookToJSON();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawRecipeGridDesigner(AlchemyRecipe recipe)
        {
            GUILayout.Label("Grid Pattern Designer", EditorStyles.boldLabel);
            
            int gridWidth = 6; // Default grid size
            int gridHeight = 6;
            
            EditorGUILayout.BeginVertical("Box");
            
            // Grid size controls
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Grid Size:", GUILayout.Width(80));
            gridWidth = EditorGUILayout.IntSlider("Width", gridWidth, 3, 8);
            gridHeight = EditorGUILayout.IntSlider("Height", gridHeight, 3, 8);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Pattern type selector
            string[] patternTypes = { "Free Placement", "Required Pattern", "Aspect Locked", "Shape Specific" };
            int patternType = EditorGUILayout.Popup("Pattern Type", 0, patternTypes);
            
            GUILayout.Space(10);
            
            // Visual grid designer
            EditorGUILayout.LabelField("Click cells to mark as required/optional:");
            
            Rect gridRect = GUILayoutUtility.GetRect(gridWidth * 25, gridHeight * 25);
            float cellSize = 25f;
            
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Rect cellRect = new Rect(
                        gridRect.x + x * cellSize,
                        gridRect.y + y * cellSize,
                        cellSize - 1,
                        cellSize - 1
                    );
                    
                    // Simple cell state (you'd store this in the recipe)
                    Color cellColor = (x + y) % 2 == 0 ? Color.gray : Color.white;
                    
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    if (GUI.Button(cellRect, "", GUIStyle.none))
                    {
                        // Toggle cell state
                        Debug.Log($"Clicked cell {x}, {y}");
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        // New helper methods
        private void ApplyGridPattern(AlchemyRecipe recipe) { }
        private void CreateCrossPattern() { }
        private void CreateLShapePattern() { }
        private void CreateDiamondPattern() { }
        private void CreateCustomPattern() { }
        private void CreateEntryFromTemplate(AlchemyBookEntryTemplate template) 
        {
            var entry = template.CreateEntry();
            string path = $"Assets/Resources/AlchemyBook/{template.entryType}s/";
            
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split('/');
                string currentPath = "";
                foreach (string folder in folders)
                {
                    if (string.IsNullOrEmpty(folder)) continue;
                    string newPath = string.IsNullOrEmpty(currentPath) ? folder : $"{currentPath}/{folder}";
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folder);
                    }
                    currentPath = newPath;
                }
            }
            
            string assetPath = $"{path}{entry.title.Replace(" ", "")}.asset";
            AssetDatabase.CreateAsset(entry, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = entry;
            EditorGUIUtility.PingObject(entry);
        }
        
        private void CreateNewBookTemplate() 
        {
            var template = ScriptableObject.CreateInstance<AlchemyBookEntryTemplate>();
            template.templateName = "New Template";
            
            string path = "Assets/Resources/AlchemyBook/Templates/";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Resources/AlchemyBook", "Templates");
            }
            
            string assetPath = $"{path}NewTemplate.asset";
            AssetDatabase.CreateAsset(template, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = template;
        }
        
        private void ValidateBookEntries() 
        {
            Debug.Log("📚 Validating all book entries...");
            // Implementation for validation
        }
        
        private void ExportBookToJSON() 
        {
            Debug.Log("📄 Exporting book to JSON...");
            // Implementation for export
        }

        // Placeholder implementations for all the helper methods
        private void CreateIngredientVariant(Ingredient ingredient) { }
        private void AutoSetupRefinements(Ingredient ingredient) { }
        private void TestIngredientInGame(Ingredient ingredient) { }
        private void CreateGroundVersion(Ingredient ingredient) { }
        private void CreateDistilledVersion(Ingredient ingredient) { }
        private void CreateRoastedVersion(Ingredient ingredient) { }
        private void TestRecipe(AlchemyRecipe recipe) { }
        private void RebuildDatabase(AlchemyRecipeDatabase database) { }
        private void ValidateAllRecipes(AlchemyRecipeDatabase database) { }
        private void CreateAlchemyDatabase() { }
        private void DuplicateIngredient(Ingredient ingredient) { }
        private void CreateRecipeFromIngredient(Ingredient ingredient) { }
        private void FindIngredientReferences(Ingredient ingredient) { }
        private void DuplicateRecipe(AlchemyRecipe recipe) { }
        private void ExportRecipe(AlchemyRecipe recipe) { }
        private void DuplicatePotion(Potion potion) { }
        private void CreateRecipeFromPotion(Potion potion) { }
        private void AddPotionToBook(Potion potion) { }
    }
}