using System;
using FourFatesStudios.ProjectWarden.Utilities.CSVImport;
using UnityEditor;
using UnityEngine;

public class CSVImportConfirmPopup : EditorWindow
{
    private CSVValidationResult result;
    private Action onConfirm;

    public static void ShowPopup(CSVValidationResult result, Action onConfirm) {
        var window = CreateInstance<CSVImportConfirmPopup>();
        window.result = result;
        window.onConfirm = onConfirm;
        window.titleContent = new GUIContent("Confirm CSV Import");
        window.ShowUtility();
    }

    private void OnGUI() {
        GUILayout.Label("CSV Validation Results", EditorStyles.boldLabel);
        GUILayout.Label($"Valid entries: {result.ValidCount}");
        GUILayout.Label($"Invalid entries: {result.InvalidCount}");
        GUILayout.Label($"New entries: {result.NewCount}");
        GUILayout.Label($"Updated entries: {result.UpdatedCount}");
        GUILayout.Label($"Deleted entries: {result.DeletedCount}");

        GUILayout.Space(10);
        GUILayout.Label("Details logged to console.", EditorStyles.wordWrappedLabel);

        GUILayout.Space(20);
        if (GUILayout.Button("Proceed with Import")) {
            Close();
            onConfirm?.Invoke();
        }

        if (GUILayout.Button("Cancel")) {
            Close();
        }
    }
}
