using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Distills;

[CustomEditor(typeof(Distill))]
public class DistillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Distill distill = (Distill)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        bool isValid = ValidateDistill(distill);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Input Ingredient", distill.InputIngredient?.ItemID ?? "None");
        EditorGUILayout.TextField("Output Ingredient", distill.OutputIngredient?.ItemID ?? "None");
        EditorGUILayout.FloatField("Required Duration", distill.RequiredDuration);
        EditorGUILayout.FloatField("Total Duration", distill.TotalDuration);
        EditorGUILayout.IntField("Progression Stages", distill.ProgressionStages?.Count ?? 0);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Validate Distill"))
        {
            if (isValid)
            {
                Debug.Log("Distill configuration is valid.");
            }
            else
            {
                Debug.LogWarning("One or more distill fields are invalid.");
            }
        }

        if (!isValid)
        {
            EditorGUILayout.HelpBox("Validation failed. Ensure durations are logical and progression stages are unique and non-empty.", MessageType.Error);
        }
    }

    private bool ValidateDistill(Distill distill)
    {
        bool durationCheck = distill.RequiredDuration > 0 && distill.TotalDuration > 0 && distill.RequiredDuration <= distill.TotalDuration;
        bool progressionCheck = distill.ProgressionStages != null && distill.ProgressionStages.Count > 0;

        var uniqueStages = new HashSet<DistillVisualState>(distill.ProgressionStages);
        bool duplicatesCheck = uniqueStages.Count == distill.ProgressionStages.Count;

        return durationCheck && progressionCheck && duplicatesCheck;
    }
}
