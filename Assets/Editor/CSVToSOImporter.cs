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
        CustomLogger.Init(LogSystem.CSVImporter, clear:true);
        
        // Ensure that both the CSVFile field and ScriptableObject field are assigned
        if (csvFile == null || scriptableObjectType == null) {
            CustomLogger.LogError(LogSystem.CSVImporter, "CSV file or ScriptableObject type not assigned.");
            Debug.LogError("CSV file or ScriptableObject type not assigned.");
            return;
        }
        
        // Ensure the ScriptableObject inherits from BaseDataSO
        System.Type type = scriptableObjectType.GetClass();
        if (!typeof(BaseDataSO).IsAssignableFrom(type)) {
            CustomLogger.LogError(LogSystem.CSVImporter, $"Type {type.Name} must inherit from BaseDataSO.");
            Debug.LogError($"Type {type.Name} must inherit from BaseDataSO.");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) {
            CustomLogger.LogError(LogSystem.CSVImporter, "CSV file has no data rows.");
            Debug.LogWarning("CSV file has no data rows.");
            return;
        }

        string[] headersRaw = lines[0].Split(',');
        string[] headers = headersRaw.Select(h => h.Split(':')[0].Trim()).ToArray();
        string[] declaredTypes = headersRaw.Select(h => h.Contains(":") ? h.Split(':')[1].Trim() : "string").ToArray();

        int idIndex = System.Array.FindIndex(headers, h => h.Equals("ID", StringComparison.OrdinalIgnoreCase));
        if (idIndex < 0) {
            CustomLogger.LogError(LogSystem.CSVImporter, "CSV must include an 'ID' column.");
            Debug.LogError("CSV must include an 'ID' column.");
            return;
        }

        var allFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < headers.Length; i++) {
            string header = headers[i];
            string declaredType = declaredTypes[i];

            // For ID, accept either a public property or a private serialized field named 'id' (lowercase)
            if (header.Equals("ID", StringComparison.OrdinalIgnoreCase)) {
                var field = allFields.FirstOrDefault(f => f.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
                var prop = allProps.FirstOrDefault(p => p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase));
                if (field == null && prop == null) {
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Header '{header}' not found as 'id' field or 'ID' property in {type.Name}. Aborting import.");
                    Debug.LogError($"Header '{header}' not found as 'id' field or 'ID' property in {type.Name}. Aborting import.");
                    return;
                }
                continue;
            }

            var fieldNormal = allFields.FirstOrDefault(f => f.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
            var propNormal = allProps.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));

            if (fieldNormal == null && propNormal == null) {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Header '{header}' not found as a field or property in {type.Name}. Aborting import.");
                Debug.LogError($"Header '{header}' not found as a field or property in {type.Name}. Aborting import.");
                return;
            }

            System.Type actualType = fieldNormal != null ? fieldNormal.FieldType : propNormal.PropertyType;
            if (!IsTypeCompatible(declaredType, actualType)) {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Type mismatch for '{header}': CSV declared '{declaredType}', but actual is '{actualType.Name}'. Aborting import.");
                Debug.LogError($"Type mismatch for '{header}': CSV declared '{declaredType}', but actual is '{actualType.Name}'. Aborting import.");
                return;
            }
        }

        idToLastRow.Clear();

        for (int i = 1; i < lines.Length; i++) {
            int csvRow = i + 1;
            string[] values = lines[i].Split(',');

            if (values.Length != headers.Length) {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Column count mismatch. Expected {headers.Length}, got {values.Length}. Skipping row.");
                continue;
            }

            string id = values[idIndex].Trim();
            if (string.IsNullOrEmpty(id)) {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Missing ID. Skipping row.");
                continue;
            }

            bool anyEmpty = false;
            for (int c = 0; c < values.Length; c++) {
                if (c == idIndex) continue;
                if (string.IsNullOrWhiteSpace(values[c])) {
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Empty value in column '{headers[c]}'. Skipping row.");
                    anyEmpty = true;
                }
            }
            if (anyEmpty) continue;

            if (idToLastRow.ContainsKey(id)) {
                CustomLogger.Log(LogSystem.CSVImporter, $"Row {csvRow}: Duplicate ID '{id}' found. Updating previous entry.");
                idToLastRow[id] = csvRow;
            }
            else {
                idToLastRow.Add(id, csvRow);
            }

            string assetPath = $"{outputFolder}/{type.Name+id}.asset";
            ScriptableObject instance = AssetDatabase.LoadAssetAtPath(assetPath, type) as ScriptableObject;
            bool isNew = instance == null;
            if (isNew) {
                instance = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(instance, assetPath);
                CustomLogger.Log(LogSystem.CSVImporter, $"Row {csvRow}: Created new SO asset at {assetPath}.");
            }

            SerializedObject serialized = new SerializedObject(instance);

            bool parseError = false;
            for (int c = 0; c < headers.Length; c++) {
                string header = headers[c];
                string declaredType = declaredTypes[c];
                string value = values[c].Trim();

                if (header.Equals("ID", StringComparison.OrdinalIgnoreCase)) {
                    SerializedProperty idProp = serialized.FindProperty("id"); // lowercase to match field name
                    if (idProp == null) {
                        CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: SerializedProperty 'id' not found on {type.Name}.");
                        parseError = true;
                        break;
                    }
                    idProp.stringValue = value;
                    continue;
                }

                SerializedProperty prop = serialized.FindProperty(header);
                if (prop == null) {
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: SerializedProperty '{header}' not found on {type.Name}. Skipping row.");
                    parseError = true;
                    break;
                }

                if (TryParseAndSetProperty(prop, declaredType, value, csvRow, header)) continue;
                parseError = true;
                break;
            }

            if (parseError) {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Skipping row due to previous errors.");
                continue;
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(instance);
        }

        AssetDatabase.SaveAssets();
        CustomLogger.Log(LogSystem.CSVImporter, $"CSV import complete. Processed {lines.Length - 1} rows.");
        Debug.Log("CSV import complete.");
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
