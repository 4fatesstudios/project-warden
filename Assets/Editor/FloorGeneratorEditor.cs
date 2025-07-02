#if UNITY_EDITOR
using FourFatesStudios.ProjectWarden.ProceduralGeneration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FloorGenerator))]
public class FloorGeneratorEditor : Editor
{
    
    public override void OnInspectorGUI() {
        FloorGenerator floorGenerator = (FloorGenerator)target;
        
        DrawDefaultInspector();
        
        if (GUILayout.Button("Generate Floor")) {
            floorGenerator.GenerateFloor();
        }
    }
}
#endif