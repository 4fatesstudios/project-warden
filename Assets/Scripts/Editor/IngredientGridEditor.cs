using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.Editor
{
    [System.Serializable]
    public class IngredientGridEditor
    {
        private const int CELL_SIZE = 30;
        private const int MAX_GRID_SIZE = 8;
        
        private bool[,] ingredientGrid;
        private int gridWidth = 1;
        private int gridHeight = 1;
        private bool isInitialized = false;
        
        public void Initialize(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            gridWidth = Mathf.Clamp(ingredient.GridWidth, 1, MAX_GRID_SIZE);
            gridHeight = Mathf.Clamp(ingredient.GridHeight, 1, MAX_GRID_SIZE);
            
            if (ingredientGrid == null || ingredientGrid.GetLength(0) != gridWidth || ingredientGrid.GetLength(1) != gridHeight)
            {
                ingredientGrid = new bool[gridWidth, gridHeight];
                
                // Initialize with a simple filled rectangle by default
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        ingredientGrid[x, y] = true;
                    }
                }
            }
            
            isInitialized = true;
        }
        
        public void DrawGridEditor(Ingredient ingredient, SerializedObject serializedIngredient = null)
        {
            if (ingredient == null) return;
            
            if (!isInitialized)
            {
                Initialize(ingredient);
            }
            
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Ingredient Grid Shape", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Grid Size:", GUILayout.Width(70));
            
            int newWidth = EditorGUILayout.IntSlider("Width", gridWidth, 1, MAX_GRID_SIZE);
            int newHeight = EditorGUILayout.IntSlider("Height", gridHeight, 1, MAX_GRID_SIZE);
            
            EditorGUILayout.EndHorizontal();
            
            // Update grid size if changed
            if (newWidth != gridWidth || newHeight != gridHeight)
            {
                ResizeGrid(newWidth, newHeight);
                gridWidth = newWidth;
                gridHeight = newHeight;
                
                // Update the ingredient's grid size using SerializedProperty if available
                if (serializedIngredient != null)
                {
                    var gridWidthProp = serializedIngredient.FindProperty("gridWidth");
                    var gridHeightProp = serializedIngredient.FindProperty("gridHeight");
                    
                    if (gridWidthProp != null)
                        gridWidthProp.intValue = gridWidth;
                    if (gridHeightProp != null)
                        gridHeightProp.intValue = gridHeight;
                        
                    serializedIngredient.ApplyModifiedProperties();
                }
                else
                {
                    // Fallback: Use reflection to set private fields
                    var gridWidthField = typeof(Ingredient).GetField("gridWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var gridHeightField = typeof(Ingredient).GetField("gridHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    gridWidthField?.SetValue(ingredient, gridWidth);
                    gridHeightField?.SetValue(ingredient, gridHeight);
                    
                    EditorUtility.SetDirty(ingredient);
                }
            }
            
            EditorGUILayout.Space(10);
            
            // Draw grid visual editor
            DrawGridVisual();
            
            EditorGUILayout.Space(10);
            
            // Control buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Fill All", GUILayout.Width(80)))
            {
                FillGrid(true);
            }
            
            if (GUILayout.Button("Clear All", GUILayout.Width(80)))
            {
                FillGrid(false);
            }
            
            if (GUILayout.Button("Invert", GUILayout.Width(80)))
            {
                InvertGrid();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Instructions
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Click on grid cells to toggle ingredient shape. Green = ingredient present, Gray = empty space.", MessageType.Info);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGridVisual()
        {
            if (ingredientGrid == null) return;
            
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
                        CELL_SIZE - 1, // Leave 1px border
                        CELL_SIZE - 1
                    );
                    
                    // Determine cell color
                    Color cellColor = ingredientGrid[x, y] 
                        ? new Color(0.2f, 0.8f, 0.2f, 1f) // Green for filled
                        : new Color(0.6f, 0.6f, 0.6f, 1f); // Gray for empty
                    
                    // Highlight cell under mouse
                    if (isMouseInGrid && cellRect.Contains(mousePos))
                    {
                        cellColor = Color.Lerp(cellColor, Color.white, 0.3f);
                        
                        // Handle click
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            ingredientGrid[x, y] = !ingredientGrid[x, y];
                            Event.current.Use();
                        }
                    }
                    
                    EditorGUI.DrawRect(cellRect, cellColor);
                    
                    // Draw border using lines
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
                    
                    // Draw coordinate text for clarity
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
        
        private void ResizeGrid(int newWidth, int newHeight)
        {
            var newGrid = new bool[newWidth, newHeight];
            
            // Copy existing data where possible
            if (ingredientGrid != null)
            {
                for (int x = 0; x < Mathf.Min(gridWidth, newWidth); x++)
                {
                    for (int y = 0; y < Mathf.Min(gridHeight, newHeight); y++)
                    {
                        newGrid[x, y] = ingredientGrid[x, y];
                    }
                }
            }
            
            // Fill new areas with true by default
            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    if (x >= gridWidth || y >= gridHeight)
                    {
                        newGrid[x, y] = true;
                    }
                }
            }
            
            ingredientGrid = newGrid;
        }
        
        private void FillGrid(bool value)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    ingredientGrid[x, y] = value;
                }
            }
        }
        
        private void InvertGrid()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    ingredientGrid[x, y] = !ingredientGrid[x, y];
                }
            }
        }
        
        public bool[,] GetGridData()
        {
            return (bool[,])ingredientGrid?.Clone();
        }
        
        public void SetGridData(bool[,] data)
        {
            if (data != null)
            {
                gridWidth = data.GetLength(0);
                gridHeight = data.GetLength(1);
                ingredientGrid = (bool[,])data.Clone();
                isInitialized = true;
            }
        }
        
        public void ResetToDefault()
        {
            FillGrid(true);
        }
    }
}