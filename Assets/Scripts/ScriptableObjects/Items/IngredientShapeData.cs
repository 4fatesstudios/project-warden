using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [System.Serializable]
    public class IngredientShapeData
    {
        [Header("Grid Dimensions")]
        [SerializeField, Range(1, 8)]
        private int gridWidth = 1;
        
        [SerializeField, Range(1, 8)]
        private int gridHeight = 1;
        
        [Header("Shape Configuration")]
        [SerializeField, Tooltip("Serialized grid data as a list of active cell positions")]
        private List<Vector2Int> activeCells = new List<Vector2Int>();
        
        [SerializeField, Tooltip("Shape template used (for reference and consistency)")]
        private ShapeTemplate template = ShapeTemplate.Rectangle;
        
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
        public ShapeTemplate Template => template;
        public Color ShapeColor => shapeColor;
        public Sprite ShapeSprite => shapeSprite;
        public string LastModified => lastModified;
        public int DataVersion => dataVersion;

        public IngredientShapeData()
        {
            Initialize();
        }

        public IngredientShapeData(int width, int height, ShapeTemplate shapeTemplate = ShapeTemplate.Rectangle)
        {
            gridWidth = Mathf.Clamp(width, 1, 8);
            gridHeight = Mathf.Clamp(height, 1, 8);
            template = shapeTemplate;
            Initialize();
            ApplyTemplate();
        }

        private void Initialize()
        {
            if (activeCells == null)
                activeCells = new List<Vector2Int>();
                
            lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
            var result = new bool[gridWidth, gridHeight];
            
            foreach (var cell in activeCells)
            {
                if (cell.x >= 0 && cell.x < gridWidth && 
                    cell.y >= 0 && cell.y < gridHeight)
                {
                    result[cell.x, cell.y] = true;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Apply a predefined shape template
        /// </summary>
        public void ApplyTemplate()
        {
            ApplyTemplate(template);
        }

        /// <summary>
        /// Apply a specific shape template
        /// </summary>
        public void ApplyTemplate(ShapeTemplate shapeTemplate)
        {
            template = shapeTemplate;
            activeCells.Clear();
            
            switch (template)
            {
                case ShapeTemplate.Rectangle:
                    ApplyRectangleTemplate();
                    break;
                case ShapeTemplate.Cross:
                    ApplyCrossTemplate();
                    break;
                case ShapeTemplate.LShape:
                    ApplyLShapeTemplate();
                    break;
                case ShapeTemplate.TShape:
                    ApplyTShapeTemplate();
                    break;
                case ShapeTemplate.ZShape:
                    ApplyZShapeTemplate();
                    break;
                case ShapeTemplate.Circle:
                    ApplyCircleTemplate();
                    break;
                case ShapeTemplate.Diamond:
                    ApplyDiamondTemplate();
                    break;
                case ShapeTemplate.Custom:
                    // Keep existing shape for custom
                    break;
            }
            
            UpdateTimestamp();
        }

        private void ApplyRectangleTemplate()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    activeCells.Add(new Vector2Int(x, y));
                }
            }
        }

        private void ApplyCrossTemplate()
        {
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            
            // Horizontal line
            for (int x = 0; x < gridWidth; x++)
            {
                activeCells.Add(new Vector2Int(x, centerY));
            }
            
            // Vertical line
            for (int y = 0; y < gridHeight; y++)
            {
                activeCells.Add(new Vector2Int(centerX, y));
            }
        }

        private void ApplyLShapeTemplate()
        {
            // Bottom row
            for (int x = 0; x < gridWidth; x++)
            {
                activeCells.Add(new Vector2Int(x, 0));
            }
            
            // Left column
            for (int y = 0; y < gridHeight; y++)
            {
                activeCells.Add(new Vector2Int(0, y));
            }
        }

        private void ApplyTShapeTemplate()
        {
            // Top row
            for (int x = 0; x < gridWidth; x++)
            {
                activeCells.Add(new Vector2Int(x, gridHeight - 1));
            }
            
            // Center column
            int centerX = gridWidth / 2;
            for (int y = 0; y < gridHeight; y++)
            {
                activeCells.Add(new Vector2Int(centerX, y));
            }
        }

        private void ApplyZShapeTemplate()
        {
            if (gridWidth < 3 || gridHeight < 3) return;
            
            // Top row
            for (int x = 0; x < gridWidth; x++)
            {
                activeCells.Add(new Vector2Int(x, gridHeight - 1));
            }
            
            // Diagonal
            int steps = Mathf.Min(gridWidth, gridHeight);
            for (int i = 0; i < steps; i++)
            {
                int x = gridWidth - 1 - i;
                int y = gridHeight - 1 - i;
                if (x >= 0 && y >= 0)
                {
                    activeCells.Add(new Vector2Int(x, y));
                }
            }
            
            // Bottom row
            for (int x = 0; x < gridWidth; x++)
            {
                activeCells.Add(new Vector2Int(x, 0));
            }
        }

        private void ApplyCircleTemplate()
        {
            float centerX = (gridWidth - 1) / 2f;
            float centerY = (gridHeight - 1) / 2f;
            float radius = Mathf.Min(centerX, centerY);
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (distance <= radius)
                    {
                        activeCells.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private void ApplyDiamondTemplate()
        {
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;
            
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    int manhattanDistance = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
                    if (manhattanDistance <= Mathf.Min(centerX, centerY))
                    {
                        activeCells.Add(new Vector2Int(x, y));
                    }
                }
            }
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
            ApplyRectangleTemplate();
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
            template = ShapeTemplate.Custom;
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
                template = template.ToString(),
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
                
                if (Enum.TryParse<ShapeTemplate>(importData.template, out var parsedTemplate))
                {
                    template = parsedTemplate;
                }
                
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
        /// Auto-fix any invalid data
        /// </summary>
        public void ValidateAndFix()
        {
            gridWidth = Mathf.Clamp(gridWidth, 1, 8);
            gridHeight = Mathf.Clamp(gridHeight, 1, 8);
            
            // Remove invalid cells
            activeCells.RemoveAll(cell => 
                cell.x < 0 || cell.x >= gridWidth || 
                cell.y < 0 || cell.y >= gridHeight);
            
            // If no cells are active, create a default 1x1 shape
            if (activeCells.Count == 0)
            {
                activeCells.Add(new Vector2Int(0, 0));
            }
            
            UpdateTimestamp();
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

    public enum ShapeTemplate
    {
        Rectangle,
        Cross,
        LShape,
        TShape,
        ZShape,
        Circle,
        Diamond,
        Custom
    }
}