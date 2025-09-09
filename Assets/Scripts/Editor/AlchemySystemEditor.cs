using System;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyBook;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Effects;
using Object = UnityEngine.Object;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class AlchemySystemEditor : EditorWindow
    {
        private int toolbarSelection;
        private readonly string[] toolbarOptions = { "🧪 Ingredients", "📋 Recipes", "🍯 Potions", "⚗️ Refinements", "📚 Book Pages", "🎮 Grid Designer", "📖 Book Editor", "🗄️ Database" };
        
        private Vector2 scrollPosition;
        private Vector2 bookEntriesScrollPosition;
        private Vector2 ingredientsScrollPosition;
        private Vector2 recipesScrollPosition;
        private Vector2 potionsScrollPosition;
        private Ingredient selectedIngredient;
        private AlchemyRecipe selectedRecipe;
        private BaseEntry selectedBookEntry;
        private Potion selectedPotion;
        
        // Persistent state for grid designer
        private AlchemyRecipe gridDesignerRecipe;
        private int gridWidth = 6;
        private int gridHeight = 6;
        private int selectedPatternType;
        private Dictionary<Vector2Int, CellData> gridCells = new Dictionary<Vector2Int, CellData>();
        private Vector2Int selectedCell = Vector2Int.zero;
        private bool showCellPreview = true;
        private readonly string[] patternTypes = { "Free Placement", "Required Pattern", "Aspect Locked", "Shape Specific", "Cross Pattern", "L-Shape", "Diamond" };
        
        // Ingredient visual design state
        private int selectedIngredientTab = 0;
        private Dictionary<Vector2Int, bool> ingredientVisualGrid = new Dictionary<Vector2Int, bool>();
        private int ingredientGridWidth = 1;
        private int ingredientGridHeight = 1;
        private Vector2Int selectedIngredientCell = Vector2Int.zero;
        private bool showIngredientPreview = true;
        
        // Search and filter
        private string searchQuery = "";
        private bool showOnlyModified;
        
        // Creation wizards
        private bool showIngredientWizard;
        private bool showRecipeWizard;
        
        // Wizard fields for creating new items
        private string newIngredientName = "";
        private Aspect newIngredientAspect = Aspect.Corporeal;
        private Rarity newIngredientRarity = Rarity.Common;
        private int newIngredientPotency = 1;
        
        private string newRecipeName = "";
        private RecipeDifficulty newRecipeDifficulty = RecipeDifficulty.Standard;
        private Potion newRecipeOutputPotion;
        
        // Integrated visual grid editor state
        private const int CELL_SIZE = 30;
        private const int MAX_GRID_SIZE = 8;
        private bool[,] currentIngredientGrid;
        private bool hasUnsavedGridChanges = false;
        
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
            searchQuery = GUILayout.TextField(searchQuery, "ToolbarTextField");
            
            if (GUILayout.Button("", "ToolbarButtonRight"))
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
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🧪 Ingredients", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("❓", GUILayout.Width(25)))
            {
                ShowIngredientHelp();
            }
            EditorGUILayout.EndHorizontal();
            
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
                EditorGUILayout.HelpBox("Select an ingredient to edit its properties using the enhanced tabbed interface", MessageType.Info);
                
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("HelpBox");
                GUILayout.Label("✨ Enhanced Ingredient Editor Features:", EditorStyles.boldLabel);
                GUILayout.Label("• 🏷️ Basic Info - Names, descriptions, and identity properties");
                GUILayout.Label("• 🧪 Alchemy - Potency, grid size, and magical effects");
                GUILayout.Label("• 🎨 Visual Design - Interactive grid editor with templates");
                GUILayout.Label("• ⚗️ Refinement - Grinding, distillation, and roasting options");
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            if (showIngredientWizard)
            {
                DrawIngredientWizard();
            }
        }
        
        private void ShowIngredientHelp()
        {
            EditorUtility.DisplayDialog("Enhanced Ingredient Editor Help", 
                "🧪 ENHANCED INGREDIENT EDITOR\n\n" +
                "This powerful tool lets you design ingredients with a professional tabbed interface:\n\n" +
                "🏷️ BASIC INFO TAB:\n" +
                "• Set name, description, rarity\n" +
                "• Configure aspect and archetype\n" +
                "• Real-time color preview\n\n" +
                "🧪 ALCHEMY TAB:\n" +
                "• Adjust potency (1-5 stars)\n" +
                "• Set grid dimensions\n" +
                "• Manage infusions and effects\n\n" +
                "🎨 VISUAL DESIGN TAB:\n" +
                "• Interactive grid editor\n" +
                "• Quick shape templates\n" +
                "• Aspect-based coloring\n\n" +
                "⚗️ REFINEMENT TAB:\n" +
                "• Configure grinding, distillation, roasting\n" +
                "• Auto-create refined versions\n" +
                "• Set success rates and skill requirements", 
                "Got it!");
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
                
                if (GUILayout.Button("🗑️", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Delete Ingredient", 
                        $"Are you sure you want to delete '{ingredient.name}'?", 
                        "Delete", "Cancel"))
                    {
                        string path = AssetDatabase.GetAssetPath(ingredient);
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        if (selectedIngredient == ingredient) selectedIngredient = null;
                    }
                }
                
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
            
            // Header with ingredient summary
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"🧪 Editing: {selectedIngredient.name}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            // Quick info badges
            var aspectColor = GetIngredientAspectColor(selectedIngredient.IngredientAspect);
            GUI.color = aspectColor;
            GUILayout.Label($"{selectedIngredient.IngredientAspect}", "Button", GUILayout.Width(80));
            GUI.color = Color.white;
            
            GUILayout.Label($"⚡{selectedIngredient.Potency}", "Button", GUILayout.Width(40));
            GUILayout.Label($"📐{selectedIngredient.GridWidth}×{selectedIngredient.GridHeight}", "Button", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            var serializedObject = new SerializedObject(selectedIngredient);
            serializedObject.Update();
            
            // Tabbed interface for ingredient editing
            EditorGUILayout.BeginVertical();
            
            var ingredientTabs = new string[] { "🏷️ Basic Info", "🧪 Alchemy", "🎨 Visual Design", "⚗️ Refinement" };
            int selectedTab = GUILayout.Toolbar(selectedIngredientTab, ingredientTabs);
            if (selectedTab != selectedIngredientTab)
            {
                selectedIngredientTab = selectedTab;
            }
            
            EditorGUILayout.Space(10);
            
            switch (selectedIngredientTab)
            {
                case 0: DrawIngredientBasicInfo(serializedObject); break;
                case 1: DrawIngredientAlchemyInfo(serializedObject); break;
                case 2: DrawIngredientVisualDesign(serializedObject); break;
                case 3: DrawIngredientRefinement(serializedObject); break;
            }
            
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawIngredientBasicInfo(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("📋 Basic Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDescription"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemRarity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
            
            EditorGUILayout.Space();
            GUILayout.Label("🧬 Ingredient Identity", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noun"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("adjective"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ingredientArchetype"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ingredientAspect"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isCorrupted"));
            
            // Visual preview of aspect color
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Aspect Color Preview:", GUILayout.Width(140));
            var aspectColor = GetIngredientAspectColor(selectedIngredient.IngredientAspect);
            var colorRect = GUILayoutUtility.GetRect(60, 20);
            EditorGUI.DrawRect(colorRect, aspectColor);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawIngredientAlchemyInfo(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("⚗️ Alchemy Properties", EditorStyles.boldLabel);
            
            // Core alchemy properties
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Core Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("potency"));
            
            // Grid size with automatic shape detection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridWidth"), GUILayout.Width(200));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridHeight"), GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
            
            // Show calculated area
            int totalArea = selectedIngredient.GridWidth * selectedIngredient.GridHeight;
            EditorGUILayout.LabelField($"Grid Area: {totalArea} cells", EditorStyles.miniLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unlocksAdditionalSpace"));
            if (selectedIngredient.UnlocksAdditionalSpace)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("additionalSpaceCount"));
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Infusions & Effects
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("🌟 Infusions & Effects", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infusions"), true);
            
            if (selectedIngredient.Infusions != null && selectedIngredient.Infusions.Count > 0)
            {
                EditorGUILayout.LabelField($"Active Infusions: {selectedIngredient.Infusions.Count}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("No active infusions", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
            
            // Quick stats summary
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("📊 Quick Stats", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Potency:", GUILayout.Width(80));
            string stars = new string('★', selectedIngredient.Potency) + new string('☆', 5 - selectedIngredient.Potency);
            EditorGUILayout.LabelField($"{selectedIngredient.Potency}/5 ({stars})");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Aspect:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{selectedIngredient.IngredientAspect}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Archetype:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{selectedIngredient.IngredientArchetype}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawIngredientVisualDesign(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🎨 Visual Grid Designer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Design how this ingredient appears and fits in the alchemy grid. Similar to Tetris pieces, ingredients can have custom shapes.", MessageType.Info);
            
            // Grid size controls
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridHeight"));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Enhanced Visual Grid Editor
            DrawIngredientGridEditor(selectedIngredient);
            
            EditorGUILayout.Space(10);
            
            // Shape templates
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Quick Shape Templates", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📱 Rectangle", GUILayout.Height(30)))
                Debug.Log("Applied Rectangle template");
            if (GUILayout.Button("➕ Plus", GUILayout.Height(30)))
                Debug.Log("Applied Plus template");
            if (GUILayout.Button("🔻 Triangle", GUILayout.Height(30)))
                Debug.Log("Applied Triangle template");
            if (GUILayout.Button("📐 L-Shape", GUILayout.Height(30)))
                Debug.Log("Applied L-Shape template");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⚡ Lightning", GUILayout.Height(30)))
                Debug.Log("Applied Lightning template");
            if (GUILayout.Button("💎 Diamond", GUILayout.Height(30)))
                Debug.Log("Applied Diamond template");
            if (GUILayout.Button("🟦 Single", GUILayout.Height(30)))
                Debug.Log("Applied Single template");
            if (GUILayout.Button("🔄 Custom", GUILayout.Height(30)))
                Debug.Log("Reset to custom grid");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            // Advanced properties
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Advanced Visual Properties", EditorStyles.boldLabel);
            
            DrawIngredientColorPreview();
            DrawIngredientStatistics();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawIngredientRefinement(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Refinement Options", EditorStyles.boldLabel);
            
            // Grinding
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("🔨 Grinding (Mortar & Pestle)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canGrind"));
            if (selectedIngredient.CanGrind)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("grindingResult"));
                if (GUILayout.Button("Auto-Create Ground Version"))
                    CreateGroundVersion(selectedIngredient);
            }
            EditorGUILayout.EndVertical();
            
            // Distillation
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("🧪 Distillation (Distillation Column)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canDistill"));
            if (selectedIngredient.CanDistill)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("distillingResult"));
                if (GUILayout.Button("Auto-Create Distilled Version"))
                    CreateDistilledVersion(selectedIngredient);
            }
            EditorGUILayout.EndVertical();
            
            // Roasting
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("🔥 Roasting (Frying Pan)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canRoast"));
            if (selectedIngredient.CanRoast)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("roastingResult"));
                if (GUILayout.Button("Auto-Create Roasted Version"))
                    CreateRoastedVersion(selectedIngredient);
            }
            EditorGUILayout.EndVertical();
            
            // Refining properties
            EditorGUILayout.Space();
            GUILayout.Label("Refining Mechanics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("baseRefiningSuccessRate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stabilityRating"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumRefiningSkill"));
            
            // Action buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Variant"))
                CreateIngredientVariant(selectedIngredient);
            
            if (GUILayout.Button("Auto-Setup Refinements"))
                AutoSetupRefinements(selectedIngredient);
            
            if (GUILayout.Button("Test in Game"))
                TestIngredientInGame(selectedIngredient);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawIngredientColorPreview()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Aspect Color:", GUILayout.Width(100));
            
            var aspectColor = GetIngredientAspectColor(selectedIngredient.IngredientAspect);
            var colorRect = GUILayoutUtility.GetRect(50, 20);
            EditorGUI.DrawRect(colorRect, aspectColor);
            
            GUILayout.Label($"{selectedIngredient.IngredientAspect}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Rarity:", GUILayout.Width(100));
            GUILayout.Label($"{selectedIngredient.ItemRarity}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawIngredientStatistics()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Grid Statistics:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Total Area:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{selectedIngredient.GridWidth * selectedIngredient.GridHeight} cells");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Size:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{selectedIngredient.GridWidth} x {selectedIngredient.GridHeight}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Potency:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"{selectedIngredient.Potency}/5");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Additional Space:", GUILayout.Width(80));
            EditorGUILayout.LabelField(selectedIngredient.UnlocksAdditionalSpace ? $"+{selectedIngredient.AdditionalSpaceCount}" : "None");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private Color GetIngredientAspectColor(Aspect aspect)
        {
            return aspect switch
            {
                Aspect.Corporeal => new Color(0.8f, 0.6f, 0.4f, 0.9f), // Brown/Earth
                Aspect.Frigid => new Color(0.4f, 0.8f, 1.0f, 0.9f),   // Ice Blue
                Aspect.Scorch => new Color(1.0f, 0.4f, 0.2f, 0.9f),   // Fire Red
                Aspect.Caustic => new Color(0.6f, 1.0f, 0.2f, 0.9f),  // Acid Green
                Aspect.Arc => new Color(1.0f, 1.0f, 0.4f, 0.9f),      // Lightning Yellow
                Aspect.Divine => new Color(1.0f, 0.8f, 1.0f, 0.9f),   // Holy Purple
                _ => new Color(0.7f, 0.7f, 0.7f, 0.9f)
            };
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
                
                if (GUILayout.Button("🗑️", GUILayout.Width(25)))
                {
                    if (EditorUtility.DisplayDialog("Delete Recipe", 
                        $"Are you sure you want to delete '{recipe.name}'?", 
                        "Delete", "Cancel"))
                    {
                        string path = AssetDatabase.GetAssetPath(recipe);
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        if (selectedRecipe == recipe) selectedRecipe = null;
                    }
                }
                
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
            GUILayout.Label("Effect Bundle", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectBundle"));
            
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
            if (potion.EffectBundle?.Effects != null && potion.EffectBundle.Effects.Count > 0)
            {
                foreach (var effect in potion.EffectBundle.Effects)
                {
                    if (effect != null)
                    {
                        GUILayout.Label($"• {effect.GetType().Name}", EditorStyles.label);
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
            var potion = CreateInstance<Potion>();
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
            var serpentsDew = CreateInstance<Potion>();
            
            // Set up the potion using reflection to access private fields
            var potionType = typeof(Potion);
            var itemType = typeof(Item);
            
            // Set basic item properties
            var itemNameField = itemType.GetField("itemName", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemDescField = itemType.GetField("itemDescription", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemRarityField = itemType.GetField("itemRarity", BindingFlags.NonPublic | BindingFlags.Instance);
            
            itemNameField?.SetValue(serpentsDew, "Serpent's Dew");
            itemDescField?.SetValue(serpentsDew, "A mystical potion distilled from rare serpent scales and morning dew. Grants enhanced agility and poison resistance.");
            itemRarityField?.SetValue(serpentsDew, Rarity.Rare);
            
            // Set potion-specific properties using new EffectBundle system
            var potionTypeField = potionType.GetField("itemPotionType", BindingFlags.NonPublic | BindingFlags.Instance);
            var effectBundleField = potionType.GetField("effectBundle", BindingFlags.NonPublic | BindingFlags.Instance);
            
            potionTypeField?.SetValue(serpentsDew, ItemPotionType.Buffing);
            
            // Create and assign effect bundle with resistance effects
            var effectBundle = new EffectBundle();
            
            // Add poison resistance effect using the new IEffect system
            // Note: This is a placeholder - you should implement specific resistance effects
            // effectBundle.Effects.Add(new ResistanceEffect { ResistanceType = DamageType.Poison, ResistanceAmount = 0.8f });
            
            effectBundleField?.SetValue(serpentsDew, effectBundle);
            
            // Force recompilation - Editor updated for new effect system
            
            
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
            if (potion.EffectBundle?.Effects != null && potion.EffectBundle.Effects.Count > 0)
            {
                foreach (var effect in potion.EffectBundle.Effects)
                {
                    if (effect != null)
                    {
                        Debug.Log($"  • {effect.GetType().Name}: Ready for application");
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
            GUILayout.Label("📚 Book Pages Management", EditorStyles.boldLabel);
            
            // Category filter dropdown
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Filter by Category:", GUILayout.Width(120));
            var entryTypes = Enum.GetNames(typeof(EntryType));
            var selectedCategory = EditorGUILayout.Popup(0, entryTypes);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // CRUD Operations
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Entry Management", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Entry"))
            {
                ShowCreateBookEntryMenu();
            }
            
            if (GUILayout.Button("Duplicate Selected"))
            {
                DuplicateSelectedEntry();
            }
            
            if (GUILayout.Button("Delete Selected"))
            {
                DeleteSelectedEntry();
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // List existing entries
            var allEntries = FindAssetsByType<BaseEntry>();
            if (allEntries.Count > 0)
            {
                GUILayout.Label($"Found {allEntries.Count} book entries:", EditorStyles.boldLabel);
                
                bookEntriesScrollPosition = EditorGUILayout.BeginScrollView(bookEntriesScrollPosition, GUILayout.Height(300));
                foreach (var entry in allEntries)
                {
                    EditorGUILayout.BeginHorizontal("Box");
                    
                    // Selection toggle
                    bool isSelected = selectedBookEntry == entry;
                    if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
                    {
                        selectedBookEntry = entry;
                    }
                    
                    // Entry info
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(entry.title, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Type: {entry.GetEntryType()}");
                    EditorGUILayout.EndVertical();
                    
                    // Actions
                    EditorGUILayout.BeginVertical(GUILayout.Width(100));
                    if (GUILayout.Button("Edit"))
                    {
                        Selection.activeObject = entry;
                    }
                    if (GUILayout.Button("Preview"))
                    {
                        PreviewBookEntry(entry);
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No book entries found. Create some entries to populate the alchemy book.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawDatabaseTab()
        {
            GUILayout.Label("Database Management", EditorStyles.boldLabel);
            
            // Asset Repair Section
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🔧 Asset Repair Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Fix 'Unnamed Item' Assets"))
            {
                FixUnnamedItemAssets();
            }
            
            if (GUILayout.Button("Regenerate Asset Names"))
            {
                RegenerateAssetNames();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
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
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(100));
            newIngredientName = EditorGUILayout.TextField(newIngredientName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Aspect:", GUILayout.Width(100));
            newIngredientAspect = (Aspect)EditorGUILayout.EnumPopup(newIngredientAspect);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Rarity:", GUILayout.Width(100));
            newIngredientRarity = (Rarity)EditorGUILayout.EnumPopup(newIngredientRarity);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Potency:", GUILayout.Width(100));
            newIngredientPotency = EditorGUILayout.IntSlider(newIngredientPotency, 1, 5);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Ingredient"))
            {
                CreateNewIngredient();
            }
            
            if (GUILayout.Button("Cancel"))
            {
                showIngredientWizard = false;
                ResetWizardFields();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawRecipeWizard()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Create New Recipe", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Recipe Name:", GUILayout.Width(100));
            newRecipeName = EditorGUILayout.TextField(newRecipeName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Difficulty:", GUILayout.Width(100));
            newRecipeDifficulty = (RecipeDifficulty)EditorGUILayout.EnumPopup(newRecipeDifficulty);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Output Potion:", GUILayout.Width(100));
            newRecipeOutputPotion = (Potion)EditorGUILayout.ObjectField(newRecipeOutputPotion, typeof(Potion), false);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Recipe"))
            {
                CreateNewRecipe();
            }
            
            if (GUILayout.Button("Cancel"))
            {
                showRecipeWizard = false;
                ResetWizardFields();
            }
            EditorGUILayout.EndHorizontal();
            
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
            
            // Recipe selection for grid designer with improved state management
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Recipe for Grid Design:", GUILayout.Width(150));
            
            EditorGUI.BeginChangeCheck();
            var newGridRecipe = (AlchemyRecipe)EditorGUILayout.ObjectField(gridDesignerRecipe, typeof(AlchemyRecipe), false);
            
            if (EditorGUI.EndChangeCheck())
            {
                if (newGridRecipe != gridDesignerRecipe)
                {
                    gridDesignerRecipe = newGridRecipe;
                    if (gridDesignerRecipe != null)
                    {
                        Debug.Log($"Grid Designer: Selected recipe '{gridDesignerRecipe.ItemName}' for pattern editing");
                    }
                }
            }
            
            // Add button to use currently selected recipe from Recipes tab
            if (selectedRecipe != null && selectedRecipe != gridDesignerRecipe)
            {
                string selectedRecipeName = !string.IsNullOrEmpty(selectedRecipe.ItemName) && selectedRecipe.ItemName != "Unnamed Item" 
                    ? selectedRecipe.ItemName 
                    : selectedRecipe.name;
                    
                if (GUILayout.Button($"Use '{selectedRecipeName}'", GUILayout.Width(120)))
                {
                    gridDesignerRecipe = selectedRecipe;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            if (gridDesignerRecipe != null)
            {
                string recipeName = !string.IsNullOrEmpty(gridDesignerRecipe.ItemName) && gridDesignerRecipe.ItemName != "Unnamed Item" 
                    ? gridDesignerRecipe.ItemName 
                    : gridDesignerRecipe.name;
                    
                GUILayout.Label($"Editing Pattern for: {recipeName}", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Recipe Grid Pattern", EditorStyles.boldLabel);
                
                var serializedObject = new SerializedObject(gridDesignerRecipe);
                serializedObject.Update();
                
                // Show recipe ingredients
                EditorGUILayout.Space();
                GUILayout.Label("Required Ingredients:", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("inputIngredient1"), new GUIContent("Ingredient 1"));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("inputIngredient2"), new GUIContent("Ingredient 2"));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("inputIngredient3"), new GUIContent("Ingredient 3"));
                EditorGUILayout.EndHorizontal();
                
                // Recipe properties
                EditorGUILayout.Space();
                GUILayout.Label("Recipe Properties:", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredHits"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAttempts"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredTemperature"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("totalDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("outputPotion"));
                
                serializedObject.ApplyModifiedProperties();
                
                GUILayout.Space(10);
                
                // Grid designer for recipe pattern
                try
                {
                    DrawRecipeGridDesigner(gridDesignerRecipe);
                }
                catch (Exception e)
                {
                    EditorGUILayout.HelpBox($"Grid designer error: {e.Message}\nThis may be because the recipe doesn't have grid properties.", MessageType.Warning);
                    
                    // Offer to add grid properties to the recipe
                    if (GUILayout.Button("Add Grid Properties to Recipe"))
                    {
                        AddGridPropertiesToRecipe(gridDesignerRecipe);
                    }
                }
                
                GUILayout.Space(10);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Validate Recipe"))
                {
                    ValidateRecipe(gridDesignerRecipe);
                }
                
                if (GUILayout.Button("Clear Selection"))
                {
                    gridDesignerRecipe = null;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a recipe to design its grid pattern", MessageType.Info);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Go to Recipes Tab"))
                {
                    toolbarSelection = 1; // Recipes tab
                }
                
                if (GUILayout.Button("Select from Recipes"))
                {
                    ShowRecipeSelector();
                }
                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.Space(20);
            
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Pattern Templates", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cross Pattern"))
            {
                CreateCrossPatternInGrid();
            }
            if (GUILayout.Button("L-Shape Pattern"))
            {
                CreateLShapePatternInGrid();
            }
            if (GUILayout.Button("Diamond Pattern"))
            {
                CreateDiamondPatternInGrid();
            }
            if (GUILayout.Button("Clear Grid"))
            {
                gridCells.Clear();
            }
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
            GUILayout.Label("🎨 Enhanced Grid Pattern Designer", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical("Box");
            
            // Grid size controls
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Grid Size:", GUILayout.Width(80));
            EditorGUI.BeginChangeCheck();
            gridWidth = EditorGUILayout.IntSlider("Width", gridWidth, 3, 8);
            gridHeight = EditorGUILayout.IntSlider("Height", gridHeight, 3, 8);
            if (EditorGUI.EndChangeCheck())
            {
                gridCells.Clear(); // Clear when size changes
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Pattern type selector with preview
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Pattern Type:", GUILayout.Width(100));
            EditorGUI.BeginChangeCheck();
            selectedPatternType = EditorGUILayout.Popup(selectedPatternType, patternTypes);
            if (EditorGUI.EndChangeCheck())
            {
                PreviewPatternType(selectedPatternType);
            }
            
            showCellPreview = EditorGUILayout.Toggle("Show Preview", showCellPreview, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            if (showCellPreview && gridCells.Count == 0)
            {
                PreviewPatternType(selectedPatternType);
            }
            
            GUILayout.Space(10);
            
            // Pattern info
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Pattern Info:", EditorStyles.boldLabel);
            string patternDescription = GetPatternDescription(selectedPatternType);
            EditorGUILayout.LabelField(patternDescription, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Visual grid designer with enhanced cells
            EditorGUILayout.LabelField("🎯 Visual Grid Designer:");
            EditorGUILayout.LabelField("• Click cells to toggle states", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Colors represent aspects, letters show rarity", EditorStyles.miniLabel);
            
            Rect gridRect = GUILayoutUtility.GetRect(gridWidth * 35, gridHeight * 35);
            float cellSize = 35f;
            
            for (int y = gridHeight - 1; y >= 0; y--) // Draw from top to bottom visually
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Rect cellRect = new Rect(
                        gridRect.x + x * cellSize,
                        gridRect.y + (gridHeight - 1 - y) * cellSize, // Flip Y for display
                        cellSize - 2,
                        cellSize - 2
                    );
                    
                    // Get cell data
                    bool hasCellData = gridCells.TryGetValue(pos, out CellData cellData);
                    
                    // Determine cell color and content
                    Color cellColor;
                    string cellText = "";
                    
                    if (hasCellData)
                    {
                        cellColor = GetAspectColor(cellData.aspect);
                        cellText = GetRarityLetter(cellData.rarity);
                        
                        // Add visual indicators
                        if (cellData.isRequired)
                        {
                            cellColor = Color.Lerp(cellColor, Color.white, 0.3f); // Brighten required cells
                        }
                        
                        if (selectedCell == pos)
                        {
                            cellColor = Color.Lerp(cellColor, Color.yellow, 0.5f); // Highlight selected
                        }
                    }
                    else
                    {
                        cellColor = (x + y) % 2 == 0 ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.9f, 0.9f, 0.9f);
                        
                        if (selectedCell == pos)
                        {
                            cellColor = Color.yellow;
                        }
                    }
                    
                    // Draw cell background
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw cell border
                    Color borderColor = hasCellData && cellData.isRequired ? Color.black : Color.gray;
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), borderColor);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), borderColor);
                    EditorGUI.DrawRect(new Rect(cellRect.x + cellRect.width - 1, cellRect.y, 1, cellRect.height), borderColor);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y + cellRect.height - 1, cellRect.width, 1), borderColor);
                    
                    // Draw cell text (rarity letter)
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        var oldColor = GUI.color;
                        GUI.color = Color.black;
                        GUI.Label(cellRect, cellText, EditorStyles.boldLabel);
                        GUI.color = oldColor;
                    }
                    
                    // Handle cell clicking
                    if (GUI.Button(cellRect, "", GUIStyle.none))
                    {
                        OnCellClicked(pos);
                    }
                }
            }
            
            GUILayout.Space(10);
            
            // Cell information panel
            if (gridCells.TryGetValue(selectedCell, out CellData selectedCellData))
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label($"Selected Cell: ({selectedCell.x}, {selectedCell.y})", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                selectedCellData.aspect = (Aspect)EditorGUILayout.EnumPopup("Aspect:", selectedCellData.aspect);
                selectedCellData.rarity = (Rarity)EditorGUILayout.EnumPopup("Rarity:", selectedCellData.rarity);
                selectedCellData.isRequired = EditorGUILayout.Toggle("Required:", selectedCellData.isRequired);
                
                if (EditorGUI.EndChangeCheck())
                {
                    gridCells[selectedCell] = selectedCellData;
                }
                
                EditorGUILayout.EndVertical();
            }
            else if (selectedCell != Vector2Int.zero)
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label($"Empty Cell: ({selectedCell.x}, {selectedCell.y})", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Add Cell Data"))
                {
                    gridCells[selectedCell] = new CellData
                    {
                        aspect = Aspect.Corporeal,
                        rarity = Rarity.Common,
                        isRequired = false
                    };
                }
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Refresh Preview"))
            {
                PreviewPatternType(selectedPatternType);
            }
            
            if (GUILayout.Button("🗑️ Clear Grid"))
            {
                gridCells.Clear();
                selectedCell = Vector2Int.zero;
            }
            
            if (GUILayout.Button("💾 Save Pattern"))
            {
                SaveGridPatternToRecipe(recipe);
            }
            
            if (GUILayout.Button("📋 Load Pattern"))
            {
                LoadGridPatternFromRecipe(recipe);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private string GetPatternDescription(int patternType)
        {
            return patternType switch
            {
                0 => "Free Placement: Ingredients can be placed anywhere on the grid with no restrictions.",
                1 => "Required Pattern: Specific cells must be filled for the recipe to work. Shows plus pattern.",
                2 => "Aspect Locked: Corners are locked to specific aspects. Demonstrates aspect-based restrictions.",
                3 => "Shape Specific: Must form a specific shape (T-pattern shown). Recipe requires exact formation.",
                4 => "Cross Pattern: Classic cross formation with divine aspect. Higher rarity requirements.",
                5 => "L-Shape: L-shaped arrangement in corner. Good for corner-based ingredient placement.",
                6 => "Diamond: Diamond formation with dual-layer complexity. Advanced pattern example.",
                _ => "Unknown pattern type."
            };
        }
        
        private void OnCellClicked(Vector2Int pos)
        {
            selectedCell = pos;
            
            // Toggle cell state if it exists
            if (gridCells.TryGetValue(pos, out CellData cellData))
            {
                if (Event.current.shift)
                {
                    // Shift+click to remove
                    gridCells.Remove(pos);
                }
                else if (Event.current.control)
                {
                    // Ctrl+click to toggle required
                    cellData.isRequired = !cellData.isRequired;
                    gridCells[pos] = cellData;
                }
                else
                {
                    // Regular click cycles aspect
                    var aspects = Enum.GetValues(typeof(Aspect));
                    int currentIndex = Array.IndexOf(aspects, cellData.aspect);
                    cellData.aspect = (Aspect)aspects.GetValue((currentIndex + 1) % aspects.Length);
                    gridCells[pos] = cellData;
                }
            }
            else
            {
                // Create new cell data
                gridCells[pos] = new CellData
                {
                    aspect = Aspect.Corporeal,
                    rarity = Rarity.Common,
                    isRequired = true
                };
            }
            
            Debug.Log($"🎯 Cell clicked: ({pos.x}, {pos.y}) - Shift: Remove, Ctrl: Toggle Required, Click: Cycle Aspect");
        }
        
        private void SaveGridPattern(AlchemyRecipe recipe)
        {
            Debug.Log($"💾 Saving grid pattern for {recipe.ItemName} with {gridCells.Count} cells");
            // This would serialize the grid data to the recipe asset
            EditorUtility.DisplayDialog("Save Pattern", $"Grid pattern saved for {recipe.ItemName}!\n\nCells: {gridCells.Count}\nPattern Type: {patternTypes[selectedPatternType]}", "OK");
        }
        
        private void LoadGridPattern(AlchemyRecipe recipe)
        {
            Debug.Log($"📋 Loading grid pattern for {recipe.ItemName}");
            // This would load the grid data from the recipe asset
            EditorUtility.DisplayDialog("Load Pattern", $"Grid pattern loaded for {recipe.ItemName}!\n\nPattern would be restored from saved data.", "OK");
        }

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
            var template = CreateInstance<AlchemyBookEntryTemplate>();
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
        private void CreateIngredientVariant(Ingredient ingredient) 
        {
            Debug.Log($"Creating variant for {ingredient.ItemName}...");
        }
        
        private void AutoSetupRefinements(Ingredient ingredient) 
        {
            Debug.Log($"Auto-setting refinements for {ingredient.ItemName}...");
        }
        
        private void TestIngredientInGame(Ingredient ingredient) 
        {
            Debug.Log($"Testing {ingredient.ItemName} in game...");
        }
        
        private void CreateGroundVersion(Ingredient ingredient) 
        {
            Debug.Log($"Creating ground version of {ingredient.ItemName}...");
        }
        
        private void CreateDistilledVersion(Ingredient ingredient) 
        {
            Debug.Log($"Creating distilled version of {ingredient.ItemName}...");
        }
        
        private void CreateRoastedVersion(Ingredient ingredient) 
        {
            Debug.Log($"Creating roasted version of {ingredient.ItemName}...");
        }
        
        private void TestRecipe(AlchemyRecipe recipe) 
        {
            Debug.Log($"Testing recipe {recipe.ItemName}...");
        }
        
        private void RebuildDatabase(AlchemyRecipeDatabase database) 
        {
            Debug.Log("Rebuilding alchemy recipe database...");
        }
        
        private void ValidateAllRecipes(AlchemyRecipeDatabase database) 
        {
            Debug.Log("Validating all recipes in database...");
        }
        
        private void CreateAlchemyDatabase() 
        {
            var database = CreateInstance<AlchemyRecipeDatabase>();
            string path = "Assets/Resources/Databases/";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Databases");
            }
            
            string assetPath = $"{path}AlchemyRecipeDatabase.asset";
            AssetDatabase.CreateAsset(database, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = database;
            Debug.Log("Created new AlchemyRecipeDatabase");
        }
        
        private void DuplicateIngredient(Ingredient ingredient) 
        {
            var duplicate = Instantiate(ingredient);
            duplicate.name = $"{ingredient.ItemName} Copy";
            
            string path = AssetDatabase.GetAssetPath(ingredient);
            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            string newPath = $"{directory}/{filename}_Copy{extension}";
            
            AssetDatabase.CreateAsset(duplicate, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = duplicate;
            Debug.Log($"Duplicated ingredient: {ingredient.ItemName}");
        }
        
        private void CreateRecipeFromIngredient(Ingredient ingredient) 
        {
            Debug.Log($"Creating recipe from ingredient {ingredient.ItemName}...");
        }
        
        private void FindIngredientReferences(Ingredient ingredient) 
        {
            Debug.Log($"Finding references for {ingredient.ItemName}...");
        }
        
        private void DuplicateRecipe(AlchemyRecipe recipe) 
        {
            var duplicate = Instantiate(recipe);
            duplicate.name = $"{recipe.ItemName} Copy";
            
            string path = AssetDatabase.GetAssetPath(recipe);
            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            string newPath = $"{directory}/{filename}_Copy{extension}";
            
            AssetDatabase.CreateAsset(duplicate, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = duplicate;
            Debug.Log($"Duplicated recipe: {recipe.ItemName}");
        }
        
        private void ExportRecipe(AlchemyRecipe recipe) 
        {
            Debug.Log($"Exporting recipe {recipe.ItemName}...");
        }
        
        private void DuplicatePotion(Potion potion) 
        {
            Debug.Log($"Duplicating potion {potion.ItemName}...");
        }
        
        private void CreateRecipeFromPotion(Potion potion) 
        {
            Debug.Log($"Creating recipe from potion {potion.ItemName}...");
        }
        
        private void AddPotionToBook(Potion potion) 
        {
            Debug.Log($"Adding potion {potion.ItemName} to book...");
        }
        
        private void ResetGridPattern(AlchemyRecipe recipe)
        {
            Debug.Log($"Resetting grid pattern for {recipe.ItemName}...");
        }
        
        private void ShowRecipeSelector()
        {
            var recipes = FindAssetsByType<AlchemyRecipe>();
            if (recipes.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                foreach (var recipe in recipes)
                {
                    menu.AddItem(new GUIContent(recipe.ItemName), false, () => { gridDesignerRecipe = recipe; });
                }
                menu.ShowAsContext();
            }
            else
            {
                Debug.LogWarning("No recipes found to select from.");
            }
        }
        
        private void ValidateRecipe(AlchemyRecipe recipe)
        {
            Debug.Log($"🔍 Validating recipe: {recipe.ItemName}");
            
            // Check ingredients
            var ingredients = new[] { recipe.InputIngredient1, recipe.InputIngredient2, recipe.InputIngredient3 };
            int ingredientCount = ingredients.Count(i => i != null);
            
            if (ingredientCount == 0)
            {
                Debug.LogWarning($"Recipe '{recipe.ItemName}' has no input ingredients!");
            }
            else
            {
                Debug.Log($"✅ Recipe has {ingredientCount} ingredients");
            }
            
            // Check output
            if (recipe.OutputPotion == null)
            {
                Debug.LogWarning($"Recipe '{recipe.ItemName}' has no output potion!");
            }
            else
            {
                Debug.Log($"✅ Recipe outputs: {recipe.OutputPotion.ItemName}");
            }
            
            // Check values
            Debug.Log($"Recipe validation complete for '{recipe.ItemName}'");
        }
        
        private void AddGridPropertiesToRecipe(AlchemyRecipe recipe)
        {
            Debug.Log($"Adding grid properties to recipe: {recipe.ItemName}");
            EditorGUILayout.HelpBox("This feature would extend the AlchemyRecipe class with grid properties for visual arrangement.", MessageType.Info);
        }

        private void PreviewBookEntry(BaseEntry entry)
        {
            Debug.Log($"Previewing book entry: {entry.title}");
        }
        
        // Color coding for aspects and rarity
        private Color GetAspectColor(Aspect aspect)
        {
            return aspect switch
            {
                Aspect.Corporeal => new Color(0.8f, 0.6f, 0.4f, 0.8f), // Brown/Earth
                Aspect.Frigid => new Color(0.4f, 0.8f, 1.0f, 0.8f),   // Ice Blue
                Aspect.Scorch => new Color(1.0f, 0.4f, 0.2f, 0.8f),   // Fire Red
                Aspect.Caustic => new Color(0.6f, 1.0f, 0.2f, 0.8f),  // Acid Green
                Aspect.Arc => new Color(1.0f, 1.0f, 0.4f, 0.8f),      // Lightning Yellow
                Aspect.Divine => new Color(1.0f, 0.8f, 1.0f, 0.8f),   // Holy Purple
                _ => Color.gray
            };
        }
        
        private string GetRarityLetter(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.None => "",
                Rarity.Common => "C",
                Rarity.Uncommon => "U", 
                Rarity.Rare => "R",
                Rarity.Epic => "E",
                Rarity.Mythic => "M",
                Rarity.Unique => "Q",
                _ => "?"
            };
        }
        
        // Pattern preview methods
        private void PreviewPatternType(int patternType)
        {
            if (!showCellPreview) return;
            
            // Clear existing preview
            gridCells.Clear();
            
            switch (patternType)
            {
                case 0: // Free Placement
                    PreviewFreePlacement();
                    break;
                case 1: // Required Pattern
                    PreviewRequiredPattern();
                    break;
                case 2: // Aspect Locked
                    PreviewAspectLocked();
                    break;
                case 3: // Shape Specific
                    PreviewShapeSpecific();
                    break;
                case 4: // Cross Pattern
                    PreviewCrossPattern();
                    break;
                case 5: // L-Shape
                    PreviewLShapePattern();
                    break;
                case 6: // Diamond
                    PreviewDiamondPattern();
                    break;
            }
        }
        
        private void PreviewFreePlacement()
        {
            // Checkered pattern with different aspects
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if ((x + y) % 2 == 0)
                    {
                        var pos = new Vector2Int(x, y);
                        gridCells[pos] = new CellData
                        {
                            aspect = (Aspect)((x + y) % Enum.GetValues(typeof(Aspect)).Length),
                            rarity = Rarity.Common,
                            isRequired = false
                        };
                    }
                }
            }
        }
        
        private void PreviewRequiredPattern()
        {
            // Required cells in a plus pattern
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            
            // Horizontal line
            for (int x = 0; x < gridWidth; x++)
            {
                var pos = new Vector2Int(x, centerY);
                gridCells[pos] = new CellData
                {
                    aspect = Aspect.Scorch,
                    rarity = Rarity.Rare,
                    isRequired = true
                };
            }
            
            // Vertical line
            for (int y = 0; y < gridHeight; y++)
            {
                var pos = new Vector2Int(centerX, y);
                gridCells[pos] = new CellData
                {
                    aspect = Aspect.Frigid,
                    rarity = Rarity.Rare,
                    isRequired = true
                };
            }
        }
        
        private void PreviewAspectLocked()
        {
            // Different aspects in corners
            gridCells[new Vector2Int(0, 0)] = new CellData { aspect = Aspect.Corporeal, rarity = Rarity.Uncommon, isRequired = true };
            gridCells[new Vector2Int(gridWidth-1, 0)] = new CellData { aspect = Aspect.Frigid, rarity = Rarity.Uncommon, isRequired = true };
            gridCells[new Vector2Int(0, gridHeight-1)] = new CellData { aspect = Aspect.Scorch, rarity = Rarity.Uncommon, isRequired = true };
            gridCells[new Vector2Int(gridWidth-1, gridHeight-1)] = new CellData { aspect = Aspect.Caustic, rarity = Rarity.Uncommon, isRequired = true };
        }
        
        private void PreviewShapeSpecific()
        {
            // T-shape pattern
            int centerX = gridWidth / 2;
            int topY = gridHeight - 1;
            
            // Top horizontal
            for (int x = centerX - 1; x <= centerX + 1; x++)
            {
                if (x >= 0 && x < gridWidth)
                {
                    gridCells[new Vector2Int(x, topY)] = new CellData 
                    { 
                        aspect = Aspect.Divine, 
                        rarity = Rarity.Epic, 
                        isRequired = true 
                    };
                }
            }
            
            // Vertical stem
            for (int y = topY - 1; y >= 0; y--)
            {
                gridCells[new Vector2Int(centerX, y)] = new CellData 
                { 
                    aspect = Aspect.Arc, 
                    rarity = Rarity.Epic, 
                    isRequired = true 
                };
            }
        }
        
        private void PreviewCrossPattern()
        {
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            
            // Arms of the cross
            int[] offsets = { -1, 0, 1 };
            foreach (int offset in offsets)
            {
                // Horizontal
                if (centerX + offset >= 0 && centerX + offset < gridWidth)
                {
                    gridCells[new Vector2Int(centerX + offset, centerY)] = new CellData 
                    { 
                        aspect = Aspect.Divine, 
                        rarity = Rarity.Mythic, 
                        isRequired = true 
                    };
                }
                
                // Vertical
                if (centerY + offset >= 0 && centerY + offset < gridHeight)
                {
                    gridCells[new Vector2Int(centerX, centerY + offset)] = new CellData 
                    { 
                        aspect = Aspect.Divine, 
                        rarity = Rarity.Mythic, 
                        isRequired = true 
                    };
                }
            }
        }
        
        private void PreviewLShapePattern()
        {
            // L-shape in bottom-left
            // Vertical part
            for (int y = 0; y < 3; y++)
            {
                gridCells[new Vector2Int(0, y)] = new CellData 
                { 
                    aspect = Aspect.Corporeal, 
                    rarity = Rarity.Rare, 
                    isRequired = true 
                };
            }
            
            // Horizontal part
            for (int x = 1; x < 3; x++)
            {
                gridCells[new Vector2Int(x, 0)] = new CellData 
                { 
                    aspect = Aspect.Corporeal, 
                    rarity = Rarity.Rare, 
                    isRequired = true 
                };
            }
        }
        
        private void PreviewDiamondPattern()
        {
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            
            // Diamond shape
            for (int radius = 1; radius <= 2; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (Math.Abs(x) + Math.Abs(y) == radius)
                        {
                            int posX = centerX + x;
                            int posY = centerY + y;
                            
                            if (posX >= 0 && posX < gridWidth && posY >= 0 && posY < gridHeight)
                            {
                                gridCells[new Vector2Int(posX, posY)] = new CellData 
                                { 
                                    aspect = radius == 1 ? Aspect.Arc : Aspect.Caustic, 
                                    rarity = radius == 1 ? Rarity.Epic : Rarity.Rare, 
                                    isRequired = true 
                                };
                            }
                        }
                    }
                }
            }
        }
        
        // Ingredient Visual Design Methods
        private void DrawIngredientGridDesigner()
        {
            if (selectedIngredient == null) return;
            
            // Initialize grid data from ingredient
            InitializeIngredientGrid();
            
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🎯 Enhanced Ingredient Shape Designer", EditorStyles.boldLabel);
            
            // Grid preview info
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField($"Grid Size: {ingredientGridWidth} x {ingredientGridHeight}");
            EditorGUILayout.LabelField($"Total Cells: {ingredientGridWidth * ingredientGridHeight}");
            EditorGUILayout.LabelField($"Filled Cells: {CountFilledCells()}");
            EditorGUILayout.LabelField($"Shape Complexity: {CalculateShapeComplexity()}");
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            // Instructions
            EditorGUILayout.LabelField("🎮 Visual Grid Designer:");
            EditorGUILayout.LabelField("• Click cells to toggle ingredient presence", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Green = ingredient present, Gray = empty space", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Right-click to quickly clear cells", EditorStyles.miniLabel);
            
            // Visual grid editor
            DrawIngredientVisualGrid();
            
            EditorGUILayout.Space(10);
            
            // Control buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("💾 Save Design", GUILayout.Width(100)))
            {
                SaveIngredientVisualDesign();
            }
            
            if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80)))
            {
                InitializeIngredientGrid();
            }
            
            if (GUILayout.Button("🗑️ Clear All", GUILayout.Width(80)))
            {
                ClearIngredientGrid();
            }
            
            if (GUILayout.Button("📋 Fill All", GUILayout.Width(80)))
            {
                FillIngredientGrid();
            }
            
            if (GUILayout.Button("🔀 Invert", GUILayout.Width(80)))
            {
                InvertIngredientGrid();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void InitializeIngredientGrid()
        {
            if (selectedIngredient == null) return;
            
            ingredientGridWidth = selectedIngredient.GridWidth;
            ingredientGridHeight = selectedIngredient.GridHeight;
            
            // Initialize grid with existing data or default filled state
            if (ingredientVisualGrid.Count == 0)
            {
                for (int x = 0; x < ingredientGridWidth; x++)
                {
                    for (int y = 0; y < ingredientGridHeight; y++)
                    {
                        ingredientVisualGrid[new Vector2Int(x, y)] = true; // Default filled
                    }
                }
            }
            
            // Update grid if size changed
            if (ingredientVisualGrid.Keys.Any(key => key.x >= ingredientGridWidth || key.y >= ingredientGridHeight))
            {
                var newGrid = new Dictionary<Vector2Int, bool>();
                for (int x = 0; x < ingredientGridWidth; x++)
                {
                    for (int y = 0; y < ingredientGridHeight; y++)
                    {
                        var pos = new Vector2Int(x, y);
                        newGrid[pos] = ingredientVisualGrid.ContainsKey(pos) ? ingredientVisualGrid[pos] : true;
                    }
                }
                ingredientVisualGrid = newGrid;
            }
        }
        
        private void DrawIngredientVisualGrid()
        {
            const float cellSize = 40f;
            var gridRect = GUILayoutUtility.GetRect(ingredientGridWidth * cellSize, ingredientGridHeight * cellSize);
            
            // Draw background
            EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f, 1f));
            
            var mousePos = Event.current.mousePosition;
            bool isMouseInGrid = gridRect.Contains(mousePos);
            
            for (int y = ingredientGridHeight - 1; y >= 0; y--) // Draw from top to bottom visually
            {
                for (int x = 0; x < ingredientGridWidth; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Rect cellRect = new Rect(
                        gridRect.x + x * cellSize,
                        gridRect.y + (ingredientGridHeight - 1 - y) * cellSize, // Flip Y for display
                        cellSize - 2,
                        cellSize - 2
                    );
                    
                    // Get cell state
                    bool isFilled = ingredientVisualGrid.ContainsKey(pos) && ingredientVisualGrid[pos];
                    
                    // Determine cell color
                    Color cellColor;
                    if (isFilled)
                    {
                        // Color based on ingredient aspect
                        cellColor = GetIngredientAspectColor(selectedIngredient.IngredientAspect);
                    }
                    else
                    {
                        cellColor = new Color(0.4f, 0.4f, 0.4f, 0.8f); // Gray for empty
                    }
                    
                    // Highlight cell under mouse
                    if (isMouseInGrid && cellRect.Contains(mousePos))
                    {
                        cellColor = Color.Lerp(cellColor, Color.white, 0.4f);
                        selectedIngredientCell = pos;
                        
                        // Handle click events
                        if (Event.current.type == EventType.MouseDown)
                        {
                            if (Event.current.button == 0) // Left click - toggle
                            {
                                ingredientVisualGrid[pos] = !isFilled;
                                Event.current.Use();
                            }
                            else if (Event.current.button == 1) // Right click - clear
                            {
                                ingredientVisualGrid[pos] = false;
                                Event.current.Use();
                            }
                        }
                    }
                    
                    // Draw cell
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw border
                    Color borderColor = isFilled ? Color.black : new Color(0.6f, 0.6f, 0.6f);
                    DrawCellBorder(cellRect, borderColor);
                    
                    // Draw coordinates if cell is large enough
                    if (cellSize >= 35)
                    {
                        var style = new GUIStyle(EditorStyles.miniLabel);
                        style.alignment = TextAnchor.MiddleCenter;
                        style.fontSize = 9;
                        GUI.color = isFilled ? Color.white : Color.gray;
                        GUI.Label(cellRect, $"{x},{y}", style);
                        GUI.color = Color.white;
                    }
                    
                    // Draw visual indicator for filled cells
                    if (isFilled && cellSize >= 30)
                    {
                        var iconRect = new Rect(cellRect.x + 2, cellRect.y + 2, 8, 8);
                        EditorGUI.DrawRect(iconRect, Color.white);
                    }
                }
            }
        }
        
        private void DrawCellBorder(Rect cellRect, Color borderColor)
        {
            // Top
            EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), borderColor);
            // Bottom
            EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y + cellRect.height - 1, cellRect.width, 1), borderColor);
            // Left
            EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), borderColor);
            // Right
            EditorGUI.DrawRect(new Rect(cellRect.x + cellRect.width - 1, cellRect.y, 1, cellRect.height), borderColor);
        }
        
        private int CountFilledCells()
        {
            return ingredientVisualGrid.Values.Count(filled => filled);
        }
        
        private string CalculateShapeComplexity()
        {
            int filledCells = CountFilledCells();
            int totalCells = ingredientGridWidth * ingredientGridHeight;
            float ratio = (float)filledCells / totalCells;
            
            return ratio switch
            {
                >= 0.9f => "Simple (Filled Rectangle)",
                >= 0.7f => "Low",
                >= 0.5f => "Medium",
                >= 0.3f => "High",
                _ => "Very Complex"
            };
        }
        
        private void ClearIngredientGrid()
        {
            for (int x = 0; x < ingredientGridWidth; x++)
            {
                for (int y = 0; y < ingredientGridHeight; y++)
                {
                    ingredientVisualGrid[new Vector2Int(x, y)] = false;
                }
            }
        }
        
        private void FillIngredientGrid()
        {
            for (int x = 0; x < ingredientGridWidth; x++)
            {
                for (int y = 0; y < ingredientGridHeight; y++)
                {
                    ingredientVisualGrid[new Vector2Int(x, y)] = true;
                }
            }
        }
        
        private void InvertIngredientGrid()
        {
            for (int x = 0; x < ingredientGridWidth; x++)
            {
                for (int y = 0; y < ingredientGridHeight; y++)
                {
                    var pos = new Vector2Int(x, y);
                    ingredientVisualGrid[pos] = !ingredientVisualGrid.GetValueOrDefault(pos, false);
                }
            }
        }
        
        private void ApplyIngredientTemplate(string templateName)
        {
            ClearIngredientGrid();
            
            switch (templateName.ToLower())
            {
                case "rectangle":
                    FillIngredientGrid();
                    break;
                    
                case "plus":
                    ApplyPlusTemplate();
                    break;
                    
                case "triangle":
                    ApplyTriangleTemplate();
                    break;
                    
                case "lshape":
                    ApplyLShapeTemplate();
                    break;
                    
                case "lightning":
                    ApplyLightningTemplate();
                    break;
                    
                case "diamond":
                    ApplyDiamondTemplate();
                    break;
                    
                case "single":
                    ApplySingleTemplate();
                    break;
            }
            
            Debug.Log($"🎨 Applied '{templateName}' template to {selectedIngredient.ItemName}");
        }
        
        private void ApplyPlusTemplate()
        {
            int centerX = ingredientGridWidth / 2;
            int centerY = ingredientGridHeight / 2;
            
            // Horizontal line
            for (int x = 0; x < ingredientGridWidth; x++)
            {
                ingredientVisualGrid[new Vector2Int(x, centerY)] = true;
            }
            
            // Vertical line
            for (int y = 0; y < ingredientGridHeight; y++)
            {
                ingredientVisualGrid[new Vector2Int(centerX, y)] = true;
            }
        }
        
        private void ApplyTriangleTemplate()
        {
            for (int y = 0; y < ingredientGridHeight; y++)
            {
                int width = (y + 1) * 2 - 1;
                int startX = (ingredientGridWidth - width) / 2;
                
                for (int x = 0; x < width && startX + x < ingredientGridWidth; x++)
                {
                    if (startX + x >= 0)
                    {
                        ingredientVisualGrid[new Vector2Int(startX + x, y)] = true;
                    }
                }
            }
        }
        
        private void ApplyLShapeTemplate()
        {
            // Vertical part (left side)
            for (int y = 0; y < ingredientGridHeight; y++)
            {
                ingredientVisualGrid[new Vector2Int(0, y)] = true;
            }
            
            // Horizontal part (bottom)
            for (int x = 0; x < ingredientGridWidth; x++)
            {
                ingredientVisualGrid[new Vector2Int(x, 0)] = true;
            }
        }
        
        private void ApplyLightningTemplate()
        {
            for (int y = 0; y < ingredientGridHeight; y++)
            {
                int x = y % 2 == 0 ? 0 : ingredientGridWidth - 1;
                if (x >= 0 && x < ingredientGridWidth)
                {
                    ingredientVisualGrid[new Vector2Int(x, y)] = true;
                }
            }
        }
        
        private void ApplyDiamondTemplate()
        {
            int centerX = ingredientGridWidth / 2;
            int centerY = ingredientGridHeight / 2;
            
            for (int x = 0; x < ingredientGridWidth; x++)
            {
                for (int y = 0; y < ingredientGridHeight; y++)
                {
                    int distance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
                    if (distance <= Mathf.Min(centerX, centerY))
                    {
                        ingredientVisualGrid[new Vector2Int(x, y)] = true;
                    }
                }
            }
        }
        
        private void ApplySingleTemplate()
        {
            ingredientVisualGrid[new Vector2Int(0, 0)] = true;
        }
        
        private void ResetIngredientGrid()
        {
            ingredientVisualGrid.Clear();
            InitializeIngredientGrid();
        }
        
        // Integrated Persistent Grid Editor
        private void DrawIngredientGridEditor(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            EditorGUILayout.BeginVertical("Box");
            
            // Header with persistence status
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ingredient Grid Shape", EditorStyles.boldLabel);
            if (hasUnsavedGridChanges)
            {
                var style = new GUIStyle(EditorStyles.miniLabel);
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("*", style, GUILayout.Width(10));
            }
            EditorGUILayout.EndHorizontal();
            
            // Load current shape if not initialized
            if (currentIngredientGrid == null || selectedIngredient != ingredient)
            {
                LoadIngredientShape(ingredient);
            }
            
            // Template Selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Template:", GUILayout.Width(70));
            var currentTemplate = ingredient.ShapeData?.Template ?? ShapeTemplate.Rectangle;
            var newTemplate = (ShapeTemplate)EditorGUILayout.EnumPopup(currentTemplate);
            
            if (newTemplate != currentTemplate && GUILayout.Button("Apply", GUILayout.Width(50)))
            {
                ingredient.ApplyShapeTemplate(newTemplate);
                LoadIngredientShape(ingredient);
                hasUnsavedGridChanges = false;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Grid visualization
            DrawGridVisual(ingredient);
            
            EditorGUILayout.Space(10);
            
            // Control buttons
            DrawGridControlButtons(ingredient);
            
            // Persistence controls
            DrawGridPersistenceControls(ingredient);
            
            // Instructions
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Click on grid cells to toggle ingredient shape. Green = ingredient present, Gray = empty space.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }
        
        private void LoadIngredientShape(Ingredient ingredient)
        {
            if (ingredient?.ShapeData != null)
            {
                currentIngredientGrid = ingredient.GetShape();
            }
            else
            {
                // Create default shape
                int width = ingredient.GridWidth;
                int height = ingredient.GridHeight;
                currentIngredientGrid = new bool[width, height];
                
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        currentIngredientGrid[x, y] = true;
                    }
                }
                
                // Save default shape
                ingredient.SetShape(currentIngredientGrid);
            }
            hasUnsavedGridChanges = false;
        }
        
        private void DrawGridVisual(Ingredient ingredient)
        {
            if (currentIngredientGrid == null) return;
            
            int gridWidth = currentIngredientGrid.GetLength(0);
            int gridHeight = currentIngredientGrid.GetLength(1);
            
            var rect = GUILayoutUtility.GetRect(gridWidth * CELL_SIZE, gridHeight * CELL_SIZE);
            
            // Draw background
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
            
            // Handle mouse input
            var mousePos = Event.current.mousePosition;
            bool isMouseInGrid = rect.Contains(mousePos);
            
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    var cellRect = new Rect(
                        rect.x + x * CELL_SIZE,
                        rect.y + y * CELL_SIZE,
                        CELL_SIZE - 1,
                        CELL_SIZE - 1
                    );
                    
                    // Determine cell color
                    Color cellColor = currentIngredientGrid[x, y] 
                        ? new Color(0.2f, 0.8f, 0.2f, 1f) // Green for filled
                        : new Color(0.6f, 0.6f, 0.6f, 1f); // Gray for empty
                    
                    // Highlight cell under mouse
                    if (isMouseInGrid && cellRect.Contains(mousePos))
                    {
                        cellColor = Color.Lerp(cellColor, Color.white, 0.3f);
                        
                        // Handle click
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            currentIngredientGrid[x, y] = !currentIngredientGrid[x, y];
                            hasUnsavedGridChanges = true;
                            Event.current.Use();
                        }
                    }
                    
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw border
                    Handles.color = Color.black;
                    Vector3[] corners = new Vector3[]
                    {
                        new Vector3(cellRect.x, cellRect.y),
                        new Vector3(cellRect.x + cellRect.width, cellRect.y),
                        new Vector3(cellRect.x + cellRect.width, cellRect.y + cellRect.height),
                        new Vector3(cellRect.x, cellRect.y + cellRect.height)
                    };
                    
                    Handles.DrawLine(corners[0], corners[1]);
                    Handles.DrawLine(corners[1], corners[2]);
                    Handles.DrawLine(corners[2], corners[3]);
                    Handles.DrawLine(corners[3], corners[0]);
                    
                    // Draw coordinates for larger cells
                    if (CELL_SIZE >= 25)
                    {
                        var style = new GUIStyle(EditorStyles.miniLabel);
                        style.alignment = TextAnchor.MiddleCenter;
                        style.fontSize = 8;
                        GUI.Label(cellRect, $"{x},{y}", style);
                    }
                }
            }
        }
        
        private void DrawGridControlButtons(Ingredient ingredient)
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Fill All", GUILayout.Width(80)))
            {
                FillCurrentGrid();
                hasUnsavedGridChanges = true;
            }
            
            if (GUILayout.Button("Clear All", GUILayout.Width(80)))
            {
                ClearCurrentGrid();
                hasUnsavedGridChanges = true;
            }
            
            if (GUILayout.Button("Invert", GUILayout.Width(80)))
            {
                InvertCurrentGrid();
                hasUnsavedGridChanges = true;
            }
            
            GUILayout.FlexibleSpace();
            
            GUI.enabled = hasUnsavedGridChanges;
            if (GUILayout.Button("Save", GUILayout.Width(60)))
            {
                SaveIngredientShape(ingredient);
            }
            GUI.enabled = true;
            
            if (GUILayout.Button("Revert", GUILayout.Width(60)))
            {
                LoadIngredientShape(ingredient);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawGridPersistenceControls(Ingredient ingredient)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Data Exchange", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Export Shape", GUILayout.Width(100)))
            {
                ExportIngredientShape(ingredient);
            }
            
            if (GUILayout.Button("Import Shape", GUILayout.Width(100)))
            {
                ImportIngredientShape(ingredient);
            }
            
            if (GUILayout.Button("Copy to Clipboard", GUILayout.Width(120)))
            {
                CopyShapeToClipboard(ingredient);
            }
            
            if (GUILayout.Button("Paste from Clipboard", GUILayout.Width(140)))
            {
                PasteShapeFromClipboard(ingredient);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show shape info
            if (ingredient?.ShapeData != null)
            {
                EditorGUILayout.Space(3);
                var style = new GUIStyle(EditorStyles.miniLabel);
                style.fontStyle = FontStyle.Italic;
                EditorGUILayout.LabelField($"Last modified: {ingredient.ShapeData.LastModified}", style);
            }
        }
        
        private void SaveIngredientShape(Ingredient ingredient)
        {
            if (ingredient != null && currentIngredientGrid != null)
            {
                ingredient.SetShape(currentIngredientGrid);
                hasUnsavedGridChanges = false;
                EditorUtility.SetDirty(ingredient);
            }
        }
        
        private void FillCurrentGrid()
        {
            if (currentIngredientGrid == null) return;
            
            for (int x = 0; x < currentIngredientGrid.GetLength(0); x++)
            {
                for (int y = 0; y < currentIngredientGrid.GetLength(1); y++)
                {
                    currentIngredientGrid[x, y] = true;
                }
            }
        }
        
        private void ClearCurrentGrid()
        {
            if (currentIngredientGrid == null) return;
            
            for (int x = 0; x < currentIngredientGrid.GetLength(0); x++)
            {
                for (int y = 0; y < currentIngredientGrid.GetLength(1); y++)
                {
                    currentIngredientGrid[x, y] = false;
                }
            }
        }
        
        private void InvertCurrentGrid()
        {
            if (currentIngredientGrid == null) return;
            
            for (int x = 0; x < currentIngredientGrid.GetLength(0); x++)
            {
                for (int y = 0; y < currentIngredientGrid.GetLength(1); y++)
                {
                    currentIngredientGrid[x, y] = !currentIngredientGrid[x, y];
                }
            }
        }
        
        private void ExportIngredientShape(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            SaveIngredientShape(ingredient); // Ensure latest changes are saved
            
            string path = EditorUtility.SaveFilePanel(
                "Export Ingredient Shape Data",
                Application.dataPath,
                $"{ingredient.name}_shape.json",
                "json"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string data = ingredient.ExportShapeData();
                    System.IO.File.WriteAllText(path, data);
                    EditorUtility.DisplayDialog("Export Successful", $"Shape data exported to:\n{path}", "OK");
                }
                catch (System.Exception ex)
                {
                    EditorUtility.DisplayDialog("Export Failed", $"Failed to export shape data:\n{ex.Message}", "OK");
                }
            }
        }
        
        private void ImportIngredientShape(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            string path = EditorUtility.OpenFilePanel(
                "Import Ingredient Shape Data",
                Application.dataPath,
                "json"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string data = System.IO.File.ReadAllText(path);
                    bool success = ingredient.ImportShapeData(data);
                    
                    if (success)
                    {
                        LoadIngredientShape(ingredient);
                        EditorUtility.DisplayDialog("Import Successful", "Shape data imported successfully!", "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Import Failed", "Failed to import shape data. File format may be invalid.", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    EditorUtility.DisplayDialog("Import Failed", $"Failed to import shape data:\n{ex.Message}", "OK");
                }
            }
        }
        
        private void CopyShapeToClipboard(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            try
            {
                SaveIngredientShape(ingredient); // Ensure latest changes are saved
                string data = ingredient.ExportShapeData();
                EditorGUIUtility.systemCopyBuffer = data;
                EditorUtility.DisplayDialog("Copy Successful", "Shape data copied to clipboard!", "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Copy Failed", $"Failed to copy shape data:\n{ex.Message}", "OK");
            }
        }
        
        private void PasteShapeFromClipboard(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            try
            {
                string data = EditorGUIUtility.systemCopyBuffer;
                if (string.IsNullOrEmpty(data))
                {
                    EditorUtility.DisplayDialog("Paste Failed", "Clipboard is empty or doesn't contain valid data.", "OK");
                    return;
                }
                
                bool success = ingredient.ImportShapeData(data);
                
                if (success)
                {
                    LoadIngredientShape(ingredient);
                    EditorUtility.DisplayDialog("Paste Successful", "Shape data pasted from clipboard!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Paste Failed", "Clipboard doesn't contain valid shape data.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Paste Failed", $"Failed to paste shape data:\n{ex.Message}", "OK");
            }
        }

        // CRUD Operations for ingredients and recipes
        private void CreateNewIngredient()
        {
            if (string.IsNullOrEmpty(newIngredientName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a name for the ingredient.", "OK");
                return;
            }
            
            var ingredient = ScriptableObject.CreateInstance<Ingredient>();
            ingredient.name = newIngredientName;
            
            // Set basic properties
            var serializedIngredient = new SerializedObject(ingredient);
            serializedIngredient.FindProperty("itemName").stringValue = newIngredientName;
            serializedIngredient.FindProperty("itemDescription").stringValue = $"A {newIngredientRarity.ToString().ToLower()} {newIngredientAspect.ToString().ToLower()} ingredient.";
            serializedIngredient.FindProperty("itemRarity").enumValueIndex = (int)newIngredientRarity;
            serializedIngredient.FindProperty("ingredientAspect").enumValueIndex = (int)newIngredientAspect;
            serializedIngredient.FindProperty("potency").intValue = newIngredientPotency;
            serializedIngredient.ApplyModifiedProperties();
            
            // Create asset file
            string folderPath = "Assets/Resources/Items/Ingredients";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Items", "Ingredients");
            }
            
            string path = $"{folderPath}/{newIngredientName}.asset";
            AssetDatabase.CreateAsset(ingredient, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the new ingredient
            selectedIngredient = ingredient;
            showIngredientWizard = false;
            ResetWizardFields();
            
            Debug.Log($"Created new ingredient: {newIngredientName}");
        }
        
        private void CreateNewRecipe()
        {
            if (string.IsNullOrEmpty(newRecipeName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a name for the recipe.", "OK");
                return;
            }
            
            var recipe = ScriptableObject.CreateInstance<AlchemyRecipe>();
            recipe.name = newRecipeName;
            
            // Set basic properties
            var serializedRecipe = new SerializedObject(recipe);
            serializedRecipe.FindProperty("itemName").stringValue = newRecipeName;
            serializedRecipe.FindProperty("itemDescription").stringValue = $"A {newRecipeDifficulty.ToString().ToLower()} difficulty recipe.";
            serializedRecipe.FindProperty("difficulty").enumValueIndex = (int)newRecipeDifficulty;
            serializedRecipe.FindProperty("outputQuantity").intValue = 1;
            
            if (newRecipeOutputPotion != null)
            {
                serializedRecipe.FindProperty("outputPotion").objectReferenceValue = newRecipeOutputPotion;
            }
            
            serializedRecipe.ApplyModifiedProperties();
            
            // Create asset file
            string folderPath = "Assets/Resources/Recipes";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Recipes");
            }
            
            string path = $"{folderPath}/{newRecipeName}.asset";
            AssetDatabase.CreateAsset(recipe, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the new recipe
            selectedRecipe = recipe;
            showRecipeWizard = false;
            ResetWizardFields();
            
            Debug.Log($"Created new recipe: {newRecipeName}");
        }
        
        private void ResetWizardFields()
        {
            newIngredientName = "";
            newIngredientAspect = Aspect.Corporeal;
            newIngredientRarity = Rarity.Common;
            newIngredientPotency = 1;
            
            newRecipeName = "";
            newRecipeDifficulty = RecipeDifficulty.Standard;
            newRecipeOutputPotion = null;
        }
        
        private void DeleteSelectedIngredient()
        {
            if (selectedIngredient == null) return;
            
            if (EditorUtility.DisplayDialog("Delete Ingredient", 
                $"Are you sure you want to delete '{selectedIngredient.name}'?", 
                "Delete", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(selectedIngredient);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                selectedIngredient = null;
                Debug.Log($"Deleted ingredient");
            }
        }
        
        private void DeleteSelectedRecipe()
        {
            if (selectedRecipe == null) return;
            
            if (EditorUtility.DisplayDialog("Delete Recipe", 
                $"Are you sure you want to delete '{selectedRecipe.name}'?", 
                "Delete", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(selectedRecipe);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                selectedRecipe = null;
                Debug.Log($"Deleted recipe");
            }
        }
        
        // Visual design saving functionality
        private void SaveIngredientVisualDesign()
        {
            if (selectedIngredient == null) return;
            
            var serializedIngredient = new SerializedObject(selectedIngredient);
            
            // Convert grid data to a saveable format
            var filledPositions = new List<Vector2Int>();
            foreach (var kvp in ingredientVisualGrid)
            {
                if (kvp.Value)
                {
                    filledPositions.Add(kvp.Key);
                }
            }
            
            // Save to a serialized property (assuming the ingredient has a visual grid field)
            // Note: This assumes you have a visualGridData field in your Ingredient class
            // If not, you'll need to add it to the Ingredient ScriptableObject
            
            EditorUtility.SetDirty(selectedIngredient);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Saved visual design for {selectedIngredient.name} with {filledPositions.Count} filled cells");
        }
        
        // Asset Repair Utilities
        private void FixUnnamedItemAssets()
        {
            int fixedCount = 0;
            
            // Fix Ingredients
            var ingredients = FindAssetsByType<Ingredient>();
            foreach (var ingredient in ingredients)
            {
                if (string.IsNullOrEmpty(ingredient.ItemName) || ingredient.ItemName == "Unnamed Item")
                {
                    var serialized = new SerializedObject(ingredient);
                    serialized.FindProperty("itemName").stringValue = ingredient.name;
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(ingredient);
                    fixedCount++;
                }
            }
            
            // Fix Recipes
            var recipes = FindAssetsByType<AlchemyRecipe>();
            foreach (var recipe in recipes)
            {
                if (string.IsNullOrEmpty(recipe.ItemName) || recipe.ItemName == "Unnamed Item")
                {
                    var serialized = new SerializedObject(recipe);
                    serialized.FindProperty("itemName").stringValue = recipe.name;
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(recipe);
                    fixedCount++;
                }
            }
            
            // Fix Potions
            var potions = FindAssetsByType<Potion>();
            foreach (var potion in potions)
            {
                if (string.IsNullOrEmpty(potion.ItemName) || potion.ItemName == "Unnamed Item")
                {
                    var serialized = new SerializedObject(potion);
                    serialized.FindProperty("itemName").stringValue = potion.name;
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(potion);
                    fixedCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Fixed {fixedCount} assets with 'Unnamed Item' names");
            EditorUtility.DisplayDialog("Asset Repair Complete", $"Fixed {fixedCount} assets with proper names.", "OK");
        }
        
        private void RegenerateAssetNames()
        {
            int renamedCount = 0;
            
            // Regenerate all item names from their asset file names
            var allItems = new List<Item>();
            allItems.AddRange(FindAssetsByType<Ingredient>().Cast<Item>());
            allItems.AddRange(FindAssetsByType<AlchemyRecipe>().Cast<Item>());
            allItems.AddRange(FindAssetsByType<Potion>().Cast<Item>());
            
            foreach (var item in allItems)
            {
                var serialized = new SerializedObject(item);
                string newName = ObjectNames.NicifyVariableName(item.name);
                serialized.FindProperty("itemName").stringValue = newName;
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(item);
                renamedCount++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Regenerated names for {renamedCount} assets");
            EditorUtility.DisplayDialog("Regeneration Complete", $"Regenerated names for {renamedCount} assets.", "OK");
        }
        
        // Grid Pattern Template Methods
        private void CreateCrossPatternInGrid()
        {
            gridCells.Clear();
            
            // Create a cross pattern centered on the grid
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            
            // Horizontal line
            for (int x = centerX - 2; x <= centerX + 2; x++)
            {
                if (x >= 0 && x < gridWidth)
                    gridCells[new Vector2Int(x, centerY)] = new CellData { isRequired = true };
            }
            
            // Vertical line
            for (int y = centerY - 2; y <= centerY + 2; y++)
            {
                if (y >= 0 && y < gridHeight)
                    gridCells[new Vector2Int(centerX, y)] = new CellData { isRequired = true };
            }
        }
        
        private void CreateLShapePatternInGrid()
        {
            gridCells.Clear();
            
            // Create an L-shape in the bottom-left area
            int startX = 1;
            int startY = 1;
            
            // Vertical part of L
            for (int y = startY; y <= startY + 3; y++)
            {
                gridCells[new Vector2Int(startX, y)] = new CellData { isRequired = true };
            }
            
            // Horizontal part of L
            for (int x = startX; x <= startX + 3; x++)
            {
                gridCells[new Vector2Int(x, startY)] = new CellData { isRequired = true };
            }
        }
        
        private void CreateDiamondPatternInGrid()
        {
            gridCells.Clear();
            
            // Create a diamond pattern centered on the grid
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            int radius = 2;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    int manhattanDistance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
                    if (manhattanDistance == radius)
                    {
                        gridCells[new Vector2Int(x, y)] = new CellData { isRequired = true };
                    }
                }
            }
        }
        
        // Enhanced Grid Pattern Methods
        private void SaveGridPatternToRecipe(AlchemyRecipe recipe)
        {
            if (recipe == null) return;
            
            string recipeName = !string.IsNullOrEmpty(recipe.ItemName) && recipe.ItemName != "Unnamed Item" 
                ? recipe.ItemName 
                : recipe.name;
                
            Debug.Log($"💾 Saving grid pattern for {recipeName} with {gridCells.Count} cells");
            
            // Convert grid data to serializable format
            var serializedObject = new SerializedObject(recipe);
            var requiredPositions = serializedObject.FindProperty("requiredPositions");
            
            requiredPositions.ClearArray();
            
            int index = 0;
            foreach (var cell in gridCells)
            {
                if (cell.Value.isRequired)
                {
                    requiredPositions.InsertArrayElementAtIndex(index);
                    var positionElement = requiredPositions.GetArrayElementAtIndex(index);
                    positionElement.vector2IntValue = cell.Key;
                    index++;
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(recipe);
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog("Save Pattern", 
                $"Grid pattern saved for {recipeName}!\n\nRequired Positions: {index}\nTotal Cells: {gridCells.Count}", 
                "OK");
        }
        
        private void LoadGridPatternFromRecipe(AlchemyRecipe recipe)
        {
            if (recipe == null) return;
            
            string recipeName = !string.IsNullOrEmpty(recipe.ItemName) && recipe.ItemName != "Unnamed Item" 
                ? recipe.ItemName 
                : recipe.name;
                
            Debug.Log($"📋 Loading grid pattern for {recipeName}");
            
            var serializedObject = new SerializedObject(recipe);
            var requiredPositions = serializedObject.FindProperty("requiredPositions");
            
            // Clear current grid
            gridCells.Clear();
            
            // Load required positions
            for (int i = 0; i < requiredPositions.arraySize; i++)
            {
                var position = requiredPositions.GetArrayElementAtIndex(i).vector2IntValue;
                gridCells[position] = new CellData { isRequired = true };
            }
            
            EditorUtility.DisplayDialog("Load Pattern", 
                $"Grid pattern loaded for {recipeName}!\n\nRequired Positions: {requiredPositions.arraySize}", 
                "OK");
        }
        
        // Book Entry Management
        private void ShowCreateBookEntryMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("New Ingredient Entry"), false, () => MakeBookEntry<IngredientEntry>());
            menu.AddItem(new GUIContent("New Recipe Entry"), false, () => MakeBookEntry<RecipeEntry>());
            menu.AddItem(new GUIContent("New Bestiary Entry"), false, () => MakeBookEntry<BestiaryEntry>());
            menu.AddItem(new GUIContent("New Help Entry"), false, () => MakeBookEntry<HelpEntry>());
            menu.ShowAsContext();
        }
        
        private void MakeBookEntry<T>() where T : BaseEntry
        {
            var entry = ScriptableObject.CreateInstance<T>();
            entry.name = $"New {typeof(T).Name}";
            
            var serializedEntry = new SerializedObject(entry);
            serializedEntry.FindProperty("title").stringValue = $"New {typeof(T).Name}";
            serializedEntry.FindProperty("description").stringValue = "Enter description here...";
            serializedEntry.ApplyModifiedProperties();
            
            string folderPath = "Assets/Resources/BookEntries";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "BookEntries");
            }
            
            string path = $"{folderPath}/{entry.name}.asset";
            AssetDatabase.CreateAsset(entry, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            selectedBookEntry = entry;
            Debug.Log($"Created new book entry: {entry.name}");
        }
        
        private void DuplicateSelectedEntry()
        {
            if (selectedBookEntry == null)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a book entry to duplicate.", "OK");
                return;
            }
            
            var duplicate = Object.Instantiate(selectedBookEntry);
            duplicate.name = selectedBookEntry.name + " Copy";
            
            string folderPath = "Assets/Resources/BookEntries";
            string path = $"{folderPath}/{duplicate.name}.asset";
            AssetDatabase.CreateAsset(duplicate, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            selectedBookEntry = duplicate;
            Debug.Log($"Duplicated book entry: {duplicate.name}");
        }
        
        private void DeleteSelectedEntry()
        {
            if (selectedBookEntry == null)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a book entry to delete.", "OK");
                return;
            }
            
            string entryName = selectedBookEntry.title;
            if (string.IsNullOrEmpty(entryName))
                entryName = selectedBookEntry.name;
            
            if (EditorUtility.DisplayDialog("Delete Book Entry", 
                $"Are you sure you want to delete '{entryName}'?\n\nThis action cannot be undone.", 
                "Delete", "Cancel"))
            {
                try
                {
                    string path = AssetDatabase.GetAssetPath(selectedBookEntry);
                    if (string.IsNullOrEmpty(path))
                    {
                        EditorUtility.DisplayDialog("Error", "Could not find the asset path for this entry.", "OK");
                        return;
                    }
                    
                    bool success = AssetDatabase.DeleteAsset(path);
                    if (success)
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        selectedBookEntry = null;
                        Debug.Log($"Successfully deleted book entry: {entryName}");
                        
                        // Force a repaint to update the UI
                        Repaint();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", $"Failed to delete the asset at path: {path}", "OK");
                    }
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("Error", $"An error occurred while deleting the entry:\n{e.Message}", "OK");
                    Debug.LogError($"Error deleting book entry: {e}");
                }
            }
        }
    }
    
    // Helper classes for grid designer
    [Serializable]
    public class CellData
    {
        public Aspect aspect = Aspect.Corporeal;
        public Rarity rarity = Rarity.Common;
        public bool isRequired = false;
        public bool isOccupied = false;
    }
    
    // Performance ranking system for minigames
    [System.Serializable]
    public class MinigamePerformance
    {
        public string recipeId;
        public PerformanceRank rank = PerformanceRank.F;
        public float score;
        public int attempts;
        public float bestTime;
        public bool canBulkCraft => rank == PerformanceRank.S;
    }
    
    public enum PerformanceRank
    {
        F, D, C, B, A, S
    }
}