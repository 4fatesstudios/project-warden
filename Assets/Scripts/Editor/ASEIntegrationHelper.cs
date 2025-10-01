using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Helper class for Amplify Shader Editor (ASE) integration with ingredient shapes
    /// Provides methods to export/import shape data for use in ASE nodes and custom functions
    /// </summary>
    public static class ASEIntegrationHelper
    {
        private const string ASE_EXPORT_FOLDER = "Assets/ASE_ShapeData";
        private const string ASE_FUNCTION_TEMPLATE_PATH = "Assets/Editor/Templates/ASE_ShapeFunction.template";
        
        /// <summary>
        /// Export ingredient shape as ASE-compatible texture data
        /// </summary>
        public static bool ExportShapeAsTexture(Ingredient ingredient, string filePath = null)
        {
            if (ingredient?.ShapeData == null) return false;

            try
            {
                var shapeData = ingredient.ShapeData;
                int width = shapeData.GridWidth;
                int height = shapeData.GridHeight;
                
                // Create texture representing the shape
                Texture2D shapeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                shapeTexture.filterMode = FilterMode.Point; // Pixel-perfect for grid data
                
                // Fill texture based on shape data
                Color[] pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        bool isActive = shapeData.IsCellActive(x, y);
                        int index = y * width + x;
                        
                        // Use shape color for active cells, transparent for inactive
                        pixels[index] = isActive ? shapeData.ShapeColor : Color.clear;
                    }
                }
                
                shapeTexture.SetPixels(pixels);
                shapeTexture.Apply();
                
                // Save as PNG for ASE
                if (string.IsNullOrEmpty(filePath))
                {
                    Directory.CreateDirectory(ASE_EXPORT_FOLDER);
                    filePath = Path.Combine(ASE_EXPORT_FOLDER, $"{ingredient.name}_shape.png");
                }
                
                byte[] pngData = shapeTexture.EncodeToPNG();
                File.WriteAllBytes(filePath, pngData);
                
                UnityEngine.Object.DestroyImmediate(shapeTexture);
                AssetDatabase.Refresh();
                
                Debug.Log($"[ASE Integration] Exported shape texture for '{ingredient.name}' to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ASE Integration] Failed to export shape texture: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Export ingredient shape as ASE custom function
        /// </summary>
        public static bool ExportShapeAsASEFunction(Ingredient ingredient, string functionName = null)
        {
            if (ingredient?.ShapeData == null) return false;

            try
            {
                if (string.IsNullOrEmpty(functionName))
                {
                    functionName = $"{ingredient.name}Shape";
                }

                var shapeData = ingredient.ShapeData;
                var functionCode = GenerateASEShapeFunction(shapeData, functionName);
                
                Directory.CreateDirectory(ASE_EXPORT_FOLDER);
                string filePath = Path.Combine(ASE_EXPORT_FOLDER, $"{functionName}.hlsl");
                File.WriteAllText(filePath, functionCode);
                
                AssetDatabase.Refresh();
                
                Debug.Log($"[ASE Integration] Exported ASE function '{functionName}' to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ASE Integration] Failed to export ASE function: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate HLSL code for ASE custom function
        /// </summary>
        private static string GenerateASEShapeFunction(IngredientShapeData shapeData, string functionName)
        {
            var code = new System.Text.StringBuilder();
            
            code.AppendLine($"// Auto-generated shape function for ingredient");
            code.AppendLine($"// Generated on: {DateTime.Now}");
            code.AppendLine($"// Grid Size: {shapeData.GridWidth}x{shapeData.GridHeight}");
            code.AppendLine();
            
            code.AppendLine($"void {functionName}_float(float2 UV, float2 GridSize, out float Shape)");
            code.AppendLine("{");
            code.AppendLine("    // Convert UV to grid coordinates");
            code.AppendLine($"    float2 gridCoord = floor(UV * GridSize);");
            code.AppendLine($"    float gridWidth = {shapeData.GridWidth};");
            code.AppendLine($"    float gridHeight = {shapeData.GridHeight};");
            code.AppendLine();
            code.AppendLine("    // Default to empty");
            code.AppendLine("    Shape = 0.0;");
            code.AppendLine();
            
            // Generate shape logic
            foreach (var cell in shapeData.ActiveCells)
            {
                code.AppendLine($"    if (gridCoord.x == {cell.x} && gridCoord.y == {cell.y}) Shape = 1.0;");
            }
            
            code.AppendLine("}");
            
            return code.ToString();
        }

        /// <summary>
        /// Export shape data as JSON for ASE custom nodes
        /// </summary>
        public static bool ExportShapeAsJSON(Ingredient ingredient, string filePath = null)
        {
            if (ingredient?.ShapeData == null) return false;

            try
            {
                var aseData = new ASEShapeNodeData
                {
                    ingredientName = ingredient.name,
                    gridWidth = ingredient.ShapeData.GridWidth,
                    gridHeight = ingredient.ShapeData.GridHeight,
                    activeCells = ingredient.ShapeData.ActiveCells.ToArray(),
                    color = ColorUtility.ToHtmlStringRGBA(ingredient.ShapeData.ShapeColor),
                    template = ingredient.ShapeData.Template.ToString(),
                    exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    version = "1.0"
                };

                if (string.IsNullOrEmpty(filePath))
                {
                    Directory.CreateDirectory(ASE_EXPORT_FOLDER);
                    filePath = Path.Combine(ASE_EXPORT_FOLDER, $"{ingredient.name}_ase_data.json");
                }

                string json = JsonUtility.ToJson(aseData, true);
                File.WriteAllText(filePath, json);
                
                AssetDatabase.Refresh();
                
                Debug.Log($"[ASE Integration] Exported ASE JSON data for '{ingredient.name}' to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ASE Integration] Failed to export ASE JSON: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Import shape data from ASE JSON format
        /// </summary>
        public static bool ImportShapeFromASEJSON(Ingredient ingredient, string filePath)
        {
            if (ingredient == null || !File.Exists(filePath)) return false;

            try
            {
                string json = File.ReadAllText(filePath);
                var aseData = JsonUtility.FromJson<ASEShapeNodeData>(json);

                if (aseData == null) return false;

                // Create new shape data from ASE data
                var shapeData = new IngredientShapeData();
                shapeData.ResizeGrid(aseData.gridWidth, aseData.gridHeight);
                
                // Clear and set active cells
                shapeData.ClearShape();
                foreach (var cell in aseData.activeCells)
                {
                    shapeData.SetCellActive(cell.x, cell.y, true);
                }

                // Set color if valid
                if (ColorUtility.TryParseHtmlString("#" + aseData.color, out var color))
                {
                    shapeData.SetVisualProperties(color);
                }

                // Apply template if valid
                if (Enum.TryParse<ShapeTemplate>(aseData.template, out var template))
                {
                    shapeData.ApplyTemplate(template);
                }

                // Update ingredient
                ingredient.SetShape(shapeData.ToBoolArray());
                
                Debug.Log($"[ASE Integration] Imported shape data for '{ingredient.name}' from ASE JSON");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ASE Integration] Failed to import from ASE JSON: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create ASE-compatible material preview for ingredient shapes
        /// </summary>
        public static Material CreateShapePreviewMaterial(Ingredient ingredient)
        {
            if (ingredient?.ShapeData == null) return null;

            try
            {
                // Create a simple unlit material for preview
                Shader shader = Shader.Find("Unlit/Texture");
                Material material = new Material(shader);
                
                // Export shape as texture and assign to material
                string tempPath = Path.Combine(Application.temporaryCachePath, $"{ingredient.name}_preview.png");
                if (ExportShapeAsTexture(ingredient, tempPath))
                {
                    var texture = new Texture2D(2, 2);
                    byte[] data = File.ReadAllBytes(tempPath);
                    texture.LoadImage(data);
                    material.mainTexture = texture;
                }

                return material;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ASE Integration] Failed to create preview material: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Batch export all ingredients for ASE
        /// </summary>
        public static void BatchExportForASE()
        {
            var ingredients = GetAllIngredients();
            int exportCount = 0;

            foreach (var ingredient in ingredients)
            {
                if (ingredient.ShapeData != null)
                {
                    if (ExportShapeAsTexture(ingredient) && 
                        ExportShapeAsJSON(ingredient) && 
                        ExportShapeAsASEFunction(ingredient))
                    {
                        exportCount++;
                    }
                }
            }

            Debug.Log($"[ASE Integration] Batch exported {exportCount} ingredients for ASE");
        }

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

        // Menu items for easy access
        [MenuItem("Tools/Alchemy System/ASE Integration/Export All for ASE")]
        public static void MenuBatchExportForASE()
        {
            BatchExportForASE();
        }

        [MenuItem("Tools/Alchemy System/ASE Integration/Open ASE Export Folder")]
        public static void MenuOpenASEFolder()
        {
            Directory.CreateDirectory(ASE_EXPORT_FOLDER);
            EditorUtility.RevealInFinder(ASE_EXPORT_FOLDER);
        }
    }

    /// <summary>
    /// Data structure for ASE node integration
    /// </summary>
    [System.Serializable]
    public class ASEShapeNodeData
    {
        public string ingredientName;
        public int gridWidth;
        public int gridHeight;
        public Vector2Int[] activeCells;
        public string color;
        public string template;
        public string exportTime;
        public string version;
    }
}