using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

[CanEditMultipleObjects]
[CustomEditor(typeof(Item), true)] // 'true' allows inheritance
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Item item = (Item)target;

        EditorGUI.BeginDisabledGroup(true);  // Disable editing
        EditorGUILayout.TextField("Item ID", item.ID);
        EditorGUI.EndDisabledGroup();
    }
}