using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    [System.Serializable]
    public class GridCell
    {
        public Vector2Int Position { get; private set; }
        public bool IsOccupied { get; private set; }
        public Ingredient OccupiedByIngredient { get; private set; }
        public bool IsHighlighted { get; set; }
        public bool IsValidPlacement { get; set; }
        
        // Visual state properties
        public CellVisualState VisualState { get; private set; }
        public Color CellColor { get; private set; }
        public Aspect? CellAspect { get; private set; }
        public float CellIntensity { get; private set; }
        
        public GridCell(int x, int y)
        {
            Position = new Vector2Int(x, y);
            Clear();
        }
        
        public void SetOccupied(Ingredient ingredient)
        {
            IsOccupied = true;
            OccupiedByIngredient = ingredient;
            CellAspect = ingredient.IngredientAspect;
            CellIntensity = ingredient.Potency / 5f; // Normalize potency to 0-1
            UpdateVisualState();
        }
        
        public void Clear()
        {
            IsOccupied = false;
            OccupiedByIngredient = null;
            IsHighlighted = false;
            IsValidPlacement = true;
            CellAspect = null;
            CellIntensity = 0f;
            VisualState = CellVisualState.Empty;
            UpdateCellColor();
        }
        
        public void SetHighlighted(bool highlighted, bool validPlacement = true)
        {
            IsHighlighted = highlighted;
            IsValidPlacement = validPlacement;
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            if (IsOccupied)
            {
                VisualState = CellVisualState.Occupied;
            }
            else if (IsHighlighted)
            {
                VisualState = IsValidPlacement ? CellVisualState.ValidHighlight : CellVisualState.InvalidHighlight;
            }
            else
            {
                VisualState = CellVisualState.Empty;
            }
            
            UpdateCellColor();
        }
        
        private void UpdateCellColor()
        {
            switch (VisualState)
            {
                case CellVisualState.Empty:
                    CellColor = Color.white;
                    break;
                    
                case CellVisualState.ValidHighlight:
                    CellColor = Color.green;
                    break;
                    
                case CellVisualState.InvalidHighlight:
                    CellColor = Color.red;
                    break;
                    
                case CellVisualState.Occupied:
                    CellColor = GetAspectColor();
                    break;
            }
        }
        
        private Color GetAspectColor()
        {
            if (!CellAspect.HasValue)
                return Color.gray;
                
            // Color mapping for different aspects
            switch (CellAspect.Value)
            {
                case Aspect.Scorch:
                    return Color.Lerp(Color.red, Color.yellow, 0.3f);
                    
                case Aspect.Frigid:
                    return Color.Lerp(Color.blue, Color.cyan, 0.5f);
                    
                case Aspect.Corporeal:
                    return Color.gray;
                    
                case Aspect.Divine:
                    return Color.Lerp(Color.black, Color.magenta, 0.3f);
                    
                case Aspect.Arc:
                    return Color.Lerp(Color.blue, Color.white, 0.5f);
                    
                case Aspect.Caustic:
                    return Color.Lerp(Color.green, Color.magenta, 0.4f);
                    
                default:
                    return Color.gray;
            }
        }
        
        public float GetTemperature()
        {
            if (!IsOccupied || !CellAspect.HasValue)
                return 0f;
                
            switch (CellAspect.Value)
            {
                case Aspect.Scorch:
                    return CellIntensity;
                case Aspect.Frigid:
                    return -CellIntensity;
                default:
                    return 0f;
            }
        }
        
        public bool HasElementalAspect()
        {
            if (!CellAspect.HasValue)
                return false;
                
            return CellAspect.Value == Aspect.Scorch ||
                   CellAspect.Value == Aspect.Frigid ||
                   CellAspect.Value == Aspect.Arc ||
                   CellAspect.Value == Aspect.Caustic;
        }
    }
    
    public enum CellVisualState
    {
        Empty,
        ValidHighlight,
        InvalidHighlight,
        Occupied
    }
}