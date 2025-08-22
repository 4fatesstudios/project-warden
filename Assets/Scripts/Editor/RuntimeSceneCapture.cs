using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Captures the current runtime scene state and saves it as a new scene file
    /// Use this to save your crafting system setup after it's generated at runtime
    /// </summary>
    public class RuntimeSceneCapture : EditorWindow
    {
        private string sceneName = "CraftingSystemDemo_Generated";
        private string savePath = "Assets/Scenes/";
        private bool includeInactiveObjects = true;
        private bool createPrefabVariants;

        [MenuItem("Tools/Runtime Scene Capture")]
        public static void ShowWindow()
        {
            GetWindow<RuntimeSceneCapture>("Runtime Scene Capture");
        }

        private void OnGUI()
        {
            GUILayout.Label("Runtime Scene Capture", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode first to generate your crafting system, then use this tool to capture it.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("✅ Play Mode Active - Ready to capture runtime scene state!", MessageType.None);
            GUILayout.Space(10);

            // Scene name input
            GUILayout.Label("Scene Settings", EditorStyles.boldLabel);
            sceneName = EditorGUILayout.TextField("Scene Name:", sceneName);
            
            // Path selection
            GUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Save Path:", savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.SaveFolderPanel("Select Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length) + "/";
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Options
            GUILayout.Label("Capture Options", EditorStyles.boldLabel);
            includeInactiveObjects = EditorGUILayout.Toggle("Include Inactive Objects", includeInactiveObjects);
            createPrefabVariants = EditorGUILayout.Toggle("Create Prefab Variants", createPrefabVariants);

            GUILayout.Space(15);

            // Capture button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Capture Current Scene State", GUILayout.Height(40)))
            {
                CaptureRuntimeScene();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Quick analysis
            DisplaySceneAnalysis();
        }

        private void DisplaySceneAnalysis()
        {
            GUILayout.Label("Current Scene Analysis", EditorStyles.boldLabel);
            
            var craftingObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb.GetType().Name.Contains("Crafting") || 
                           mb.GetType().Name.Contains("SimpleWorkingUI") ||
                           mb.GetType().Name.Contains("Demo"))
                .ToArray();

            EditorGUILayout.LabelField("Crafting System Objects:", craftingObjects.Length.ToString());
            
            foreach (var obj in craftingObjects.Take(5)) // Show first 5
            {
                EditorGUILayout.LabelField($"  • {obj.name} ({obj.GetType().Name})");
            }
            
            if (craftingObjects.Length > 5)
            {
                EditorGUILayout.LabelField($"  ... and {craftingObjects.Length - 5} more");
            }

            var uiDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            EditorGUILayout.LabelField("UI Documents:", uiDocuments.Length.ToString());
        }

        private void CaptureRuntimeScene()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Must be in Play Mode to capture runtime state!", "OK");
                return;
            }

            try
            {
                // Create the directory if it doesn't exist
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                string fullPath = $"{savePath}{sceneName}.unity";

                // Check if file already exists
                if (File.Exists(fullPath))
                {
                    if (!EditorUtility.DisplayDialog("File Exists", 
                        $"Scene '{sceneName}.unity' already exists. Overwrite?", "Yes", "Cancel"))
                    {
                        return;
                    }
                }

                // Save current scene state
                var currentScene = SceneManager.GetActiveScene();
                
                // Create a new scene and copy objects
                var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                
                // Copy all root objects to new scene
                var rootObjects = currentScene.GetRootGameObjects();
                foreach (var rootObj in rootObjects)
                {
                    if (includeInactiveObjects || rootObj.activeInHierarchy)
                    {
                        // Create a copy in the new scene
                        var copy = Instantiate(rootObj);
                        SceneManager.MoveGameObjectToScene(copy, newScene);
                        
                        // Remove runtime-only components that shouldn't be saved
                        CleanupRuntimeComponents(copy);
                    }
                }

                // Save the new scene
                EditorSceneManager.SaveScene(newScene, fullPath);
                AssetDatabase.Refresh();

                // Remove the temporary scene
                EditorSceneManager.CloseScene(newScene, true);

                EditorUtility.DisplayDialog("Success", 
                    $"Scene captured and saved as '{fullPath}'!\n\nYou can now exit Play Mode and edit the saved scene.", "OK");

                Debug.Log($"✅ Runtime scene captured successfully: {fullPath}");

                // Optionally create prefabs
                if (createPrefabVariants)
                {
                    CreatePrefabVariants();
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to capture scene: {e.Message}", "OK");
                Debug.LogError($"Scene capture failed: {e}");
            }
        }

        private void CleanupRuntimeComponents(GameObject obj)
        {
            // Remove components that are runtime-only and shouldn't be saved
            var componentsToRemove = new System.Type[]
            {
                // Add any runtime-only component types here
            };

            foreach (var compType in componentsToRemove)
            {
                var components = obj.GetComponentsInChildren(compType);
                foreach (var comp in components)
                {
                    DestroyImmediate(comp);
                }
            }

            // Clean up UIDocument references that might be runtime-generated
            var uiDocs = obj.GetComponentsInChildren<UIDocument>();
            foreach (var uiDoc in uiDocs)
            {
                // Keep the UIDocument but clear runtime-generated visual trees
                if (uiDoc.visualTreeAsset == null)
                {
                    // This was programmatically generated, clear it
                    uiDoc.rootVisualElement?.Clear();
                }
            }
        }

        private void CreatePrefabVariants()
        {
            Debug.Log("Creating prefab variants for crafting system components...");
            
            string prefabPath = savePath + "Prefabs/";
            if (!Directory.Exists(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
            }

            var craftingObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(mb => mb.GetType().Name.Contains("Crafting") || 
                           mb.GetType().Name.Contains("SimpleWorkingUI"))
                .Select(mb => mb.gameObject)
                .Distinct()
                .ToArray();

            foreach (var obj in craftingObjects)
            {
                string prefabName = $"{prefabPath}{obj.name}_Prefab.prefab";
                PrefabUtility.SaveAsPrefabAsset(obj, prefabName);
                Debug.Log($"Created prefab: {prefabName}");
            }
        }

        // Helper context menu for quick access
        [MenuItem("GameObject/Save Runtime State", false, 10)]
        private static void SaveRuntimeStateFromContext()
        {
            if (Application.isPlaying)
            {
                ShowWindow();
            }
            else
            {
                EditorUtility.DisplayDialog("Not in Play Mode", 
                    "Enter Play Mode first, then right-click any GameObject to capture runtime state.", "OK");
            }
        }
    }
}