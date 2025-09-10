// Simple Grid Debug Overlay without namespace conflicts
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Simple runtime debug overlay for grid state
    /// </summary>
    public class GridDebugOverlaySimple : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool showDebugOverlay = true;
        public KeyCode toggleKey = KeyCode.F1;
        
        [Header("Visual Settings")]
        public Color textColor = Color.white;
        public Color backgroundColor = new Color(0, 0, 0, 0.7f);
        public int fontSize = 12;

        private GridGameManager gridManager;
        private GUIStyle labelStyle;
        private GUIStyle backgroundStyle;
        private GUIStyle boldStyle;

        void Start()
        {
            gridManager = FindFirstObjectByType<GridGameManager>();
            
            // Setup GUI styles
            labelStyle = new GUIStyle();
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = textColor;
            labelStyle.alignment = TextAnchor.UpperLeft;
            
            boldStyle = new GUIStyle(labelStyle);
            boldStyle.fontStyle = FontStyle.Bold;
            
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
            GUI.Box(new Rect(10, 10, 300, 180), "", backgroundStyle);

            // Grid info
            GUILayout.BeginArea(new Rect(20, 20, 280, 160));
            
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
            
            // Add verification info if we have placed ingredients
            if (Application.isPlaying)
            {
                var placedIngredients = gridManager.GetComponent<IngredientPlacer>()?.GetAllPlacedIngredients();
                if (placedIngredients != null && placedIngredients.Count > 0)
                {
                    GUILayout.Space(5);
                    GUILayout.Label($"Placed Ingredients: {placedIngredients.Count}", labelStyle);
                    
                    if (GUILayout.Button("🔍 Verify Occupancy", GUILayout.Height(20)))
                    {
                        gridManager.VerifyAllPlacedIngredients();
                    }
                }
                
                // Show continuous verification status
                GUILayout.Space(5);
                var verificationField = typeof(GridGameManager).GetField("enableContinuousVerification", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var intervalField = typeof(GridGameManager).GetField("verificationInterval", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (verificationField != null && intervalField != null)
                {
                    bool isEnabled = (bool)verificationField.GetValue(gridManager);
                    float interval = (float)intervalField.GetValue(gridManager);
                    
                    string status = isEnabled ? $"✅ Auto-Verify: {interval:F1}s" : "❌ Auto-Verify: OFF";
                    GUILayout.Label(status, labelStyle);
                    
                    if (GUILayout.Button(isEnabled ? "Disable Auto-Verify" : "Enable Auto-Verify", GUILayout.Height(20)))
                    {
                        gridManager.ToggleContinuousVerification();
                    }
                }
            }
            
            GUILayout.EndArea();
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