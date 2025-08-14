using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu
{
    public class GridMinigameController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        public UIDocument UIDocument => uiDocument;

        [Header("Grid Settings")]
        [SerializeField] private int baseGridWidth = 4;
        [SerializeField] private int baseGridHeight = 4;
        [SerializeField] private float cellSize = 50f;

        private CraftingGrid craftingGrid;
        private VisualElement gridContainer;
        private VisualElement ingredientPalette;
        private Button confirmButton;
        private Button cancelButton;
        private Label instructionsLabel;
        private Label gridStatusLabel;

        private List<Ingredient> availableIngredients;
        private Ingredient selectedIngredient;
        private bool isDragging;
        private VisualElement dragPreview;

        public event Action<bool, CraftingRank> OnMinigameEnd;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            craftingGrid = new CraftingGrid(baseGridWidth, baseGridHeight);
            Hide();
        }

        public void Init(List<Ingredient> ingredients)
        {
            availableIngredients = new List<Ingredient>(ingredients);
            craftingGrid.Clear();
            
            SetupUI();
            Show();
        }

        private void SetupUI()
        {
            var root = uiDocument.rootVisualElement;

            gridContainer = root.Q<VisualElement>("GridContainer");
            ingredientPalette = root.Q<VisualElement>("IngredientPalette");
            confirmButton = root.Q<Button>("ConfirmButton");
            cancelButton = root.Q<Button>("CancelButton");
            instructionsLabel = root.Q<Label>("InstructionsLabel");
            gridStatusLabel = root.Q<Label>("GridStatusLabel");

            if (gridContainer == null || ingredientPalette == null)
            {
                Debug.LogError("GridMinigameController: Missing required UI elements");
                return;
            }

            confirmButton?.RegisterCallback<ClickEvent>(_ => ConfirmCrafting());
            cancelButton?.RegisterCallback<ClickEvent>(_ => CancelCrafting());

            BuildGrid();
            BuildIngredientPalette();
            UpdateUI();
        }

        private void BuildGrid()
        {
            gridContainer.Clear();
            gridContainer.style.width = craftingGrid.Width * cellSize;
            gridContainer.style.height = craftingGrid.Height * cellSize;
            gridContainer.style.flexDirection = FlexDirection.Row;
            gridContainer.style.flexWrap = Wrap.Wrap;

            for (int y = 0; y < craftingGrid.Height; y++)
            {
                for (int x = 0; x < craftingGrid.Width; x++)
                {
                    var cell = new VisualElement();
                    cell.name = $"GridCell_{x}_{y}";
                    cell.style.width = cellSize;
                    cell.style.height = cellSize;
                    cell.style.backgroundColor = new StyleColor(Color.gray);
                    cell.style.borderLeftColor = new StyleColor(Color.black);
                    cell.style.borderRightColor = new StyleColor(Color.black);
                    cell.style.borderTopColor = new StyleColor(Color.black);
                    cell.style.borderBottomColor = new StyleColor(Color.black);
                    cell.style.borderLeftWidth = 1;
                    cell.style.borderRightWidth = 1;
                    cell.style.borderTopWidth = 1;
                    cell.style.borderBottomWidth = 1;

                    var pos = new GridPosition(x, y);
                    cell.RegisterCallback<PointerDownEvent>(evt => OnCellClicked(pos));
                    cell.RegisterCallback<PointerEnterEvent>(evt => OnCellHover(pos));

                    gridContainer.Add(cell);
                }
            }

            // Add placed ingredients visual representation
            foreach (var placed in craftingGrid.PlacedIngredients)
            {
                AddIngredientVisual(placed);
            }
        }

        private void BuildIngredientPalette()
        {
            ingredientPalette.Clear();

            foreach (var ingredient in availableIngredients)
            {
                var ingredientButton = new Button(() => SelectIngredient(ingredient));
                ingredientButton.text = $"{ingredient.ItemName}\n{ingredient.GridWidth}x{ingredient.GridHeight}\nPotency: {ingredient.Potency}";
                ingredientButton.style.width = 100;
                ingredientButton.style.height = 60;
                ingredientButton.style.marginAll = 2;

                if (ingredient.UnlocksAdditionalSpace)
                {
                    ingredientButton.style.backgroundColor = new StyleColor(Color.yellow);
                    ingredientButton.tooltip = $"Unlocks {ingredient.AdditionalSpaceCount} additional grid spaces";
                }

                ingredientPalette.Add(ingredientButton);
            }
        }

        private void SelectIngredient(Ingredient ingredient)
        {
            selectedIngredient = ingredient;
            UpdateUI();
        }

        private void OnCellClicked(GridPosition position)
        {
            if (selectedIngredient == null) return;

            // Try to place the ingredient
            if (craftingGrid.CanPlaceIngredient(selectedIngredient, position))
            {
                if (craftingGrid.PlaceIngredient(selectedIngredient, position))
                {
                    // Remove from available ingredients
                    availableIngredients.Remove(selectedIngredient);
                    selectedIngredient = null;

                    // Rebuild UI to reflect changes
                    BuildGrid();
                    BuildIngredientPalette();
                    UpdateUI();
                }
            }
            else
            {
                Debug.Log("Cannot place ingredient at this position");
            }
        }

        private void OnCellHover(GridPosition position)
        {
            if (selectedIngredient == null) return;

            // Show preview of where ingredient would be placed
            ShowPlacementPreview(position);
        }

        private void ShowPlacementPreview(GridPosition position)
        {
            // Clear previous preview
            ClearPlacementPreview();

            if (selectedIngredient == null) return;

            bool canPlace = craftingGrid.CanPlaceIngredient(selectedIngredient, position);
            var previewColor = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);

            for (int x = 0; x < selectedIngredient.GridWidth; x++)
            {
                for (int y = 0; y < selectedIngredient.GridHeight; y++)
                {
                    var cellPos = new GridPosition(position.x + x, position.y + y);
                    if (cellPos.x >= 0 && cellPos.x < craftingGrid.Width &&
                        cellPos.y >= 0 && cellPos.y < craftingGrid.Height)
                    {
                        var cell = gridContainer.Q<VisualElement>($"GridCell_{cellPos.x}_{cellPos.y}");
                        if (cell != null)
                        {
                            cell.style.backgroundColor = new StyleColor(previewColor);
                            cell.AddToClassList("preview-cell");
                        }
                    }
                }
            }
        }

        private void ClearPlacementPreview()
        {
            var previewCells = gridContainer.Query<VisualElement>(className: "preview-cell").ToList();
            foreach (var cell in previewCells)
            {
                cell.style.backgroundColor = new StyleColor(Color.gray);
                cell.RemoveFromClassList("preview-cell");
            }
        }

        private void AddIngredientVisual(PlacedIngredient placedIngredient)
        {
            var ingredient = placedIngredient.ingredient;
            var position = placedIngredient.position;

            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var cellPos = new GridPosition(position.x + x, position.y + y);
                    var cell = gridContainer.Q<VisualElement>($"GridCell_{cellPos.x}_{cellPos.y}");
                    if (cell != null)
                    {
                        cell.style.backgroundColor = new StyleColor(GetIngredientColor(ingredient));
                        
                        // Add ingredient name on the first cell
                        if (x == 0 && y == 0)
                        {
                            var label = new Label(ingredient.ItemName);
                            label.style.fontSize = 8;
                            label.style.unityTextAlign = TextAnchor.MiddleCenter;
                            label.style.color = new StyleColor(Color.white);
                            cell.Add(label);
                        }

                        // Make cell clickable for removal
                        cell.RegisterCallback<ClickEvent>(evt =>
                        {
                            if (evt.ctrlKey) // Ctrl+Click to remove
                            {
                                RemoveIngredient(placedIngredient);
                            }
                        });
                    }
                }
            }
        }

        private Color GetIngredientColor(Ingredient ingredient)
        {
            return ingredient.IngredientArchetype switch
            {
                IngredientArchetype.Solvent => Color.blue,
                IngredientArchetype.Base => Color.green,
                IngredientArchetype.Modifier => Color.red,
                IngredientArchetype.Catalyst => Color.yellow,
                _ => Color.gray
            };
        }

        private void RemoveIngredient(PlacedIngredient placedIngredient)
        {
            if (craftingGrid.RemoveIngredient(placedIngredient))
            {
                // Add back to available ingredients
                availableIngredients.Add(placedIngredient.ingredient);

                // Rebuild UI
                BuildGrid();
                BuildIngredientPalette();
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (instructionsLabel != null)
            {
                if (selectedIngredient != null)
                {
                    instructionsLabel.text = $"Click on grid to place {selectedIngredient.ItemName} ({selectedIngredient.GridWidth}x{selectedIngredient.GridHeight})";
                }
                else
                {
                    instructionsLabel.text = "Select an ingredient from the palette to place it on the grid";
                }
            }

            if (gridStatusLabel != null)
            {
                var efficiency = craftingGrid.CalculateGridEfficiency();
                var potency = craftingGrid.CalculateTotalPotency();
                var isValid = craftingGrid.HasValidConfiguration();

                gridStatusLabel.text = $"Grid Efficiency: {efficiency:P0}\nTotal Potency: {potency}\nValid: {(isValid ? "Yes" : "No")}";
            }

            if (confirmButton != null)
            {
                confirmButton.SetEnabled(craftingGrid.HasValidConfiguration());
            }
        }

        private void ConfirmCrafting()
        {
            if (!craftingGrid.HasValidConfiguration())
            {
                Debug.LogWarning("Invalid crafting configuration");
                return;
            }

            var rank = CalculateCraftingRank();
            OnMinigameEnd?.Invoke(true, rank);
            Hide();
        }

        private void CancelCrafting()
        {
            OnMinigameEnd?.Invoke(false, CraftingRank.F);
            Hide();
        }

        private CraftingRank CalculateCraftingRank()
        {
            var efficiency = craftingGrid.CalculateGridEfficiency();
            var potency = craftingGrid.CalculateTotalPotency();
            var ingredientCount = craftingGrid.PlacedIngredients.Count;

            // Calculate score based on multiple factors
            float score = 0f;
            
            // Efficiency component (40% of score)
            score += efficiency * 0.4f;
            
            // Potency component (30% of score)
            float avgPotency = (float)potency / ingredientCount;
            score += (avgPotency / 5f) * 0.3f; // Max potency is 5
            
            // Ingredient utilization (30% of score)
            float utilization = (float)ingredientCount / availableIngredients.Count;
            score += utilization * 0.3f;

            // Convert score to rank
            return score switch
            {
                >= 0.95f => CraftingRank.S,
                >= 0.85f => CraftingRank.A,
                >= 0.75f => CraftingRank.B,
                >= 0.65f => CraftingRank.C,
                >= 0.50f => CraftingRank.D,
                >= 0.35f => CraftingRank.E,
                _ => CraftingRank.F
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
    }
}