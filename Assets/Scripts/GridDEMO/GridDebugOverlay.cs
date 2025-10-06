// Fixed GridDebugOverlay - Runtime debug overlay for grid state visualization
using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.GridDEMO
{
    /// <summary>
    /// Runtime debug overlay for grid state visualization
    /// </summary>
    public class GridDebugOverlay : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool showDebugOverlay = true;
        public bool showCellCoordinates = true;
        public bool showAspectInfo = true;
        public KeyCode toggleKey = KeyCode.F1;
    
        [Header("Visual Settings")]
        public Color textColor = Color.white;
        public Color backgroundColor = new Color(0, 0, 0, 0.7f);
        public int fontSize = 12;

        private GridGameManager gridManager;
        private GUIStyle labelStyle;
        private GUIStyle backgroundStyle;

        void Start()
        {
            gridManager = FindFirstObjectByType<GridGameManager>();
        
            // Setup GUI styles
            labelStyle = new GUIStyle();
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = textColor;
            labelStyle.alignment = TextAnchor.UpperLeft;
        
            backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = MakeTexture(2, 2, backgroundColor);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                showDebugOverlay = !showDebugOverlay;
                Debug.Log($"Grid Debug Overlay: {(showDebugOverlay ? "ON" : "OFF")}");
            }
        }

        void OnGUI()
        {
            if (!showDebugOverlay || gridManager == null)
                return;

            // Background
            GUI.Box(new Rect(10, 10, 300, 200), "", backgroundStyle);

            // Grid info
            GUILayout.BeginArea(new Rect(20, 20, 280, 180));
        
            // Create a bold style
            var boldStyle = new GUIStyle(labelStyle);
            boldStyle.fontStyle = FontStyle.Bold;
            
            GUILayout.Label("GRID DEBUG INFO", boldStyle);
            GUILayout.Space(5);
        
            GUILayout.Label($"Grid: {gridManager.gridWidth}x{gridManager.gridHeight}", labelStyle);
            GUILayout.Label($"Cell Size: {gridManager.cellSize:F2}", labelStyle);
            GUILayout.Label($"Selected: {gridManager.CurrentSelectedIngredient?.ItemName ?? "None"}", labelStyle);
            GUILayout.Label($"Available Ingredients: {gridManager.availableIngredients.Count}", labelStyle);
        
            GUILayout.Space(10);
        
            // Statistics
            if (Application.isPlaying)
            {
                int occupied = 0;
                int highlighted = 0;
            
                for (int x = 0; x < gridManager.gridWidth; x++)
                {
                    for (int y = 0; y < gridManager.gridHeight; y++)
                    {
                        var cell = gridManager.GetCell(x, y);
                        if (cell != null)
                        {
                            if (cell.IsOccupied) occupied++;
                            if (cell.IsHighlighted) highlighted++;
                        }
                    }
                }
            
                int total = gridManager.gridWidth * gridManager.gridHeight;
                float occupancyPercent = (occupied * 100f) / total;
            
                GUILayout.Label($"Occupancy: {occupied}/{total} ({occupancyPercent:F1}%)", labelStyle);
                GUILayout.Label($"Highlighted: {highlighted}", labelStyle);
            }
        
            GUILayout.Space(10);
            GUILayout.Label($"Toggle: {toggleKey}", labelStyle);
        
            GUILayout.EndArea();

            // Mouse hover info
            if (showCellCoordinates)
            {
                ShowMouseHoverInfo();
            }
        }

        private void ShowMouseHoverInfo()
        {
            // Get the main camera
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null) return;
            
            // Convert mouse position to world position
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.y = 0; // Project to grid plane
        
            // Convert to grid coordinates
            Vector2Int gridPos = gridManager.WorldToGridPosition(mouseWorldPos);
        
            // Check if position is valid
            if (gridPos.x >= 0 && gridPos.x < gridManager.gridWidth && 
                gridPos.y >= 0 && gridPos.y < gridManager.gridHeight)
            {
                var cell = gridManager.GetCell(gridPos.x, gridPos.y);
                if (cell != null)
                {
                    // Position info near mouse
                    Vector2 mouseScreenPos = Input.mousePosition;
                    mouseScreenPos.y = Screen.height - mouseScreenPos.y; // Flip Y for GUI
                
                    // Background for cell info
                    Rect infoRect = new Rect(mouseScreenPos.x + 10, mouseScreenPos.y - 60, 200, 60);
                    GUI.Box(infoRect, "", backgroundStyle);
                
                    GUILayout.BeginArea(infoRect);
                    GUILayout.Label($"Cell ({gridPos.x}, {gridPos.y})", labelStyle);
                    GUILayout.Label($"State: {cell.VisualState}", labelStyle);
                
                    if (cell.IsOccupied && showAspectInfo)
                    {
                        GUILayout.Label($"Aspect: {cell.CellAspect}", labelStyle);
                        GUILayout.Label($"Intensity: {cell.CellIntensity:F2}", labelStyle);
                    }
                    GUILayout.EndArea();
                }
            }
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = color;
        
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        [ContextMenu("Toggle Debug Overlay")]
        public void ToggleDebugOverlay()
        {
            showDebugOverlay = !showDebugOverlay;
            Debug.Log($"Grid Debug Overlay: {(showDebugOverlay ? "ON" : "OFF")}");
        }
    }
}