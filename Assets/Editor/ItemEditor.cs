using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Item item = (Item)target;

        EditorGUI.BeginDisabledGroup(true);  // Disable editing
        EditorGUILayout.TextField("Item ID", item.ItemID);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Regenerate Item ID"))
        {
            Undo.RecordObject(item, "Regenerate Item ID");
            item.RegenerateID();
            EditorUtility.SetDirty(item);
        }
    }
}