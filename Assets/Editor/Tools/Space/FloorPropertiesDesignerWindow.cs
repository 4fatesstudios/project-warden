using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;
using UnityEngine;

public class FloorPropertiesDesignerWindow : EditorWindow 
{
    private FloorProperties selectedFloorProperties;
    private Editor floorPropertiesEditor;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Floors/Floor Properties Designer")]
    public static void ShowWindow() {
        ShowWindow(null);
    }

    public static void ShowWindow(FloorProperties selectedFloorProperties) {
        var window = GetWindow<FloorPropertiesDesignerWindow>("Floor Properties Design Tool");
        window.selectedFloorProperties = selectedFloorProperties;
        window.Focus();
    }

    private void OnGUI() {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Floor Properties Designer", EditorStyles.boldLabel);
        selectedFloorProperties = EditorGUILayout.ObjectField("Floor Properties", selectedFloorProperties, typeof(FloorProperties), false) as FloorProperties;

        if (selectedFloorProperties == null) {
            EditorGUILayout.HelpBox("No Floor Properties selected.", MessageType.Info);

            if (GUILayout.Button("Create New Floor Properties", GUILayout.Height(30))) {
                FloorPropertiesCreatorWindow.ShowWindow();
            }
            
            EditorGUILayout.EndScrollView();
            return;
        }
        
        // FloorProperties inspector
        if (floorPropertiesEditor == null || floorPropertiesEditor.target != selectedFloorProperties)
            floorPropertiesEditor = Editor.CreateEditor(selectedFloorProperties);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Floor Properties", EditorStyles.boldLabel);
        floorPropertiesEditor.OnInspectorGUI();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Floor Previews", EditorStyles.boldLabel);
        
        
        
        EditorGUILayout.EndScrollView();
    }
}
