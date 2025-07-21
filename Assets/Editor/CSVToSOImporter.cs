using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Utilities;
using System.IO;

public class CSVToSOImporter : EditorWindow
{
    private TextAsset csvFile;
    private MonoScript scriptableObjectType;
    private string outputFolder = "Assets/Data/ScriptableObjects/";
    
    // For tracking duplicates and row info
    private Dictionary<string, int> idToLastRow = new Dictionary<string, int>();

    [MenuItem("Tools/CSV Importer")]
    public static void ShowWindow()
    {
        GetWindow<CSVToSOImporter>("CSV to ScriptableObject");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV to ScriptableObject Importer", EditorStyles.boldLabel);

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        scriptableObjectType = (MonoScript)EditorGUILayout.ObjectField("Entry ScriptableObject Type", scriptableObjectType, typeof(MonoScript), false);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Import"))
        {
            ImportCSV();
        }
    }

    private void ImportCSV()
    {
        CustomLogger.Init(LogSystem.CSVImporter, clear:true);

        if (csvFile == null || scriptableObjectType == null)
        {
            CustomLogger.LogError(LogSystem.CSVImporter, "CSV file or ScriptableObject type not assigned.");
            Debug.LogError("CSV file or ScriptableObject type not assigned.");
            return;
        }

        System.Type type = scriptableObjectType.GetClass();
        if (!typeof(ScriptableObject).IsAssignableFrom(type))
        {
            CustomLogger.LogError(LogSystem.CSVImporter, $"Type {type.Name} must inherit from ScriptableObject.");
            Debug.LogError($"Type {type.Name} must inherit from ScriptableObject.");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            CustomLogger.LogError(LogSystem.CSVImporter, "CSV file has no data rows.");
            Debug.LogWarning("CSV file has no data rows.");
            return;
        }

        // Parse header and validate
        string[] headersRaw = lines[0].Split(',');
        string[] headers = headersRaw.Select(h => h.Split(':')[0].Trim()).ToArray();
        string[] declaredTypes = headersRaw.Select(h => h.Contains(":") ? h.Split(':')[1].Trim() : "string").ToArray();

        int idIndex = System.Array.IndexOf(headers, "ID");
        if (idIndex < 0)
        {
            CustomLogger.LogError(LogSystem.CSVImporter, "CSV must include an 'ID' column.");
            Debug.LogError("CSV must include an 'ID' column.");
            return;
        }

        // Validate headers exist as serialized fields or public properties
        var allFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 0; i < headers.Length; i++)
        {
            string header = headers[i];
            string declaredType = declaredTypes[i];
            // Try to find a private serialized field matching the lowercase header
            var field = allFields.FirstOrDefault(f => f.Name.Equals(header, System.StringComparison.OrdinalIgnoreCase));
            var prop = allProps.FirstOrDefault(p => p.Name.Equals(header, System.StringComparison.OrdinalIgnoreCase));

            if (header == "ID")
            {
                // ID is allowed to be a public property or field
                if (field == null && prop == null)
                {
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Header '{header}' not found as a field or property in {type.Name}. Aborting import.");
                    Debug.LogError($"Header '{header}' not found as a field or property in {type.Name}. Aborting import.");
                    return;
                }
                continue;
            }

            if (field == null && prop == null)
            {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Header '{header}' not found as a field or property in {type.Name}. Aborting import.");
                Debug.LogError($"Header '{header}' not found as a field or property in {type.Name}. Aborting import.");
                return;
            }

            // Validate type matches - parse declared type vs field/prop type
            System.Type actualType = field != null ? field.FieldType : prop.PropertyType;
            if (!IsTypeCompatible(declaredType, actualType))
            {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Type mismatch for '{header}': CSV declared '{declaredType}', but actual is '{actualType.Name}'. Aborting import.");
                Debug.LogError($"Type mismatch for '{header}': CSV declared '{declaredType}', but actual is '{actualType.Name}'. Aborting import.");
                return;
            }
        }

        // Clear tracking
        idToLastRow.Clear();

