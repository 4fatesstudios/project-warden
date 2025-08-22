using UnityEngine;
using UnityEditor;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Simple tool to check compilation status and force refresh
    /// </summary>
    public class CompilationChecker : EditorWindow
    {
        [MenuItem("Tools/Check Compilation Status")]
        public static void ShowWindow()
        {
            GetWindow<CompilationChecker>("Compilation Checker");
        }

        [MenuItem("Tools/Force Refresh Assets", false, 200)]
        public static void ForceRefresh()
        {
            AssetDatabase.Refresh();
            Debug.Log("🔄 Forced asset refresh complete");
        }

        private void OnGUI()
        {
            GUILayout.Label("Compilation Status Checker", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.LabelField($"Is Compiling: {EditorApplication.isCompiling}");
            EditorGUILayout.LabelField($"Play Mode State: {EditorApplication.isPlaying}");

            GUILayout.Space(10);

            if (GUILayout.Button("Force Asset Refresh"))
            {
                AssetDatabase.Refresh();
                Debug.Log("🔄 Forced asset refresh");
            }

            if (GUILayout.Button("Check Console Errors"))
            {
                CheckConsoleErrors();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "If compilation errors persist:\n" +
                "1. Force refresh assets\n" +
                "2. Check console for specific errors\n" +
                "3. Clear console and refresh again", 
                MessageType.Info);
        }

        private void CheckConsoleErrors()
        {
            Debug.Log("🔍 Checking for compilation errors...");
            
            if (EditorApplication.isCompiling)
            {
                Debug.Log("⏳ Unity is currently compiling scripts");
            }
            else
            {
                Debug.Log("✅ No active compilation detected");
            }
        }
    }
}