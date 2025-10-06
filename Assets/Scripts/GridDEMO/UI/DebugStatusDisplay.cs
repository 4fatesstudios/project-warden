using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// UI component to display debug system status and obstacle spawn statistics.
    /// Shows real-time debug information in the game view.
    /// </summary>
    public class DebugStatusDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI debugStatusText;
        [SerializeField] private TextMeshProUGUI obstacleStatsText;
        [SerializeField] private Toggle showDebugToggle;
        [SerializeField] private Button refreshStatsButton;
        
        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 1.0f;
        [SerializeField] private bool autoUpdate = true;
        
        private GridGameManager gridManager;
        // private ObstacleSpawnDebugger obstacleDebugger; // TODO: Implement ObstacleSpawnDebugger class
        private float lastUpdateTime;
        
        private void Start()
        {
            InitializeReferences();
            SetupUI();
        }
        
        private void InitializeReferences()
        {
            // Find GridGameManager
            gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogWarning("DebugStatusDisplay: No GridGameManager found in scene");
                return;
            }
            
            // Find ObstacleSpawnDebugger
            // obstacleDebugger = gridManager.GetComponent<ObstacleSpawnDebugger>();
            // if (obstacleDebugger == null)
            // {
            //     Debug.LogWarning("DebugStatusDisplay: No ObstacleSpawnDebugger found on GridGameManager");
            // }
        }
        
        private void SetupUI()
        {
            // Setup toggle
            if (showDebugToggle != null)
            {
                showDebugToggle.onValueChanged.AddListener(OnToggleDebugDisplay);
                showDebugToggle.isOn = true; // Start visible
            }
            
            // Setup refresh button
            if (refreshStatsButton != null)
            {
                refreshStatsButton.onClick.AddListener(RefreshDisplayImmediately);
            }
            
            // Create default UI elements if not assigned
            CreateDefaultUIIfNeeded();
            
            // Initial update
            RefreshDisplayImmediately();
        }
        
        private void CreateDefaultUIIfNeeded()
        {
            if (debugStatusText == null)
            {
                // Create a simple text component
                GameObject textObject = new GameObject("DebugStatusText");
                textObject.transform.SetParent(transform);
                
                debugStatusText = textObject.AddComponent<TextMeshProUGUI>();
                debugStatusText.text = "Debug Status";
                debugStatusText.fontSize = 14;
                debugStatusText.color = Color.white;
                
                // Position in top-left corner
                RectTransform rectTransform = debugStatusText.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchoredPosition = new Vector2(10, -10);
                rectTransform.sizeDelta = new Vector2(300, 100);
            }
            
            if (obstacleStatsText == null)
            {
                // Create obstacle stats text
                GameObject statsObject = new GameObject("ObstacleStatsText");
                statsObject.transform.SetParent(transform);
                
                obstacleStatsText = statsObject.AddComponent<TextMeshProUGUI>();
                obstacleStatsText.text = "Obstacle Stats";
                obstacleStatsText.fontSize = 12;
                obstacleStatsText.color = Color.yellow;
                
                // Position below debug status
                RectTransform rectTransform = obstacleStatsText.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchoredPosition = new Vector2(10, -120);
                rectTransform.sizeDelta = new Vector2(400, 80);
            }
        }
        
        private void Update()
        {
            if (autoUpdate && Time.time - lastUpdateTime >= updateInterval)
            {
                RefreshDisplay();
                lastUpdateTime = Time.time;
            }
        }
        
        private void OnToggleDebugDisplay(bool isVisible)
        {
            // Show/hide all debug UI elements
            if (debugStatusText != null)
                debugStatusText.gameObject.SetActive(isVisible);
            
            if (obstacleStatsText != null)
                obstacleStatsText.gameObject.SetActive(isVisible);
                
            if (refreshStatsButton != null)
                refreshStatsButton.gameObject.SetActive(isVisible);
        }
        
        private void RefreshDisplayImmediately()
        {
            RefreshDisplay();
            lastUpdateTime = Time.time;
        }
        
        private void RefreshDisplay()
        {
            UpdateDebugStatusText();
            UpdateObstacleStatsText();
        }
        
        private void UpdateDebugStatusText()
        {
            if (debugStatusText == null) return;
            
            string statusText = "🔧 DEBUG SYSTEM STATUS\n";
            statusText += $"Obstacle Spawn: {(DebugSystemConfig.ObstacleSpawnDebug ? "ON" : "OFF")}\n";
            statusText += $"Placement: {(DebugSystemConfig.IngredientPlacementDebug ? "ON" : "OFF")}\n";
            statusText += $"Collision: {(DebugSystemConfig.CollisionDetectionDebug ? "ON" : "OFF")}\n";
            statusText += $"Particles: {(DebugSystemConfig.ParticleEffectsDebug ? "ON" : "OFF")}\n";
            statusText += $"Proficiency: {(DebugSystemConfig.ProficiencyGradingDebug ? "ON" : "OFF")}";
            
            debugStatusText.text = statusText;
        }
        
        private void UpdateObstacleStatsText()
        {
            if (obstacleStatsText == null || gridManager == null) return;
            
            string statsText = "🚧 OBSTACLE STATISTICS\n";
            
            // Basic obstacle info
            statsText += $"Spawn Chance: {gridManager.obstacleSpawnChance:P1}\n";
            statsText += $"Current Obstacles: {gridManager.aspectObstacles.Count}\n";
            
            // Spawn debugger stats
            // if (obstacleDebugger != null)
            // {
            //     string summary = obstacleDebugger.GetSpawnStatsSummary();
            //     statsText += $"Spawn Stats: {summary}\n";
            // }
            // else
            // {
            statsText += "Spawn Debugger: Not Available\n";
            // }
            
            // Grid info
            int totalCells = gridManager.gridWidth * gridManager.gridHeight;
            int maxPossibleObstacles = totalCells - 9; // Subtract center area
            float expectedObstacles = maxPossibleObstacles * gridManager.obstacleSpawnChance;
            float actualVsExpected = gridManager.aspectObstacles.Count / expectedObstacles;
            
            statsText += $"Expected: {expectedObstacles:F1} | Ratio: {actualVsExpected:F2}x";
            
            obstacleStatsText.text = statsText;
        }
        
        /// <summary>
        /// Context menu method to test the display
        /// </summary>
        [ContextMenu("Test Debug Display")]
        public void TestDebugDisplay()
        {
            RefreshDisplayImmediately();
            Debug.Log("🔧 Debug display refreshed manually");
        }
        
        /// <summary>
        /// Show/hide the debug display
        /// </summary>
        public void SetDebugDisplayVisible(bool visible)
        {
            if (showDebugToggle != null)
            {
                showDebugToggle.isOn = visible;
            }
            else
            {
                OnToggleDebugDisplay(visible);
            }
        }
        
        /// <summary>
        /// Enable/disable auto-updating of the display
        /// </summary>
        public void SetAutoUpdate(bool enabled)
        {
            autoUpdate = enabled;
            Debug.Log($"🔧 Debug display auto-update: {(enabled ? "ENABLED" : "DISABLED")}");
        }
        
        /// <summary>
        /// Set the update interval for auto-refresh
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.1f, interval);
            Debug.Log($"🔧 Debug display update interval: {updateInterval:F1}s");
        }
    }
}