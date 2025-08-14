using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.Collections.Generic;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class AlchemySystemEditor : EditorWindow
    {
        private int toolbarSelection = 0;
        private readonly string[] toolbarOptions = { "Ingredients", "Recipes", "Refinements", "Book Pages", "Database" };
        
        private Vector2 scrollPosition;
        private Ingredient selectedIngredient;
        private AlchemyRecipe selectedRecipe;
        private PotionEffect selectedEffect;
        
        // Search and filter
        private string searchQuery = "";
        private bool showOnlyModified = false;
        
        // Creation wizards
        private bool showIngredientWizard = false;
        private bool showRecipeWizard = false;
        
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
                case 2: DrawRefinementsTab(); break;
                case 3: DrawBookPagesTab(); break;
                case 4: DrawDatabaseTab(); break;
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

        private void DrawRefinementsTab()
        {
            EditorGUILayout.HelpBox("Refinement editor for setting up grinding, distillation, and roasting processes", MessageType.Info);
            
            // Implementation for refinement editing
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
    }
}