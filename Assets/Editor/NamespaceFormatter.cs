using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class NamespaceAssetProcessor : AssetModificationProcessor
{
    private const string BaseNamespace = "FourFatesStudios.ProjectWarden";
    private const string CodeRoot = "Assets/Scripts";

    public static void OnWillCreateAsset(string metaPath)
    {
        if (!metaPath.EndsWith(".cs.meta")) return;

        string assetPath = metaPath.Replace(".meta", "");
        string fullPath = Path.GetFullPath(assetPath);

        EditorApplication.delayCall += () =>
        {
            if (File.Exists(fullPath))
            {
                InjectNamespace(fullPath, assetPath);
                AssetDatabase.Refresh();
            }
        };
    }

    private static void InjectNamespace(string fullPath, string assetPath)
    {
        if (!assetPath.StartsWith(CodeRoot)) return;

        string contents = File.ReadAllText(fullPath);
        if (contents.Contains("namespace")) return;

        // Extract "using" directives
        var usingMatches = Regex.Matches(contents, @"^\s*using\s.+?;\s*$", RegexOptions.Multiline);
        string usings = string.Join("\n", usingMatches.Select(m => m.Value));

        // Strip out usings from original content
        string codeWithoutUsings = Regex.Replace(contents, @"^\s*using\s.+?;\s*$", "", RegexOptions.Multiline).Trim();

        // Determine namespace from folder structure
        string relativePath = assetPath.Substring(CodeRoot.Length).TrimStart('/', '\\');
        string[] pathParts = Path.GetDirectoryName(relativePath).Split(Path.DirectorySeparatorChar);
        string namespacePath = string.Join(".", pathParts.Where(p => !string.IsNullOrEmpty(p)));
        string finalNamespace = string.IsNullOrEmpty(namespacePath)
            ? BaseNamespace
            : $"{BaseNamespace}.{namespacePath}";

        string indentedCode = IndentCode(codeWithoutUsings);

        string wrapped =
$@"{usings}
namespace {finalNamespace}
{{
{indentedCode}
}}";

        File.WriteAllText(fullPath, wrapped);
    }

    private static string IndentCode(string code)
    {
        var lines = code.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
                lines[i] = "    " + lines[i];
        }
        return string.Join("\n", lines);
    }

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
