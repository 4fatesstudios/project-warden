/* COMMENTED OUT - Template Setup (Development Component)
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using GameSystems.CraftingMenu.AlchemyBookMenu;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Editor utility to set up AlchemyBook with the entry template - DISABLED FOR PRODUCTION
    /// </summary>
    public class AlchemyBookTemplateSetup : EditorWindow
    {
        [MenuItem("Tools/Alchemy Book Template Setup")]
        public static void ShowWindow()
        {
            GetWindow<AlchemyBookTemplateSetup>("Alchemy Book Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Alchemy Book Template Setup", EditorStyles.boldLabel);
        
            EditorGUILayout.Space();
        
            if (GUILayout.Button("Auto-Setup AlchemyBook Template"))
            {
                SetupAlchemyBookTemplate();
            }
        
            if (GUILayout.Button("Find AlchemyBook in Scene"))
            {
                var alchemyBook = FindFirstObjectByType<AlchemyBook>();
                if (alchemyBook != null)
                {
                    Selection.activeGameObject = alchemyBook.gameObject;
                    Debug.Log($"✅ Found AlchemyBook on {alchemyBook.gameObject.name}");
                }
                else
                {
                    Debug.LogWarning("❌ No AlchemyBook found in scene!");
                }
            }
        
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This will automatically assign the entry template and style sheet to any AlchemyBook components in the scene.", MessageType.Info);
        }

        private void SetupAlchemyBookTemplate()
        {
            // Load the template assets
            var entryTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/AlchemyBookEntry.uxml");
            var entryStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/Styles/AlchemyBookEntry.uss");
            var bookUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/UI/CraftingSystem/BookSystem/AlchemyBook.uxml");
            var bookStyles = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/UI/CraftingSystem/BookSystem/AlchemyBookStyles.uss");

            if (entryTemplate == null)
            {
                Debug.LogError("❌ Could not find AlchemyBookEntry.uxml template!");
                return;
            }

            // Find all AlchemyBook components in the scene
            var alchemyBooks = FindObjectsByType<AlchemyBook>(FindObjectsSortMode.None);
            int setupCount = 0;

            foreach (var book in alchemyBooks)
            {
                // Set the entry template
                var serializedObject = new SerializedObject(book);
                var entryTemplateProp = serializedObject.FindProperty("entryTemplate");
                var bookStyleSheetProp = serializedObject.FindProperty("bookStyleSheet");

                if (entryTemplateProp != null)
                {
                    entryTemplateProp.objectReferenceValue = entryTemplate;
                    Debug.Log($"✅ Set entry template for {book.gameObject.name}");
                }

                if (bookStyleSheetProp != null && bookStyles != null)
                {
                    bookStyleSheetProp.objectReferenceValue = bookStyles;
                    Debug.Log($"✅ Set book style sheet for {book.gameObject.name}");
                }

                // Set up UIDocument if available
                var uiDocument = book.GetComponent<UIDocument>();
                if (uiDocument != null)
                {
                    var uiSerializedObject = new SerializedObject(uiDocument);
                    var visualTreeAssetProp = uiSerializedObject.FindProperty("m_VisualTreeAsset");

                    if (visualTreeAssetProp != null && bookUXML != null)
                    {
                        visualTreeAssetProp.objectReferenceValue = bookUXML;
                        Debug.Log($"✅ Set UXML file for {book.gameObject.name}");
                    }

                    // Add entry styles to the document
                    if (entryStyles != null)
                    {
                        var styleSheetsProp = uiSerializedObject.FindProperty("m_StyleSheets");
                        if (styleSheetsProp != null)
                        {
                            // Check if the style sheet is already added
                            bool hasEntryStyles = false;
                            for (int i = 0; i < styleSheetsProp.arraySize; i++)
                            {
                                if (styleSheetsProp.GetArrayElementAtIndex(i).objectReferenceValue == entryStyles)
                                {
                                    hasEntryStyles = true;
                                    break;
                                }
                            }

                            if (!hasEntryStyles)
                            {
                                styleSheetsProp.arraySize++;
                                var newElement = styleSheetsProp.GetArrayElementAtIndex(styleSheetsProp.arraySize - 1);
                                newElement.objectReferenceValue = entryStyles;
                                Debug.Log($"✅ Added entry styles to {book.gameObject.name}");
                            }
                        }
                    }

                    uiSerializedObject.ApplyModifiedProperties();
                }

                serializedObject.ApplyModifiedProperties();
                setupCount++;
            }

            if (setupCount > 0)
            {
                Debug.Log($"🎉 Successfully set up {setupCount} AlchemyBook component(s)!");
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogWarning("❌ No AlchemyBook components found in the scene!");
            }
        }
    }
}
END COMMENTED OUT - Template Setup */