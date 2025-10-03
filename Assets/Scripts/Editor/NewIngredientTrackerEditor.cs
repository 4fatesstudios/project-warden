using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.UI;

namespace FourFatesStudios.ProjectWarden.Editor
{
    [CustomEditor(typeof(NewIngredientTracker))]
    public class NewIngredientTrackerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            NewIngredientTracker tracker = (NewIngredientTracker)target;

            GUILayout.Space(20);
            GUILayout.Label("Testing Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Reset All Ingredients (Mark as New)", GUILayout.Height(30)))
            {
                tracker.ResetAllIngredients();
                EditorUtility.DisplayDialog("Reset Complete", "All ingredients have been marked as new for testing!", "OK");
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Use this button to test the new ingredient star system. After clicking, all ingredients will show the new star indicator until hovered over.", MessageType.Info);
        }
    }
}