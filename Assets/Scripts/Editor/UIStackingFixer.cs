using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Tool to diagnose and fix UI stacking issues
    /// </summary>
    public class UIStackingFixer : EditorWindow
    {
        [MenuItem("Tools/Fix UI Stacking Issues")]
        public static void ShowWindow()
        {
            GetWindow<UIStackingFixer>("UI Stacking Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("UI Stacking Diagnosis & Fix", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to diagnose UI stacking issues.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("🔍 Analyzing UI documents in the scene...", MessageType.None);

            // Analyze current UI state
            var allUIDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            
            GUILayout.Label($"Found {allUIDocuments.Length} UIDocument(s):", EditorStyles.boldLabel);
            
            int enabledCount = 0;
            int activeCount = 0;
            
            foreach (var doc in allUIDocuments)
            {
                string status = "";
                if (doc.enabled) 
                {
                    enabledCount++;
                    status += "✅ Enabled ";
                }
                else 
                {
                    status += "❌ Disabled ";
                }
                
                if (doc.gameObject.activeInHierarchy) 
                {
                    activeCount++;
                    status += "🟢 Active";
                }
                else 
                {
                    status += "🔴 Inactive";
                }
                
                int childCount = doc.rootVisualElement?.childCount ?? 0;
                status += $" ({childCount} children)";
                
                EditorGUILayout.LabelField($"  • {doc.gameObject.name}: {status}");
            }

            GUILayout.Space(10);
            
            if (enabledCount > 1)
            {
                EditorGUILayout.HelpBox($"⚠️ Found {enabledCount} enabled UIDocuments - this may cause stacking!", MessageType.Warning);
                
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Fix: Keep Only SimpleWorkingUI Active"))
                {
                    FixUIStacking();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox("✅ No stacking issues detected!", MessageType.Info);
            }

            GUILayout.Space(10);
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Refresh SimpleWorkingUI"))
            {
                RefreshSimpleWorkingUI();
            }
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Clear All UI Content"))
            {
                ClearAllUIContent();
            }
            GUI.backgroundColor = Color.white;
        }

        private void FixUIStacking()
        {
            var allUIDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            int disabledCount = 0;
            
            foreach (var doc in allUIDocuments)
            {
                // Keep only SimpleWorkingUI active
                if (doc.gameObject.GetComponent<SimpleWorkingUI>() == null && doc.enabled)
                {
                    doc.enabled = false;
                    disabledCount++;
                    Debug.Log($"🚫 Disabled UIDocument on {doc.gameObject.name}");
                }
            }
            
            Debug.Log($"✅ UI Stacking Fix: Disabled {disabledCount} conflicting UIDocuments");
            
            // Refresh the SimpleWorkingUI
            RefreshSimpleWorkingUI();
        }

        private void RefreshSimpleWorkingUI()
        {
            var simpleUI = Object.FindFirstObjectByType<SimpleWorkingUI>();
            if (simpleUI != null)
            {
                // Trigger a refresh by calling the method via reflection
                var method = simpleUI.GetType().GetMethod("SetupUI", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(simpleUI, null);
                    Debug.Log("🔄 Refreshed SimpleWorkingUI");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ SimpleWorkingUI not found in scene");
            }
        }

        private void ClearAllUIContent()
        {
            var allUIDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            
            foreach (var doc in allUIDocuments)
            {
                if (doc.rootVisualElement != null)
                {
                    doc.rootVisualElement.Clear();
                }
            }
            
            Debug.Log("🧹 Cleared all UI content - restart SimpleWorkingUI to recreate");
        }

        [MenuItem("Tools/Quick Fix UI Stacking", false, 200)]
        public static void QuickFixUIStacking()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Not in Play Mode", "Enter Play Mode first to fix UI stacking.", "OK");
                return;
            }

            var allUIDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            int disabledCount = 0;
            
            foreach (var doc in allUIDocuments)
            {
                // Keep only SimpleWorkingUI active
                if (doc.gameObject.GetComponent<SimpleWorkingUI>() == null && doc.enabled)
                {
                    doc.enabled = false;
                    disabledCount++;
                }
            }
            
            Debug.Log($"⚡ Quick Fix: Disabled {disabledCount} conflicting UIDocuments");
            
            EditorUtility.DisplayDialog("UI Stacking Fixed!", 
                $"Disabled {disabledCount} conflicting UI documents.\n\nOnly SimpleWorkingUI should now be visible.", 
                "OK");
        }
    }
}