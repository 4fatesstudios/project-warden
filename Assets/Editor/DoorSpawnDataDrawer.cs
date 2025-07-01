using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;

[CustomPropertyDrawer(typeof(DoorSpawnData))]
public class DoorSpawnDataDrawer : PropertyDrawer
{
    private const float LineSpacing = 4f;
    private const float ColorBoxSize = 18f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty doorSpawnPoint = property.FindPropertyRelative("doorSpawnPoint");
        SerializedProperty doorType = property.FindPropertyRelative("doorType");
        SerializedProperty spawnWeight = property.FindPropertyRelative("spawnWeight");
        SerializedProperty spawnDirection = property.FindPropertyRelative("spawnDirection");
        SerializedProperty doorSpawnGroup = property.FindPropertyRelative("doorSpawnGroup");

        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float y = position.y;
        float fullWidth = position.width;

        // Top row labels
        float colWidth = (fullWidth - ColorBoxSize - 2 * LineSpacing) / 3f;
        EditorGUI.LabelField(new Rect(position.x, y, colWidth, lineHeight), "Type");
        EditorGUI.LabelField(new Rect(position.x + colWidth + LineSpacing, y, colWidth, lineHeight), "Weight");
        EditorGUI.LabelField(new Rect(position.x + 2 * (colWidth + LineSpacing), y, colWidth - ColorBoxSize, lineHeight), "Group");

        y += lineHeight;

        // Top row fields
        Rect doorTypeRect = new(position.x, y, colWidth, lineHeight);
        Rect weightRect = new(position.x + colWidth + LineSpacing, y, colWidth, lineHeight);
        Rect groupRect = new(position.x + 2 * (colWidth + LineSpacing), y, colWidth - ColorBoxSize, lineHeight);
        Rect colorRect = new(position.xMax - ColorBoxSize, y + 1, ColorBoxSize - 2, lineHeight - 2);

        EditorGUI.PropertyField(doorTypeRect, doorType, GUIContent.none);
        EditorGUI.PropertyField(weightRect, spawnWeight, GUIContent.none);
        EditorGUI.PropertyField(groupRect, doorSpawnGroup, GUIContent.none);
        EditorGUI.DrawRect(colorRect, GetColorFromGroup(doorSpawnGroup.intValue));

        y += lineHeight + LineSpacing;

        // Bottom row: direction value and readonly GameObject field
        Rect directionRect = new(position.x, y, 60f, lineHeight); // Reduced width for just the value
        Rect pointRect = new(position.x + 65f, y, fullWidth - 65f, lineHeight); // Adjusted position

        string directionValue = ((CardinalDirection)spawnDirection.enumValueIndex).ToString();
        EditorGUI.LabelField(directionRect, directionValue); // Show just the value without label
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.ObjectField(pointRect, GUIContent.none, doorSpawnPoint.objectReferenceValue, typeof(GameObject), true);
        EditorGUI.EndDisabledGroup();

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 3 * EditorGUIUtility.singleLineHeight + LineSpacing;
    }

    private static Color GetColorFromGroup(int group)
    {
        return group switch
        {
            0 => Color.black,
            1 => Color.red,
            2 => Color.blue,
            3 => Color.green,
            4 => Color.yellow,
            5 => Color.magenta,
            6 => Color.cyan,
            _ => Color.white
        };
    }
}