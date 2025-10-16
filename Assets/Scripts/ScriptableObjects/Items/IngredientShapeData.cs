using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    public enum ShapeTemplate
    {
        Custom,
        SingleCell,
        Rectangle,
        HorizontalLine,
        VerticalLine,
        Square2x2,
        LShape,
        TShape,
        Cross,
        Circle,
        Diamond
    }

    [Serializable]
    [CreateAssetMenu(menuName="Alchemy/IngredientShapeData")]
    public class IngredientShapeData
    {
        public string id; // debug name
        public Sprite icon;
        public Vector2Int pivot = new Vector2Int(0,0); // anchor/origin for offsets

        // Offsets the ingredient occupies relative to the pivot (e.g. {(0,0)} for 1x1, {(0,0),(1,0)} for 2x1)
        public Vector2Int[] occupiedOffsets;

        // Offsets relative to this ingredient's pivot that represent cells this ingredient can "expand into"
        // (e.g. if an ingredient has expansionOffsets {(2,0)} then when placed, it enables placement in pivot + (2,0) if adjacency rules satisfied)
        public Vector2Int[] expansionOffsets;

        // Whether the expansion offsets are applied regardless of rotation or if they rotate with the ingredient
        public bool expansionRotatesWithIngredient = true;
        public bool rotatable = true;

        // For editor/preview only:
        public Color expansionColor = Color.yellow;
        
        [Header("Grid Dimensions")]
        [SerializeField, Range(1, 8)]
        private int gridWidth = 1;
        
        [SerializeField, Range(1, 8)]
        private int gridHeight = 1;
        
        [Header("Shape Configuration")]
        [SerializeField, Tooltip("Serialized grid data as a list of active cell positions")]
        private List<Vector2Int> activeCells = new List<Vector2Int>();
        
        [Header("Visual Properties")]
        [SerializeField, Tooltip("Color tint for the ingredient shape in the grid")]
        private Color shapeColor = Color.green;
        
        [SerializeField, Tooltip("Optional sprite overlay for the shape")]
        private Sprite shapeSprite;
        
        [Header("Metadata")]
        [SerializeField, Tooltip("Last modified timestamp for tracking changes")]
        private string lastModified;
        
        [SerializeField, Tooltip("Version number for backward compatibility")]
        private int dataVersion = 1;
        
        [SerializeField, Tooltip("Shape template type for categorization")]
        private ShapeTemplate template = ShapeTemplate.Custom;

        public int GridWidth 
        { 
            get => gridWidth; 
            set => gridWidth = Mathf.Clamp(value, 1, 8); 
        }
        
        public int GridHeight 
        { 
            get => gridHeight; 
            set => gridHeight = Mathf.Clamp(value, 1, 8); 
        }
        
        public List<Vector2Int> ActiveCells => activeCells;
        public Color ShapeColor => shapeColor;
        public Sprite ShapeSprite => shapeSprite;
        public string LastModified => lastModified;
        public int DataVersion => dataVersion;
        public ShapeTemplate Template => template;

        public IngredientShapeData()
        {
            Initialize();
        }

        public IngredientShapeData(int width, int height)
        {
            gridWidth = Mathf.Clamp(width, 1, 8);
            gridHeight = Mathf.Clamp(height, 1, 8);
            Initialize();
        }

        public IngredientShapeData(int width, int height, ShapeTemplate template)
        {
            gridWidth = Mathf.Clamp(width, 1, 8);
            gridHeight = Mathf.Clamp(height, 1, 8);
            Initialize();
            ApplyTemplate(template);
        }

        private void Initialize()
        {
            if (activeCells == null)
                activeCells = new List<Vector2Int>();
                
            lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            InitializeExpansions();
            
            // Auto-validate and fix any issues during initialization
            ValidateAndFix();
        }
        
        private void InitializeExpansions()
        {
            if (occupiedOffsets == null)
                occupiedOffsets = new Vector2Int[] { Vector2Int.zero };
                
            if (expansionOffsets == null)
                expansionOffsets = new Vector2Int[0];
        }
        
        /// <summary>
        /// Apply a predefined shape template
        /// </summary>
        public void ApplyTemplate(ShapeTemplate newTemplate)
        {
            template = newTemplate;
            activeCells.Clear();
            
            switch (newTemplate)
            {
                case ShapeTemplate.SingleCell:
                    gridWidth = gridHeight = 1;
                    activeCells.Add(new Vector2Int(0, 0));
                    break;
                    
                case ShapeTemplate.Rectangle:
                    // Use current grid dimensions for rectangle
                    for (int x = 0; x < gridWidth; x++)
                    {
                        for (int y = 0; y < gridHeight; y++)
                        {
                            activeCells.Add(new Vector2Int(x, y));
                        }
                    }
                    break;
                    
                case ShapeTemplate.HorizontalLine:
                    gridWidth = 3;
                    gridHeight = 1;
                    activeCells.Add(new Vector2Int(0, 0));
                    activeCells.Add(new Vector2Int(1, 0));
                    activeCells.Add(new Vector2Int(2, 0));
                    break;
                    
                case ShapeTemplate.VerticalLine:
                    gridWidth = 1;
                    gridHeight = 3;
                    activeCells.Add(new Vector2Int(0, 0));
                    activeCells.Add(new Vector2Int(0, 1));
                    activeCells.Add(new Vector2Int(0, 2));
                    break;
                    
                case ShapeTemplate.Square2x2:
                    gridWidth = gridHeight = 2;
                    activeCells.Add(new Vector2Int(0, 0));
                    activeCells.Add(new Vector2Int(1, 0));
                    activeCells.Add(new Vector2Int(0, 1));
                    activeCells.Add(new Vector2Int(1, 1));
                    break;
                    
                case ShapeTemplate.LShape:
                    gridWidth = gridHeight = 3;
                    activeCells.Add(new Vector2Int(0, 0));
                    activeCells.Add(new Vector2Int(0, 1));
                    activeCells.Add(new Vector2Int(0, 2));
                    activeCells.Add(new Vector2Int(1, 0));
                    break;
                    
                case ShapeTemplate.TShape:
                    gridWidth = gridHeight = 3;
                    activeCells.Add(new Vector2Int(0, 1));
                    activeCells.Add(new Vector2Int(1, 1));
                    activeCells.Add(new Vector2Int(2, 1));
                    activeCells.Add(new Vector2Int(1, 0));
                    break;
                    
                case ShapeTemplate.Cross:
                    gridWidth = gridHeight = 3;
                    activeCells.Add(new Vector2Int(1, 0));
                    activeCells.Add(new Vector2Int(0, 1));
                    activeCells.Add(new Vector2Int(1, 1));
                    activeCells.Add(new Vector2Int(2, 1));
                    activeCells.Add(new Vector2Int(1, 2));
                    break;
                    
                case ShapeTemplate.Circle:
                    gridWidth = gridHeight = 4;
                    // Simple circle approximation in 4x4 grid
                    activeCells.Add(new Vector2Int(1, 0));
                    activeCells.Add(new Vector2Int(2, 0));
                    activeCells.Add(new Vector2Int(0, 1));
                    activeCells.Add(new Vector2Int(1, 1));
                    activeCells.Add(new Vector2Int(2, 1));
                    activeCells.Add(new Vector2Int(3, 1));
                    activeCells.Add(new Vector2Int(0, 2));
                    activeCells.Add(new Vector2Int(1, 2));
                    activeCells.Add(new Vector2Int(2, 2));
                    activeCells.Add(new Vector2Int(3, 2));
                    activeCells.Add(new Vector2Int(1, 3));
                    activeCells.Add(new Vector2Int(2, 3));
                    break;
                    
                case ShapeTemplate.Diamond:
                    gridWidth = gridHeight = 4;
                    // Diamond shape in 4x4 grid
                    activeCells.Add(new Vector2Int(2, 0));
                    activeCells.Add(new Vector2Int(1, 1));
                    activeCells.Add(new Vector2Int(2, 1));
                    activeCells.Add(new Vector2Int(3, 1));
                    activeCells.Add(new Vector2Int(0, 2));
                    activeCells.Add(new Vector2Int(1, 2));
                    activeCells.Add(new Vector2Int(2, 2));
                    activeCells.Add(new Vector2Int(1, 3));
                    break;
                    
                case ShapeTemplate.Custom:
                default:
                    // Keep existing shape for custom
                    break;
            }
            
            UpdateOffsetsFromShape();
            UpdateTimestamp();
        }
        
        /// <summary>
        /// Get occupied offsets, applying rotation if needed
        /// </summary>
        public Vector2Int[] GetOccupiedOffsets(int rotation = 0)
        {
            // Auto-fix: Ensure offsets are initialized and valid
            if (occupiedOffsets == null || occupiedOffsets.Length == 0)
            {
                Debug.LogWarning("IngredientShapeData: Occupied offsets missing, regenerating from shape data");
                UpdateOffsetsFromShape();
            }
            
            // Double-check after update - if still empty, force default
            if (occupiedOffsets == null || occupiedOffsets.Length == 0)
            {
                Debug.LogWarning("IngredientShapeData: Still no occupied offsets after update, using default single cell");
                occupiedOffsets = new Vector2Int[] { Vector2Int.zero };
            }
            
            if (!rotatable || rotation == 0)
                return occupiedOffsets;
                
            return RotateOffsets(occupiedOffsets, rotation);
        }
        
        /// <summary>
        /// Get expansion offsets, applying rotation if needed
        /// </summary>
        public Vector2Int[] GetExpansionOffsets(int rotation = 0)
        {
            if (!rotatable || rotation == 0 || !expansionRotatesWithIngredient)
                return expansionOffsets;
                
            return RotateOffsets(expansionOffsets, rotation);
        }
        
        /// <summary>
        /// Rotate an array of offsets by 90-degree increments
        /// </summary>
        private Vector2Int[] RotateOffsets(Vector2Int[] offsets, int rotation)
        {
            if (offsets == null || offsets.Length == 0) return offsets;
            
            var rotated = new Vector2Int[offsets.Length];
            int rotations = ((rotation % 360) / 90) % 4;
            
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
        
        /// <summary>
        /// Set occupied and expansion offsets from current shape data
        /// </summary>
        public void UpdateOffsetsFromShape()
        {
            var occupied = new List<Vector2Int>();
            
            // Auto-fix: If no active cells, default to single cell at origin
            if (activeCells == null || activeCells.Count == 0)
            {
                Debug.LogWarning($"IngredientShapeData: No active cells found, defaulting to single cell at origin");
                activeCells = new List<Vector2Int> { Vector2Int.zero };
                
                // Ensure grid dimensions are valid
                if (gridWidth < 1) gridWidth = 1;
                if (gridHeight < 1) gridHeight = 1;
            }
            
            // Auto-fix: Validate all active cells are within grid bounds
            var validCells = new List<Vector2Int>();
            foreach (var cell in activeCells)
            {
                if (cell.x >= 0 && cell.x < gridWidth && cell.y >= 0 && cell.y < gridHeight)
                {
                    validCells.Add(cell);
                }
                else
                {
                    Debug.LogWarning($"IngredientShapeData: Cell {cell} is outside grid bounds {gridWidth}x{gridHeight}, removing");
                }
            }
            
            // If all cells were invalid, add default cell
            if (validCells.Count == 0)
            {
                validCells.Add(Vector2Int.zero);
                Debug.LogWarning($"IngredientShapeData: All cells were invalid, added default cell at origin");
            }
            
            activeCells = validCells;
            
            // Generate occupied offsets relative to pivot
            foreach (var cell in activeCells)
            {
                Vector2Int offset = cell - pivot;
                occupied.Add(offset);
            }
            
            occupiedOffsets = occupied.ToArray();
            
            // Auto-fix: Initialize expansion offsets if null
            if (expansionOffsets == null)
            {
                expansionOffsets = new Vector2Int[0];
            }
            
            UpdateTimestamp();
        }
        
        /// <summary>
        /// Set expansion offsets manually (for editor use)
        /// </summary>
        public void SetExpansionOffsets(Vector2Int[] offsets)
        {
            expansionOffsets = offsets ?? new Vector2Int[0];
            UpdateTimestamp();
        }

        /// <summary>
        /// Converts a 2D bool array (from grid editor) to persistent shape data
        /// </summary>
        public void SetFromBoolArray(bool[,] gridData)
        {
            if (gridData == null) return;
            
            gridWidth = gridData.GetLength(0);
            gridHeight = gridData.GetLength(1);
            activeCells.Clear();
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridData[x, y])
                    {
                        activeCells.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            UpdateTimestamp();
        }

        /// <summary>
        /// Converts persistent shape data to a 2D bool array (for grid editor)
        /// </summary>
        public bool[,] ToBoolArray()
        {
            // Auto-fix: Ensure shape data is properly initialized
            if (activeCells == null || activeCells.Count == 0)
            {
                Debug.LogWarning("IngredientShapeData: No active cells when converting to bool array, auto-fixing");
                UpdateOffsetsFromShape(); // This will create default cells
            }
            
            // Auto-fix: Ensure grid dimensions are valid
            if (gridWidth < 1 || gridHeight < 1)
            {
                Debug.LogWarning($"IngredientShapeData: Invalid grid dimensions {gridWidth}x{gridHeight}, setting to 1x1");
                gridWidth = gridHeight = 1;
            }
            
            var result = new bool[gridWidth, gridHeight];
            
            foreach (var cell in activeCells)
            {
                if (cell.x >= 0 && cell.x < gridWidth && 
                    cell.y >= 0 && cell.y < gridHeight)
                {
                    result[cell.x, cell.y] = true;
                }
            }
            
            // Auto-fix: If no valid cells were found, ensure at least one cell is active
            bool hasActiveCell = false;
            for (int x = 0; x < gridWidth && !hasActiveCell; x++)
            {
                for (int y = 0; y < gridHeight && !hasActiveCell; y++)
                {
                    if (result[x, y]) hasActiveCell = true;
                }
            }
            
            if (!hasActiveCell)
            {
                Debug.LogWarning("IngredientShapeData: No valid cells in bool array, setting origin cell to active");
                result[0, 0] = true;
                // Also update the active cells list to match
                if (!activeCells.Contains(Vector2Int.zero))
                {
                    activeCells.Add(Vector2Int.zero);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Check if a specific cell is active in the shape
        /// </summary>
        public bool IsCellActive(int x, int y)
        {
            return activeCells.Contains(new Vector2Int(x, y));
        }

        /// <summary>
        /// Set a cell's active state
        /// </summary>
        public void SetCellActive(int x, int y, bool active)
        {
            var cell = new Vector2Int(x, y);
            bool currentlyActive = activeCells.Contains(cell);
            
            if (active && !currentlyActive)
            {
                activeCells.Add(cell);
                UpdateTimestamp();
            }
            else if (!active && currentlyActive)
            {
                activeCells.Remove(cell);
                UpdateTimestamp();
            }
        }

        /// <summary>
        /// Clear all active cells
        /// </summary>
        public void ClearShape()
        {
            activeCells.Clear();
            UpdateTimestamp();
        }

        /// <summary>
        /// Fill all cells in the grid
        /// </summary>
        public void FillShape()
        {
            activeCells.Clear();
        }

        /// <summary>
        /// Invert the current shape
        /// </summary>
        public void InvertShape()
        {
            var newActiveCells = new List<Vector2Int>();
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!activeCells.Contains(cell))
                    {
                        newActiveCells.Add(cell);
                    }
                }
            }
            
            activeCells = newActiveCells;
            UpdateTimestamp();
        }

        /// <summary>
        /// Resize the grid, preserving existing shape where possible
        /// </summary>
        public void ResizeGrid(int newWidth, int newHeight)
        {
            newWidth = Mathf.Clamp(newWidth, 1, 8);
            newHeight = Mathf.Clamp(newHeight, 1, 8);
            
            // Remove cells that are outside the new bounds
            activeCells.RemoveAll(cell => cell.x >= newWidth || cell.y >= newHeight);
            
            gridWidth = newWidth;
            gridHeight = newHeight;
            UpdateTimestamp();
        }

        /// <summary>
        /// Update visual properties
        /// </summary>
        public void SetVisualProperties(Color color, Sprite sprite = null)
        {
            shapeColor = color;
            shapeSprite = sprite;
            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Export shape data as a compact string for external tools
        /// </summary>
        public string ExportAsString()
        {
            var data = new ShapeExportData
            {
                width = gridWidth,
                height = gridHeight,
                cells = activeCells.ToArray(),
                color = ColorUtility.ToHtmlStringRGBA(shapeColor),
                version = dataVersion
            };
            
            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// Import shape data from a string (for external tools)
        /// </summary>
        public bool ImportFromString(string data)
        {
            try
            {
                var importData = JsonUtility.FromJson<ShapeExportData>(data);
                
                gridWidth = Mathf.Clamp(importData.width, 1, 8);
                gridHeight = Mathf.Clamp(importData.height, 1, 8);
                activeCells = new List<Vector2Int>(importData.cells);
                
                if (ColorUtility.TryParseHtmlString("#" + importData.color, out var parsedColor))
                {
                    shapeColor = parsedColor;
                }
                
                UpdateTimestamp();
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to import shape data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate the shape data for consistency
        /// </summary>
        public bool IsValid()
        {
            if (gridWidth <= 0 || gridHeight <= 0) return false;
            if (gridWidth > 8 || gridHeight > 8) return false;
            
            // Check if all active cells are within bounds
            foreach (var cell in activeCells)
            {
                if (cell.x < 0 || cell.x >= gridWidth || 
                    cell.y < 0 || cell.y >= gridHeight)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Auto-fix any invalid data with comprehensive validation
        /// </summary>
        public void ValidateAndFix()
        {
            bool wasModified = false;
            
            // Fix grid dimensions
            int originalWidth = gridWidth;
            int originalHeight = gridHeight;
            gridWidth = Mathf.Clamp(gridWidth, 1, 8);
            gridHeight = Mathf.Clamp(gridHeight, 1, 8);
            
            if (gridWidth != originalWidth || gridHeight != originalHeight)
            {
                wasModified = true;
                Debug.LogWarning($"IngredientShapeData: Fixed grid dimensions from {originalWidth}x{originalHeight} to {gridWidth}x{gridHeight}");
            }
            
            // Initialize active cells if null
            if (activeCells == null)
            {
                activeCells = new List<Vector2Int>();
                wasModified = true;
            }
            
            // Remove invalid cells and track if any were removed
            int originalCellCount = activeCells.Count;
            activeCells.RemoveAll(cell => 
                cell.x < 0 || cell.x >= gridWidth || 
                cell.y < 0 || cell.y >= gridHeight);
                
            if (activeCells.Count != originalCellCount)
            {
                wasModified = true;
                Debug.LogWarning($"IngredientShapeData: Removed {originalCellCount - activeCells.Count} invalid cells");
            }
            
            // If no cells are active, create a default 1x1 shape
            if (activeCells.Count == 0)
            {
                activeCells.Add(new Vector2Int(0, 0));
                wasModified = true;
                // Only log warning in editor, not at runtime to reduce console spam
                #if UNITY_EDITOR
                Debug.LogWarning("IngredientShapeData: No valid cells found, added default cell at origin");
                #endif
            }
            
            // Validate and fix pivot position
            Vector2Int originalPivot = pivot;
            pivot = new Vector2Int(
                Mathf.Clamp(pivot.x, 0, gridWidth - 1),
                Mathf.Clamp(pivot.y, 0, gridHeight - 1)
            );
            
            if (pivot != originalPivot)
            {
                wasModified = true;
                Debug.LogWarning($"IngredientShapeData: Fixed pivot from {originalPivot} to {pivot}");
            }
            
            // Auto-fix: Regenerate offsets if they're missing or invalid
            if (occupiedOffsets == null || occupiedOffsets.Length == 0 || occupiedOffsets.Length != activeCells.Count)
            {
                UpdateOffsetsFromShape();
                wasModified = true;
                Debug.LogWarning("IngredientShapeData: Regenerated occupied offsets");
            }
            
            // Initialize expansion offsets if null
            if (expansionOffsets == null)
            {
                expansionOffsets = new Vector2Int[0];
                wasModified = true;
            }
            
            if (wasModified)
            {
                UpdateTimestamp();
                // Only log in editor, not at runtime
                #if UNITY_EDITOR
                Debug.Log("IngredientShapeData: Auto-fixed shape data inconsistencies");
                #endif
            }
        }
    }

    [System.Serializable]
    public class ShapeExportData
    {
        public int width;
        public int height;
        public Vector2Int[] cells;
        public string template;
        public string color;
        public int version;
    }
}