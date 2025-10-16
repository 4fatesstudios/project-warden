using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    [Serializable]
    public struct GridPosition
    {
        public int x;
        public int y;

        public GridPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static GridPosition operator +(GridPosition a, GridPosition b)
        {
            return new GridPosition(a.x + b.x, a.y + b.y);
        }

        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition position && x == position.x && y == position.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }

    [Serializable]
    public class PlacedIngredient
    {
        public Ingredient ingredient;
        public GridPosition position;
        public List<GridPosition> occupiedCells;
        public List<GridPosition> expansionCells;
        public bool isOverlapping;
        public List<string> interactions;
        public int rotation;

        public PlacedIngredient(Ingredient ingredient, GridPosition position, int rotation = 0)
        {
            this.ingredient = ingredient;
            this.position = position;
            this.rotation = rotation;
            this.occupiedCells = new List<GridPosition>();
            this.expansionCells = new List<GridPosition>();
            this.isOverlapping = false;
            this.interactions = new List<string>();

            // Calculate occupied cells based on ingredient size
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    occupiedCells.Add(new GridPosition(position.x + x, position.y + y));
                }
            }
            
            // Calculate expansion cells if this ingredient unlocks additional space
            if (ingredient.UnlocksAdditionalSpace && ingredient.ShapeData != null)
            {
                var expansionOffsets = ingredient.ShapeData.GetExpansionOffsets(rotation);
                foreach (var offset in expansionOffsets)
                {
                    expansionCells.Add(new GridPosition(position.x + offset.x, position.y + offset.y));
                }
            }
        }
    }

    public class CraftingGrid
    {
        private readonly int baseWidth;
        private readonly int baseHeight;
        private int currentWidth;
        private int currentHeight;
        private readonly HashSet<GridPosition> occupiedCells;
        private readonly List<PlacedIngredient> placedIngredients;

        public int Width => currentWidth;
        public int Height => currentHeight;
        public IReadOnlyList<PlacedIngredient> PlacedIngredients => placedIngredients;

        public CraftingGrid(int baseWidth = 4, int baseHeight = 4)
        {
            this.baseWidth = baseWidth;
            this.baseHeight = baseHeight;
            this.currentWidth = baseWidth;
            this.currentHeight = baseHeight;
            this.occupiedCells = new HashSet<GridPosition>();
            this.placedIngredients = new List<PlacedIngredient>();
        }

        public bool CanPlaceIngredient(Ingredient ingredient, GridPosition position)
        {
            if (ingredient == null) return false;

            // Check if the ingredient fits within current grid bounds
            if (position.x < 0 || position.y < 0 ||
                position.x + ingredient.GridWidth > currentWidth ||
                position.y + ingredient.GridHeight > currentHeight)
            {
                return false;
            }

            // Check if any cells are already occupied
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var cellPos = new GridPosition(position.x + x, position.y + y);
                    if (occupiedCells.Contains(cellPos))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool PlaceIngredient(Ingredient ingredient, GridPosition position)
        {
            if (!CanPlaceIngredient(ingredient, position))
                return false;

            var placedIngredient = new PlacedIngredient(ingredient, position);
            placedIngredients.Add(placedIngredient);

            // Mark cells as occupied
            foreach (var cell in placedIngredient.occupiedCells)
            {
                occupiedCells.Add(cell);
            }

            // Check if this ingredient unlocks additional space
            if (ingredient.UnlocksAdditionalSpace)
            {
                ExpandGrid(ingredient.AdditionalSpaceCount);
            }

            return true;
        }

        public bool RemoveIngredient(PlacedIngredient placedIngredient)
        {
            if (!placedIngredients.Contains(placedIngredient))
                return false;

            placedIngredients.Remove(placedIngredient);

            // Free up occupied cells
            foreach (var cell in placedIngredient.occupiedCells)
            {
                occupiedCells.Remove(cell);
            }

            // Recalculate grid size (in case we need to shrink it back)
            RecalculateGridSize();

            return true;
        }

        private void ExpandGrid(int additionalCells)
        {
            // Simple expansion: add to width first, then height
            int cellsToAdd = additionalCells;
            
            while (cellsToAdd > 0)
            {
                if (currentWidth <= currentHeight)
                {
                    currentWidth++;
                    cellsToAdd -= currentHeight;
                }
                else
                {
                    currentHeight++;
                    cellsToAdd -= currentWidth;
                }
            }

            // Ensure we don't go below what we need
            if (cellsToAdd < 0)
            {
                // We added too much, adjust
                if (currentWidth > currentHeight)
                    currentWidth--;
                else
                    currentHeight--;
            }
        }

        private void RecalculateGridSize()
        {
            // Reset to base size
            currentWidth = baseWidth;
            currentHeight = baseHeight;

            // Re-apply expansions from placed ingredients
            foreach (var placed in placedIngredients)
            {
                if (placed.ingredient.UnlocksAdditionalSpace)
                {
                    ExpandGrid(placed.ingredient.AdditionalSpaceCount);
                }
            }
        }

        public void Clear()
        {
            placedIngredients.Clear();
            occupiedCells.Clear();
            currentWidth = baseWidth;
            currentHeight = baseHeight;
        }

        public float CalculateGridEfficiency()
        {
            if (placedIngredients.Count == 0) return 0f;

            int totalBaseCells = baseWidth * baseHeight;
            int occupiedBaseCells = occupiedCells.Count(pos => 
                pos.x < baseWidth && pos.y < baseHeight);

            return (float)occupiedBaseCells / totalBaseCells;
        }

        public int CalculateTotalPotency()
        {
            return placedIngredients.Sum(p => p.ingredient.Potency);
        }

        public bool HasValidConfiguration()
        {
            // Must have at least 2 ingredients including 1 solvent
            if (placedIngredients.Count < 2) return false;

            bool hasSolvent = placedIngredients.Any(p => 
                p.ingredient.IngredientArchetype == Enums.IngredientArchetype.Solvent);

            return hasSolvent;
        }
    }
}