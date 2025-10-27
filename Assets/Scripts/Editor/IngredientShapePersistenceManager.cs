using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Manager for handling ingredient shape persistence and integration with external tools
    /// Provides APIs for ASE (Amplify Shader Editor) and other visual design tools
    /// </summary>
    public static class IngredientShapePersistenceManager
    {
        private const string SHAPE_DATA_FOLDER = "Assets/Data/IngredientShapes";
        private const string TEMP_EXPORT_FOLDER = "Assets/Temp/ShapeExports";
        
        // Events for external tool integration
        public static System.Action<Ingredient, IngredientShapeData> OnShapeDataChanged;
        public static System.Action<string> OnShapeExported;
        public static System.Action<string> OnShapeImported;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // Ensure required directories exist
            if (!Directory.Exists(SHAPE_DATA_FOLDER))
            {
                Directory.CreateDirectory(SHAPE_DATA_FOLDER);
            }
            
            if (!Directory.Exists(TEMP_EXPORT_FOLDER))
            {
                Directory.CreateDirectory(TEMP_EXPORT_FOLDER);
            }
        }

        /// <summary>
        /// Export ingredient shape data to a file for external tools
        /// </summary>
        public static bool ExportShapeToFile(Ingredient ingredient, string filePath = null)
        {
            if (ingredient == null) return false;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = Path.Combine(TEMP_EXPORT_FOLDER, $"{ingredient.name}_shape_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                }

                string shapeData = ingredient.ExportShapeData();
                File.WriteAllText(filePath, shapeData);
                
                OnShapeExported?.Invoke(filePath);
                AssetDatabase.Refresh();
                
                Debug.Log($"[Shape Persistence] Exported shape data for '{ingredient.name}' to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Shape Persistence] Failed to export shape data for '{ingredient.name}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Import ingredient shape data from a file (for external tools)
        /// </summary>
        public static bool ImportShapeFromFile(Ingredient ingredient, string filePath)
        {
            if (ingredient == null || !File.Exists(filePath)) return false;

            try
            {
                string shapeData = File.ReadAllText(filePath);
                bool success = ingredient.ImportShapeData(shapeData);
                
                if (success)
                {
                    OnShapeImported?.Invoke(filePath);
                    OnShapeDataChanged?.Invoke(ingredient, ingredient.ShapeData);
                    
                    Debug.Log($"[Shape Persistence] Imported shape data for '{ingredient.name}' from: {filePath}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Shape Persistence] Failed to import shape data for '{ingredient.name}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create a backup of all ingredient shapes in the project
        /// </summary>
        public static void BackupAllShapes()
        {
            var ingredients = GetAllIngredients();
            var backupFolder = Path.Combine(SHAPE_DATA_FOLDER, $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(backupFolder);

            int successCount = 0;
            foreach (var ingredient in ingredients)
            {
                if (ingredient.ShapeData != null)
                {
                    string fileName = $"{ingredient.name}_shape.json";
                    string filePath = Path.Combine(backupFolder, fileName);
                    
                    if (ExportShapeToFile(ingredient, filePath))
                    {
                        successCount++;
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Shape Persistence] Backed up {successCount} ingredient shapes to: {backupFolder}");
        }

        /// <summary>
        /// Restore shapes from a backup folder
        /// </summary>
        public static void RestoreShapesFromBackup(string backupFolderPath)
        {
            if (!Directory.Exists(backupFolderPath)) return;

            var jsonFiles = Directory.GetFiles(backupFolderPath, "*.json");
            var ingredients = GetAllIngredients();
            var ingredientDict = new Dictionary<string, Ingredient>();
            
            foreach (var ingredient in ingredients)
            {
                ingredientDict[ingredient.name] = ingredient;
            }

            int restoredCount = 0;
            foreach (var jsonFile in jsonFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(jsonFile);
                string ingredientName = fileName.Replace("_shape", "");
                
                if (ingredientDict.TryGetValue(ingredientName, out var ingredient))
                {
                    if (ImportShapeFromFile(ingredient, jsonFile))
                    {
                        restoredCount++;
                    }
                }
            }

            Debug.Log($"[Shape Persistence] Restored {restoredCount} ingredient shapes from backup.");
        }

        /// <summary>
        /// Get all ingredient assets in the project
        /// </summary>
        private static List<Ingredient> GetAllIngredients()
        {
            var ingredients = new List<Ingredient>();
            var guids = AssetDatabase.FindAssets("t:Ingredient");
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(path);
                if (ingredient != null)
                {
                    ingredients.Add(ingredient);
                }
            }
            
            return ingredients;
        }

        /// <summary>
        /// Convert shape data to a format compatible with external tools
        /// </summary>
        public static ExternalToolShapeData ConvertToExternalFormat(IngredientShapeData shapeData)
        {
            if (shapeData == null) return null;

            return new ExternalToolShapeData
            {
                width = shapeData.GridWidth,
                height = shapeData.GridHeight,
                cells = shapeData.ActiveCells.ToArray(),
                template = shapeData.Template.ToString(),
                color = ColorUtility.ToHtmlStringRGBA(shapeData.ShapeColor),
                metadata = new Dictionary<string, object>
                {
                    ["lastModified"] = shapeData.LastModified,
                    ["version"] = shapeData.DataVersion,
                    ["tool"] = "AlchemySystemEditor"
                }
            };
        }

        /// <summary>
        /// Convert external tool format back to ingredient shape data
        /// </summary>
        public static bool ConvertFromExternalFormat(ExternalToolShapeData externalData, out IngredientShapeData shapeData)
        {
            shapeData = null;
            if (externalData == null) return false;

            try
            {
                shapeData = new IngredientShapeData();
                shapeData.ResizeGrid(externalData.width, externalData.height);
                
                // Clear and set active cells
                shapeData.ClearShape();
                foreach (var cell in externalData.cells)
                {
                    shapeData.SetCellActive(cell.x, cell.y, true);
                }

                // Set color if valid
                if (ColorUtility.TryParseHtmlString("#" + externalData.color, out var color))
                {
                    shapeData.SetVisualProperties(color);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Shape Persistence] Failed to convert external format: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Watch for changes to ingredient shape files and auto-import
        /// </summary>
        public static void EnableAutoImport(bool enable)
        {
            if (enable)
            {
                FileSystemWatcher watcher = new FileSystemWatcher(TEMP_EXPORT_FOLDER, "*.json");
                watcher.Changed += OnShapeFileChanged;
                watcher.EnableRaisingEvents = true;
            }
        }

        private static void OnShapeFileChanged(object sender, FileSystemEventArgs e)
        {
            // Auto-import logic can be implemented here
            Debug.Log($"[Shape Persistence] Shape file changed: {e.FullPath}");
        }

        /// <summary>
        /// Generate shape templates for common patterns
        /// </summary>
        public static void GenerateCommonTemplates()
        {
            var templates = new Dictionary<string, IngredientShapeData>
            {
                ["1x1_dot"] = new IngredientShapeData(1, 1, ShapeTemplate.Rectangle),
                ["2x2_square"] = new IngredientShapeData(2, 2, ShapeTemplate.Rectangle),
                ["3x1_line"] = new IngredientShapeData(3, 1, ShapeTemplate.Rectangle),
                ["1x3_line"] = new IngredientShapeData(1, 3, ShapeTemplate.Rectangle),
                ["3x3_cross"] = new IngredientShapeData(3, 3, ShapeTemplate.Cross),
                ["3x3_L"] = new IngredientShapeData(3, 3, ShapeTemplate.LShape),
                ["3x3_T"] = new IngredientShapeData(3, 3, ShapeTemplate.TShape),
                ["4x4_circle"] = new IngredientShapeData(4, 4, ShapeTemplate.Circle),
                ["4x4_diamond"] = new IngredientShapeData(4, 4, ShapeTemplate.Diamond)
            };

            string templatesFolder = Path.Combine(SHAPE_DATA_FOLDER, "Templates");
            Directory.CreateDirectory(templatesFolder);

            foreach (var template in templates)
            {
                string filePath = Path.Combine(templatesFolder, $"{template.Key}.json");
                string data = template.Value.ExportAsString();
                File.WriteAllText(filePath, data);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Shape Persistence] Generated {templates.Count} shape templates in: {templatesFolder}");
        }

        // Menu items for easy access
        [MenuItem("Tools/Alchemy System/Shape Persistence/Backup All Shapes")]
        public static void MenuBackupAllShapes()
        {
            BackupAllShapes();
        }

        [MenuItem("Tools/Alchemy System/Shape Persistence/Generate Templates")]
        public static void MenuGenerateTemplates()
        {
            GenerateCommonTemplates();
        }

        [MenuItem("Tools/Alchemy System/Shape Persistence/Open Shape Data Folder")]
        public static void MenuOpenShapeDataFolder()
        {
            EditorUtility.RevealInFinder(SHAPE_DATA_FOLDER);
        }
    }

    /// <summary>
    /// Data structure for external tool integration
    /// </summary>
    [System.Serializable]
    public class ExternalToolShapeData
    {
        public int width;
        public int height;
        public Vector2Int[] cells;
        public string template;
        public string color;
        public Dictionary<string, object> metadata;
    }
}