using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.GridDemo;
using System.Text;

/// <summary>
/// Working Grid Debug Window without namespace conflicts
/// </summary>
public class GridDebugWindowWorking : EditorWindow
{
    [MenuItem("Tools/Grid Debug Working")]
    public static void ShowWindow()
    {
        GetWindow<GridDebugWindowWorking>("Grid Debug");
    }

    private Vector2 scrollPosition;
    private bool autoRefresh = true;
    private int selectedCellX = 0;
    private int selectedCellY = 0;

    private void OnGUI()
    {
        GUILayout.Label("Grid Debug Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Auto-refresh toggle
        autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
        
        EditorGUILayout.Space();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Basic Info Section
        DrawBasicInfoSection();
        
        EditorGUILayout.Space();
        
        // Cell Inspector Section
        DrawCellInspectorSection();
        
        EditorGUILayout.Space();
        
        // Debug Actions Section
        DrawDebugActionsSection();
        
        EditorGUILayout.EndScrollView();
        
        // Auto-refresh
        if (autoRefresh && Application.isPlaying)
        {
            Repaint();
        }
    }

    private void DrawBasicInfoSection()
    {
        EditorGUILayout.LabelField("Grid Information", EditorStyles.boldLabel);
        
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null)
        {
            EditorGUILayout.HelpBox("❌ GridGameManager not found in scene!", MessageType.Error);
            return;
        }

        EditorGUILayout.BeginVertical("box");
        
        // Basic grid info
        EditorGUILayout.LabelField("Grid Dimensions", $"{gridManager.gridWidth} x {gridManager.gridHeight}");
        EditorGUILayout.LabelField("Cell Size", gridManager.cellSize.ToString("F2"));
        EditorGUILayout.LabelField("Grid Start Position", gridManager.gridStartPosition.ToString());
        
        // Current selection
        EditorGUILayout.LabelField("Selected Ingredient", gridManager.CurrentSelectedIngredient?.ItemName ?? "None");
        
        // Available ingredients count
        EditorGUILayout.LabelField("Available Ingredients", gridManager.availableIngredients.Count.ToString());
        
        EditorGUILayout.EndVertical();
    }

    private void DrawCellInspectorSection()
    {
        EditorGUILayout.LabelField("Cell Inspector", EditorStyles.boldLabel);
        
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null || !Application.isPlaying) 
        {
            EditorGUILayout.HelpBox("Grid manager not found or not in play mode", MessageType.Info);
            return;
        }
        
        EditorGUILayout.BeginVertical("box");
        
        // Cell selector
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Inspect Cell:", GUILayout.Width(80));
        selectedCellX = EditorGUILayout.IntSlider("X", selectedCellX, 0, gridManager.gridWidth - 1);
        selectedCellY = EditorGUILayout.IntSlider("Y", selectedCellY, 0, gridManager.gridHeight - 1);
        EditorGUILayout.EndHorizontal();
        
        // Get cell data using the public method
        var cell = gridManager.GetCell(selectedCellX, selectedCellY);
        if (cell != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Cell ({selectedCellX}, {selectedCellY}) Details:", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Position", cell.Position.ToString());
            EditorGUILayout.LabelField("Is Occupied", cell.IsOccupied.ToString());
            EditorGUILayout.LabelField("Is Highlighted", cell.IsHighlighted.ToString());
            EditorGUILayout.LabelField("Is Valid Placement", cell.IsValidPlacement.ToString());
            EditorGUILayout.LabelField("Visual State", cell.VisualState.ToString());
            EditorGUILayout.LabelField("Cell Color", cell.CellColor.ToString());
            
