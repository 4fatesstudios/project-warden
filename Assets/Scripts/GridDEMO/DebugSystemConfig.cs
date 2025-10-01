using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Centralized debug configuration system for the Grid Demo.
    /// Provides toggleable debug categories with clear descriptions.
    /// </summary>
    [System.Serializable]
    public class DebugSystemConfig : MonoBehaviour
    {
        [Header("🚧 Obstacle System Debug")]
        [SerializeField] private bool enableObstacleSpawnDebug = true;
        [Tooltip("Log when obstacles are spawned, their types, and spawn chance calculations")]
        
        [SerializeField] private bool enableObstacleInteractionDebug = true;
        [Tooltip("Log obstacle interactions when ingredients are placed on them")]
        
        [Header("🎯 Grid Placement Debug")]
        [SerializeField] private bool enableIngredientPlacementDebug = false;
        [Tooltip("Log detailed ingredient placement operations and collision detection")]
        
        [SerializeField] private bool enableCollisionDetectionDebug = false;
        [Tooltip("Log collision detection checks and why placements succeed or fail")]
        
        [SerializeField] private bool enableShapeCollisionDebug = false;
        [Tooltip("Log detailed shape-based collision checking for complex ingredient shapes")]
        
        [Header("🔄 Grid State Debug")]
        [SerializeField] private bool enableGridStateDebug = false;
        [Tooltip("Log grid state changes and cell occupancy updates")]
        
        [SerializeField] private bool enableCellVerificationDebug = false;
        [Tooltip("Log continuous verification of cell occupancy integrity")]
        
        [Header("🎨 Visual Effects Debug")]
        [SerializeField] private bool enableParticleEffectsDebug = true;
        [Tooltip("Log particle effect creation, destruction, and interactions")]
        
        [SerializeField] private bool enableVisualizerDebug = false;
        [Tooltip("Log grid visualizer operations and cell color updates")]
        
        [Header("📊 Proficiency System Debug")]
        [SerializeField] private bool enableProficiencyGradingDebug = true;
        [Tooltip("Log proficiency calculations, grades, and scoring breakdowns")]
        
        [Header("🧪 Test & Development Debug")]
        [SerializeField] private bool enableTestingDebug = true;
        [Tooltip("Log test operations, context menu actions, and development tools")]
        
        [SerializeField] private bool enableInputDebug = false;
        [Tooltip("Log mouse input, hover detection, and user interactions")]
        
        [Header("🚨 Critical System Debug")]
        [SerializeField] private bool enableErrorRecoveryDebug = true;
        [Tooltip("Log error recovery, system repairs, and critical failures")]
        
        [SerializeField] private bool enablePerformanceDebug = false;
        
        [Header("🔧 Development Tools")]
        [Space(10)]
        [Tooltip("Run UI Initialization Tests - Creates proficiency display and tests UI components")]
        public bool uiInitTestVerbose = true;
        
        [Space(10)]
        [Tooltip("Run System Validation Tests - Validates entire system readiness")]
        public bool systemValidationVerbose = true;
        
        // Singleton instance for global access
        public static DebugSystemConfig Instance { get; private set; }
        
        private void Awake()
        {
            // Don't use singleton pattern for scene-specific debug configs
            // Just set the instance to this component
            Instance = this;
            LogDebugSystemInitialization();
        }
        
        private void OnDestroy()
        {
            // Clear instance when destroyed
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void LogDebugSystemInitialization()
        {
            // Use regular Debug.Log for the initialization message since this is system-level
            Debug.Log("🔧 === DEBUG SYSTEM INITIALIZED ===");
            Debug.Log($"🚧 Obstacle Debug: {enableObstacleSpawnDebug} / {enableObstacleInteractionDebug}");
            Debug.Log($"🎯 Placement Debug: {enableIngredientPlacementDebug} / {enableCollisionDetectionDebug} / {enableShapeCollisionDebug}");
            Debug.Log($"🔄 Grid State Debug: {enableGridStateDebug} / {enableCellVerificationDebug}");
            Debug.Log($"🎨 Visual Debug: {enableParticleEffectsDebug} / {enableVisualizerDebug}");
            Debug.Log($"📊 Proficiency Debug: {enableProficiencyGradingDebug}");
            Debug.Log($"🧪 Testing Debug: {enableTestingDebug} / {enableInputDebug}");
            Debug.Log($"🚨 Critical Debug: {enableErrorRecoveryDebug} / {enablePerformanceDebug}");
            Debug.Log("🔧 Use the Inspector to toggle debug categories on/off");
            Debug.Log("🔧 === END DEBUG SYSTEM STATUS ===");
        }
        
        // Debug category getters - now with better null handling
        public static bool ObstacleSpawnDebug => Instance?.enableObstacleSpawnDebug ?? false;
        public static bool ObstacleInteractionDebug => Instance?.enableObstacleInteractionDebug ?? false;
        public static bool IngredientPlacementDebug => Instance?.enableIngredientPlacementDebug ?? false;
        public static bool CollisionDetectionDebug => Instance?.enableCollisionDetectionDebug ?? false;
        public static bool ShapeCollisionDebug => Instance?.enableShapeCollisionDebug ?? false;
        public static bool GridStateDebug => Instance?.enableGridStateDebug ?? false;
        public static bool CellVerificationDebug => Instance?.enableCellVerificationDebug ?? false;
        public static bool ParticleEffectsDebug => Instance?.enableParticleEffectsDebug ?? false;
        public static bool VisualizerDebug => Instance?.enableVisualizerDebug ?? false;
        public static bool ProficiencyGradingDebug => Instance?.enableProficiencyGradingDebug ?? false;
        public static bool TestingDebug => Instance?.enableTestingDebug ?? false;
        public static bool InputDebug => Instance?.enableInputDebug ?? false;
        public static bool ErrorRecoveryDebug => Instance?.enableErrorRecoveryDebug ?? false;
        public static bool PerformanceDebug => Instance?.enablePerformanceDebug ?? false;
        
        // Convenient logging methods with null safety
        public static void LogObstacleSpawn(string message) 
        {
            if (Instance != null && ObstacleSpawnDebug) Debug.Log($"🚧 OBSTACLE SPAWN: {message}");
        }
        
        public static void LogObstacleInteraction(string message) 
        {
            if (Instance != null && ObstacleInteractionDebug) Debug.Log($"🚧 OBSTACLE INTERACTION: {message}");
        }
        
        public static void LogIngredientPlacement(string message) 
        {
            if (Instance != null && IngredientPlacementDebug) Debug.Log($"🎯 PLACEMENT: {message}");
        }
        
        public static void LogCollisionDetection(string message) 
        {
            if (Instance != null && CollisionDetectionDebug) Debug.Log($"🎯 COLLISION: {message}");
        }
        
        public static void LogShapeCollision(string message) 
        {
            if (Instance != null && ShapeCollisionDebug) Debug.Log($"🎯 SHAPE: {message}");
        }
        
        public static void LogGridState(string message) 
        {
            if (Instance != null && GridStateDebug) Debug.Log($"🔄 GRID STATE: {message}");
        }
        
        public static void LogCellVerification(string message) 
        {
            if (Instance != null && CellVerificationDebug) Debug.Log($"🔄 VERIFICATION: {message}");
        }
        
        public static void LogParticleEffects(string message) 
        {
            if (Instance != null && ParticleEffectsDebug) Debug.Log($"🎨 PARTICLES: {message}");
        }
        
        public static void LogVisualizer(string message) 
        {
            if (Instance != null && VisualizerDebug) Debug.Log($"🎨 VISUALIZER: {message}");
        }
        
        public static void LogProficiencyGrading(string message) 
        {
            if (Instance != null && ProficiencyGradingDebug) Debug.Log($"📊 PROFICIENCY: {message}");
        }
        
        public static void LogTesting(string message) 
        {
            if (Instance != null && TestingDebug) Debug.Log($"🧪 TEST: {message}");
        }
        
        public static void LogInput(string message) 
        {
            if (Instance != null && InputDebug) Debug.Log($"🧪 INPUT: {message}");
        }
        
        public static void LogErrorRecovery(string message) 
        {
            if (Instance != null && ErrorRecoveryDebug) Debug.Log($"🚨 ERROR RECOVERY: {message}");
        }
        
        public static void LogPerformance(string message) 
        {
            if (Instance != null && PerformanceDebug) Debug.Log($"🚨 PERFORMANCE: {message}");
        }
        
        // Context menu methods for quick testing
        [ContextMenu("Enable All Debug Categories")]
        public void EnableAllDebugCategories()
        {
            enableObstacleSpawnDebug = true;
            enableObstacleInteractionDebug = true;
            enableIngredientPlacementDebug = true;
            enableCollisionDetectionDebug = true;
            enableShapeCollisionDebug = true;
            enableGridStateDebug = true;
            enableCellVerificationDebug = true;
            enableParticleEffectsDebug = true;
            enableVisualizerDebug = true;
            enableProficiencyGradingDebug = true;
            enableTestingDebug = true;
            enableInputDebug = true;
            enableErrorRecoveryDebug = true;
            enablePerformanceDebug = true;
            
            Debug.Log("🔧 ALL DEBUG CATEGORIES ENABLED");
        }
        
        [ContextMenu("Disable All Debug Categories")]
        public void DisableAllDebugCategories()
        {
            enableObstacleSpawnDebug = false;
            enableObstacleInteractionDebug = false;
            enableIngredientPlacementDebug = false;
            enableCollisionDetectionDebug = false;
            enableShapeCollisionDebug = false;
            enableGridStateDebug = false;
            enableCellVerificationDebug = false;
            enableParticleEffectsDebug = false;
            enableVisualizerDebug = false;
            enableProficiencyGradingDebug = false;
            enableTestingDebug = false;
            enableInputDebug = false;
            enableErrorRecoveryDebug = false;
            enablePerformanceDebug = false;
            
            Debug.Log("🔧 ALL DEBUG CATEGORIES DISABLED");
        }
        
        [ContextMenu("Enable Essential Debug Only")]
        public void EnableEssentialDebugOnly()
        {
            enableObstacleSpawnDebug = true;
            enableObstacleInteractionDebug = false;
            enableIngredientPlacementDebug = false;
            enableCollisionDetectionDebug = false;
            enableShapeCollisionDebug = false;
            enableGridStateDebug = false;
            enableCellVerificationDebug = false;
            enableParticleEffectsDebug = true;
            enableVisualizerDebug = false;
            enableProficiencyGradingDebug = true;
            enableTestingDebug = true;
            enableInputDebug = false;
            enableErrorRecoveryDebug = true;
            enablePerformanceDebug = false;
            
            Debug.Log("🔧 ESSENTIAL DEBUG CATEGORIES ENABLED (Obstacles, Particles, Proficiency, Testing, Error Recovery)");
        }
        
        [ContextMenu("Show Current Debug Status")]
        public void ShowCurrentDebugStatus()
        {
            LogDebugSystemInitialization();
        }
        
        [ContextMenu("Test Debug System")]
        public void TestDebugSystem()
        {
            Debug.Log("🧪 === TESTING DEBUG SYSTEM ===");
            LogObstacleSpawn("Test obstacle spawn message");
            LogObstacleInteraction("Test obstacle interaction message");
            LogIngredientPlacement("Test ingredient placement message");
            LogCollisionDetection("Test collision detection message");
            LogShapeCollision("Test shape collision message");
            LogGridState("Test grid state message");
            LogCellVerification("Test cell verification message");
            LogParticleEffects("Test particle effects message");
            LogVisualizer("Test visualizer message");
            LogProficiencyGrading("Test proficiency grading message");
            LogTesting("Test testing message");
            LogInput("Test input message");
            LogErrorRecovery("Test error recovery message");
            LogPerformance("Test performance message");
            Debug.Log("🧪 === END DEBUG SYSTEM TEST ===");
            Debug.Log("🔧 Only enabled categories should show messages above. Check the Inspector to toggle categories on/off.");
        }
        
        // Public toggle methods for runtime toggling
        public static void ToggleCollisionDetectionDebug()
        {
            if (Instance != null)
            {
                Instance.enableCollisionDetectionDebug = !Instance.enableCollisionDetectionDebug;
                LogTesting($"Collision debug logging: {(Instance.enableCollisionDetectionDebug ? "ENABLED" : "DISABLED")}");
            }
        }
        
        // ================================================
        // CONSOLIDATED UI & SYSTEM TESTING TOOLS
        // ================================================
        
        [ContextMenu("🧪 Run UI Initialization Test")]
        public void RunUIInitializationTest()
        {
            LogUITest("🧪 [UI INIT TEST] Starting UI Initialization Test...");
            
            // Check what canvases exist
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            LogUITest($"🧪 [UI INIT TEST] Found {canvases.Length} canvases in scene:");
            foreach (var canvas in canvases)
            {
                LogUITest($"🧪 [UI INIT TEST] Canvas: '{canvas.name}' - RenderMode: {canvas.renderMode} - Active: {canvas.gameObject.activeInHierarchy}");
            }
            
            // Check if ProficiencyDisplayManager exists
            var proficiencyManager = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GridDemo.UI.ProficiencyDisplayManager>();
            if (proficiencyManager != null)
            {
                LogUITest($"🧪 [UI INIT TEST] Found existing ProficiencyDisplayManager on {proficiencyManager.gameObject.name}");
                LogUITest($"🧪 [UI INIT TEST] Is initialized: {proficiencyManager.IsInitialized}");
                
                // Force initialization
                if (!proficiencyManager.IsInitialized)
                {
                    proficiencyManager.ForceInitialize();
                }
                proficiencyManager.ForceRefreshProficiency();
                
                LogUITest("✅ [UI INIT TEST] ProficiencyDisplayManager initialization complete!");
            }
            else
            {
                LogUITest("⚠️ [UI INIT TEST] No ProficiencyDisplayManager found - consider adding one manually");
            }
        }
        
        [ContextMenu("🎯 Run Complete System Validation")]
        public void RunCompleteSystemValidation()
        {
            LogSystemTest("🚀 Starting Complete System Validation Test...");
            LogSystemTest("=====================================");
            
            bool allTestsPassed = true;
            
            // Test 1: Check if core components can be found
            LogSystemTest("📋 Test 1: Checking for core component types...");
            
            var inventoryType = System.Type.GetType("FourFatesStudios.ProjectWarden.Characters.Components.InventoryComponent");
            var uiManagerType = System.Type.GetType("FourFatesStudios.ProjectWarden.GridDemo.UI.SimpleUIStartupManager");
            var completedPotionsType = System.Type.GetType("FourFatesStudios.ProjectWarden.GridDemo.UI.CompletedPotionsUIDocument");
            
            LogSystemTest($"   InventoryComponent type: {GetTestResult(inventoryType != null)}");
            LogSystemTest($"   SimpleUIStartupManager type: {GetTestResult(uiManagerType != null)}");
            LogSystemTest($"   CompletedPotionsUIDocument type: {GetTestResult(completedPotionsType != null)}");
            
            if (inventoryType == null || uiManagerType == null || completedPotionsType == null)
            {
                allTestsPassed = false;
            }
            
            // Test 2: Check compilation status
            LogSystemTest("📋 Test 2: Checking compilation status...");
            
            try
            {
                // Try to create instances to test compilation
                var testGO = new GameObject("__TEST__");
                
                if (inventoryType != null)
                {
                    var inventory = testGO.AddComponent(inventoryType);
                    LogSystemTest($"   InventoryComponent instantiation: {GetTestResult(inventory != null)}");
                    if (inventory == null) allTestsPassed = false;
                }
                
                if (uiManagerType != null)
                {
                    var manager = testGO.AddComponent(uiManagerType);
                    LogSystemTest($"   SimpleUIStartupManager instantiation: {GetTestResult(manager != null)}");
                    if (manager == null) allTestsPassed = false;
                }
                
                DestroyImmediate(testGO);
                LogSystemTest("   Component instantiation test: ✅ PASSED");
            }
            catch (System.Exception ex)
            {
                LogSystemTest($"   Component instantiation test: ❌ FAILED - {ex.Message}");
                allTestsPassed = false;
            }
            
            // Test 3: Check resource files
            LogSystemTest("📋 Test 3: Checking UI resource files...");
            
            var uxml = Resources.Load("UI/UXML/CompletedPotionsList");
            var uss = Resources.Load("UI/Styles/CompletedPotionsListStyles");
            
            LogSystemTest($"   UXML resource: {GetTestResult(uxml != null)}");
            LogSystemTest($"   USS resource: {GetTestResult(uss != null)}");
            
            if (uxml == null || uss == null)
            {
                LogSystemTest("   ⚠️ Some UI resources missing but system can work without them");
            }
            
            // Test 4: Final system readiness
            LogSystemTest("📋 Test 4: Overall system readiness...");
            
            bool coreSystemReady = inventoryType != null && uiManagerType != null && completedPotionsType != null;
            LogSystemTest($"   Core system components: {GetTestResult(coreSystemReady)}");
            
            if (!coreSystemReady)
            {
                allTestsPassed = false;
            }
            
            // Final result
            LogSystemTest("=====================================");
            
            if (allTestsPassed)
            {
                LogSystemTest("🎉 SYSTEM VALIDATION: ✅ ALL TESTS PASSED");
                LogSystemTest("🚀 The Automatic Inventory UI System is READY!");
                LogSystemTest("📝 System is ready for production use");
            }
            else
            {
                LogSystemTest("❌ SYSTEM VALIDATION: SOME TESTS FAILED");
                LogSystemTest("⚠️ There may be compilation issues preventing full system readiness");
                LogSystemTest("💡 Check console for any remaining compilation errors and restart Unity Editor");
            }
        }
        
        [ContextMenu("🎨 Debug UI Visibility Test")]
        public void DebugUIVisibilityTest()
        {
            LogUITest("🧪 [UI VISIBILITY TEST] Making all text components bright red and large for visibility testing...");
            
            // Find all TextMeshProUGUI components
            var tmpTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var text in tmpTexts)
            {
                text.color = Color.red;
                text.fontSize = 24f;
                LogUITest($"🧪 [UI VISIBILITY TEST] Made TextMeshPro red and large: '{text.text}' on {text.gameObject.name}");
            }
            
            // Find all UI.Text components
            var uiTexts = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None);
            foreach (var text in uiTexts)
            {
                text.color = Color.red;
                text.fontSize = 24;
                LogUITest($"🧪 [UI VISIBILITY TEST] Made UI.Text red and large: '{text.text}' on {text.gameObject.name}");
            }
            
            LogUITest($"🧪 [UI VISIBILITY TEST] Modified {tmpTexts.Length} TextMeshPro and {uiTexts.Length} UI.Text components");
            
            // Make all backgrounds yellow
            var images = FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None);
            foreach (var image in images)
            {
                image.color = Color.yellow;
                image.type = UnityEngine.UI.Image.Type.Simple;
                image.sprite = null;
                LogUITest($"🧪 [UI VISIBILITY TEST] Made Image yellow: {image.gameObject.name}");
            }
            
            LogUITest($"🧪 [UI VISIBILITY TEST] Modified {images.Length} Image components");
        }
        
        // Helper methods for consolidated testing
        private string GetTestResult(bool success)
        {
            return success ? "✅ PASS" : "❌ FAIL";
        }
        
        private void LogUITest(string message)
        {
            if (uiInitTestVerbose)
                Debug.Log($"[DebugSystemConfig - UI Test] {message}");
        }
        
        private void LogSystemTest(string message)
        {
            if (systemValidationVerbose)
                Debug.Log($"[DebugSystemConfig - System Test] {message}");
        }
    }
}