using FourFatesStudios.ProjectWarden.Enums;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Effects;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "Items/Ingredient")]
    public class Ingredient : Item {
        [SerializeField, Tooltip("Descriptive noun for custom potion naming.")] 
        private string noun;
        
        [SerializeField, Tooltip("Descriptive adjective for custom potion naming.")]
        private string adjective;
        
        [SerializeField, Tooltip("Alchemical ingredient type.")]
        private IngredientArchetype ingredientArchetype;

        [SerializeField, Tooltip("Alchemical ingredient aspect.")]
        private Aspect ingredientAspect;

        [SerializeField, Tooltip("Is alchemical ingredient corrupted or not.")]
        private bool isCorrupted;

        [Header("Crafting Properties")]
        [SerializeField, Tooltip("Potency level for alchemy crafting (1-5, higher = stronger effects).")]
        [Range(1, 5)]
        private int potency = 1;

        [SerializeField, Tooltip("Grid width for tetris-style crafting.")]
        [Range(1, 4)]
        private int gridWidth = 1;

        [SerializeField, Tooltip("Grid height for tetris-style crafting.")]
        [Range(1, 4)]
        private int gridHeight = 1;

        [SerializeField, Tooltip("Can this ingredient unlock additional grid space when placed?")]
        private bool unlocksAdditionalSpace = false;

        [SerializeField, Tooltip("Additional grid spaces unlocked (if applicable).")]
        [Range(0, 8)]
        private int additionalSpaceCount = 0;

        [Header("Visual Shape Design")]
        [SerializeField, Tooltip("Serialized shape data for the ingredient's visual grid design.")]
        private IngredientShapeData shapeData = new IngredientShapeData();

        [Header("Ingredient Effect")] 
        [SerializeField, Tooltip("The infusion(s) given from the ingredient")] 
        private List<Infusion> infusions;

        [Header("Refinement Configuration")] 
        [SerializeField, Tooltip("Can this ingredient be ground (for Ore types)?")]
        private bool canGrind;
        [SerializeField, Tooltip("Result of grinding (optional - can be auto-generated)")]
        private Ingredient grindingResult;
        
        [SerializeField, Tooltip("Can this ingredient be distilled (for Herb/Organic/Solvent types)?")]
        private bool canDistill;
        [SerializeField, Tooltip("Result of distilling (optional - can be auto-generated)")]
        private Ingredient distillingResult;
        
        [SerializeField, Tooltip("Can this ingredient be roasted (for Herb/Organic types)?")]
        private bool canRoast;
        [SerializeField, Tooltip("Result of roasting (optional - can be auto-generated)")]
        private Ingredient roastingResult;
        
        [Header("Refining Properties")]
        [SerializeField, Tooltip("Base success rate for refining this ingredient (0.0-1.0)")]
        [Range(0f, 1f)]
        private float baseRefiningSuccessRate = 0.7f;
        
        [SerializeField, Tooltip("How resistant this ingredient is to being lost during failed refining")]
        [Range(0f, 1f)]
        private float stabilityRating = 0.8f;
        
        [SerializeField, Tooltip("Minimum skill level required to attempt refining this ingredient")]
        [Range(1, 100)]
        private int minimumRefiningSkill = 1;
        

        public string Noun => noun;
        public string Adjective => adjective;
        public IngredientArchetype IngredientArchetype => ingredientArchetype;
        public Aspect IngredientAspect => ingredientAspect;
        public bool IsCorrupted => isCorrupted;
        public virtual int Potency => potency;
        public virtual int GridWidth => gridWidth;
        public virtual int GridHeight => gridHeight;
        public virtual bool UnlocksAdditionalSpace => unlocksAdditionalSpace;
        public virtual int AdditionalSpaceCount => additionalSpaceCount;
        public List<Infusion> Infusions => infusions;
        public bool CanGrind => canGrind;
        public Ingredient GrindingResult => grindingResult;
        public bool CanDistill => canDistill;
        public Ingredient DistillingResult => distillingResult;
        public bool CanRoast => canRoast;
        public Ingredient RoastingResult => roastingResult;
        public float BaseRefiningSuccessRate => baseRefiningSuccessRate;
        public float StabilityRating => stabilityRating;
        public int MinimumRefiningSkill => minimumRefiningSkill;
        public bool CanBeRefined => canGrind || canDistill || canRoast;

        // Shape Data Properties
        public IngredientShapeData ShapeData => shapeData;
        
        /// <summary>
        /// Get the ingredient's shape as a bool array for grid operations
        /// </summary>
        public bool[,] GetShape()
        {
            if (shapeData == null)
            {
                shapeData = new IngredientShapeData(gridWidth, gridHeight);
            }
            return shapeData.ToBoolArray();
        }
        
        /// <summary>
        /// Set the ingredient's shape from a bool array (used by grid editor)
        /// </summary>
        public void SetShape(bool[,] shape)
        {
            if (shapeData == null)
            {
                shapeData = new IngredientShapeData();
            }
            shapeData.SetFromBoolArray(shape);
            
            // Update grid dimensions to match shape
            gridWidth = shapeData.GridWidth;
            gridHeight = shapeData.GridHeight;
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// Apply a shape template to this ingredient
        /// </summary>
        public void ApplyShapeTemplate(ShapeTemplate template)
        {
            if (shapeData == null)
            {
                shapeData = new IngredientShapeData(gridWidth, gridHeight, template);
            }
            else
            {
                shapeData.ApplyTemplate(template);
            }
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// Check if the ingredient occupies a specific grid cell
        /// </summary>
        public bool OccupiesCell(int x, int y)
        {
            if (shapeData == null) return x == 0 && y == 0; // Default to single cell
            return shapeData.IsCellActive(x, y);
        }
        
        /// <summary>
        /// Export shape data for external tools (ASE, etc.)
        /// </summary>
        public string ExportShapeData()
        {
            if (shapeData == null)
            {
                shapeData = new IngredientShapeData(gridWidth, gridHeight);
            }
            return shapeData.ExportAsString();
        }
        
        /// <summary>
        /// Import shape data from external tools
        /// </summary>
        public bool ImportShapeData(string data)
        {
            if (shapeData == null)
            {
                shapeData = new IngredientShapeData();
            }
            
            bool success = shapeData.ImportFromString(data);
            if (success)
            {
                gridWidth = shapeData.GridWidth;
                gridHeight = shapeData.GridHeight;
                
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            return success;
        }

#if UNITY_EDITOR
        private new void OnValidate()
        {
            base.OnValidate();
            
            // Validate infusions are properly initialized
            if (infusions == null)
            {
                infusions = new List<Infusion>();
            }
            
            // Validate and initialize shape data
            if (shapeData == null)
            {
                shapeData = new IngredientShapeData(gridWidth, gridHeight);
            }
            else
            {
                // Ensure shape data matches current grid dimensions
                if (shapeData.GridWidth != gridWidth || shapeData.GridHeight != gridHeight)
                {
                    shapeData.ResizeGrid(gridWidth, gridHeight);
                }
                
                // Validate shape data consistency
                shapeData.ValidateAndFix();
            }

            if (!canGrind) grindingResult = null;
            if (!canDistill) distillingResult = null;
            if (!canRoast) roastingResult = null;
            
            // Validate refining methods match ingredient archetype
            if (canGrind && ingredientArchetype != IngredientArchetype.Ore)
            {
                Debug.LogWarning($"[{name}] Grinding is only valid for Ore ingredients, but this is {ingredientArchetype}", this);
            }
            
            if (canDistill && !(ingredientArchetype == IngredientArchetype.Herb || 
                               ingredientArchetype == IngredientArchetype.Organic || 
                               ingredientArchetype == IngredientArchetype.Solvent))
            {
                Debug.LogWarning($"[{name}] Distilling is only valid for Herb, Organic, or Solvent ingredients, but this is {ingredientArchetype}", this);
            }
            
            if (canRoast && !(ingredientArchetype == IngredientArchetype.Herb || 
                             ingredientArchetype == IngredientArchetype.Organic))
            {
                Debug.LogWarning($"[{name}] Roasting is only valid for Herb or Organic ingredients, but this is {ingredientArchetype}", this);
            }
            
            // Warn about refined effects
            if ((canGrind || canDistill || canRoast) && 
                grindingResult == null && distillingResult == null && roastingResult == null)
            {
                Debug.LogWarning($"[{name}] Can be refined but has no refined results assigned. Consider creating refined versions or the ingredient will be lost.", this);
            }
            
            // Validate skill requirements
            if (minimumRefiningSkill > potency * 20)
            {
                Debug.LogWarning($"[{name}] Minimum refining skill ({minimumRefiningSkill}) seems very high for potency {potency}. Consider lowering it.", this);
            }
        }
#endif
        public override string ToString()
        {
            return $"{ItemName} ({IngredientArchetype}) - {ItemDescription}";
        }
    }
}