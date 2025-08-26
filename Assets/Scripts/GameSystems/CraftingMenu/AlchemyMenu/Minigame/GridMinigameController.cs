using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu
{
    public class GridMinigameController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;
        public UIDocument UIDocument => uiDocument;
        
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 6;
        [SerializeField] private float cellSize = 40f;
        
        [Header("Recipe Settings")]
        [SerializeField] private List<AlchemyRecipe> availableRecipes;
        
        // UI Elements
        private VisualElement mainContainer;
        private Button backButton;
        private Button alchemyBookButton;
        private ScrollView ingredientPalette;
        private VisualElement gridContainer;
        private ScrollView recipeSelection;
        private Button clearButton;
        private Button craftButton;
        private Label gridStatusLabel;
        private Label patternMatchLabel;
        private Label recipeInfoLabel;
        
        // Game State
        private GridCell[,] craftingGrid;
        private AlchemyRecipe selectedRecipe;
        private List<Ingredient> availableIngredients;
        private Dictionary<Vector2Int, Ingredient> placedIngredients;
        private Ingredient selectedIngredient;
        
        // Events
        public event Action OnBackPressed;
        public event Action OnAlchemyBookPressed;
        public event Action<AlchemyRecipe, Dictionary<Vector2Int, Ingredient>> OnCraftingCompleted;
        
        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
                
            placedIngredients = new Dictionary<Vector2Int, Ingredient>();
            InitializeGrid();
        }
        
        private void OnEnable()
        {
            if (uiDocument?.rootVisualElement != null)
                SetupUI();
        }
        
        private void OnDisable()
        {
            CleanupUI();
        }
        
        private void InitializeGrid()
        {
            craftingGrid = new GridCell[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    craftingGrid[x, y] = new GridCell();
                }
            }
        }
        
        public void SetAvailableIngredients(List<Ingredient> ingredients)
        {
            availableIngredients = new List<Ingredient>(ingredients);
            RefreshIngredientPalette();
        }
        
        public void SetAvailableRecipes(List<AlchemyRecipe> recipes)
        {
            availableRecipes = new List<AlchemyRecipe>(recipes);
            RefreshRecipeSelection();
        }
        
        private void SetupUI()
        {
            var root = uiDocument.rootVisualElement;
            
            // Get UI elements
            mainContainer = root.Q<VisualElement>("MainContainer");
            backButton = root.Q<Button>("BackButton");
            alchemyBookButton = root.Q<Button>("AlchemyBookButton");
            ingredientPalette = root.Q<ScrollView>("IngredientPalette");
            gridContainer = root.Q<VisualElement>("GridContainer");
            recipeSelection = root.Q<ScrollView>("RecipeSelection");
            clearButton = root.Q<Button>("ClearButton");
            craftButton = root.Q<Button>("CraftButton");
            gridStatusLabel = root.Q<Label>("GridStatusLabel");
            patternMatchLabel = root.Q<Label>("PatternMatchLabel");
            recipeInfoLabel = root.Q<Label>("RecipeInfoLabel");
            
            // Setup event handlers
            if (backButton != null)
                backButton.clicked += () => OnBackPressed?.Invoke();
                
            if (alchemyBookButton != null)
                alchemyBookButton.clicked += () => OnAlchemyBookPressed?.Invoke();
            else
                Debug.LogWarning("Alchemy book button not found in UI!");
                
            if (clearButton != null)
                clearButton.clicked += ClearGrid;
                
            if (craftButton != null)
                craftButton.clicked += AttemptCrafting;
            
            // Initialize UI
            CreateGridUI();
            RefreshIngredientPalette();
            RefreshRecipeSelection();
            UpdateUI();
        }
        
        private void CleanupUI()
        {
            if (backButton != null)
                backButton.clicked -= () => OnBackPressed?.Invoke();
                
            if (alchemyBookButton != null)
                alchemyBookButton.clicked -= () => OnAlchemyBookPressed?.Invoke();
                
            if (clearButton != null)
                clearButton.clicked -= ClearGrid;
                
            if (craftButton != null)
                craftButton.clicked -= AttemptCrafting;
        }
        
        private void CreateGridUI()
        {
            if (gridContainer == null) return;
            
            gridContainer.Clear();
            
            var gridElement = new VisualElement();
            gridElement.style.flexDirection = FlexDirection.Column;
            
            for (int y = 0; y < gridHeight; y++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                
                for (int x = 0; x < gridWidth; x++)
                {
                    var cell = CreateGridCell(x, y);
                    row.Add(cell);
                }
                
                gridElement.Add(row);
            }
            
            gridContainer.Add(gridElement);
        }
        
        private VisualElement CreateGridCell(int x, int y)
        {
            var cell = new VisualElement();
            cell.AddToClassList("grid-cell");
            cell.AddToClassList("empty");
            
            cell.style.width = cellSize;
            cell.style.height = cellSize;
            
            var position = new Vector2Int(x, y);
            
            // Add click handler
            cell.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.ctrlKey)
                {
                    // Remove ingredient
                    RemoveIngredient(position);
                }
                else if (selectedIngredient != null)
                {
                    // Place ingredient
                    PlaceIngredient(position, selectedIngredient);
                }
            });
            
            // Add hover effects
            cell.RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (selectedIngredient != null && CanPlaceIngredient(position, selectedIngredient))
                {
                    cell.AddToClassList("preview");
                }
            });
            
            cell.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                cell.RemoveFromClassList("preview");
            });
            
            craftingGrid[x, y].visualElement = cell;
            return cell;
        }
        
        private void RefreshIngredientPalette()
        {
            if (ingredientPalette == null || availableIngredients == null) return;
            
            ingredientPalette.Clear();
            
            // Group ingredients by relevance (used in selected recipe)
            var relevantIngredients = new List<Ingredient>();
            var otherIngredients = new List<Ingredient>();
            
            foreach (var ingredient in availableIngredients)
            {
                if (selectedRecipe != null && IsIngredientRelevant(ingredient))
                    relevantIngredients.Add(ingredient);
                else
                    otherIngredients.Add(ingredient);
            }
            
            // Add relevant ingredients first (show only 3)
            if (relevantIngredients.Count > 0)
            {
                var relevantLabel = new Label("Relevant for Recipe:");
                relevantLabel.AddToClassList("section-title");
                ingredientPalette.Add(relevantLabel);
                
                foreach (var ingredient in relevantIngredients.Take(3))
                {
                    var item = CreateIngredientItem(ingredient);
                    ingredientPalette.Add(item);
                }
            }
            
            // Add scroll section for other ingredients
            if (otherIngredients.Count > 0)
            {
                var otherLabel = new Label("All Ingredients:");
                otherLabel.AddToClassList("section-title");
                ingredientPalette.Add(otherLabel);
                
                foreach (var ingredient in otherIngredients)
                {
                    var item = CreateIngredientItem(ingredient);
                    ingredientPalette.Add(item);
                }
            }
        }
        
        private VisualElement CreateIngredientItem(Ingredient ingredient)
        {
            var item = new VisualElement();
            item.AddToClassList("ingredient-item");
            
            // Create tetris block preview
            var preview = CreateIngredientPreview(ingredient);
            item.Add(preview);
            
            var labelContainer = new VisualElement();
            labelContainer.style.flexGrow = 1;
            
            var nameLabel = new Label(ingredient.ItemName);
            nameLabel.AddToClassList("ingredient-label");
            labelContainer.Add(nameLabel);
            
            var countLabel = new Label($"x{GetIngredientCount(ingredient)}");
            countLabel.AddToClassList("ingredient-count");
            labelContainer.Add(countLabel);
            
            item.Add(labelContainer);
            
            // Add click handler
            item.RegisterCallback<ClickEvent>(evt =>
            {
                SelectIngredient(ingredient);
            });
            
            return item;
        }
        
        private VisualElement CreateIngredientPreview(Ingredient ingredient)
        {
            var preview = new VisualElement();
            preview.AddToClassList("ingredient-preview");
            
            // Create mini grid representation showing tetris block shape
            var miniGrid = new VisualElement();
            miniGrid.style.flexDirection = FlexDirection.Column;
            
            for (int y = 0; y < ingredient.GridHeight; y++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                
                for (int x = 0; x < ingredient.GridWidth; x++)
                {
                    var cell = new VisualElement();
                    cell.style.width = 6;
                    cell.style.height = 6;
                    cell.style.backgroundColor = GetAspectColor(ingredient.IngredientAspect);
                    cell.style.marginTop = 1;
                    cell.style.marginLeft = 1;
                    row.Add(cell);
                }
                
                miniGrid.Add(row);
            }
            
            preview.Add(miniGrid);
            return preview;
        }

        #region Enhanced Recipe and Grid System Methods

        private void RefreshRecipeSelection()
        {
            if (recipeSelection == null || availableRecipes == null) return;
            
            recipeSelection.Clear();
            
            foreach (var recipe in availableRecipes)
            {
                var item = CreateRecipeItem(recipe);
                recipeSelection.Add(item);
            }
        }
        
        private VisualElement CreateRecipeItem(AlchemyRecipe recipe)
        {
            var item = new VisualElement();
            item.AddToClassList("recipe-item");
            
            if (selectedRecipe == recipe)
                item.AddToClassList("selected");
            
            var nameLabel = new Label(recipe.ItemName);
            nameLabel.AddToClassList("recipe-name");
            item.Add(nameLabel);
            
            var difficultyLabel = new Label("Standard Difficulty");
            difficultyLabel.AddToClassList("recipe-difficulty");
            item.Add(difficultyLabel);
            
            item.RegisterCallback<ClickEvent>(evt =>
            {
                SelectRecipe(recipe);
            });
            
            return item;
        }
        
        private void SelectIngredient(Ingredient ingredient)
        {
            selectedIngredient = ingredient;
            UpdateIngredientSelection();
        }
        
        private void SelectRecipe(AlchemyRecipe recipe)
        {
            selectedRecipe = recipe;
            UpdateRecipeSelection();
            RefreshIngredientPalette();
            UpdateRecipeInfo();
            UpdateUI();
        }
        
        private void UpdateIngredientSelection()
        {
            if (ingredientPalette == null) return;
            
            var items = ingredientPalette.Query<VisualElement>("ingredient-item").ToList();
            foreach (var item in items)
            {
                item.RemoveFromClassList("selected");
            }
        }
        
        private void UpdateRecipeSelection()
        {
            if (recipeSelection == null) return;
            
            var items = recipeSelection.Query<VisualElement>("recipe-item").ToList();
            foreach (var item in items)
            {
                item.RemoveFromClassList("selected");
                if (selectedRecipe != null)
                {
                    var nameLabel = item.Q<Label>();
                    if (nameLabel?.text == selectedRecipe.ItemName)
                        item.AddToClassList("selected");
                }
            }
        }
        
        private void UpdateRecipeInfo()
        {
            if (recipeInfoLabel == null) return;
            
            if (selectedRecipe == null)
            {
                recipeInfoLabel.text = "Select a recipe to see pattern requirements";
            }
            else
            {
                recipeInfoLabel.text = $"Recipe: {selectedRecipe.ItemName}\nRequired Pattern: {gridWidth}x{gridHeight} grid\nFill specific positions with matching aspect ingredients";
            }
        }
        
        private bool CanPlaceIngredient(Vector2Int position, Ingredient ingredient)
        {
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var checkPos = new Vector2Int(position.x + x, position.y + y);
                    if (checkPos.x >= gridWidth || checkPos.y >= gridHeight)
                        return false;
                        
                    if (placedIngredients.ContainsKey(checkPos))
                        return false;
                }
            }
            
            return true;
        }
        
        private void PlaceIngredient(Vector2Int position, Ingredient ingredient)
        {
            if (!CanPlaceIngredient(position, ingredient)) return;
            
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var cellPos = new Vector2Int(position.x + x, position.y + y);
                    placedIngredients[cellPos] = ingredient;
                    
                    var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                    cell.RemoveFromClassList("empty");
                    cell.AddToClassList("filled");
                    cell.AddToClassList($"aspect-{ingredient.IngredientAspect.ToString().ToLower()}");
                    
                    craftingGrid[cellPos.x, cellPos.y].ingredient = ingredient;
                    craftingGrid[cellPos.x, cellPos.y].isEmpty = false;
                }
            }
            
            UpdateUI();
        }
        
        private void RemoveIngredient(Vector2Int position)
        {
            if (!placedIngredients.ContainsKey(position)) return;
            
            var ingredient = placedIngredients[position];
            
            Vector2Int origin = position;
            for (int x = position.x - ingredient.GridWidth + 1; x <= position.x; x++)
            {
                for (int y = position.y - ingredient.GridHeight + 1; y <= position.y; y++)
                {
                    var checkPos = new Vector2Int(x, y);
                    if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight &&
                        placedIngredients.ContainsKey(checkPos) && placedIngredients[checkPos] == ingredient)
                    {
                        origin = checkPos;
                        break;
                    }
                }
            }
            
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var cellPos = new Vector2Int(origin.x + x, origin.y + y);
                    if (cellPos.x < gridWidth && cellPos.y < gridHeight)
                    {
                        placedIngredients.Remove(cellPos);
                        
                        var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                        cell.RemoveFromClassList("filled");
                        cell.RemoveFromClassList($"aspect-{ingredient.IngredientAspect.ToString().ToLower()}");
                        cell.AddToClassList("empty");
                        
                        craftingGrid[cellPos.x, cellPos.y].ingredient = null;
                        craftingGrid[cellPos.x, cellPos.y].isEmpty = true;
                    }
                }
            }
            
            UpdateUI();
        }
        
        private void ClearGrid()
        {
            placedIngredients.Clear();
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = craftingGrid[x, y].visualElement;
                    cell.RemoveFromClassList("filled");
                    
                    foreach (Aspect aspect in Enum.GetValues(typeof(Aspect)))
                    {
                        cell.RemoveFromClassList($"aspect-{aspect.ToString().ToLower()}");
                    }
                    
                    cell.AddToClassList("empty");
                    
                    craftingGrid[x, y].ingredient = null;
                    craftingGrid[x, y].isEmpty = true;
                }
            }
            
            UpdateUI();
        }
        
        private void AttemptCrafting()
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("No recipe selected!");
                return;
            }
            
            if (placedIngredients.Count == 0)
            {
                Debug.LogWarning("No ingredients placed!");
                return;
            }
            
            float patternMatch = CalculatePatternMatch();
            
            if (patternMatch >= 0.8f)
            {
                OnCraftingCompleted?.Invoke(selectedRecipe, new Dictionary<Vector2Int, Ingredient>(placedIngredients));
                ClearGrid();
            }
            else
            {
                Debug.LogWarning($"Pattern match too low: {patternMatch:P}. Need at least 80%.");
            }
        }
        
        private float CalculatePatternMatch()
        {
            if (selectedRecipe == null) return 0f;
            
            int totalCells = gridWidth * gridHeight;
            int filledCells = placedIngredients.Count;
            
            return (float)filledCells / totalCells;
        }
        
        private void UpdateUI()
        {
            if (gridStatusLabel != null)
            {
                string status = placedIngredients.Count == 0 ? "Empty" : $"{placedIngredients.Count} cells filled";
                gridStatusLabel.text = $"Grid Status: {status}";
            }
            
            if (patternMatchLabel != null)
            {
                float match = CalculatePatternMatch();
                patternMatchLabel.text = $"Pattern Match: {match:P}";
            }
            
            if (craftButton != null)
            {
                bool canCraft = selectedRecipe != null && placedIngredients.Count > 0;
                craftButton.SetEnabled(canCraft);
            }
        }

        #endregion
        
        private bool IsIngredientRelevant(Ingredient ingredient)
        {
            if (selectedRecipe == null) return false;
            
            // Check if ingredient is in recipe requirements
            return selectedRecipe.InputIngredient1 == ingredient || 
                   selectedRecipe.InputIngredient2 == ingredient || 
                   selectedRecipe.InputIngredient3 == ingredient;
        }
        
        private int GetIngredientCount(Ingredient ingredient)
        {
            // TODO: Get actual count from inventory system
            return 5; // Placeholder
        }
        
        private Color GetAspectColor(Aspect aspect)
        {
            return aspect switch
            {
                Aspect.Scorch => new Color(0.8f, 0.3f, 0.3f),
                Aspect.Frigid => new Color(0.3f, 0.5f, 0.8f),
                Aspect.Corporeal => new Color(0.5f, 0.6f, 0.3f),
                Aspect.Arc => new Color(0.7f, 0.7f, 0.3f),
                Aspect.Divine => new Color(0.6f, 0.5f, 0.7f),
                Aspect.Caustic => new Color(0.9f, 0.6f, 0.2f),
                _ => Color.gray
            };
        }

        public void Show()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                enabled = true;
            }
        }

        public void Hide()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.None;
                enabled = false;
            }
        }
        
        [System.Serializable]
        private class GridCell
        {
            public bool isEmpty = true;
            public Ingredient ingredient;
            public VisualElement visualElement;
        }
    }
}