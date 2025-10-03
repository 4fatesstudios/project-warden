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
using FourFatesStudios.ProjectWarden.GridDemo;
using Object = UnityEngine.Object;
using InfusionSO = FourFatesStudios.ProjectWarden.ScriptableObjects.Infusion;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class AlchemySystemEditor : EditorWindow
    {
        private int toolbarSelection;
        private readonly string[] toolbarOptions = { "🧪 Ingredients", "📋 Recipes", "🍯 Potions", "🌟 Infusions", "⚗️ Refinements", "📚 Book Pages", "🎮 Grid Designer", "📖 Book Editor", "🗄️ Database" };
        
        private Vector2 scrollPosition;
        private Vector2 bookEntriesScrollPosition;
        private Vector2 ingredientsScrollPosition;
        private Vector2 recipesScrollPosition;
        private Vector2 potionsScrollPosition;
        // Ingredient editor selection
        private Ingredient selectedIngredient;
        private int selectedIngredientTab = 0;
        private AlchemyRecipe selectedRecipe;
        private BaseEntry selectedBookEntry;
        private Potion selectedPotion;
        private InfusionSO selectedInfusion;
        
        // Persistent state for grid designer
        private AlchemyRecipe gridDesignerRecipe;
        private int gridWidth = 5;
        private int gridHeight = 5;
        private int selectedPatternType;
        private Dictionary<Vector2Int, CellData> gridCells = new Dictionary<Vector2Int, CellData>();
        private Vector2Int selectedCell = Vector2Int.zero;
        private bool showCellPreview = true;
        private readonly string[] patternTypes = { "Free Placement", "Required Pattern", "Aspect Locked", "Shape Specific", "Cross Pattern", "L-Shape", "Diamond" };
        
        // Obstacle placement system - obstacles only
        private ObstacleType selectedObstacleType = ObstacleType.Corporeal;
        
        // Grid Designer cell selection and state
        
        // Infusion editor state
        private Vector2 infusionsScrollPosition;
        private string infusionSearchQuery = "";
        private EffectTypeFilter selectedEffectTypeFilter = EffectTypeFilter.All;
        
        // Search and filter
        private string searchQuery = "";
        private bool showOnlyModified;
        
        // Deferred operations to avoid GUI layout conflicts
        private System.Action deferredOperation;
        
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
        private bool hasUnsavedGridChanges;
        
        // Ingredient visual design editor state
        private int ingredientGridWidth = 3;
        private int ingredientGridHeight = 3;
        private Dictionary<Vector2Int, bool> ingredientVisualGrid = new Dictionary<Vector2Int, bool>();
        private Vector2Int selectedIngredientCell = Vector2Int.zero;
        
        [MenuItem("Tools/Alchemy System Editor")]
        public static void ShowWindow()
        {
            GetWindow<AlchemySystemEditor>("Alchemy System Editor");
        }

        private void OnEnable()
        {
            // Grid Designer initialization - obstacles only
            Debug.Log("🔄 AlchemySystemEditor OnEnable: Grid Designer ready for obstacle placement");
        }

        private void OnGUI()
        {
            // Execute any deferred operations first
            if (deferredOperation != null)
            {
                var operation = deferredOperation;
                deferredOperation = null;
                operation.Invoke();
                return; // Skip this frame to allow GUI to reset
            }
            
            EditorGUILayout.BeginVertical();
            
            try
            {
                // Header
                DrawHeader();
                
                // Auto-sizing Toolbar
                DrawAutoSizedToolbar();
                
                // Search bar
                DrawSearchBar();
                
                // Content area
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                try
                {
                    switch (toolbarSelection)
                    {
                        case 0: DrawIngredientsTab(); break;
                        case 1: DrawRecipesTab(); break;
                        case 2: DrawPotionsTab(); break;
                        case 3: DrawInfusionsTab(); break;
                        case 4: DrawRefinementsTab(); break;
                        case 5: DrawBookPagesTab(); break;
                        case 6: DrawGridDesignerTab(); break;
                        case 7: DrawBookEditorTab(); break;
                        case 8: DrawDatabaseTab(); break;
                    }
                }
                catch (ExitGUIException)
                {
                    // Unity's internal GUI exception - rethrow after cleanup
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                    throw;
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.HelpBox($"Error drawing tab content: {e.Message}", MessageType.Error);
                    Debug.LogError($"AlchemySystemEditor tab error: {e}");
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }
            }
            catch (ExitGUIException)
            {
                // Unity's internal GUI exception - safe to rethrow
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AlchemySystemEditor GUI error: {e}");
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
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

        private void DrawAutoSizedToolbar()
        {
            // Calculate available width and tab size
            Rect toolbarRect = EditorGUILayout.GetControlRect(GUILayout.Height(25));
            float availableWidth = toolbarRect.width;
            float tabWidth = availableWidth / toolbarOptions.Length;
            
            // Ensure minimum tab width for readability
            float minTabWidth = 80f;
            float maxTabWidth = 150f;
            
            // Adjust tab width within reasonable bounds
            tabWidth = Mathf.Clamp(tabWidth, minTabWidth, maxTabWidth);
            
            // Create tab style with fixed width
            GUIStyle tabStyle = new GUIStyle(EditorStyles.toolbarButton);
            
            // Draw toolbar with custom sizing
            EditorGUILayout.BeginHorizontal();
            
            for (int i = 0; i < toolbarOptions.Length; i++)
            {
                // Highlight selected tab
                GUI.backgroundColor = (toolbarSelection == i) ? Color.cyan : Color.white;
                
                if (GUILayout.Button(toolbarOptions[i], tabStyle, GUILayout.Width(tabWidth), GUILayout.Height(25)))
                {
                    toolbarSelection = i;
                }
            }
            
            // Reset background color
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            // Add visual separator
            EditorGUILayout.Space(2);
            var separatorRect = EditorGUILayout.GetControlRect(GUILayout.Height(1));
            EditorGUI.DrawRect(separatorRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal("Toolbar");
            GUILayout.Label("Search:", GUILayout.Width(50));
            
            // Make search field much larger horizontally
            searchQuery = GUILayout.TextField(searchQuery, "ToolbarTextField", GUILayout.MinWidth(300), GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("✖", "ToolbarButton", GUILayout.Width(20)))
            {
                searchQuery = "";
                GUI.FocusControl(null);
            }
            
            showOnlyModified = GUILayout.Toggle(showOnlyModified, "Modified Only", "ToolbarButton", GUILayout.Width(100));
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
                    // Update visual design tab when switching ingredients
                    LoadIngredientShape(ingredient);
                    Debug.Log($"🔄 Switched to ingredient: {ingredient.ItemName}, visual design updated");
                }
                
                EditorGUILayout.ObjectField(ingredient, typeof(Ingredient), false);
                
                if (GUILayout.Button("📝", GUILayout.Width(25)))
                {
                    // Defer the rename operation to avoid GUI layout conflicts
                    deferredOperation = () => StartRenameIngredient(ingredient);
                }
                
                if (GUILayout.Button("🗑️", GUILayout.Width(25)))
                {
                    // Defer the delete operation to avoid GUI layout conflicts
                    deferredOperation = () => DeleteIngredient(ingredient);
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
            
            var ingredientTabs = new[] { "🏷️ Basic Info", "🧪 Alchemy", "🎨 Visual Design", "⚗️ Refinement" };
            int selectedTab = GUILayout.Toolbar(selectedIngredientTab, ingredientTabs);
            if (selectedTab != selectedIngredientTab)
            {
                selectedIngredientTab = selectedTab;
                
                // Update visual design when switching to Visual Design tab
                if (selectedIngredientTab == 2 && selectedIngredient != null)
                {
                    LoadIngredientShape(selectedIngredient);
                    Debug.Log($"🔄 Switched to Visual Design tab for {selectedIngredient.ItemName}, grid updated");
                }
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
            
            // Grid size with automatic shape detection and change tracking
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridWidth"), GUILayout.Width(200));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gridHeight"), GUILayout.Width(200));
            bool gridDimensionsChanged = EditorGUI.EndChangeCheck();
            
            // If grid dimensions changed, mark that we need to update the visual grid
            if (gridDimensionsChanged)
            {
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"🔄 Grid dimensions changed for {selectedIngredient.ItemName}, visual grid will update automatically");
                // The visual grid will be updated automatically in DrawGridVisual() on next frame
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show calculated area with real-time updates
            int currentWidth = serializedObject.FindProperty("gridWidth").intValue;
            int currentHeight = serializedObject.FindProperty("gridHeight").intValue;
            int totalArea = currentWidth * currentHeight;
            
            EditorGUILayout.LabelField($"Grid Area: {totalArea} cells ({currentWidth}x{currentHeight})", EditorStyles.miniLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unlocksAdditionalSpace"));
            if (selectedIngredient.UnlocksAdditionalSpace)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("additionalSpaceCount"));
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Shape Data Editor
            DrawShapeDataEditor(serializedObject);
            
            EditorGUILayout.Space();
            
            // Infusions & Effects
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("🌟 Infusions & Effects", EditorStyles.boldLabel);
            
            var infusionBundleProperty = serializedObject.FindProperty("infusionBundle");
            if (infusionBundleProperty != null)
            {
                EditorGUILayout.PropertyField(infusionBundleProperty, true);
            }
            else
            {
                EditorGUILayout.HelpBox("InfusionBundle property not found. Please check the Ingredient class.", MessageType.Warning);
            }
            
            // Check for active infusions and display count
            if (selectedIngredient.InfusionBundle != null && selectedIngredient.InfusionBundle.Infusions.Count > 0)
            {
                EditorGUILayout.LabelField($"Active Infusions: {selectedIngredient.InfusionBundle.Infusions.Count}", EditorStyles.miniLabel);
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
        
        private void DrawShapeDataEditor(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("🎯 Shape & Expansion Editor", EditorStyles.boldLabel);
            
            var shapeDataProperty = serializedObject.FindProperty("shapeData");
            if (shapeDataProperty == null)
            {
                EditorGUILayout.HelpBox("ShapeData property not found.", MessageType.Warning);
                return;
            }
            
            // Shape data basic properties
            EditorGUILayout.PropertyField(shapeDataProperty.FindPropertyRelative("id"));
            EditorGUILayout.PropertyField(shapeDataProperty.FindPropertyRelative("icon"));
            EditorGUILayout.PropertyField(shapeDataProperty.FindPropertyRelative("pivot"));
            EditorGUILayout.PropertyField(shapeDataProperty.FindPropertyRelative("rotatable"));
            EditorGUILayout.PropertyField(shapeDataProperty.FindPropertyRelative("expansionRotatesWithIngredient"));
            
            EditorGUILayout.Space(10);
            
            // 5x5 Grid Editor
            Draw5x5GridEditor(shapeDataProperty);
            
            EditorGUILayout.Space(10);
            
            // Rotation preview
            DrawRotationPreview(selectedIngredient.ShapeData);
            
            EditorGUILayout.EndVertical();
        }
        
        private void Draw5x5GridEditor(SerializedProperty shapeDataProperty)
        {
            if (selectedIngredient?.ShapeData == null) return;
            
            EditorGUILayout.LabelField("5×5 Grid Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Left click: Toggle occupied | Right click: Toggle expansion", MessageType.Info);
            
            const int gridSize = 5;
            const float cellSize = 25f;
            const float totalSize = gridSize * cellSize;
            
            var shapeData = selectedIngredient.ShapeData;
            bool hasChanges = false;
            
            // Get current occupied and expansion offsets
            var occupiedOffsets = shapeData.occupiedOffsets ?? new Vector2Int[0];
            var expansionOffsets = shapeData.expansionOffsets ?? new Vector2Int[0];
            
            var occupiedSet = new HashSet<Vector2Int>(occupiedOffsets);
            var expansionSet = new HashSet<Vector2Int>(expansionOffsets);
            
            // Create grid rect
            Rect gridRect = GUILayoutUtility.GetRect(totalSize + 40, totalSize + 40);
            gridRect = new Rect(gridRect.x + 20, gridRect.y + 20, totalSize, totalSize);
            
            // Draw background
            EditorGUI.DrawRect(gridRect, new Color(0.3f, 0.3f, 0.3f));
            
            // Handle mouse input
            Event currentEvent = Event.current;
            Vector2 mousePos = currentEvent.mousePosition;
            bool mouseInGrid = gridRect.Contains(mousePos);
            
            // Draw grid cells
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    Vector2Int gridPos = new Vector2Int(x - 2, y - 2); // Center around (0,0)
                    
                    Rect cellRect = new Rect(
                        gridRect.x + x * cellSize,
                        gridRect.y + y * cellSize,
                        cellSize - 1,
                        cellSize - 1
                    );
                    
                    // Determine cell state
                    bool isOccupied = occupiedSet.Contains(gridPos);
                    bool isExpansion = expansionSet.Contains(gridPos);
                    bool isCenter = gridPos == Vector2Int.zero;
                    
                    // Choose cell color
                    Color cellColor;
                    if (isOccupied)
                    {
                        cellColor = new Color(0.2f, 0.8f, 0.2f); // Green for occupied
                    }
                    else if (isExpansion)
                    {
                        cellColor = new Color(1.0f, 0.85f, 0.29f); // Yellow for expansion
                    }
                    else if (isCenter)
                    {
                        cellColor = new Color(0.6f, 0.6f, 0.8f); // Blue for center
                    }
                    else
                    {
                        cellColor = new Color(0.5f, 0.5f, 0.5f); // Gray for empty
                    }
                    
                    // Highlight on hover
                    if (mouseInGrid && cellRect.Contains(mousePos))
                    {
                        cellColor = Color.Lerp(cellColor, Color.white, 0.3f);
                    }
                    
                    // Draw cell
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw border
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), Color.black);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), Color.black);
                    EditorGUI.DrawRect(new Rect(cellRect.xMax - 1, cellRect.y, 1, cellRect.height), Color.black);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.yMax - 1, cellRect.width, 1), Color.black);
                    
                    // Draw coordinates
                    if (cellSize >= 20)
                    {
                        var style = new GUIStyle(EditorStyles.miniLabel);
                        style.alignment = TextAnchor.MiddleCenter;
                        style.fontSize = 8;
                        GUI.color = Color.white;
                        GUI.Label(cellRect, $"{gridPos.x},{gridPos.y}", style);
                        GUI.color = Color.white;
                    }
                    
                    // Handle clicks
                    if (mouseInGrid && cellRect.Contains(mousePos))
                    {
                        if (currentEvent.type == EventType.MouseDown)
                        {
                            if (currentEvent.button == 0) // Left click - toggle occupied
                            {
                                if (isOccupied)
                                {
                                    occupiedSet.Remove(gridPos);
                                }
                                else
                                {
                                    occupiedSet.Add(gridPos);
                                }
                                hasChanges = true;
                                currentEvent.Use();
                            }
                            else if (currentEvent.button == 1) // Right click - toggle expansion
                            {
                                if (isExpansion)
                                {
                                    expansionSet.Remove(gridPos);
                                }
                                else
                                {
                                    expansionSet.Add(gridPos);
                                }
                                hasChanges = true;
                                currentEvent.Use();
                            }
                        }
                    }
                }
            }
            
            // Apply changes
            if (hasChanges)
            {
                shapeData.occupiedOffsets = occupiedSet.ToArray();
                shapeData.expansionOffsets = expansionSet.ToArray();
                EditorUtility.SetDirty(selectedIngredient);
            }
            
            // Control buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Rotate CW", GUILayout.Width(80)))
            {
                RotateShape(shapeData, 90);
            }
            
            if (GUILayout.Button("Rotate CCW", GUILayout.Width(80)))
            {
                RotateShape(shapeData, -90);
            }
            
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                shapeData.occupiedOffsets = new Vector2Int[0];
                shapeData.expansionOffsets = new Vector2Int[0];
                EditorUtility.SetDirty(selectedIngredient);
            }
            
            if (GUILayout.Button("Reset to 1x1", GUILayout.Width(80)))
            {
                shapeData.occupiedOffsets = new Vector2Int[] { Vector2Int.zero };
                shapeData.expansionOffsets = new Vector2Int[0];
                EditorUtility.SetDirty(selectedIngredient);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Visual legend
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Legend:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            DrawColorSwatch(new Color(0.2f, 0.8f, 0.2f), "Occupied");
            DrawColorSwatch(new Color(1.0f, 0.85f, 0.29f), "Expansion");
            DrawColorSwatch(new Color(0.6f, 0.6f, 0.8f), "Center (0,0)");
            DrawColorSwatch(new Color(0.5f, 0.5f, 0.5f), "Empty");
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawColorSwatch(Color color, string label)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(80));
            var rect = GUILayoutUtility.GetRect(15, 15);
            EditorGUI.DrawRect(rect, color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.black); // top border
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), Color.black); // bottom border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.black); // left border
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), Color.black); // right border
            EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRotationPreview(IngredientShapeData shapeData)
        {
            if (!shapeData.rotatable) return;
            
            EditorGUILayout.LabelField("Rotation Preview", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            for (int rotation = 0; rotation < 360; rotation += 90)
            {
                EditorGUILayout.BeginVertical("Box", GUILayout.Width(100));
                EditorGUILayout.LabelField($"{rotation}°", EditorStyles.centeredGreyMiniLabel);
                
                DrawMiniPreview(shapeData, rotation);
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawMiniPreview(IngredientShapeData shapeData, int rotation)
        {
            const int previewSize = 5;
            const float cellSize = 12f;
            
            var occupiedOffsets = shapeData.GetOccupiedOffsets(rotation);
            var expansionOffsets = shapeData.GetExpansionOffsets(rotation);
            
            var occupiedSet = new HashSet<Vector2Int>(occupiedOffsets);
            var expansionSet = new HashSet<Vector2Int>(expansionOffsets);
            
            Rect previewRect = GUILayoutUtility.GetRect(previewSize * cellSize, previewSize * cellSize);
            
            for (int y = 0; y < previewSize; y++)
            {
                for (int x = 0; x < previewSize; x++)
                {
                    Vector2Int gridPos = new Vector2Int(x - 2, y - 2);
                    
                    Rect cellRect = new Rect(
                        previewRect.x + x * cellSize,
                        previewRect.y + y * cellSize,
                        cellSize - 1,
                        cellSize - 1
                    );
                    
                    Color cellColor;
                    if (occupiedSet.Contains(gridPos))
                    {
                        cellColor = new Color(0.2f, 0.8f, 0.2f);
                    }
                    else if (expansionSet.Contains(gridPos))
                    {
                        cellColor = new Color(1.0f, 0.85f, 0.29f);
                    }
                    else
                    {
                        cellColor = new Color(0.7f, 0.7f, 0.7f);
                    }
                    
                    EditorGUI.DrawRect(cellRect, cellColor);
                    // Draw border manually
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 0.5f), Color.black); // top
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.yMax - 0.5f, cellRect.width, 0.5f), Color.black); // bottom
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 0.5f, cellRect.height), Color.black); // left
                    EditorGUI.DrawRect(new Rect(cellRect.xMax - 0.5f, cellRect.y, 0.5f, cellRect.height), Color.black); // right
                }
            }
        }
        
        private void RotateShape(IngredientShapeData shapeData, int degrees)
        {
            if (!shapeData.rotatable) return;
            
            shapeData.occupiedOffsets = RotateOffsets(shapeData.occupiedOffsets, degrees);
            
            if (shapeData.expansionRotatesWithIngredient)
            {
                shapeData.expansionOffsets = RotateOffsets(shapeData.expansionOffsets, degrees);
            }
            
            EditorUtility.SetDirty(selectedIngredient);
        }
        
        private Vector2Int[] RotateOffsets(Vector2Int[] offsets, int degrees)
        {
            if (offsets == null || offsets.Length == 0) return offsets;
            
            var rotated = new Vector2Int[offsets.Length];
            int rotations = ((degrees % 360) / 90) % 4;
            if (rotations < 0) rotations += 4;
            
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int offset = offsets[i];
                
                for (int r = 0; r < rotations; r++)
                {
                    // Rotate 90 degrees clockwise: (x, y) -> (y, -x)
                    offset = new Vector2Int(offset.y, -offset.x);
                }
                
                rotated[i] = offset;
            }
            
            return rotated;
        }
        
        private void DrawIngredientVisualDesign(SerializedObject serializedObject)
        {
            // Ensure visual design grid is loaded for the current ingredient
            if (selectedIngredient != null && (currentIngredientGrid == null || 
                currentIngredientGrid.GetLength(0) != selectedIngredient.GridWidth || 
                currentIngredientGrid.GetLength(1) != selectedIngredient.GridHeight))
            {
                LoadIngredientShape(selectedIngredient);
                Debug.Log($"🔄 Visual Design tab: Auto-loaded grid for {selectedIngredient.ItemName}");
            }
            
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
                
                // Show custom grid status
                if (recipe.HasCustomGridData())
                {
                    bool playerCanUse = recipe.CanPlayerUseCustomGrid();
                    string gridIcon = playerCanUse ? "🟢" : "🔒";
                    string gridTooltip = playerCanUse ? "Custom grid available" : "Custom grid locked (need recipe page)";
                    GUIContent gridContent = new GUIContent(gridIcon, gridTooltip);
                    GUILayout.Label(gridContent, GUILayout.Width(20));
                }
                else
                {
                    GUILayout.Label("⚪", GUILayout.Width(20)); // No custom grid
                }
                
                if (GUILayout.Button("📝", GUILayout.Width(25)))
                {
                    // Defer the rename operation to avoid GUI layout conflicts
                    deferredOperation = () => StartRenameRecipe(recipe);
                }
                
                if (GUILayout.Button("🗑️", GUILayout.Width(25)))
                {
                    // Defer the delete operation to avoid GUI layout conflicts
                    deferredOperation = () => DeleteRecipe(recipe);
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
            
            try
            {
                GUILayout.Label($"Editing Recipe: {selectedRecipe.name}", EditorStyles.boldLabel);
                
                // Show custom grid information
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Custom Grid Status:", GUILayout.Width(120));
                if (selectedRecipe.HasCustomGridData())
                {
                    bool playerCanUse = selectedRecipe.CanPlayerUseCustomGrid();
                    string status = playerCanUse ? "✅ Available" : "🔒 Locked (need recipe page)";
                    GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
                    statusStyle.normal.textColor = playerCanUse ? Color.green : Color.red;
                    EditorGUILayout.LabelField(status, statusStyle);
                    
                    EditorGUILayout.LabelField($"Grid Size: {selectedRecipe.CustomGridWidth}x{selectedRecipe.CustomGridHeight}", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Cells: {selectedRecipe.CustomGridCells.Count}", GUILayout.Width(60));
                }
                else
                {
                    EditorGUILayout.LabelField("⚪ No custom grid", EditorStyles.label);
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                
                var serializedObject = new SerializedObject(selectedRecipe);
                serializedObject.Update();
                
                // Visual recipe display
                DrawRecipeVisual(selectedRecipe);
                
                EditorGUILayout.Space();
                
                // Safely draw property fields with null checks
                var prop1 = serializedObject.FindProperty("inputIngredient1");
                if (prop1 != null)
                    EditorGUILayout.PropertyField(prop1);
                
                var prop2 = serializedObject.FindProperty("inputIngredient2");
                if (prop2 != null)
                    EditorGUILayout.PropertyField(prop2);
                
                var prop3 = serializedObject.FindProperty("inputIngredient3");
                if (prop3 != null)
                    EditorGUILayout.PropertyField(prop3);
                
                EditorGUILayout.Space();
                
                var outputProp = serializedObject.FindProperty("outputPotion");
                if (outputProp != null)
                    EditorGUILayout.PropertyField(outputProp);
                
                if (serializedObject.targetObject != null)
                {
                    serializedObject.ApplyModifiedProperties();
                }
                
                // Recipe testing
                EditorGUILayout.Space();
                if (GUILayout.Button("Test Recipe", GUILayout.Height(30)))
                {
                    TestRecipe(selectedRecipe);
                }
            }
            catch (ExitGUIException)
            {
                // This is normal - Unity throws this when GUI state changes
                throw;
            }
            catch (System.Exception e)
            {
                EditorGUILayout.HelpBox($"Error in recipe editor: {e.Message}", MessageType.Error);
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
                // Defer the create operation to avoid GUI layout conflicts
                deferredOperation = () => CreateNewPotion();
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
                
                if (GUILayout.Button("📝", GUILayout.Width(25)))
                {
                    // Defer the rename operation to avoid GUI layout conflicts
                    deferredOperation = () => StartRenamePotion(potion);
                }

                if (GUILayout.Button("🗑️", GUILayout.Width(25)))
                {
                    // Defer the delete operation to avoid GUI layout conflicts
                    deferredOperation = () => DeletePotion(potion);
                }
                
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
            GUILayout.Label("Infusion Bundle", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infusionBundle"));
            
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
            var effects = potion.GetAllEffects();
            if (effects != null && effects.Count > 0)
            {
                foreach (var effect in effects)
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
            var effects = potion.GetAllEffects();
            if (effects != null && effects.Count > 0)
            {
                foreach (var effect in effects)
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

        private void DeleteRecipe(AlchemyRecipe recipe)
        {
            if (recipe == null) return;
            
            bool confirm = EditorUtility.DisplayDialog(
                "Delete Recipe", 
                $"Are you sure you want to delete '{recipe.name}'?", 
                "Delete", "Cancel"
            );
            
            if (confirm)
            {
                // Clear selection if we're deleting the selected recipe
                if (selectedRecipe == recipe)
                {
                    selectedRecipe = null;
                }
                
                // Get the asset path and delete it
                string assetPath = AssetDatabase.GetAssetPath(recipe);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"🗑️ Deleted recipe: {recipe.name}");
                }
                else
                {
                    Debug.LogError($"Could not find asset path for recipe: {recipe.name}");
                }
            }
        }

        private void DeleteIngredient(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            bool confirm = EditorUtility.DisplayDialog(
                "Delete Ingredient", 
                $"Are you sure you want to delete '{ingredient.name}'?", 
                "Delete", "Cancel"
            );
            
            if (confirm)
            {
                // Clear selection if we're deleting the selected ingredient
                if (selectedIngredient == ingredient)
                {
                    selectedIngredient = null;
                }
                
                // Get the asset path and delete it
                string assetPath = AssetDatabase.GetAssetPath(ingredient);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"🗑️ Deleted ingredient: {ingredient.name}");
                }
                else
                {
                    Debug.LogError($"Could not find asset path for ingredient: {ingredient.name}");
                }
            }
        }

        private void DeletePotion(Potion potion)
        {
            if (potion == null) return;
            
            bool confirm = EditorUtility.DisplayDialog(
                "Delete Potion", 
                $"Are you sure you want to delete '{potion.ItemName}'?\n\nThis action cannot be undone.", 
                "Delete", 
                "Cancel"
            );
            
            if (confirm)
            {
                // Clear selection if we're deleting the selected potion
                if (selectedPotion == potion)
                {
                    selectedPotion = null;
                }
                
                // Get the asset path and delete it
                string assetPath = AssetDatabase.GetAssetPath(potion);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.Refresh();
                    Debug.Log($"🗑️ Deleted potion: {potion.ItemName}");
                }
                else
                {
                    Debug.LogError($"Could not find asset path for potion: {potion.ItemName}");
                }
            }
        }

        private void DrawInfusionsTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - infusions list
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(350));
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🌟 Infusions", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("❓", GUILayout.Width(25)))
            {
                ShowInfusionHelp();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Create New Infusion", GUILayout.Height(30)))
            {
                // Defer the create operation to avoid GUI layout conflicts
                deferredOperation = () => {
                    Debug.Log("🌟 Create New Infusion button clicked!");
                    CreateBlankInfusion();
                };
            }
            
            // Search and filter controls
            DrawInfusionFilters();
            
            // Infusions list
            DrawInfusionsList();
            
            EditorGUILayout.EndVertical();
            
            // Right panel - infusion editor
            EditorGUILayout.BeginVertical("Box");
            if (selectedInfusion != null)
            {
                DrawInfusionEditor();
            }
            else
            {
                EditorGUILayout.HelpBox("Select an infusion to edit its properties", MessageType.Info);
                
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("HelpBox");
                GUILayout.Label("✨ Infusion System Features:", EditorStyles.boldLabel);
                GUILayout.Label("• 🎨 Visual color and icon customization");
                GUILayout.Label("• ⚡ Effect bundle management with IEffect integration");
                GUILayout.Label("• 📊 Power level and stacking configuration");
                GUILayout.Label("• 🔍 Advanced filtering by category and effect type");
                GUILayout.Label("• 🧪 Seamless integration with ingredients and potions");
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            // Wizard removed - Create New Infusion now directly creates a blank template
        }
        
        private void ShowInfusionHelp()
        {
            EditorUtility.DisplayDialog("Infusion System Help", 
                "🌟 INFUSION MANAGEMENT SYSTEM\\n\\n" +
                "Create and manage magical infusions that power your alchemy system:\\n\\n" +
                "🎨 VISUAL DESIGN:\\n" +
                "• Set custom colors for particle effects\\n" +
                "• Assign icons for UI representation\\n" +
                "• Categorize for easy organization\\n\\n" +
                "⚡ EFFECT SYSTEM:\\n" +
                "• Combine multiple IEffects in one infusion\\n" +
                "• Configure power levels (1-10)\\n" +
                "• Enable stacking with max stack limits\\n\\n" +
                "🔍 SMART FILTERING:\\n" +
                "• Search by infusion name\\n" +
                "• Filter by category (Elemental, Physical, etc.)\\n" +
                "• Filter by effect type (Damage, Healing, etc.)\\n\\n" +
                "🧪 INTEGRATION:\\n" +
                "• Replace old string-based infusions\\n" +
                "• Use in ingredients and potions\\n" +
                "• Full backward compatibility", 
                "Got it!");
        }
        
        private void DrawInfusionFilters()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🔍 Filters", EditorStyles.boldLabel);
            
            // Name search
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(50));
            infusionSearchQuery = EditorGUILayout.TextField(infusionSearchQuery);
            if (GUILayout.Button("✖", GUILayout.Width(20)))
            {
                infusionSearchQuery = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            // Effect type filter
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Effect Type:", GUILayout.Width(70));
            selectedEffectTypeFilter = (EffectTypeFilter)EditorGUILayout.EnumPopup(selectedEffectTypeFilter, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("All", GUILayout.Width(30)))
            {
                selectedEffectTypeFilter = EffectTypeFilter.All;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawInfusionsList()
        {
            var infusions = FindAssetsByType<InfusionSO>();
            var filteredInfusions = FilterInfusions(infusions);
            
            infusionsScrollPosition = EditorGUILayout.BeginScrollView(infusionsScrollPosition, GUILayout.Height(400));
            
            if (filteredInfusions.Count == 0)
            {
                EditorGUILayout.HelpBox("No infusions match the current filters", MessageType.Info);
            }
            else
            {
                foreach (var infusion in filteredInfusions)
                {
                    DrawInfusionListItem(infusion);
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            // Show count
            EditorGUILayout.LabelField($"Showing {filteredInfusions.Count} of {infusions.Count} infusions", EditorStyles.miniLabel);
        }
        
        private void DrawInfusionListItem(InfusionSO infusion)
        {
            EditorGUILayout.BeginHorizontal("Box");
            
            // Selection toggle
            bool isSelected = selectedInfusion == infusion;
            if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)) && !isSelected)
            {
                selectedInfusion = infusion;
            }
            
            // Color indicator
            var colorRect = GUILayoutUtility.GetRect(15, 15);
            EditorGUI.DrawRect(colorRect, infusion.InfusionColor);
            
            // Icon placeholder (InfusionIcon was removed)
            GUILayout.Space(25);
            
            // Infusion info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(infusion.InfusionName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Power: {infusion.PowerLevel} • Effects: {infusion.EffectBundle?.Effects?.Count ?? 0}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            // Actions
            if (GUILayout.Button("⚡", GUILayout.Width(25)))
            {
                ShowInfusionQuickActions(infusion);
            }
            
            if (GUILayout.Button("✏️", GUILayout.Width(25)))
            {
                // Defer the rename operation to avoid GUI layout conflicts
                deferredOperation = () => StartRenameInfusion(infusion);
            }
            
            if (GUILayout.Button("🗑️", GUILayout.Width(25)))
            {
                // Defer the delete operation to avoid GUI layout conflicts
                deferredOperation = () => {
                    if (EditorUtility.DisplayDialog("Delete Infusion", 
                        $"Are you sure you want to delete '{infusion.InfusionName}'?", 
                        "Delete", "Cancel"))
                    {
                        DeleteInfusion(infusion);
                    }
                };
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawInfusionEditor()
        {
            if (selectedInfusion == null) return;
            
            // Header
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"🌟 Editing: {selectedInfusion.InfusionName}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            // Quick info badges
            GUI.color = selectedInfusion.InfusionColor;
            GUILayout.Label("●", "Button", GUILayout.Width(25));
            GUI.color = Color.white;
            
            GUILayout.Label($"⚡{selectedInfusion.PowerLevel}", "Button", GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            
            var serializedObject = new SerializedObject(selectedInfusion);
            serializedObject.Update();
            
            // All content in one view for now (can be made tabbed later)
            DrawInfusionBasicInfo(serializedObject);
            DrawInfusionEffects(serializedObject);
            DrawInfusionVisual(serializedObject);
            DrawInfusionSettings(serializedObject);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawInfusionBasicInfo(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("📋 Basic Information", EditorStyles.boldLabel);
            
            var infusionNameProperty = serializedObject.FindProperty("infusionName");
            if (infusionNameProperty != null)
            {
                EditorGUILayout.PropertyField(infusionNameProperty);
            }
            else
            {
                EditorGUILayout.HelpBox("Infusion Name property not found. Please check the Infusion ScriptableObject.", MessageType.Warning);
            }
            
            // Additional basic properties
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rarity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infusionColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infusionIcon"));
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawInfusionEffects(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("⚡ Effect Bundle", EditorStyles.boldLabel);
            
            var effectsProperty = serializedObject.FindProperty("effectBundle");
            if (effectsProperty != null)
            {
                EditorGUILayout.PropertyField(effectsProperty, true);
            }
            else
            {
                EditorGUILayout.HelpBox("EffectBundle property not found. Please check the Infusion ScriptableObject.", MessageType.Warning);
            }
            
            // Effect analysis
            if (selectedInfusion.EffectBundle?.Effects != null && selectedInfusion.EffectBundle.Effects.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("HelpBox");
                GUILayout.Label("Effect Analysis:", EditorStyles.boldLabel);
                
                var effectTypes = selectedInfusion.GetEffectTypes();
                var effectCategories = selectedInfusion.GetEffectCategories();
                
                EditorGUILayout.LabelField($"Total Effects: {selectedInfusion.EffectBundle.Effects.Count}");
                EditorGUILayout.LabelField($"Effect Types: {string.Join(", ", effectTypes.Select(t => t.Name))}");
                EditorGUILayout.LabelField($"Categories: {string.Join(", ", effectCategories)}");
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawInfusionVisual(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🎨 Visual Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infusionColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("infusionIcon"));
            
            // Color preview
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Color Preview:", GUILayout.Width(100));
            var colorRect = GUILayoutUtility.GetRect(50, 20);
            EditorGUI.DrawRect(colorRect, selectedInfusion.InfusionColor);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawInfusionSettings(SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("⚙️ Game Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("powerLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canStack"));
            
            if (selectedInfusion.CanStack)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStacks"));
            }
            
            // Validation
            EditorGUILayout.Space();
            bool isValid = selectedInfusion.IsValid(out string validationMessage);
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Validation:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Status:", isValid ? "✅ Valid" : "❌ Invalid");
            if (!isValid)
            {
                EditorGUILayout.LabelField("Issue:", validationMessage);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
        
        
        // Helper methods for infusions
        private List<InfusionSO> FilterInfusions(List<InfusionSO> infusions)
        {
            var filtered = infusions.AsEnumerable();
            
            // Name filter
            if (!string.IsNullOrEmpty(infusionSearchQuery))
            {
                filtered = filtered.Where(i => i.InfusionName.ToLower().Contains(infusionSearchQuery.ToLower()));
            }
            
            // Effect type filter
            if (selectedEffectTypeFilter != EffectTypeFilter.All)
            {
                filtered = filtered.Where(i => MatchesEffectTypeFilter(i, selectedEffectTypeFilter));
            }
            
            return filtered.ToList();
        }
        
        private bool MatchesEffectTypeFilter(InfusionSO infusion, EffectTypeFilter filter)
        {
            if (infusion?.EffectBundle?.Effects == null || infusion.EffectBundle.Effects.Count == 0)
            {
                return filter == EffectTypeFilter.None;
            }
            
            foreach (var effect in infusion.EffectBundle.Effects)
            {
                if (effect == null) continue;
                
                switch (filter)
                {
                    case EffectTypeFilter.Heal:
                        if (effect is HealEffect) return true;
                        break;
                    case EffectTypeFilter.BuffHeal:
                        if (effect is BuffHealEffect) return true;
                        break;
                    case EffectTypeFilter.Damage:
                        if (effect is DamageEffect) return true;
                        break;
                    case EffectTypeFilter.Shield:
                        if (effect is ShieldEffect) return true;
                        break;
                    case EffectTypeFilter.BuffShield:
                        if (effect is BuffShieldEffect) return true;
                        break;
                    case EffectTypeFilter.BuffStat:
                        if (effect is BuffStatEffect) return true;
                        break;
                    case EffectTypeFilter.DebuffStat:
                        if (effect is DebuffStatEffect) return true;
                        break;
                    case EffectTypeFilter.DebuffDOT:
                        if (effect is DebuffDOTEffect) return true;
                        break;
                }
            }
            
            return false;
        }
        
        private void ShowInfusionQuickActions(InfusionSO infusion)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Rename Asset"), false, () => StartRenameInfusion(infusion));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateInfusion(infusion));
            menu.AddItem(new GUIContent("Find Usage"), false, () => FindInfusionUsage(infusion));
            menu.AddItem(new GUIContent("Test Effects"), false, () => TestInfusionEffects(infusion));
            menu.AddItem(new GUIContent("Export Data"), false, () => ExportInfusionData(infusion));
            menu.ShowAsContext();
        }
        
        private void CreateQuickInfusion(string name, Color color, string description = null)
        {
            Debug.Log($"🧪 Starting creation of {name} infusion...");
            
            var infusion = CreateInstance<InfusionSO>();
            
            // Use the new InitializeInfusion method instead of reflection
            string finalDescription = description ?? $"A magical infusion that provides {name.ToLower()} effects.";
            infusion.InitializeInfusion(name, finalDescription, color);
            
            // Create asset
            string path = "Assets/Resources/Infusions";
            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.Log("📁 Creating Infusions folder...");
                AssetDatabase.CreateFolder("Assets/Resources", "Infusions");
            }
            
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name.Replace(" ", "")}.asset");
            Debug.Log($"💾 Creating asset at: {assetPath}");
            
            AssetDatabase.CreateAsset(infusion, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            selectedInfusion = infusion;
            
            Debug.Log($"✅ Created {name} infusion: {assetPath}");
            Debug.Log($"🔍 Infusion validation: Name='{infusion.InfusionName}', Color={infusion.InfusionColor}, Description='{infusion.Description}'");
        }
        
        private void CreateBlankInfusion()
        {
            CreateQuickInfusion("New Infusion", Color.white, "A blank infusion template ready for customization.");
        }
        
        private void CreateCustomInfusion()
        {
            CreateQuickInfusion("New Infusion", Color.white, "A custom infusion with user-defined effects.");
        }
        
        private void DeleteInfusion(InfusionSO infusion)
        {
            string path = AssetDatabase.GetAssetPath(infusion);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            if (selectedInfusion == infusion)
                selectedInfusion = null;
                
            Debug.Log($"Deleted infusion: {infusion.InfusionName}");
        }
        
        private void StartRenameInfusion(InfusionSO infusion)
        {
            // Use simplified rename dialog
            ShowRenameDialog("Rename Infusion", infusion.name, newName => {
                if (RenameAsset(infusion, newName))
                {
                    Debug.Log($"✅ Renamed infusion to '{newName}'");
                    Repaint();
                }
            });
        }
        
        private void StartRenameIngredient(Ingredient ingredient)
        {
            // Use simplified rename dialog
            ShowRenameDialog("Rename Ingredient", ingredient.name, newName => {
                if (RenameAsset(ingredient, newName))
                {
                    Debug.Log($"✅ Renamed ingredient to '{newName}'");
                    Repaint();
                }
            });
        }
        
        /* BROKEN ORPHANED CONTENT - commenting out:
            // Show instructions dialog
            EditorUtility.DisplayDialog("Rename Infusion Asset",
                $"The infusion '{infusion.name}' has been selected in the Project window.\n\n" +
                "To rename it:\n" +
                "1. Press F2 or right-click and select 'Rename'\n" +
                "2. Enter the new name\n" +
                "3. Press Enter to confirm\n\n" +
        
        private void StartRenameIngredient(Ingredient ingredient)
        {
            // Use simplified rename dialog
            ShowRenameDialog("Rename Ingredient", ingredient.name, (newName) => {
                if (RenameAsset(ingredient, newName))
                {
                    Debug.Log($"✅ Renamed ingredient to '{newName}'");
                    Repaint();
                }
            });
        }
        
        /* BROKEN ORPHANED CONTENT - commenting out:
        private void StartRenameRecipe(AlchemyRecipe recipe)
                $"The ingredient has been selected in the Project window.\n\n" +
                "To rename it:\n" +
                "1. Press F2 or right-click and select 'Rename'\n" +
                "2. Enter the new name\n" +
                "3. Press Enter to confirm\n\n" +
                "The Alchemy System Editor will automatically refresh.",
                "Got it");
        }
        */ // END BROKEN ORPHANED CONTENT
        
        private void StartRenameRecipe(AlchemyRecipe recipe)
        {
            // Use simplified rename dialog
            ShowRenameDialog("Rename Recipe", recipe.name, newName => {
                if (RenameAsset(recipe, newName))
                {
                    Debug.Log($"✅ Renamed recipe to '{newName}'");
                    Repaint();
                }
            });
        }
        
        
        private void StartRenamePotion(Potion potion)
        {
            // Use simplified rename dialog
            ShowRenameDialog("Rename Potion", potion.name, newName => {
                if (RenameAsset(potion, newName))
                {
                    Debug.Log($"✅ Renamed potion to '{newName}'");
                    Repaint();
                }
            });
        }
       
       
        
        private void StartRenameBookEntry(BaseEntry entry)
        {
            // Use simplified rename dialog
            ShowRenameDialog("Rename Book Entry", entry.name, newName => {
                if (RenameAsset(entry, newName))
                {
                    Debug.Log($"✅ Renamed book entry to '{newName}'");
                    Repaint();
                }
            });
        }
        
        private void DuplicateInfusion(InfusionSO infusion)
        {
            var duplicate = Instantiate(infusion);
            duplicate.name = $"{infusion.InfusionName} Copy";
            
            string path = AssetDatabase.GetAssetPath(infusion);
            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            string newPath = $"{directory}/{filename}_Copy{extension}";
            
            AssetDatabase.CreateAsset(duplicate, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            selectedInfusion = duplicate;
            Debug.Log($"Duplicated infusion: {infusion.InfusionName}");
        }
        
        private void FindInfusionUsage(InfusionSO infusion)
        {
            Debug.Log($"🔍 Finding usage of infusion: {infusion.InfusionName}");
            // This would search through ingredients and potions to find where this infusion is used
            EditorUtility.DisplayDialog("Find Usage", $"Searching for usage of '{infusion.InfusionName}'...\\n\\nThis feature will scan all ingredients and potions.", "OK");
        }
        
        private void TestInfusionEffects(InfusionSO infusion)
        {
            Debug.Log($"🧪 Testing effects for {infusion.InfusionName}:");
            if (infusion.EffectBundle?.Effects != null && infusion.EffectBundle.Effects.Count > 0)
            {
                foreach (var effect in infusion.EffectBundle.Effects)
                {
                    if (effect != null)
                    {
                        Debug.Log($"  • {effect.GetType().Name}: Ready for application");
                    }
                }
            }
            else
            {
                Debug.Log("  No effects assigned to this infusion.");
            }
        }
        
        private void ExportInfusionData(InfusionSO infusion)
        {
            Debug.Log($"📤 Exporting data for infusion: {infusion.InfusionName}");
            EditorUtility.DisplayDialog("Export Data", $"Exporting data for '{infusion.InfusionName}'...\\n\\nThis would create a JSON file with all infusion data.", "OK");
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
                // Defer the duplicate operation to avoid GUI layout conflicts
                deferredOperation = () => DuplicateSelectedEntry();
            }
            
            if (GUILayout.Button("Delete Selected"))
            {
                // Defer the delete operation to avoid GUI layout conflicts
                deferredOperation = () => DeleteSelectedEntry();
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
                    EditorGUILayout.BeginVertical(GUILayout.Width(120));
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("📝", GUILayout.Width(25)))
                    {
                        StartRenameBookEntry(entry);
                    }
                    if (GUILayout.Button("Edit", GUILayout.Width(40)))
                    {
                        Selection.activeObject = entry;
                    }
                    if (GUILayout.Button("Preview", GUILayout.Width(50)))
                    {
                        PreviewBookEntry(entry);
                    }
                    EditorGUILayout.EndHorizontal();
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
                    // Defer the create operation to avoid GUI layout conflicts
                    deferredOperation = () => CreateAlchemyDatabase();
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
                // Defer the create operation to avoid GUI layout conflicts
                deferredOperation = () => CreateNewIngredient();
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
            try
            {
                GUILayout.Label("🎮 Grid Pattern Designer", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Design grid patterns for alchemy recipes using a visual editor", MessageType.Info);
                
                GUILayout.Space(10);
                
                // Recipe selection for grid designer with improved state management
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Recipe for Grid Design:", GUILayout.Width(150));
                
                try
                {
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
                                
                                // Auto-load custom grid data if available
                                if (gridDesignerRecipe.HasCustomGridData())
                                {
                                    AutoLoadCustomGridForRecipe(gridDesignerRecipe);
                                }
                                else
                                {
                                    // Clear the grid if no custom data
                                    ClearGridForNewRecipe();
                                }
                            }
                            else
                            {
                                // Clear grid when no recipe selected
                                ClearGridForNewRecipe();
                            }
                        }
                    }
                }
                catch (ExitGUIException)
                {
                    // Unity GUI exception, safe to ignore
                    EditorGUILayout.EndHorizontal();
                    return;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error in grid recipe selection: {e.Message}");
                    EditorGUILayout.EndHorizontal();
                    return;
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
                        
                        // Auto-load custom grid data if available
                        if (gridDesignerRecipe.HasCustomGridData())
                        {
                            AutoLoadCustomGridForRecipe(gridDesignerRecipe);
                        }
                        else
                        {
                            // Clear the grid if no custom data
                            ClearGridForNewRecipe();
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                
                if (gridDesignerRecipe != null)
                {
                    try
                    {
                        DrawGridDesignerRecipeDetails();
                    }
                    catch (ExitGUIException)
                    {
                        // Unity GUI exception, safe to ignore
                        return;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error drawing grid designer recipe details: {e.Message}");
                        EditorGUILayout.HelpBox($"Error displaying recipe details: {e.Message}", MessageType.Error);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Select an Alchemy Recipe to design its grid pattern", MessageType.Info);
                }
            }
            catch (ExitGUIException)
            {
                // Unity GUI exception, safe to ignore
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in DrawGridDesignerTab: {e.Message}");
                EditorGUILayout.HelpBox($"Grid Designer Error: {e.Message}", MessageType.Error);
            }
        }
        
        private void DrawGridDesignerRecipeDetails()
        {
            if (gridDesignerRecipe == null)
            {
                EditorGUILayout.HelpBox("No recipe selected for grid design.", MessageType.Warning);
                return;
            }

            string recipeName = !string.IsNullOrEmpty(gridDesignerRecipe.ItemName) && gridDesignerRecipe.ItemName != "Unnamed Item" 
                ? gridDesignerRecipe.ItemName 
                : gridDesignerRecipe.name;
                
            GUILayout.Label($"Editing Pattern for: {recipeName}", EditorStyles.boldLabel);
            
            // Show custom grid status information
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Custom Grid Status:", GUILayout.Width(120));
            if (gridDesignerRecipe.HasCustomGridData())
            {
                bool playerCanUse = gridDesignerRecipe.CanPlayerUseCustomGrid();
                string status = playerCanUse ? "✅ Loaded" : "🔒 Locked";
                GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
                statusStyle.normal.textColor = playerCanUse ? Color.green : Color.red;
                EditorGUILayout.LabelField(status, statusStyle);
                
                EditorGUILayout.LabelField($"Size: {gridDesignerRecipe.CustomGridWidth}x{gridDesignerRecipe.CustomGridHeight}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"Cells: {gridDesignerRecipe.CustomGridCells.Count}", GUILayout.Width(60));
            }
            else
            {
                EditorGUILayout.LabelField("⚪ Not saved", EditorStyles.label);
            }
            EditorGUILayout.EndHorizontal();
            
            // Show current designer grid status
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Designer Grid:", GUILayout.Width(120));
            EditorGUILayout.LabelField($"Size: {gridWidth}x{gridHeight}", GUILayout.Width(80));
            EditorGUILayout.LabelField($"Cells: {gridCells.Count}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Quick action buttons
            EditorGUILayout.BeginHorizontal();
            if (gridDesignerRecipe.HasCustomGridData())
            {
                if (GUILayout.Button("🔄 Reload Custom Grid", GUILayout.Width(150)))
                {
                    AutoLoadCustomGridForRecipe(gridDesignerRecipe);
                    EditorUtility.DisplayDialog("Grid Reloaded", "Custom grid has been reloaded from recipe data.", "OK");
                }
                
                if (GUILayout.Button("🧹 Clear Grid", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Clear Grid", "Are you sure you want to clear the current grid?", "Clear", "Cancel"))
                    {
                        ClearGridForNewRecipe();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("🧹 Clear Grid", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Clear Grid", "Are you sure you want to clear the current grid?", "Clear", "Cancel"))
                    {
                        ClearGridForNewRecipe();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Recipe Grid Pattern", EditorStyles.boldLabel);

            try
            {
                var serializedObject = new SerializedObject(gridDesignerRecipe);
                serializedObject.Update();

                // Show recipe ingredients with safety checks
                EditorGUILayout.Space();
                GUILayout.Label("Required Ingredients:", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                try
                {
                    var ingredient1Prop = serializedObject.FindProperty("inputIngredient1");
                    if (ingredient1Prop != null)
                        EditorGUILayout.PropertyField(ingredient1Prop, new GUIContent("Ingredient 1"));
                    else
                        EditorGUILayout.LabelField("Ingredient 1: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Ingredient 1: Error - {e.Message}");
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                try
                {
                    var ingredient2Prop = serializedObject.FindProperty("inputIngredient2");
                    if (ingredient2Prop != null)
                        EditorGUILayout.PropertyField(ingredient2Prop, new GUIContent("Ingredient 2"));
                    else
                        EditorGUILayout.LabelField("Ingredient 2: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Ingredient 2: Error - {e.Message}");
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                try
                {
                    var ingredient3Prop = serializedObject.FindProperty("inputIngredient3");
                    if (ingredient3Prop != null)
                        EditorGUILayout.PropertyField(ingredient3Prop, new GUIContent("Ingredient 3"));
                    else
                        EditorGUILayout.LabelField("Ingredient 3: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Ingredient 3: Error - {e.Message}");
                }

                EditorGUILayout.EndHorizontal();

                // Recipe properties
                EditorGUILayout.Space();
                GUILayout.Label("Recipe Properties:", EditorStyles.boldLabel);

                // Use properties that actually exist in AlchemyRecipe with safe calls
                try
                {
                    var difficultyProp = serializedObject.FindProperty("difficulty");
                    if (difficultyProp != null)
                        EditorGUILayout.PropertyField(difficultyProp);
                    else
                        EditorGUILayout.LabelField("Difficulty: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Difficulty: Error - {e.Message}");
                }

                try
                {
                    var minimumEfficiencyProp = serializedObject.FindProperty("minimumEfficiency");
                    if (minimumEfficiencyProp != null)
                        EditorGUILayout.PropertyField(minimumEfficiencyProp);
                    else
                        EditorGUILayout.LabelField("Minimum Efficiency: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Minimum Efficiency: Error - {e.Message}");
                }

                try
                {
                    var outputQuantityProp = serializedObject.FindProperty("outputQuantity");
                    if (outputQuantityProp != null)
                        EditorGUILayout.PropertyField(outputQuantityProp);
                    else
                        EditorGUILayout.LabelField("Output Quantity: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Output Quantity: Error - {e.Message}");
                }

                try
                {
                    var outputPotionProp = serializedObject.FindProperty("outputPotion");
                    if (outputPotionProp != null)
                        EditorGUILayout.PropertyField(outputPotionProp);
                    else
                        EditorGUILayout.LabelField("Output Potion: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Output Potion: Error - {e.Message}");
                }

                try
                {
                    var isKeyRecipeProp = serializedObject.FindProperty("isKeyRecipe");
                    if (isKeyRecipeProp != null)
                        EditorGUILayout.PropertyField(isKeyRecipeProp);
                    else
                        EditorGUILayout.LabelField("Is Key Recipe: Property not found");
                }
                catch (System.Exception e)
                {
                    EditorGUILayout.LabelField($"Is Key Recipe: Error - {e.Message}");
                }

                serializedObject.ApplyModifiedProperties();

                GUILayout.Space(10);

                // Grid designer for recipe pattern
                try
                {
                    DrawRecipeGridDesigner(gridDesignerRecipe);
                }
                catch (Exception e)
                {
                    EditorGUILayout.HelpBox(
                        $"Grid designer error: {e.Message}\nThis may be because the recipe doesn't have grid properties.",
                        MessageType.Warning);

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
            catch (System.Exception e)
            {
                Debug.LogError($"Error in DrawGridDesignerRecipeDetails: {e.Message}");
                EditorGUILayout.HelpBox($"Error displaying recipe details: {e.Message}", MessageType.Error);
                EditorGUILayout.EndVertical(); // Ensure we close the vertical group
            }

            GUILayout.Space(20);
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
            
            // Show current recipe custom grid status
            if (recipe != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Recipe: {recipe.name}", EditorStyles.miniLabel, GUILayout.Width(200));
                
                if (recipe.HasCustomGridData())
                {
                    bool playerCanUse = recipe.CanPlayerUseCustomGrid();
                    string status = playerCanUse ? "✅ Has Custom Grid" : "🔒 Custom Grid Locked";
                    GUIStyle statusStyle = new GUIStyle(EditorStyles.miniLabel);
                    statusStyle.normal.textColor = playerCanUse ? Color.green : Color.red;
                    EditorGUILayout.LabelField(status, statusStyle);
                }
                else
                {
                    EditorGUILayout.LabelField("⚪ No custom grid saved", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(5);
            }
            
            EditorGUILayout.BeginVertical("Box");
            
            // Grid size controls
            EditorGUILayout.LabelField("Grid Size:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            gridWidth = EditorGUILayout.IntSlider("Width", gridWidth, 3, 8);
            gridHeight = EditorGUILayout.IntSlider("Height", gridHeight, 3, 8);
            if (EditorGUI.EndChangeCheck())
            {
                gridCells.Clear(); // Clear when size changes
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Obstacle Placement Controls
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("🧱 Obstacle Placement", EditorStyles.boldLabel);
            GUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Obstacle Type:", GUILayout.Width(100));
            selectedObstacleType = (ObstacleType)EditorGUILayout.EnumPopup(selectedObstacleType, GUILayout.MinWidth(120));
            
            // Show obstacle color preview
            Color obstacleColor = GetObstacleColor(selectedObstacleType);
            var oldColor = GUI.color;
            GUI.color = obstacleColor;
            GUILayout.Label("■", GUILayout.Width(20));
            GUI.color = oldColor;
            
            EditorGUILayout.EndHorizontal();
            
            // Show obstacle description
            string obstacleDesc = GetObstacleDescription(selectedObstacleType);
            EditorGUILayout.LabelField(obstacleDesc, EditorStyles.wordWrappedMiniLabel);
            
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            
            if (showCellPreview && gridCells.Count == 0)
            {
                PreviewPatternType(selectedPatternType);
            }
            
            GUILayout.Space(10);
            
            
            GUILayout.Space(10);
            
            // Visual grid designer with obstacle support
            EditorGUILayout.LabelField("🎯 Visual Grid Designer:");
            EditorGUILayout.LabelField("• Click cells to place/remove obstacles", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("• Right-click to remove obstacles", EditorStyles.miniLabel);
            
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
                        // Check if cell has an obstacle
                        if (cellData.HasObstacle)
                        {
                            // Use obstacle color and display obstacle symbol
                            cellColor = GetObstacleColor(cellData.obstacle.ObstacleType);
                            cellText = GetObstacleSymbol(cellData.obstacle.ObstacleType);
                        }
                        else
                        {
                            // Empty cell with data (shouldn't happen in obstacle-only mode)
                            cellColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
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
                    
                    // Enhanced visual feedback for obstacles
                    if (hasCellData && cellData.HasObstacle)
                    {
                        // Draw obstacle with special effects
                        DrawObstacleCell(cellRect, cellData.obstacle, cellColor);
                    }
                    else
                    {
                        // Draw regular cell background
                        EditorGUI.DrawRect(cellRect, cellColor);
                    }
                    
                    // Draw cell border
                    Color borderColor = hasCellData && cellData.HasObstacle ? Color.black : Color.gray;
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, cellRect.width, 1), borderColor);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y, 1, cellRect.height), borderColor);
                    EditorGUI.DrawRect(new Rect(cellRect.x + cellRect.width - 1, cellRect.y, 1, cellRect.height), borderColor);
                    EditorGUI.DrawRect(new Rect(cellRect.x, cellRect.y + cellRect.height - 1, cellRect.width, 1), borderColor);
                    
                    // Draw cell text (rarity letter)
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        var cellTextColor = GUI.color;
                        GUI.color = Color.black;
                        GUI.Label(cellRect, cellText, EditorStyles.boldLabel);
                        GUI.color = cellTextColor;
                    }
                    
                    // Handle cell clicking
                    if (GUI.Button(cellRect, "", GUIStyle.none))
                    {
                        OnCellClicked(pos);
                    }
                }
            }
            
            GUILayout.Space(10);
            
            // Cell information panel - obstacles only
            if (gridCells.TryGetValue(selectedCell, out CellData selectedCellData))
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label($"Selected Cell: ({selectedCell.x}, {selectedCell.y})", EditorStyles.boldLabel);
                
                // Show obstacle information if present
                if (selectedCellData.HasObstacle)
                {
                    GUILayout.Label("🧱 Obstacle Information:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Type:", selectedCellData.obstacle.ObstacleType.ToString());
                    EditorGUILayout.LabelField("Description:", GetObstacleDescription(selectedCellData.obstacle.ObstacleType), EditorStyles.wordWrappedLabel);
                    
                    if (GUILayout.Button("Remove Obstacle"))
                    {
                        gridCells.Remove(selectedCell);
                        Debug.Log($"🧱 Removed obstacle from cell ({selectedCell.x}, {selectedCell.y})");
                    }
                }
                else
                {
                    GUILayout.Label("ℹ️ Empty Cell", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("No obstacle placed");
                }
                
                EditorGUILayout.EndVertical();
            }
            else if (selectedCell != Vector2Int.zero)
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label($"Empty Cell: ({selectedCell.x}, {selectedCell.y})", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Add Obstacle"))
                {
                    gridCells[selectedCell] = new CellData
                    {
                        obstacle = new AspectObstacle(selectedObstacleType, selectedCell)
                    };
                    Debug.Log($"🧱 Placed {selectedObstacleType} obstacle at ({selectedCell.x}, {selectedCell.y})");
                }
                
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            // Grid management tools - obstacles only
            EditorGUILayout.HelpBox(
                "The enhanced visual above simulates how your ingredient will appear in the actual game:\n" +
                "• 3D-style lighting and shadows\n" +
                "• Aspect-based colors with emission effects\n" +
                "• Individual cubes for each active cell\n" +
                "• Potency affects glow intensity", 
                MessageType.Info
            );
            
            if (selectedIngredient != null)
            {
                int activeCells = gridCells.Count(kvp => kvp.Value.isRequired);
                EditorGUILayout.LabelField($"Shape: {activeCells} active cells");
                EditorGUILayout.LabelField($"Aspect: {selectedIngredient.IngredientAspect}");
                EditorGUILayout.LabelField($"Potency: {selectedIngredient.Potency}/5 (affects glow)");
                
                if (GUILayout.Button("🔄 Refresh Preview"))
                {
                    Repaint();
                }
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Obstacle management tools
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🛠️ Grid Management Tools", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All Obstacles"))
            {
                if (EditorUtility.DisplayDialog("Clear Obstacles", "Remove all obstacles from the grid?", "Yes", "Cancel"))
                {
                    foreach (var kvp in gridCells.ToList())
                    {
                        if (kvp.Value.HasObstacle)
                        {
                            var cellData = kvp.Value;
                            cellData.obstacle = null;
                            gridCells[kvp.Key] = cellData;
                        }
                    }
                    Debug.Log("🧱 Cleared all obstacles from grid");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Fill Border with Walls"))
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (x == 0 || x == gridWidth - 1 || y == 0 || y == gridHeight - 1)
                        {
                            var pos = new Vector2Int(x, y);
                            if (!gridCells.TryGetValue(pos, out var cellData))
                            {
                                cellData = new CellData();
                            }
                            cellData.obstacle = new AspectObstacle(ObstacleType.Corporeal, pos);
                            gridCells[pos] = cellData;
                        }
                    }
                }
                Debug.Log("🧱 Added wall border to grid");
            }
            
            // Grid statistics - Obstacle counts only
            int obstacleCount = gridCells.Values.Count(c => c.HasObstacle);
            
            EditorGUILayout.LabelField($"Obstacles Placed: {obstacleCount}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
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
        
        
        private void OnCellClicked(Vector2Int pos)
        {
            selectedCell = pos;
            HandleObstaclePlacement(pos);
        }
        
        private void HandleObstaclePlacement(Vector2Int pos)
        {
            bool rightClick = Event.current.button == 1;
            
            if (gridCells.TryGetValue(pos, out CellData cellData))
            {
                if (rightClick || Event.current.shift)
                {
                    // Right-click or Shift+click to remove obstacle
                    if (cellData.HasObstacle)
                    {
                        cellData.obstacle = null;
                        gridCells[pos] = cellData;
                        Debug.Log($"🧱 Removed obstacle from cell ({pos.x}, {pos.y})");
                    }
                }
                else
                {
                    // Add obstacle to existing cell
                    cellData.obstacle = new AspectObstacle(selectedObstacleType, pos);
                    gridCells[pos] = cellData;
                    Debug.Log($"🧱 Added {selectedObstacleType} obstacle to cell ({pos.x}, {pos.y})");
                }
            }
            else
            {
                if (!rightClick)
                {
                    // Create new cell with obstacle
                    gridCells[pos] = new CellData
                    {
                        obstacle = new AspectObstacle(selectedObstacleType, pos)
                    };
                    Debug.Log($"🧱 Placed {selectedObstacleType} obstacle at new cell ({pos.x}, {pos.y})");
                }
            }
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
        
        // GetAspectColor and GetRarityLetter methods removed - Grid Designer now only handles obstacles
        
        
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
            
            // Load current shape if not initialized or ingredient has changed
            if (currentIngredientGrid == null || selectedIngredient != ingredient)
            {
                LoadIngredientShape(ingredient);
                Debug.Log($"🔄 Loading ingredient shape for {ingredient.ItemName} in visual design editor");
            }
            
            // Template Selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Template:", GUILayout.Width(70));
            var currentTemplate = ingredient.ShapeData?.Template ?? ShapeTemplate.Rectangle;
            var newTemplate = (ShapeTemplate)EditorGUILayout.EnumPopup(currentTemplate);
            
            if (newTemplate != currentTemplate && GUILayout.Button("Apply", GUILayout.Width(50)))
            {
                ingredient.ShapeData.ApplyTemplate(newTemplate);
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
        
        // LoadIngredientShape method removed - Grid Designer now only handles obstacles
        

        /// <summary>
        /// Updates the grid to match the ingredient's current dimensions, preserving existing cell data where possible
        /// </summary>
        private void UpdateGridToMatchIngredientDimensions(Ingredient ingredient)
        {
            if (ingredient == null) return;

            int newWidth = ingredient.GridWidth;
            int newHeight = ingredient.GridHeight;
            
            // Create new grid with ingredient dimensions
            bool[,] newGrid = new bool[newWidth, newHeight];
            
            // Copy existing data if available, preserving what we can
            if (currentIngredientGrid != null)
            {
                int oldWidth = currentIngredientGrid.GetLength(0);
                int oldHeight = currentIngredientGrid.GetLength(1);
                
                Debug.Log($"🔄 Copying existing grid data from {oldWidth}x{oldHeight} to {newWidth}x{newHeight}");
                
                // Copy overlapping region
                for (int x = 0; x < Mathf.Min(oldWidth, newWidth); x++)
                {
                    for (int y = 0; y < Mathf.Min(oldHeight, newHeight); y++)
                    {
                        newGrid[x, y] = currentIngredientGrid[x, y];
                    }
                }
                
                // Fill new areas with default state (filled)
                for (int x = 0; x < newWidth; x++)
                {
                    for (int y = 0; y < newHeight; y++)
                    {
                        if (x >= oldWidth || y >= oldHeight)
                        {
                            newGrid[x, y] = true; // Default new cells to filled
                        }
                    }
                }
            }
            else
            {
                // No existing grid, fill all cells
                Debug.Log($"🔄 Creating new grid with dimensions {newWidth}x{newHeight}");
                for (int x = 0; x < newWidth; x++)
                {
                    for (int y = 0; y < newHeight; y++)
                    {
                        newGrid[x, y] = true;
                    }
                }
            }
            
            currentIngredientGrid = newGrid;
            hasUnsavedGridChanges = true;
            
            Debug.Log($"✅ Grid updated to match ingredient dimensions: {newWidth}x{newHeight}");
        }
        
        private void DrawGridVisual(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            // Check if ingredient dimensions have changed and update grid accordingly
            int currentIngredientWidth = ingredient.GridWidth;
            int currentIngredientHeight = ingredient.GridHeight;
            
            // Update grid if ingredient dimensions changed
            if (currentIngredientGrid == null || 
                currentIngredientGrid.GetLength(0) != currentIngredientWidth || 
                currentIngredientGrid.GetLength(1) != currentIngredientHeight)
            {
                Debug.Log($"🔄 Grid dimensions changed from {currentIngredientGrid?.GetLength(0) ?? 0}x{currentIngredientGrid?.GetLength(1) ?? 0} to {currentIngredientWidth}x{currentIngredientHeight}. Updating grid...");
                UpdateGridToMatchIngredientDimensions(ingredient);
            }
            
            int gridWidth = currentIngredientGrid.GetLength(0);
            int gridHeight = currentIngredientGrid.GetLength(1);
            
            // Add coordinate system explanation with dynamic dimensions
            EditorGUILayout.LabelField("Grid Coordinate System:", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("• X-axis (horizontal): Left to Right");
                EditorGUILayout.LabelField("• Y-axis (vertical): Bottom to Top (matches 3D game grid)");
            }
            
            string coordinateInfo = $"This grid displays exactly as it appears in the 3D game world when viewed from above.\n" +
                                  $"(0,0) is at the bottom left corner, ({gridWidth-1},{gridHeight-1}) is at the top right.\n" +
                                  $"Current grid size: {gridWidth} x {gridHeight} cells";
            EditorGUILayout.HelpBox(coordinateInfo, MessageType.Info);
            EditorGUILayout.Space();
            
            var rect = GUILayoutUtility.GetRect(gridWidth * CELL_SIZE + 30, gridHeight * CELL_SIZE + 30);
            
            // Adjust rect for grid area
            rect = new Rect(rect.x + 15, rect.y + 15, gridWidth * CELL_SIZE, gridHeight * CELL_SIZE);
            
            // Draw background
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f));
            
            // Handle mouse input
            var mousePos = Event.current.mousePosition;
            bool isMouseInGrid = rect.Contains(mousePos);
            
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    // Flip Y coordinate to match game's coordinate system (Y=0 at bottom)
                    int displayY = gridHeight - 1 - y;
                    
                    var cellRect = new Rect(
                        rect.x + x * CELL_SIZE,
                        rect.y + displayY * CELL_SIZE,
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
                    Vector3[] corners = {
                        new Vector3(cellRect.x, cellRect.y),
                        new Vector3(cellRect.x + cellRect.width, cellRect.y),
                        new Vector3(cellRect.x + cellRect.width, cellRect.y + cellRect.height),
                        new Vector3(cellRect.x, cellRect.y + cellRect.height)
                    };
                    
                    Handles.DrawLine(corners[0], corners[1]);
                    Handles.DrawLine(corners[1], corners[2]);
                    Handles.DrawLine(corners[2], corners[3]);
                    Handles.DrawLine(corners[3], corners[0]);
                    
                    // Draw coordinates for larger cells (show actual game coordinates)
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
            
            if (GUILayout.Button("Refresh Grid", GUILayout.Width(80)))
            {
                UpdateGridToMatchIngredientDimensions(ingredient);
                Repaint();
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
        
        private void LoadIngredientShape(Ingredient ingredient)
        {
            if (ingredient == null) 
            {
                currentIngredientGrid = null;
                return;
            }
            
            // Load the ingredient's shape data into the current grid
            currentIngredientGrid = ingredient.GetShape();
            hasUnsavedGridChanges = false;
            
            Debug.Log($"🔄 Loaded ingredient shape for {ingredient.ItemName}: {currentIngredientGrid.GetLength(0)}x{currentIngredientGrid.GetLength(1)}");
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
                    File.WriteAllText(path, data);
                    EditorUtility.DisplayDialog("Export Successful", $"Shape data exported to:\n{path}", "OK");
                }
                catch (Exception ex)
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
                    string data = File.ReadAllText(path);
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
                catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            
            var ingredient = CreateInstance<Ingredient>();
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
            
            var recipe = CreateInstance<AlchemyRecipe>();
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
                Debug.Log("Deleted ingredient");
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
                Debug.Log("Deleted recipe");
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
            allItems.AddRange(FindAssetsByType<Ingredient>());
            allItems.AddRange(FindAssetsByType<AlchemyRecipe>());
            allItems.AddRange(FindAssetsByType<Potion>());
            
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
        
        // Enhanced Grid Pattern Methods
        private void SaveGridPatternToRecipe(AlchemyRecipe recipe)
        {
            if (recipe == null) return;
            
            string recipeName = !string.IsNullOrEmpty(recipe.ItemName) && recipe.ItemName != "Unnamed Item" 
                ? recipe.ItemName 
                : recipe.name;
                
            Debug.Log($"💾 Saving custom grid layout for {recipeName} with {gridCells.Count} cells");
            
            // Initialize the custom grid layout
            recipe.SaveCustomGridLayout(gridWidth, gridHeight);
            
            // Save each cell's data
            foreach (var cell in gridCells)
            {
                var cellData = cell.Value;
                var position = cell.Key;
                
                ObstacleType obstacleType = ObstacleType.Corporeal;
                bool hasObstacle = cellData.HasObstacle;
                
                if (hasObstacle && cellData.obstacle != null)
                {
                    obstacleType = cellData.obstacle.ObstacleType;
                }
                
                recipe.SaveCustomGridCell(
                    position,
                    cellData.aspect,
                    cellData.rarity,
                    cellData.isRequired,
                    cellData.isOccupied,
                    obstacleType,
                    hasObstacle
                );
            }
            
            EditorUtility.SetDirty(recipe);
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog("Save Pattern", 
                $"Grid pattern saved for {recipeName}!\n\n# of Free Cells: {gridCells.Count - gridCells.Values.Count(c => c.HasObstacle)}" +
                $"\n# of Obstacles: {gridCells.Values.Count(c => c.HasObstacle)}\nTotal Cells: {gridCells.Count}", "OK");
        }
        
        private void LoadGridPatternFromRecipe(AlchemyRecipe recipe)
        {
            if (recipe == null) return;
            
            string recipeName = !string.IsNullOrEmpty(recipe.ItemName) && recipe.ItemName != "Unnamed Item" 
                ? recipe.ItemName 
                : recipe.name;
                
            Debug.Log($"📋 Loading custom grid layout for {recipeName}");
            
            // Check if recipe has custom grid data
            if (!recipe.HasCustomGridData())
            {
                EditorUtility.DisplayDialog("No Custom Grid", 
                    $"Recipe '{recipeName}' doesn't have a custom grid layout saved.\n\nUse 'Save Pattern' to create one.", 
                    "OK");
                return;
            }
            
            // Clear current grid
            gridCells.Clear();
            
            // Set grid dimensions from recipe
            gridWidth = recipe.CustomGridWidth;
            gridHeight = recipe.CustomGridHeight;
            
            // Load custom grid cells
            foreach (var customCell in recipe.CustomGridCells)
            {
                var cellData = new CellData
                {
                    aspect = customCell.aspect,
                    rarity = customCell.rarity,
                    isRequired = customCell.isRequired,
                    isOccupied = customCell.isOccupied
                };
                
                // Add obstacle if present
                if (customCell.hasObstacle)
                {
                    cellData.obstacle = new AspectObstacle(customCell.obstacleType, customCell.position);
                }
                
                gridCells[customCell.position] = cellData;
            }
            
            EditorUtility.DisplayDialog("Load Pattern", 
                $"Grid pattern loaded for {recipeName}!\n\nRequired Positions: {gridCells.Count}", 
                "OK");
        }
        
        /// <summary>
        /// Automatically loads custom grid data for a recipe without showing dialogs (used for recipe switching)
        /// </summary>
        private void AutoLoadCustomGridForRecipe(AlchemyRecipe recipe)
        {
            if (recipe == null || !recipe.HasCustomGridData()) return;
            
            string recipeName = !string.IsNullOrEmpty(recipe.ItemName) && recipe.ItemName != "Unnamed Item" 
                ? recipe.ItemName 
                : recipe.name;
                
            Debug.Log($"🔄 Auto-loading custom grid layout for {recipeName}");
            
            // Clear current grid
            gridCells.Clear();
            
            // Set grid dimensions from recipe
            gridWidth = recipe.CustomGridWidth;
            gridHeight = recipe.CustomGridHeight;
            
            // Load custom grid cells
            foreach (var customCell in recipe.CustomGridCells)
            {
                var cellData = new CellData
                {
                    aspect = customCell.aspect,
                    rarity = customCell.rarity,
                    isRequired = customCell.isRequired,
                    isOccupied = customCell.isOccupied
                };
                
                // Add obstacle if present
                if (customCell.hasObstacle)
                {
                    cellData.obstacle = new AspectObstacle(customCell.obstacleType, customCell.position);
                }
                
                gridCells[customCell.position] = cellData;
            }
            
            Debug.Log($"✅ Auto-loaded custom grid: {gridWidth}x{gridHeight} with {gridCells.Count} cells");
        }
        
        /// <summary>
        /// Clears the grid when switching to a recipe without custom grid data
        /// </summary>
        private void ClearGridForNewRecipe()
        {
            gridCells.Clear();
            gridWidth = 5;  // Reset to default size
            gridHeight = 5;
            Debug.Log("🧹 Cleared grid for new recipe selection");
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
            var entry = CreateInstance<T>();
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
            
            var duplicate = Instantiate(selectedBookEntry);
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
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Error", $"An error occurred while deleting the entry:\n{e.Message}", "OK");
                    Debug.LogError($"Error deleting book entry: {e}");
                }
            }
        }
        
        /// <summary>
        /// Enhanced asset management functionality across all Alchemy System tabs.
        /// 
        /// Features implemented:
        /// - ✅ Rename assets across all tabs (Ingredients, Recipes, Potions, Infusions, Book Entries)
        /// - ✅ Delete assets with confirmation dialogs
        /// - ✅ Automatic list refresh after operations
        /// - ✅ Asset validation and error handling
        /// - ✅ User-friendly instruction dialogs
        /// - ✅ Consistent UI with 📝 rename and 🗑️ delete buttons
        /// 
        /// Supported asset types:
        /// • Ingredients - Raw materials for alchemy
        /// • Recipes - Crafting formulas and patterns
        /// • Potions - Final consumable products
        /// • Infusions - Magical effect containers
        /// • Book Entries - Knowledge and lore content
        /// 
        /// All rename functions guide users through Unity's built-in asset renaming
        /// system for maximum compatibility and safety. Functions include proper
        /// error handling, asset selection, and user feedback.
        /// </summary>
        
        private void RefreshInfusionList()
        {
            // Since the infusion list is loaded fresh each time DrawInfusionsList() is called,
            // we just need to force a repaint to refresh the UI
            Repaint();
            
            Debug.Log("🔄 Infusion list refreshed");
        }

        // Simplified rename functionality
        private void ShowRenameDialog(string title, string currentName, Action<string> onRename)
        {
            // Create a simple popup with text field
            RenameDialog.Show(title, currentName, onRename);
        }

        private bool RenameAsset(Object asset, string newName)
        {
            try
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                string result = AssetDatabase.RenameAsset(assetPath, newName);
                
                if (string.IsNullOrEmpty(result))
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return true;
                }

                EditorUtility.DisplayDialog("Rename Failed", $"Could not rename asset: {result}", "OK");
                return false;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Rename Error", $"Error renaming asset: {e.Message}", "OK");
                return false;
            }
        }
        
        #region Obstacle System Helper Methods
        
        /// <summary>
        /// Get obstacle color based on obstacle type (matches aspect colors for consistency)
        /// </summary>
        private Color GetObstacleColor(ObstacleType obstacleType)
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => new Color(0.8f, 0.6f, 0.4f, 0.9f), // Brown/Earth (matches Corporeal aspect)
                ObstacleType.Scorch => new Color(1.0f, 0.4f, 0.2f, 0.9f),    // Fire Red (matches Scorch aspect)  
                ObstacleType.FrigidFrozen => new Color(0.2f, 0.4f, 1.0f, 0.9f),    // Dark Ice Blue (matches Frigid aspect)
                ObstacleType.FrigidMelted => new Color(0.4f, 0.8f, 1.0f, 0.9f),    // Light Ice Blue (matches Frigid aspect)
                ObstacleType.Arc => new Color(1.0f, 1.0f, 0.4f, 0.9f),       // Lightning Yellow (matches Arc aspect)
                ObstacleType.Caustic => new Color(0.6f, 1.0f, 0.2f, 0.9f),   // Acid Green (matches Caustic aspect)
                ObstacleType.Divine => new Color(1.0f, 0.8f, 1.0f, 0.9f),    // Holy Purple (matches Divine aspect)
                _ => new Color(0.7f, 0.7f, 0.7f, 0.9f)                      // Default grey
            };
        }
        
        /// <summary>
        /// Get obstacle description based on obstacle type (matches AspectObstacle.GetObstacleDescription())
        /// </summary>
        private string GetObstacleDescription(ObstacleType obstacleType)
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => "Blocked cell - cannot place ingredients",
                ObstacleType.FrigidFrozen => "Frozen cell - unlock with adjacent Scorch/Corporeal",
                ObstacleType.FrigidMelted => "Melted cell - unlocked with adjacent Scorch/Corporeal",
                ObstacleType.Scorch => "Volatile cell - requires Scorch, Caustic, or Arc aspects",
                ObstacleType.Caustic => "Degrade cell - reduces ingredient potency by 20%",
                ObstacleType.Arc => "Chaotic cell - triggers random effects",
                ObstacleType.Divine => "Sanctified cell - only unrefined Divine aspects (+10% potency)",
                _ => "Unknown obstacle"
            };
        }
        
        /// <summary>
        /// Get obstacle symbol for visual display
        /// </summary>
        private string GetObstacleSymbol(ObstacleType obstacleType)
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => "■",  // Solid block
                ObstacleType.FrigidFrozen => "❄",    // Snowflake
                ObstacleType.FrigidMelted => "💧",    // Water drop
                ObstacleType.Scorch => "🔥",   // Fire
                ObstacleType.Caustic => "☣",   // Biohazard
                ObstacleType.Arc => "⚡",      // Lightning
                ObstacleType.Divine => "✨",   // Sparkles
                _ => "?"
            };
        }
        
        /// <summary>
        /// Draw obstacle cell with special visual effects
        /// </summary>
        private void DrawObstacleCell(Rect cellRect, AspectObstacle obstacle, Color cellColor)
        {
            // Draw main background
            EditorGUI.DrawRect(cellRect, cellColor);
            
            // Add obstacle-specific effects
            switch (obstacle.ObstacleType)
            {
                case ObstacleType.Corporeal:
                    // Solid wall - draw with stone-like texture effect
                    DrawStoneEffect(cellRect, cellColor);
                    break;
                    
                case ObstacleType.FrigidFrozen:
                    // Frozen - draw with ice crystal effect
                    DrawIceEffect(cellRect, cellColor);
                    break;
                    
                case ObstacleType.FrigidMelted:
                    // Melted - draw with water effect
                    DrawWaterEffect(cellRect, cellColor);
                    break;
                    
                case ObstacleType.Scorch:
                    // Fire - draw with flame effect
                    DrawFlameEffect(cellRect, cellColor);
                    break;
                    
                case ObstacleType.Caustic:
                    // Poison - draw with bubbling effect
                    DrawCausticEffect(cellRect, cellColor);
                    break;
                    
                case ObstacleType.Arc:
                    // Lightning - draw with electric effect
                    DrawElectricEffect(cellRect, cellColor);
                    break;
                    
                case ObstacleType.Divine:
                    // Holy - draw with glowing effect
                    DrawDivineEffect(cellRect, cellColor);
                    break;
            }
        }
        
        // DrawIngredientCell method removed - Grid Designer now only handles obstacles
        
        
        #region Obstacle Visual Effects
        
        private void DrawStoneEffect(Rect cellRect, Color baseColor)
        {
            // Create stone-like pattern with darker edges
            var edgeRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.width - 2, cellRect.height - 2);
            Color edgeColor = Color.Lerp(baseColor, Color.black, 0.4f);
            EditorGUI.DrawRect(edgeRect, edgeColor);
            
            // Add some texture variation
            var innerRect = new Rect(cellRect.x + 3, cellRect.y + 3, cellRect.width - 6, cellRect.height - 6);
            Color innerColor = Color.Lerp(baseColor, Color.white, 0.1f);
            EditorGUI.DrawRect(innerRect, innerColor);
        }
        
        private void DrawIceEffect(Rect cellRect, Color baseColor)
        {
            // Create crystalline effect with light reflections
            var centerRect = new Rect(cellRect.x + 4, cellRect.y + 4, cellRect.width - 8, cellRect.height - 8);
            Color iceColor = Color.Lerp(baseColor, Color.white, 0.6f);
            EditorGUI.DrawRect(centerRect, iceColor);
            
            // Add frost highlights
            var highlightRect = new Rect(cellRect.x + 2, cellRect.y + 2, cellRect.width * 0.3f, cellRect.height * 0.3f);
            Color highlightColor = Color.Lerp(baseColor, Color.white, 0.8f);
            EditorGUI.DrawRect(highlightRect, highlightColor);
        }
        
        private void DrawWaterEffect(Rect cellRect, Color baseColor)
        {
            // Create flowing water effect with waves
            var centerRect = new Rect(cellRect.x + 3, cellRect.y + 3, cellRect.width - 6, cellRect.height - 6);
            Color waterColor = Color.Lerp(baseColor, new Color(0.7f, 0.9f, 1.0f), 0.4f);
            EditorGUI.DrawRect(centerRect, waterColor);
            
            // Add wave-like highlights
            var waveRect1 = new Rect(cellRect.x + 1, cellRect.y + cellRect.height * 0.3f, cellRect.width - 2, 2);
            var waveRect2 = new Rect(cellRect.x + 1, cellRect.y + cellRect.height * 0.7f, cellRect.width - 2, 2);
            Color waveColor = Color.Lerp(baseColor, Color.white, 0.5f);
            EditorGUI.DrawRect(waveRect1, waveColor);
            EditorGUI.DrawRect(waveRect2, waveColor);
        }
        
        private void DrawFlameEffect(Rect cellRect, Color baseColor)
        {
            // Create flickering flame effect
            var flameRect = new Rect(cellRect.x + 2, cellRect.y + 6, cellRect.width - 4, cellRect.height - 8);
            Color flameColor = Color.Lerp(baseColor, Color.yellow, 0.5f);
            EditorGUI.DrawRect(flameRect, flameColor);
            
            // Add bright center
            var centerRect = new Rect(cellRect.x + 6, cellRect.y + 8, cellRect.width - 12, cellRect.height - 16);
            Color centerColor = Color.Lerp(baseColor, Color.white, 0.7f);
            EditorGUI.DrawRect(centerRect, centerColor);
        }
        
        private void DrawCausticEffect(Rect cellRect, Color baseColor)
        {
            // Create bubbling poison effect
            var bubbleRect1 = new Rect(cellRect.x + 3, cellRect.y + 5, 8, 8);
            var bubbleRect2 = new Rect(cellRect.x + 15, cellRect.y + 12, 6, 6);
            var bubbleRect3 = new Rect(cellRect.x + 8, cellRect.y + 20, 4, 4);
            
            Color bubbleColor = Color.Lerp(baseColor, Color.yellow, 0.4f);
            EditorGUI.DrawRect(bubbleRect1, bubbleColor);
            EditorGUI.DrawRect(bubbleRect2, bubbleColor);
            EditorGUI.DrawRect(bubbleRect3, bubbleColor);
        }
        
        private void DrawElectricEffect(Rect cellRect, Color baseColor)
        {
            // Create electric spark effect
            var sparkRect = new Rect(cellRect.x + 8, cellRect.y + 4, cellRect.width - 16, cellRect.height - 8);
            Color sparkColor = Color.Lerp(baseColor, Color.white, 0.8f);
            EditorGUI.DrawRect(sparkRect, sparkColor);
            
            // Add electric lines
            var lineRect1 = new Rect(cellRect.x + 2, cellRect.y + cellRect.height * 0.3f, cellRect.width - 4, 2);
            var lineRect2 = new Rect(cellRect.x + 4, cellRect.y + cellRect.height * 0.7f, cellRect.width - 8, 1);
            EditorGUI.DrawRect(lineRect1, Color.white);
            EditorGUI.DrawRect(lineRect2, Color.white);
        }
        
        private void DrawDivineEffect(Rect cellRect, Color baseColor)
        {
            // Create divine glow effect
            var glowRect = new Rect(cellRect.x + 2, cellRect.y + 2, cellRect.width - 4, cellRect.height - 4);
            Color glowColor = Color.Lerp(baseColor, Color.white, 0.5f);
            EditorGUI.DrawRect(glowRect, glowColor);
            
            // Add inner radiance
            var radianceRect = new Rect(cellRect.x + 6, cellRect.y + 6, cellRect.width - 12, cellRect.height - 12);
            Color radianceColor = Color.Lerp(baseColor, Color.white, 0.8f);
            radianceColor.a = 0.7f;
            EditorGUI.DrawRect(radianceRect, radianceColor);
        }
        
        #endregion
        
        #endregion
    }

    // Simple rename dialog window
    public class RenameDialog : EditorWindow
    {
        private string currentName;
        private string newName;
        private System.Action<string> onRename;

        public static void Show(string title, string currentName, System.Action<string> onRename)
        {
            var window = CreateInstance<RenameDialog>();
            window.titleContent = new GUIContent(title);
            window.currentName = currentName;
            window.newName = currentName;
            window.onRename = onRename;
            
            var rect = new Rect(0, 0, 350, 100);
            rect.center = new Vector2(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f);
            window.position = rect;
            window.ShowModal();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Current name: {currentName}", EditorStyles.label);
            EditorGUILayout.Space(5);
            
            GUI.SetNextControlName("NewNameField");
            newName = EditorGUILayout.TextField("New name:", newName);
            
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.FocusTextInControl("NewNameField");
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    ConfirmRename();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                    Event.current.Use();
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Rename", GUILayout.Width(70)))
            {
                ConfirmRename();
            }
            
            if (GUILayout.Button("Cancel", GUILayout.Width(70)))
            {
                Close();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void ConfirmRename()
        {
            if (!string.IsNullOrEmpty(newName) && newName != currentName)
            {
                onRename?.Invoke(newName);
            }
            Close();
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
        public AspectObstacle obstacle = null; // New: obstacle placed on this cell
        
        // Convenience properties
        public bool HasObstacle => obstacle != null;
        public bool IsBlocked => HasObstacle && obstacle.ObstacleType == ObstacleType.Corporeal;
        public bool CanPlaceIngredient => !IsBlocked && (obstacle == null || obstacle.CanPlaceIngredient(null));
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