            if (cell.IsOccupied)
            {
                EditorGUILayout.LabelField("Occupied By", cell.OccupiedByIngredient?.ItemName ?? "NULL");
                EditorGUILayout.LabelField("Cell Aspect", cell.CellAspect?.ToString() ?? "None");
                EditorGUILayout.LabelField("Cell Intensity", cell.CellIntensity.ToString("F2"));
                EditorGUILayout.LabelField("Temperature", cell.GetTemperature().ToString("F2"));
                EditorGUILayout.LabelField("Has Elemental Aspect", cell.HasElementalAspect().ToString());
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawDebugActionsSection()
    {
        EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔍 Find Grid Manager"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                Selection.activeGameObject = gridManager.gameObject;
                EditorGUIUtility.PingObject(gridManager.gameObject);
                Debug.Log($"✅ Found GridGameManager on {gridManager.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("❌ GridGameManager not found in scene!");
            }
        }
        
        if (GUILayout.Button("🔄 Refresh Grid"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                var visualizer = gridManager.GetComponent<GridVisualizer>();
                if (visualizer != null)
                {
                    visualizer.RefreshGrid();
                    Debug.Log("🔄 Grid refreshed");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🧹 Clear Grid"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                gridManager.ClearGrid();
                Debug.Log("🧹 Grid cleared");
            }
        }
        
        if (GUILayout.Button("📋 Log Grid State"))
        {
            LogCompleteGridState();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🎲 Test Random Placement"))
        {
            TestRandomPlacement();
        }
        
        if (GUILayout.Button("🔧 Force Grid Rebuild"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                // Use reflection to call InitializeComponents
                var method = typeof(GridGameManager).GetMethod("InitializeComponents", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(gridManager, null);
                    Debug.Log("🔧 Grid forcefully rebuilt");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // New verification section
        EditorGUILayout.Space();
        GUILayout.Label("🔍 Placement Verification", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔍 Verify All Placements"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                gridManager.VerifyAllPlacedIngredients();
            }
        }
        
        if (GUILayout.Button("📊 Full Occupancy Audit"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                gridManager.FullOccupancyAudit();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Continuous verification controls
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔄 Toggle Auto-Verify"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                gridManager.ToggleContinuousVerification();
            }
        }
        
        if (GUILayout.Button("⚡ Frame-by-Frame"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                gridManager.EnableFrameByFrameVerification();
            }
        }
        
        if (GUILayout.Button("🔄 Reset Normal"))
        {
            var gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager != null)
            {
                gridManager.ResetNormalVerification();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This debug window provides grid state monitoring and debugging tools. " +
                                "Enable 'Auto Refresh' to see live updates during play mode.\n\n" +
                                "Use the context menu on GridGameManager in the Inspector for additional debugging options.", 
                                MessageType.Info);
    }

    private void LogCompleteGridState()
    {
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null)
        {
            Debug.LogError("❌ GridGameManager not found!");
            return;
        }

        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ Grid state logging only works in play mode");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("📋 === COMPLETE GRID STATE DEBUG ===");
        sb.AppendLine($"Grid Dimensions: {gridManager.gridWidth} x {gridManager.gridHeight}");
        sb.AppendLine($"Cell Size: {gridManager.cellSize}");
        sb.AppendLine($"Grid Start Position: {gridManager.gridStartPosition}");
        sb.AppendLine($"Selected Ingredient: {gridManager.CurrentSelectedIngredient?.ItemName ?? "None"}");
        sb.AppendLine();

        // Visual grid representation
        sb.AppendLine("Grid Layout (O = Occupied, . = Empty, H = Highlighted):");
        for (int y = gridManager.gridHeight - 1; y >= 0; y--)
        {
            sb.Append($"{y:D2} |");
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                var cell = gridManager.GetCell(x, y);
                char symbol = '.';
                if (cell != null)
                {
                    symbol = cell.IsOccupied ? 'O' : 
                             cell.IsHighlighted ? 'H' : '.';
                }
                sb.Append($" {symbol}");
            }
            sb.AppendLine();
        }
        
        sb.Append("   +");
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            sb.Append("--");
        }
        sb.AppendLine();
        
        sb.Append("    ");
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            sb.Append($"{x:D2}".Substring(1));
        }
        sb.AppendLine();
        sb.AppendLine();

        // Detailed cell information
        sb.AppendLine("Occupied Cells Details:");
        bool hasOccupiedCells = false;
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                var cell = gridManager.GetCell(x, y);
                if (cell != null && cell.IsOccupied)
                {
                    hasOccupiedCells = true;
                    sb.AppendLine($"  ({x},{y}): {cell.OccupiedByIngredient?.ItemName ?? "NULL"} " +
                                 $"[{cell.CellAspect}] Intensity: {cell.CellIntensity:F2} " +
                                 $"Temp: {cell.GetTemperature():F2}");
                }
            }
        }
        
        if (!hasOccupiedCells)
        {
            sb.AppendLine("  No cells are currently occupied.");
        }

        Debug.Log(sb.ToString());
    }

    private void TestRandomPlacement()
    {
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null || gridManager.availableIngredients.Count == 0)
        {
            Debug.LogWarning("❌ No GridGameManager or available ingredients found!");
            return;
        }

        // Pick a random ingredient
        var randomIngredient = gridManager.availableIngredients[
            UnityEngine.Random.Range(0, gridManager.availableIngredients.Count)];
        
        // Try random position
        var randomPosition = new Vector2Int(
            UnityEngine.Random.Range(0, gridManager.gridWidth - 1),
            UnityEngine.Random.Range(0, gridManager.gridHeight - 1));

        Debug.Log($"🎲 Testing random placement: {randomIngredient.ItemName} at {randomPosition}");
        
        bool success = gridManager.TryPlaceIngredient(randomIngredient, randomPosition);
        Debug.Log($"🎲 Random placement result: {(success ? "✅ Success" : "❌ Failed")}");
    }
}