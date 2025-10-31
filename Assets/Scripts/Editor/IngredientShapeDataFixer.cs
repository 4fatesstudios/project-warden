using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class IngredientShapeDataDiagnostic : EditorWindow
    {
        [MenuItem("Tools/Ingredient Shape Data Diagnostic")]
        public static void ShowWindow()
        {
            GetWindow<IngredientShapeDataDiagnostic>("Ingredient Shape Data Diagnostic");
        }

        private void OnGUI()
        {
            GUILayout.Label("Ingredient Shape Data Diagnostic", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("This tool provides diagnostic information about ingredient shape data. All auto-fixing is now handled automatically by the IngredientShapeData class.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("List Ingredients and Their Shape Status"))
            {
                ListIngredientShapeStatus();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Force Validate All Ingredient Shape Data"))
            {
                ForceValidateAllIngredients();
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("Note: Shape data is now auto-fixed when accessed. Manual fixing is usually not needed.", MessageType.Info);
        }
        
        private static void ForceValidateAllIngredients()
        {
            string[] ingredientGuids = AssetDatabase.FindAssets("t:Ingredient");
            int processedCount = 0;
            int totalCount = ingredientGuids.Length;

            Debug.Log($"Force validating {totalCount} ingredient assets");

            for (int i = 0; i < ingredientGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(ingredientGuids[i]);
                Ingredient ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(path);

                if (ingredient != null)
                {
                    // Force validation by accessing shape data
                    if (ingredient.ShapeData != null)
                    {
                        ingredient.ShapeData.ValidateAndFix();
                        EditorUtility.SetDirty(ingredient);
                        processedCount++;
                    }
                }

                EditorUtility.DisplayProgressBar("Validating Ingredient Shape Data", 
                    $"Processing {ingredient?.name ?? "unknown"}", 
                    (float)i / totalCount);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Force validated {processedCount} out of {totalCount} ingredients");
            EditorUtility.DisplayDialog("Validation Complete", 
                $"Force validated {processedCount} out of {totalCount} ingredients.\nCheck the console for details.", "OK");
        }
        
        private static void ListIngredientShapeStatus()
        {
            string[] ingredientGuids = AssetDatabase.FindAssets("t:Ingredient");
            Debug.Log($"=== Ingredient Shape Data Status Report ===");
            
            foreach (string guid in ingredientGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Ingredient ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(path);
                
                if (ingredient != null)
                {
                    string status = "OK";
                    string details = "";
                    
                    if (ingredient.ShapeData == null)
                    {
                        status = "MISSING SHAPE DATA (will auto-create)";
                    }
                    else
                    {
                        var offsets = ingredient.ShapeData.GetOccupiedOffsets();
                        if (offsets == null || offsets.Length == 0)
                        {
                            status = "MISSING OCCUPIED OFFSETS (will auto-fix)";
                        }
                        else
                        {
                            details = $"({offsets.Length} offsets)";
                            if (ingredient.ShapeData.IsValid())
                            {
                                status = "VALID";
                            }
                            else
                            {
                                status = "NEEDS AUTO-FIX";
                            }
                        }
                    }
                    
                    Debug.Log($"{ingredient.name}: {status} {details}");
                }
            }
            
            Debug.Log($"=== End Report ===");
        }
    }
}