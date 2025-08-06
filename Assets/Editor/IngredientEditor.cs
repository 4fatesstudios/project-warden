using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

[CustomEditor(typeof(Ingredient))]
public class IngredientEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Exclude refinement fields from auto-draw
        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "canGrind",
            "grindingResult",
            "canDistill",
            "distillingResult",
            "canRoast",
            "roastingResult"
        );

        EditorGUILayout.Space();

        DrawProperty("canGrind");
        if (GetBool("canGrind")) DrawProperty("grindingResult");
        
        EditorGUILayout.Space(2);

        DrawProperty("canDistill");
        if (GetBool("canDistill")) DrawProperty("distillingResult");
        
        EditorGUILayout.Space(2);

        DrawProperty("canRoast");
        if (GetBool("canRoast")) DrawProperty("roastingResult");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProperty(string name)
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty(name), true);
    }

    private bool GetBool(string name)
    {
        return serializedObject.FindProperty(name).boolValue;
    }
}