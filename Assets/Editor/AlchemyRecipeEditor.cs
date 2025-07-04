#if UNITY_EDITOR
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AlchemyRecipe))]
public class AlchemyRecipeEditor : Editor
{
    // cached styles
    private GUIStyle _warningStyle;
    private GUIStyle _keyStyle;

    private void OnEnable()
    {
        _warningStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { textColor = Color.yellow },
            fontStyle = FontStyle.Italic
        };

        _keyStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { textColor = Color.cyan },
            alignment = TextAnchor.MiddleCenter
        };
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector but cache changes
        serializedObject.Update();

        // Ingredients 
        EditorGUILayout.LabelField("Input Ingredients", EditorStyles.boldLabel);
        DrawProperty("inputIngredient1", "Ingredient 1");
        DrawProperty("inputIngredient2", "Ingredient 2");
        DrawProperty("inputIngredient3", "Ingredient 3");

        // Rhythm Data 
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Rhythm Requirements", EditorStyles.boldLabel);
        DrawHitsAndAttempts();

        // Output Potion 
        EditorGUILayout.Space(4);
        DrawProperty("outputPotion", "Output Potion");

        // Temperature and Duration 
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Process Thresholds", EditorStyles.boldLabel);
        DrawProperty("requiredTemperature", "Required Temperature (s)");
        DrawProperty("totalDuration", "Total Duration (s)");

        // Utilities
        EditorGUILayout.Space(6);
        if (GUILayout.Button("Auto-Sort Ingredients"))
            SortIngredientsAlphabetically();

        ShowRecipeKeyPreview();
        ShowMissingWarnings();

        // commit changes
        serializedObject.ApplyModifiedProperties();
    }

    // Helper to draw a property with optional custom label

    private void DrawProperty(string propName, string label = null)
    {
        var prop = serializedObject.FindProperty(propName);
        if (prop == null) return;
        EditorGUILayout.PropertyField(prop, new GUIContent(label ?? prop.displayName));
    }

    private void DrawHitsAndAttempts()
    {
        var hitsProp = serializedObject.FindProperty("requiredHits");
        var triesProp = serializedObject.FindProperty("maxAttempts");

        EditorGUILayout.PropertyField(hitsProp, new GUIContent("Required Hits"));
        EditorGUILayout.PropertyField(triesProp, new GUIContent("Max Attempts"));

        if (hitsProp.intValue <= 0 || triesProp.intValue <= 0 || hitsProp.intValue > triesProp.intValue)
        {
            EditorGUILayout.HelpBox("Required Hits must be > 0 and <= Max Attempts.", MessageType.Error);
        }
    }

    private void SortIngredientsAlphabetically()
    {
        var ing1 = serializedObject.FindProperty("inputIngredient1");
        var ing2 = serializedObject.FindProperty("inputIngredient2");
        var ing3 = serializedObject.FindProperty("inputIngredient3");

        var list = new[] { ing1, ing2, ing3 }
                   .Select(p => p.objectReferenceValue)
                   .OfType<UnityEngine.Object>()
                   .OrderBy(o => o.name)
                   .ToArray();

        if (list.Length == 0) return;

        ing1.objectReferenceValue = list.Length > 0 ? list[0] : null;
        ing2.objectReferenceValue = list.Length > 1 ? list[1] : null;
        ing3.objectReferenceValue = list.Length > 2 ? list[2] : null;
    }

    private void ShowRecipeKeyPreview()
    {
        var ingNames = new[]
        {
            serializedObject.FindProperty("inputIngredient1").objectReferenceValue,
            serializedObject.FindProperty("inputIngredient2").objectReferenceValue,
            serializedObject.FindProperty("inputIngredient3").objectReferenceValue
        }
        .OfType<UnityEngine.Object>()
        .OrderBy(o => o.name)
        .Select(o => o.name)
        .ToArray();

        if (ingNames.Length >= 2)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Recipe Key: {string.Join(", ", ingNames)}", _keyStyle);
        }
    }

    private void ShowMissingWarnings()
    {
        var potion = serializedObject.FindProperty("outputPotion").objectReferenceValue;
        if (potion == null)
            EditorGUILayout.LabelField("Output potion is not assigned.", _warningStyle);

        var ing1 = serializedObject.FindProperty("inputIngredient1").objectReferenceValue;
        var ing2 = serializedObject.FindProperty("inputIngredient2").objectReferenceValue;
        if (ing1 == null || ing2 == null)
            EditorGUILayout.LabelField("At least two ingredients are required.", _warningStyle);
    }
}
#endif
