using UnityEngine;
using System.Collections;
using Demo;

namespace FourFatesStudios.ProjectWarden.Runtime
{
    /// <summary>
    /// Automatically saves scene state when certain conditions are met
    /// Attach this to any GameObject to enable automatic scene saving
    /// </summary>
    public class AutoSceneSaver : MonoBehaviour
    {
        [Header("Auto Save Settings")]
        [SerializeField] private bool autoSaveOnSetupComplete = true;
        [SerializeField] private float saveDelay = 2f; // Delay after setup completes
        [SerializeField] private KeyCode manualSaveKey = KeyCode.F5;
        [SerializeField] private string savedSceneName = "CraftingSystem_AutoSaved";

        [Header("Save Triggers")]
        [SerializeField] private bool saveOnFirstUIVisible = true;
        [SerializeField] private bool saveOnAllSystemsReady = true;

        [Header("Status")]
        [SerializeField] private bool setupComplete = false;
        [SerializeField] private bool alreadySaved = false;

        private Coroutine autoSaveCoroutine;

        private void Start()
        {
            if (autoSaveOnSetupComplete)
            {
                StartCoroutine(MonitorSetupProgress());
            }
        }

        private void Update()
        {
            // Manual save with F5 key
            if (Input.GetKeyDown(manualSaveKey))
            {
                TriggerSceneSave("Manual save triggered");
            }

            // Check for UI visibility
            if (saveOnFirstUIVisible && !setupComplete && IsUISystemVisible())
            {
                setupComplete = true;
                Debug.Log("UI system detected as visible - marking setup complete");
            }

            // Check for all systems ready
            if (saveOnAllSystemsReady && !setupComplete && AreAllSystemsReady())
            {
                setupComplete = true;
                Debug.Log("All crafting systems detected as ready - marking setup complete");
            }
        }

        private IEnumerator MonitorSetupProgress()
        {
            Debug.Log("AutoSceneSaver: Monitoring setup progress...");
            
            while (!setupComplete && !alreadySaved)
            {
                yield return new WaitForSeconds(0.5f);
                
                // Check various completion criteria
                if (IsSetupComplete())
                {
                    setupComplete = true;
                    Debug.Log("AutoSceneSaver: Setup completion detected!");
                    break;
                }
            }

            if (setupComplete && !alreadySaved)
            {
                // Wait for the specified delay
                yield return new WaitForSeconds(saveDelay);
                
                if (!alreadySaved) // Double-check to avoid duplicate saves
                {
                    TriggerSceneSave("Auto-save after setup completion");
                }
            }
        }

        private bool IsSetupComplete()
        {
            // Check for any component with "SimpleWorkingUI" in the name
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var comp in allComponents)
            {
                if (comp.GetType().Name.Contains("SimpleWorkingUI"))
                {
                    return true;
                }
            }

            // Check for any component with "CraftingSystemSetupGuide" in the name  
            foreach (var comp in allComponents)
            {
                if (comp.GetType().Name.Contains("CraftingSystemSetupGuide"))
                {
                    // Try to get demoReady property via reflection
                    var demoReadyField = comp.GetType().GetField("demoReady");
                    if (demoReadyField != null && (bool)demoReadyField.GetValue(comp))
                    {
                        return true;
                    }
                }
            }

            // Check for multiple UI systems
            var uiDocuments = FindObjectsByType<UnityEngine.UIElements.UIDocument>(FindObjectsSortMode.None);
            if (uiDocuments.Length >= 2)
            {
                return true;
            }

            return false;
        }

        private bool IsUISystemVisible()
        {
            // Check if any UI system has visible content
            var uiDocuments = FindObjectsByType<UnityEngine.UIElements.UIDocument>(FindObjectsSortMode.None);
            foreach (var uiDoc in uiDocuments)
            {
                if (uiDoc.rootVisualElement != null && uiDoc.rootVisualElement.childCount > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AreAllSystemsReady()
        {
            // Check if we have the main crafting components (using simplified approach)
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            // Look for crafting-related components by name
            foreach (var comp in allComponents)
            {
                var typeName = comp.GetType().Name;
                if (typeName.Contains("PotionCrafting") || 
                    typeName.Contains("GridMinigame") || 
                    typeName.Contains("SimpleWorkingUI"))
                {
                    return true;
                }
            }

            return false;
        }

        public void TriggerSceneSave(string reason = "Manual trigger")
        {
#if UNITY_EDITOR
            if (alreadySaved)
            {
                Debug.Log("Scene already auto-saved this session");
                return;
            }

            try
            {
                Debug.Log($"AutoSceneSaver: Triggering scene save - {reason}");
                
                // Use the RuntimeSceneCapture if available
                
                // Directly call the capture method via reflection or create our own simple save
                SaveCurrentState();
                
                alreadySaved = true;
                Debug.Log("✅ Auto-save completed successfully!");
                
                // Show a brief notification
                ShowSaveNotification();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Auto-save failed: {e.Message}");
            }
#else
            Debug.Log("Auto-save only works in Editor mode");
#endif
        }

        private void SaveCurrentState()
        {
#if UNITY_EDITOR
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string sceneName = $"{savedSceneName}_{timestamp}";
            string scenePath = $"Assets/Scenes/AutoSaved/{sceneName}.unity";

            // Create directory if needed
            string dirPath = "Assets/Scenes/AutoSaved/";
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            // Save the current scene
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(currentScene, scenePath, true);
            
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"Scene auto-saved to: {scenePath}");
#endif
        }

        private void ShowSaveNotification()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("Auto-Save Complete", 
                $"Scene saved automatically!\n\nFile: {savedSceneName}\nLocation: Assets/Scenes/AutoSaved/\n\nYou can now exit Play Mode and edit the saved scene.", "OK");
#endif
        }

        // Public methods for external triggering
        public void MarkSetupComplete()
        {
            setupComplete = true;
        }

        public void ForceSceneSave()
        {
            TriggerSceneSave("Force save requested");
        }

        // Context menu for easy access
        [ContextMenu("Force Save Scene Now")]
        private void ForceSaveFromMenu()
        {
            TriggerSceneSave("Context menu save");
        }

        [ContextMenu("Mark Setup Complete")]
        private void MarkCompleteFromMenu()
        {
            MarkSetupComplete();
        }

        // Display status in inspector
        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            // Show status overlay in game window
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.textColor = Color.white;

            string statusText = setupComplete ? "✅ Setup Complete" : "⏳ Monitoring Setup...";
            if (alreadySaved) statusText += " - Scene Saved!";
            statusText += $"\nPress {manualSaveKey} to save manually";

            GUI.Box(new Rect(10, Screen.height - 80, 200, 60), statusText, style);
        }
    }
}