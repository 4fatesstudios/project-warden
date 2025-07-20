using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects;

public class CSVToSOImporter : EditorWindow
{
    private TextAsset csvFile;
    private MonoScript scriptableObjectType;
    private string outputFolder = "Assets/Data/ScriptableObjects/";

    [MenuItem("Tools/CSV Importer")]
    public static void ShowWindow()
    {
        GetWindow<CSVToSOImporter>("CSV to ScriptableObject");
    }

    void OnGUI()
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

    void ImportCSV()
    {
        if (csvFile == null || scriptableObjectType == null) {
            Debug.LogError("CSV file and SO type must be assigned.");
            return;
        }

        System.Type type = scriptableObjectType.GetClass();
        if (!typeof(BaseDataSO).IsAssignableFrom(type)) {
            Debug.LogError("Type must inherit from BaseDataSO.");
            return;
        }

        string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) {
            Debug.LogWarning("CSV file is empty or missing rows.");
            return;
        }

        string[] headers = lines[0].Split(',');

        if (!headers.Contains("ID")) {
            Debug.LogError("CSV must include an 'ID' column.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            string id = values[System.Array.IndexOf(headers, "ID")];
            string assetPath = $"{outputFolder}/{id}.asset";

            BaseDataSO instance = AssetDatabase.LoadAssetAtPath(assetPath, type) as BaseDataSO;
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance(type) as BaseDataSO;
                instance.ID = id;
                AssetDatabase.CreateAsset(instance, assetPath);
            }

            for (int j = 0; j < headers.Length; j++)
            {
                string header = headers[j];
                string value = values[j];

                FieldInfo field = type.GetField(header, BindingFlags.Public | BindingFlags.Instance);
                if (field == null) continue;

                object parsed = ParseValue(value, field.FieldType);
                if (parsed != null)
                    field.SetValue(instance, parsed);
            }

            EditorUtility.SetDirty(instance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CSV Import complete.");
    }

    object ParseValue(string value, System.Type fieldType)
    {
        try
        {
            if (fieldType == typeof(string)) return value;
            if (fieldType == typeof(int)) return int.Parse(value);
            if (fieldType == typeof(float)) return float.Parse(value);
            if (fieldType == typeof(bool)) return bool.Parse(value);
            if (fieldType.IsEnum) return System.Enum.Parse(fieldType, value);
        }
        catch
        {
            Debug.LogWarning($"Failed to parse '{value}' as {fieldType.Name}");
        }
        return null;
    }
}
