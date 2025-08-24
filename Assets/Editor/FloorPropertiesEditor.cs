using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;

[CustomEditor(typeof(FloorProperties))]
public class FloorPropertiesEditor : BaseDataSOEditor {
    public override void OnInspectorGUI() {
        DrawDataBaseInspector();
        
        
    }
}
