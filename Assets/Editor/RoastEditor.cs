using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Roasts;

[CustomEditor(typeof(Roast))]
public class RoastEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Roast roast = (Roast)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        bool isValid = ValidateRoast(roast);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Input Ingredient", roast.InputIngredient?.ID ?? "None");
        EditorGUILayout.TextField("Output Ingredient", roast.OutputIngredient?.ID ?? "None");
        EditorGUILayout.FloatField("Cook Time", roast.TotalCookTime);
        EditorGUILayout.FloatField("Cooked Time", roast.CookedTime);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Validate Roast"))
        {
            if (isValid)
            {
                Debug.Log("Roast thresholds are valid.");
            }
            else
            {
                Debug.LogWarning("One or more roast thresholds are invalid.");
            }
        }

        if (!isValid)
        {
            EditorGUILayout.HelpBox("Cooking thresholds are not logically ordered or invalid.", MessageType.Error);
        }

        if (roast.RequiresCorruptedCooking)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox($"Corrupted recipe requires at least {roast.RequiredCookCount} cooked ingredients.", MessageType.Info);
        }
    }

    private bool ValidateRoast(Roast roast)
    {
        return roast.UncookedThreshold < roast.CookedThreshold &&
               roast.CookedThreshold < roast.BurntThreshold &&
               roast.TotalCookTime > 0;
    }
}
