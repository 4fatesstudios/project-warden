using FourFatesStudios.ProjectWarden.ScriptableObjects;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(BaseDataSO), true)]
public class BaseDataSOEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDataBaseInspector();
        DrawDefaultInspector();
    }

    protected void DrawDataBaseInspector() {
        BaseDataSO baseDataSo = (BaseDataSO)target;

        EditorGUI.BeginDisabledGroup(true);  // Disable editing
        EditorGUILayout.TextField("Data ID", baseDataSo.ID);
        EditorGUI.EndDisabledGroup();
    }
}
