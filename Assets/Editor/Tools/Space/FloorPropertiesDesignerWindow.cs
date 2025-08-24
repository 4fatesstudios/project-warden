using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;
using UnityEngine;

public class FloorPropertiesDesignerWindow : EditorWindow 
{
    private FloorProperties selectedFloorProperties;
    
    [MenuItem("Tools/Spaces/Floor Properties Design Tool")]
    public static void ShowWindow() {
        ShowWindow(null);
    }

    public static void ShowWindow(FloorProperties selectedFloorProperties) {
        var window = GetWindow<FloorPropertiesDesignerWindow>("Floor Properties Design Tool");
        window.selectedFloorProperties = selectedFloorProperties;
        window.Focus();
    }
}
