#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class EditorConfigUtility
{
    private static EditorConfig _cachedConfig;
    private const string DefaultPath = "Assets/Configs/EditorConfig.asset";

    public static EditorConfig GetConfig()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        // Try to locate an existing config
        string[] guids = AssetDatabase.FindAssets("t:EditorConfig");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedConfig = AssetDatabase.LoadAssetAtPath<EditorConfig>(path);
            return _cachedConfig;
        }

        // None found — create one automatically
        _cachedConfig = CreateDefaultConfig();
        return _cachedConfig;
    }

    private static EditorConfig CreateDefaultConfig()
    {
        var config = ScriptableObject.CreateInstance<EditorConfig>();

        // Ensure directory exists
        string dir = Path.GetDirectoryName(DefaultPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        AssetDatabase.CreateAsset(config, DefaultPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[EditorConfigUtility] Created default EditorConfig at {DefaultPath}");
        return config;
    }
}
#endif