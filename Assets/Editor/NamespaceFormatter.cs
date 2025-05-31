using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public static class NamespaceFormatter
{
    private const string BaseNamespace = "4FatesStudios.ProjectWarden";
    private const string CodeRoot = "Assets/Scripts";

    // Auto-inject namespace when script is created
    public static void OnWillCreateAsset(string metaPath)
    {
        if (!metaPath.EndsWith(".cs.meta")) return;

        string assetPath = metaPath.Replace(".meta", "");
        string fullPath = Path.GetFullPath(assetPath);

        InjectNamespace(fullPath, assetPath);
    }

    // Inject namespace into a file if not already present
    private static void InjectNamespace(string fullPath, string assetPath)
    {
        if (!assetPath.StartsWith(CodeRoot)) return;
        if (!File.Exists(fullPath)) return;

        string contents = File.ReadAllText(fullPath);
        if (contents.Contains("namespace")) return; // Skip if already namespaced

        string relativePath = assetPath.Substring(CodeRoot.Length).TrimStart('/', '\\');
        string[] pathParts = Path.GetDirectoryName(relativePath).Split(Path.DirectorySeparatorChar);
        string namespacePath = string.Join(".", pathParts.Where(p => !string.IsNullOrEmpty(p)));

        string finalNamespace = string.IsNullOrEmpty(namespacePath)
            ? BaseNamespace
            : $"{BaseNamespace}.{namespacePath}";

        string indentedCode = IndentCode(contents);
        string wrapped =
$@"namespace {finalNamespace}
{{
{indentedCode}
}}";

        File.WriteAllText(fullPath, wrapped);
    }

    private static string IndentCode(string code)
    {
        string[] lines = code.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = "    " + lines[i];
        }
        return string.Join("\n", lines);
    }

    // Context menu to batch-fix selected scripts
    [MenuItem("Assets/Fix Namespaces on Selected Scripts", priority = 1000)]
    public static void FixSelectedNamespaces()
    {
        var paths = Selection.assetGUIDs
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".cs") && p.StartsWith(CodeRoot))
            .ToArray();

        foreach (string assetPath in paths)
        {
            string fullPath = Path.GetFullPath(assetPath);
            string contents = File.ReadAllText(fullPath);
            if (contents.Contains("namespace")) continue;

            InjectNamespace(fullPath, assetPath);
        }

        AssetDatabase.Refresh();
        Debug.Log($"[NamespaceFormatter] Fixed namespaces for {paths.Length} scripts.");
    }
}