        // Start processing rows
        for (int i = 1; i < lines.Length; i++)
        {
            int csvRow = i + 1; // human-readable row number
            string line = lines[i];
            string[] values = line.Split(',');

            if (values.Length != headers.Length)
            {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Column count mismatch. Expected {headers.Length}, got {values.Length}. Skipping row.");
                continue;
            }

            // Check ID presence
            string id = values[idIndex].Trim();
            if (string.IsNullOrEmpty(id))
            {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Missing ID. Skipping row.");
                continue;
            }

            // Check for empty values in other columns - skip if any missing or empty (except ID)
            bool anyEmpty = false;
            for (int c = 0; c < values.Length; c++)
            {
                if (c == idIndex) continue;
                if (string.IsNullOrWhiteSpace(values[c]))
                {
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Empty value in column '{headers[c]}'. Skipping row.");
                    anyEmpty = true;
                }
            }
            if (anyEmpty) continue;

            // Check duplicates: if already processed, update last row (log duplicate)
            if (idToLastRow.ContainsKey(id))
            {
                CustomLogger.Log(LogSystem.CSVImporter, $"Row {csvRow}: Duplicate ID '{id}' found. Updating previous entry.");
                idToLastRow[id] = csvRow; // update to latest
            }
            else
            {
                idToLastRow.Add(id, csvRow);
            }

            // Load existing SO or create new
            string assetPath = $"{outputFolder}/{id}.asset";
            ScriptableObject instance = AssetDatabase.LoadAssetAtPath(assetPath, type) as ScriptableObject;
            bool isNew = instance == null;
            if (isNew)
            {
                instance = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(instance, assetPath);
                CustomLogger.Log(LogSystem.CSVImporter, $"Row {csvRow}: Created new SO asset at {assetPath}.");
            }

            SerializedObject serialized = new SerializedObject(instance);

            // Set values, but validate parse success before applying!
            bool parseError = false;
            for (int c = 0; c < headers.Length; c++)
            {
                string header = headers[c];
                string declaredType = declaredTypes[c];
                string value = values[c].Trim();

                if (header == "ID")
                {
                    // set via property if exists
                    var idProp = type.GetProperty("ID", BindingFlags.Public | BindingFlags.Instance);
                    if (idProp != null && idProp.CanWrite)
                        idProp.SetValue(instance, value);
                    else
                    {
                        var idField = type.GetField("ID", BindingFlags.Public | BindingFlags.Instance);
                        if (idField != null)
                            idField.SetValue(instance, value);
                        else
                        {
                            CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Could not set ID property or field.");
                            parseError = true;
                        }
                    }
                    continue;
                }

                SerializedProperty prop = serialized.FindProperty(header.ToLower());
                if (prop == null)
                {
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: SerializedProperty '{header.ToLower()}' not found on {type.Name}. Skipping row.");
                    parseError = true;
                    break;
                }

                if (!TryParseAndSetProperty(prop, declaredType, value, csvRow, header))
                {
                    parseError = true;
                    break;
                }
            }

            if (parseError)
            {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Skipping row due to previous errors.");
                continue;
            }

            // Save changes
            serialized.ApplyModifiedProperties();

            EditorUtility.SetDirty(instance);
        }

        AssetDatabase.SaveAssets();

        CustomLogger.Log(LogSystem.CSVImporter, $"CSV import complete. Processed {lines.Length - 1} rows.");
        Debug.Log("CSV import complete.");
    }

    private bool IsTypeCompatible(string declaredType, System.Type actualType)
    {
        declaredType = declaredType.ToLower();
        if (declaredType == "int" && (actualType == typeof(int) || actualType == typeof(long) || actualType == typeof(short)))
            return true;
        if (declaredType == "string" && actualType == typeof(string))
            return true;
        if (declaredType == "bool" && actualType == typeof(bool))
            return true;
        if (declaredType == "float" && actualType == typeof(float))
            return true;
        // add more types as needed
        return false;
    }
    
    private bool TryParseAndSetProperty(SerializedProperty prop, string declaredType, string value, int csvRow, string header)
    {
        declaredType = declaredType.ToLower();
        try
        {
            switch (declaredType)
            {
                case "int":
                    if (int.TryParse(value, out int intVal))
                    {
                        prop.intValue = intVal;
                        return true;
                    }
                    else
                    {
                        CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Failed to parse int for '{header}' with value '{value}'.");
                        return false;
                    }
                case "string":
                    prop.stringValue = value;
                    return true;
                case "bool":
                    if (bool.TryParse(value, out bool boolVal))
                    {
                        prop.boolValue = boolVal;
                        return true;
                    }
                    else
                    {
                        CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Failed to parse bool for '{header}' with value '{value}'.");
                        return false;
                    }
                case "float":
                    if (float.TryParse(value, out float floatVal))
                    {
                        prop.floatValue = floatVal;
                        return true;
                    }
                    else
                    {
                        CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Failed to parse float for '{header}' with value '{value}'.");
                        return false;
                    }
                default:
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Unsupported declared type '{declaredType}' for '{header}'.");
                    return false;
            }
        }
        catch (System.Exception ex)
        {
            CustomLogger.LogError(LogSystem.CSVImporter, $"Row {csvRow}: Exception setting property '{header}': {ex.Message}");
            return false;
        }
    }
}
