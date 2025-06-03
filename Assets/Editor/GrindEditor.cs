using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Grinds;

[CustomEditor(typeof(Grind))]
public class GrindEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Grind grind = (Grind)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

        bool isValid = ValidateGrind(grind);

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Input Ingredient", grind.InputIngredient?.ItemID ?? "None");
        EditorGUILayout.TextField("Output Ingredient", grind.OutputIngredient?.ItemID ?? "None");
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Validate Grind"))
        {
            if (isValid)
            {
                Debug.Log("Grind thresholds are valid.");
            }
            else
            {
                Debug.LogWarning("One or more grind thresholds are invalid.");
            }
        }

        if (!isValid)
        {
            EditorGUILayout.HelpBox("Thresholds are not logically ordered. Check min/max values.", MessageType.Error);
        }
    }

    private bool ValidateGrind(Grind grind)
    {
        return grind.MinGrindSpeed < grind.MaxGrindSpeed &&
               grind.MinPoundSpeed < grind.MaxPoundSpeed;
    }
}
