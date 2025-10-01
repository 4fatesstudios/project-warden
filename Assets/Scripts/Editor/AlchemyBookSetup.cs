using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using GameSystems.CraftingMenu.AlchemyBookMenu;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Setup utility for creating Alchemy Book GameObjects in the scene
    /// </summary>
    public class AlchemyBookSetup : EditorWindow
    {
        [MenuItem("Tools/Project Warden/Setup Alchemy Book")]
        public static void ShowWindow()
        {
            GetWindow<AlchemyBookSetup>("Alchemy Book Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Alchemy Book Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This will create an Alchemy Book GameObject in your scene with all necessary components configured.",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Alchemy Book GameObject", GUILayout.Height(30)))
            {
                CreateAlchemyBookGameObject();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup Book in Scene Root", GUILayout.Height(30)))
            {
                SetupBookInSceneRoot();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Clean Up Old Alchemy Book Files", GUILayout.Height(30)))
            {
                CleanUpOldFiles();
            }
        }

        private static void CreateAlchemyBookGameObject()
        {
            // Create the main GameObject
            GameObject bookGO = new GameObject("AlchemyBook");
            
            // Add UIDocument component
            UIDocument uiDocument = bookGO.AddComponent<UIDocument>();
            
            // Load the UXML and USS assets from the existing system
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/UI/CraftingSystem/BookSystem/AlchemyBook.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/UI/CraftingSystem/BookSystem/AlchemyBookStyles.uss");
            
            if (uxml != null)
            {
                uiDocument.visualTreeAsset = uxml;
            }
            else
            {
                Debug.LogWarning("Could not find AlchemyBook.uxml at Assets/Scripts/UI/CraftingSystem/BookSystem/AlchemyBook.uxml");
            }

            // Add AlchemyBook component from the existing system
            AlchemyBook alchemyBook = bookGO.AddComponent<AlchemyBook>();
            
            // Set the UIDocument reference
            var uiDocumentField = typeof(AlchemyBook).GetField("uiDocument");
            if (uiDocumentField != null)
            {
                uiDocumentField.SetValue(alchemyBook, uiDocument);
            }

            // Set the stylesheet reference
            var bookStyleSheetField = typeof(AlchemyBook).GetField("bookStyleSheet");
            if (bookStyleSheetField != null && uss != null)
            {
                bookStyleSheetField.SetValue(alchemyBook, uss);
            }

            // Position the GameObject
            bookGO.transform.position = Vector3.zero;

            // Start with the book inactive
            bookGO.SetActive(false);

            // Select the GameObject
            Selection.activeGameObject = bookGO;

            Debug.Log($"✅ Created Alchemy Book GameObject: {bookGO.name}");
            
            if (uxml == null || uss == null)
            {
                EditorUtility.DisplayDialog("Warning", 
                    "Some assets were not found:\n" +
                    $"• UXML: {(uxml != null ? "✅" : "❌ Missing at Assets/Scripts/UI/CraftingSystem/BookSystem/AlchemyBook.uxml")}\n" +
                    $"• USS: {(uss != null ? "✅" : "❌ Missing at Assets/Scripts/UI/CraftingSystem/BookSystem/AlchemyBookStyles.uss")}\n\n" +
                    "Please ensure these files exist and assign them manually.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Success!", 
                    "🎉 Alchemy Book GameObject created successfully!\n\n" +
                    "• UIDocument component configured\n" +
                    "• AlchemyBook script attached\n" +
                    "• UXML and USS assets loaded\n\n" +
                    "The book is ready to use!",
                    "Awesome!");
            }
        }

        private static void SetupBookInSceneRoot()
        {
            // Check if AlchemyBook already exists in scene
            var existingBook = FindFirstObjectByType<AlchemyBook>();
            if (existingBook != null)
            {
                var result = EditorUtility.DisplayDialogComplex("Book Exists",
                    "An Alchemy Book already exists in the scene.\nWhat would you like to do?",
                    "Select Existing", "Create New", "Cancel");

                switch (result)
                {
                    case 0: // Select existing
                        Selection.activeGameObject = existingBook.gameObject;
                        EditorGUIUtility.PingObject(existingBook.gameObject);
                        return;
                    case 1: // Create new
                        break;
                    case 2: // Cancel
                        return;
                }
            }

            CreateAlchemyBookGameObject();
        }

        private static void CleanUpOldFiles()
        {
            var filesToDelete = new[]
            {
                "/Assets/UI/AlchemyBook.uxml",
                "/Assets/UI/AlchemyBook.uss",
                "/Assets/Scripts/UI/AlchemyBook.cs"
            };

            int deletedCount = 0;
            foreach (var file in filesToDelete)
            {
                if (AssetDatabase.DeleteAsset(file))
                {
                    deletedCount++;
                    Debug.Log($"🗑️ Deleted: {file}");
                }
            }

            if (deletedCount > 0)
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Cleanup Complete", 
                    $"Successfully deleted {deletedCount} duplicate files.\n\n" +
                    "The project now uses the existing alchemy book system from the crafting menu.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Nothing to Clean", 
                    "No duplicate files were found that need to be cleaned up.",
                    "OK");
            }
        }
    }
}