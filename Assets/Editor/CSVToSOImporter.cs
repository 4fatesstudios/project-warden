using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.Utilities;
using FourFatesStudios.ProjectWarden.Utilities.CSVImport;

public class CSVToSOImporter : EditorWindow
{
    private TextAsset csvFile;
    private MonoScript scriptableObjectType;
    private string outputFolder = "Assets/Resources/";
    
    // For tracking duplicates and row info
    private Dictionary<string, int> idToLastRow = new();
    
    [MenuItem("Tools/CSV Importer")]
    public static void ShowWindow() {
        GetWindow<CSVToSOImporter>("CSV to ScriptableObject");
    }

    private void OnGUI() {
        GUILayout.Label("CSV to ScriptableObject Importer", EditorStyles.boldLabel);

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        scriptableObjectType = (MonoScript)EditorGUILayout.ObjectField("Entry ScriptableObject Type", scriptableObjectType, typeof(MonoScript), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Import")) ImportCSV();
    }
    
    private void ImportCSV() {
        CustomLogger.Init(LogSystem.CSVImporter, clear: true);

        System.Type type = scriptableObjectType.GetClass();
        if (csvFile == null || scriptableObjectType == null) {
            CustomLogger.LogError(LogSystem.CSVImporter, "CSV file or ScriptableObject type not assigned.");
            return;
        }

        if (!typeof(BaseDataSO).IsAssignableFrom(type)) {
            CustomLogger.LogError(LogSystem.CSVImporter, $"Type {type.Name} must inherit from BaseDataSO.");
            return;
        }

        var result = CSVValidator.Validate(csvFile, type, outputFolder);
        LogValidationResult(result);

        if (result.HeaderMismatch) {
            Debug.LogError("Validation aborted due to header mismatch. See log for details.");
            return;
        }

        CSVImportConfirmPopup.ShowPopup(result, () => {
            Debug.Log("Confirmed. Proceeding with import...");
            ProceedWithImport(result);
        });
    }

    private void ProceedWithImport(CSVValidationResult result) {
        System.Type type = scriptableObjectType.GetClass();
        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string[] headersRaw = lines[0].Split(',');
        string[] headers = headersRaw.Select(h => h.Split(':')[0].Trim()).ToArray();
        string[] declaredTypes = headersRaw.Select(h => h.Contains(":") ? h.Split(':')[1].Trim() : "string").ToArray();

        int idIndex = Array.FindIndex(headers, h => h.Equals("ID", StringComparison.OrdinalIgnoreCase));
        
        Dictionary<string, string[]> idToValues = new();
        for (int i = 1; i < lines.Length; i++) {
            string[] values = lines[i].Split(',');
            if (values.Length != headers.Length) continue;

            string id = values[idIndex].Trim();
            if (result.ValidIDs.Contains(id)) idToValues[id] = values;
        }

        foreach (var id in result.ValidIDs) {
            string[] values = idToValues[id];
            int row = Array.IndexOf(lines, lines.First(l => l.Contains(id))) + 1; // for logging

            string assetPath = $"{outputFolder}/{type.Name+id}.asset";
            ScriptableObject instance = AssetDatabase.LoadAssetAtPath(assetPath, type) as ScriptableObject;
            bool isNew = instance == null;

            if (isNew) {
                instance = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(instance, assetPath);
                CustomLogger.Log(LogSystem.CSVImporter, $"Row {row}: Created new SO asset at {assetPath}.");
            }

            SerializedObject serialized = new SerializedObject(instance);
            for (int c = 0; c < headers.Length; c++) {
                string header = headers[c];
                string declaredType = declaredTypes[c];
                string value = values[c].Trim();

                if (header.Equals("ID", StringComparison.OrdinalIgnoreCase)) {
                    SerializedProperty idProp = serialized.FindProperty("id");
                    if (idProp != null) idProp.stringValue = value;
                    continue;
                }

                SerializedProperty prop = serialized.FindProperty(header);
                if (prop == null) continue; // should never happen if validation passed

                CSVTypeParsers.Parsers[declaredType.ToLower()](prop, value, row, header);
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(instance);
        }

        foreach (var id in result.DeletedIDs) {
            string assetPath = $"{outputFolder}/{type.Name+id}.asset";
            if (AssetDatabase.DeleteAsset(assetPath)) {
                CustomLogger.Log(LogSystem.CSVImporter, $"Deleted SO asset not found in CSV: {assetPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CustomLogger.Log(LogSystem.CSVImporter, $"Import finished. Created: {result.NewCount}, Updated: {result.UpdatedCount}, Deleted: {result.DeletedCount}");
        Debug.Log("CSV import completed.");
    }
 
    private void LogValidationResult(CSVValidationResult result) {
        foreach (var err in result.HeaderErrors)
            CustomLogger.LogError(LogSystem.CSVImporter, err);

        foreach (var err in result.EntryErrors)
            CustomLogger.LogError(LogSystem.CSVImporter, err);

        foreach (var warn in result.EntryWarnings)
            CustomLogger.Log(LogSystem.CSVImporter, warn);

        if (result.DeletedIDs.Count > 0)
            CustomLogger.Log(LogSystem.CSVImporter, $"IDs to delete ({result.DeletedIDs.Count}): {string.Join(", ", result.DeletedIDs)}");

        if (result.UpdatedIDs.Count > 0)
            CustomLogger.Log(LogSystem.CSVImporter, $"IDs to update ({result.UpdatedIDs.Count}): {string.Join(", ", result.UpdatedIDs)}");

        if (result.NewIDs.Count > 0)
            CustomLogger.Log(LogSystem.CSVImporter, $"IDs to create ({result.NewIDs.Count}): {string.Join(", ", result.NewIDs)}");
    }

    private bool IsTypeCompatible(string declaredType, System.Type actualType) {
        declaredType = declaredType.ToLower();
        return CSVTypeParsers.Parsers.ContainsKey(declaredType);
    }
    
    private bool TryParseAndSetProperty(SerializedProperty prop, string declaredType, string value, int csvRow, string header) {
        declaredType = declaredType.ToLower();
        if (CSVTypeParsers.Parsers.TryGetValue(declaredType, out var parser))
            return parser(prop, value, csvRow, header);

        CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Unsupported declared type '{declaredType}' for '{header}'.");
        return false;
    }

}
