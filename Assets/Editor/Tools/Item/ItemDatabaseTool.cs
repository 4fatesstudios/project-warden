using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEditor;
using UnityEngine;

public class ItemDatabaseTool : MonoBehaviour {
    
    [MenuItem("Tools/Items/Update All Item Databases")]
    public static void UpdateAllDatabases()
    {
        const string DatabaseFolder = "Assets/Resources/Databases/Items/";

        if (!Directory.Exists(DatabaseFolder))
            Directory.CreateDirectory(DatabaseFolder);

        string[] guids = AssetDatabase.FindAssets("t:ItemDatabase", new[] { DatabaseFolder });

        if (guids.Length == 0)
        {
            Debug.LogWarning("No ItemDatabase assets found in the expected folder.");
            return;
        }

        var databases = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemDatabase>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(db => db != null)
            .ToList();

        foreach (var db in databases)
        {
            AutoPopulate(db);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Updated {databases.Count} item databases.");
    }

    private static void AutoPopulate(ItemDatabase db)
    {
        var itemType = db.ItemType;
        var searchPath = $"Assets/Resources/Items/{itemType.Name}s";

        string[] guids = AssetDatabase.FindAssets($"t:{itemType.Name}", new[] { searchPath });
        var listType = typeof(List<>).MakeGenericType(itemType);
        var items = (IList)System.Activator.CreateInstance(listType);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(path);
            if (item == null)
            {
                Debug.LogWarning($"Skipped null item at {path}");
                continue;
            }

            if (!itemType.IsAssignableFrom(item.GetType()))
            {
                Debug.LogWarning($"Item '{item.name}' at {path} is not assignable to type {itemType.Name}");
                continue;
            }

            items.Add(item);
        }

        var setItemsMethod = db.GetType().GetMethod("SetItems");
        if (setItemsMethod == null)
        {
            Debug.LogError($"SetItems method not found on {db.name} (type {db.GetType().Name})");
            return;
        }

        var expectedParamType = setItemsMethod.GetParameters().FirstOrDefault()?.ParameterType;
        if (expectedParamType == null || !expectedParamType.IsAssignableFrom(items.GetType()))
        {
            Debug.LogError($"SetItems parameter type mismatch on {db.name}. Expected {expectedParamType?.Name}, got {items.GetType().Name}");
            return;
        }

        try
        {
            setItemsMethod.Invoke(db, new object[] { items });
            EditorUtility.SetDirty(db);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to set items on {db.name}: {ex.Message}");
        }
    }

}
