using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.Editor
{
    [CustomEditor(typeof(Ingredient))]
    public class IngredientEditor : UnityEditor.Editor
    {
        private bool showAlchemyProperties = true;
        private bool showRefinementOptions = true;
        private bool showEffects = true;
        private bool showPreview = true;
        
        private Texture2D gridPreviewTexture;
        private Color[] gridColors;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            Ingredient ingredient = (Ingredient)target;
            
            DrawHeader(ingredient);
            EditorGUILayout.Space();
            
            DrawBasicProperties();
            EditorGUILayout.Space();
            
            DrawAlchemyProperties(ingredient);
            EditorGUILayout.Space();
            
            DrawRefinementOptions(ingredient);
            EditorGUILayout.Space();
            
            DrawEffectsSection();
            EditorGUILayout.Space();
            
            DrawPreviewSection(ingredient);
            EditorGUILayout.Space();
            
            DrawUtilityButtons(ingredient);
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(Ingredient ingredient)
        {
            EditorGUILayout.BeginVertical("Box");
            
            // Title with icon
            EditorGUILayout.BeginHorizontal();
            if (ingredient.ItemIcon != null)
            {
                GUILayout.Label(ingredient.ItemIcon.texture, GUILayout.Width(64), GUILayout.Height(64));
            }
            else
            {
                GUILayout.Box("No Icon", GUILayout.Width(64), GUILayout.Height(64));
            }
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(ingredient.name, EditorStyles.largeLabel);
            EditorGUILayout.LabelField($"Type: {ingredient.IngredientArchetype}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Aspect: {ingredient.IngredientAspect}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawBasicProperties()
        {
            EditorGUILayout.LabelField("Basic Properties", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDescription"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemRarity"));
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ingredientArchetype"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ingredientAspect"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isCorrupted"));
        }

        private void DrawAlchemyProperties(Ingredient ingredient)
        {
            showAlchemyProperties = EditorGUILayout.Foldout(showAlchemyProperties, "Alchemy Properties", true);
            
            if (showAlchemyProperties)
            {
                EditorGUILayout.BeginVertical("Box");
                
                // Potency with helpful description
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("potency"));
                EditorGUILayout.LabelField(GetPotencyDescription(ingredient.Potency), EditorStyles.miniLabel, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
                
                // Grid size with visual preview
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridWidth"), new GUIContent("Grid Width"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridHeight"), new GUIContent("Grid Height"));
                EditorGUILayout.EndHorizontal();
                
                // Draw grid preview
                DrawGridPreview(ingredient.GridWidth, ingredient.GridHeight);
                
                // Space unlocking
                EditorGUILayout.PropertyField(serializedObject.FindProperty("unlocksAdditionalSpace"));
                if (ingredient.UnlocksAdditionalSpace)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("additionalSpaceCount"));
                    EditorGUILayout.HelpBox($"This ingredient will unlock {ingredient.AdditionalSpaceCount} additional grid spaces when placed.", MessageType.Info);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawRefinementOptions(Ingredient ingredient)
        {
            showRefinementOptions = EditorGUILayout.Foldout(showRefinementOptions, "Refinement Options", true);
            
            if (showRefinementOptions)
            {
                EditorGUILayout.BeginVertical("Box");
                
                // Grinding
                DrawRefinementOption("Grinding (Mortar & Pestle)", "canGrind", "grindingResult", ingredient.CanGrind);
                
                EditorGUILayout.Space();
                
                // Distillation
                DrawRefinementOption("Distillation (Distillation Column)", "canDistill", "distillingResult", ingredient.CanDistill);
                
                EditorGUILayout.Space();
                
                // Roasting
                DrawRefinementOption("Roasting (Frying Pan)", "canRoast", "roastingResult", ingredient.CanRoast);
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawRefinementOption(string label, string canProperty, string resultProperty, bool isEnabled)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(canProperty));
            
            if (isEnabled)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(resultProperty));
                
                if (GUILayout.Button($"Auto-Create {label.Split(' ')[0]} Result"))
                {
                    CreateRefinementResult(canProperty, resultProperty);
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawEffectsSection()
        {
            showEffects = EditorGUILayout.Foldout(showEffects, "Effects & Infusions", true);
            
            if (showEffects)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("infusions"));
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPreviewSection(Ingredient ingredient)
        {
            showPreview = EditorGUILayout.Foldout(showPreview, "Preview", true);
            
            if (showPreview)
            {
                EditorGUILayout.BeginVertical("Box");
                
                // Component preview if this is an AlchemyComponent
                if (ingredient is AlchemyComponent component)
                {
                    DrawComponentPreview(component);
                }
                
                // Usage preview
                DrawUsagePreview(ingredient);
                
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawComponentPreview(AlchemyComponent component)
        {
            EditorGUILayout.LabelField("Component Information", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (component.BaseIngredient1 != null)
            {
                DrawIngredientMini(component.BaseIngredient1, "Base 1");
            }
            
            EditorGUILayout.LabelField("+", GUILayout.Width(20));
            
            if (component.BaseIngredient2 != null)
            {
                DrawIngredientMini(component.BaseIngredient2, "Base 2");
            }
            
            EditorGUILayout.LabelField("=", GUILayout.Width(20));
            
            DrawIngredientMini(component, "Result");
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField($"Calculated Potency: {component.Potency}");
            EditorGUILayout.LabelField($"Calculated Size: {component.GridWidth}x{component.GridHeight}");
        }

        private void DrawIngredientMini(Ingredient ingredient, string label)
        {
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(80));
            EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel);
            
            if (ingredient.ItemIcon != null)
            {
                GUILayout.Label(ingredient.ItemIcon.texture, GUILayout.Width(60), GUILayout.Height(60));
            }
            else
            {
                GUILayout.Box("No Icon", GUILayout.Width(60), GUILayout.Height(60));
            }
            
            EditorGUILayout.LabelField(ingredient.ItemName, EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField($"P:{ingredient.Potency}", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawUsagePreview(Ingredient ingredient)
        {
            EditorGUILayout.LabelField("Usage Information", EditorStyles.boldLabel);
            
            // Find references to this ingredient
            string[] references = FindIngredientReferences(ingredient);
            if (references.Length > 0)
            {
                EditorGUILayout.LabelField($"Used in {references.Length} recipes:");
                foreach (string reference in references)
                {
                    EditorGUILayout.LabelField($"• {reference}", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Not used in any recipes yet", EditorStyles.miniLabel);
            }
        }

        private void DrawGridPreview(int width, int height)
        {
            if (width <= 0 || height <= 0) return;
            
            const int cellSize = 20;
            Rect gridRect = GUILayoutUtility.GetRect(width * cellSize, height * cellSize);
            
            // Draw grid cells
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Rect cellRect = new Rect(
                        gridRect.x + x * cellSize,
                        gridRect.y + y * cellSize,
                        cellSize - 1,
                        cellSize - 1
                    );
                    
                    EditorGUI.DrawRect(cellRect, new Color(0.3f, 0.7f, 1f, 0.5f));
                    EditorGUI.DrawRect(cellRect, Color.black);
                }
            }
            
            EditorGUILayout.LabelField($"Grid Size: {width}x{height} cells", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawUtilityButtons(Ingredient ingredient)
        {
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Variant"))
            {
                CreateIngredientVariant(ingredient);
            }
            
            if (GUILayout.Button("Find Recipes"))
            {
                FindRecipesUsingIngredient(ingredient);
            }
            
            if (GUILayout.Button("Test in Scene"))
            {
                TestIngredientInScene(ingredient);
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private string GetPotencyDescription(int potency)
        {
            return potency switch
            {
                1 => "Weak",
                2 => "Mild",
                3 => "Moderate",
                4 => "Strong",
                5 => "Potent",
                _ => "Unknown"
            };
        }

        private void CreateRefinementResult(string canProperty, string resultProperty)
        {
            // Implementation for auto-creating refinement results
            EditorUtility.DisplayDialog("Auto-Create", "This feature would automatically create a refined version of the ingredient.", "OK");
        }

        private void CreateIngredientVariant(Ingredient ingredient)
        {
            // Implementation for creating ingredient variants
            EditorUtility.DisplayDialog("Create Variant", "This feature would create a variant of the ingredient with modified properties.", "OK");
        }

        private void FindRecipesUsingIngredient(Ingredient ingredient)
        {
            // Implementation for finding recipes
            EditorUtility.DisplayDialog("Find Recipes", $"This feature would search for all recipes using {ingredient.name}.", "OK");
        }

        private void TestIngredientInScene(Ingredient ingredient)
        {
            // Implementation for testing in scene
            EditorUtility.DisplayDialog("Test in Scene", $"This feature would add {ingredient.name} to the current scene for testing.", "OK");
        }

        private string[] FindIngredientReferences(Ingredient ingredient)
        {
            // Placeholder - would search through all recipes and return references
            return new string[0];
        }
    }
}