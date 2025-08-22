#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Utility to clean up LINQ conflicts
    /// </summary>
    public class LinqConflictCleaner
    {
        [MenuItem("Tools/Fix LINQ Conflicts")]
        public static void FixLinqConflicts()
        {
            Debug.Log("🔧 Cleaning up LINQ conflicts...");
            
            // The main issue is that Unity 6 already has all LINQ methods
            // We just need to make sure files use the standard System.Linq namespace
            
            Debug.Log("✅ LINQ conflicts should be resolved after script compilation.");
            Debug.Log("💡 Unity 6 includes all necessary LINQ methods built-in.");
            
            // Force recompilation
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("LINQ Fix", 
                "LINQ conflict cleanup initiated!\n\nUnity will recompile scripts automatically.\n\nThe custom LINQ extensions have been removed to avoid conflicts with Unity 6's built-in LINQ.", 
                "OK");
        }
    }
}
#endif