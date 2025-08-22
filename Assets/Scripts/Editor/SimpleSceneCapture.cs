using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Simplified scene capture tool that avoids type dependencies
    /// This tool works even when other scripts have compilation errors
    /// </summary>
    public class SimpleSceneCapture : EditorWindow
    {
        private string sceneName = "CraftingDemo_Captured";

        [MenuItem("Tools/Simple Scene Capture")]
        public static void ShowWindow()
        {
            GetWindow<SimpleSceneCapture>("Simple Scene Capture");
        }

        private void OnGUI()
        {
            GUILayout.Label("Simple Scene Capture", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode first, then capture your scene.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("✅ Ready to capture scene!", MessageType.None);
            GUILayout.Space(10);

            sceneName = EditorGUILayout.TextField("Scene Name:", sceneName);
            GUILayout.Space(10);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Capture Scene Now", GUILayout.Height(40)))
            {
                CaptureCurrentScene();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            GUILayout.Label("Current Scene Objects:");
            
            var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int craftingObjects = 0;
            foreach (var obj in allObjects)
            {
                if (obj.GetType().Name.Contains("Crafting") || 
                    obj.GetType().Name.Contains("SimpleWorkingUI") ||
                    obj.GetType().Name.Contains("Demo"))
                {
                    craftingObjects++;
                }
            }
            
            EditorGUILayout.LabelField($"Found {craftingObjects} crafting-related objects");
            EditorGUILayout.LabelField($"Total objects: {allObjects.Length}");
        }

        private void CaptureCurrentScene()
        {
            try
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fullName = $"{sceneName}_{timestamp}";
                string scenePath = $"Assets/Scenes/{fullName}.unity";

                // Create Scenes directory if it doesn't exist
                string dir = "Assets/Scenes";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Save current scene
                var currentScene = SceneManager.GetActiveScene();
                EditorSceneManager.SaveScene(currentScene, scenePath, true);
                
                AssetDatabase.Refresh();

                Debug.Log($"✅ Scene captured: {scenePath}");
                
                EditorUtility.DisplayDialog("Success!", 
                    $"Scene captured successfully!\n\nSaved as: {fullName}.unity\n\nYou can now exit Play Mode and edit the captured scene.", 
                    "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to capture scene: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to capture scene: {e.Message}", "OK");
            }
        }

        [MenuItem("Tools/Quick Scene Capture", false, 100)]
        public static void QuickCapture()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Not in Play Mode", "Enter Play Mode first to capture the scene.", "OK");
                return;
            }

            try
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string scenePath = $"Assets/Scenes/QuickCapture_{timestamp}.unity";

                // Create Scenes directory if it doesn't exist
                string dir = "Assets/Scenes";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Save current scene
                var currentScene = SceneManager.GetActiveScene();
                EditorSceneManager.SaveScene(currentScene, scenePath, true);
                
                AssetDatabase.Refresh();

                Debug.Log($"✅ Quick capture completed: {scenePath}");
                
                EditorUtility.DisplayDialog("Quick Capture Complete!", 
                    $"Scene saved as: QuickCapture_{timestamp}.unity\n\nLocation: Assets/Scenes/", 
                    "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Quick capture failed: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Quick capture failed: {e.Message}", "OK");
            }
        }
    }
}