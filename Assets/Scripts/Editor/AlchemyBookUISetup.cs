using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GameSystems.CraftingMenu.AlchemyBookMenu;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class AlchemyBookUISetup : EditorWindow
    {
        [MenuItem("Tools/Setup AlchemyBook UI")]
        public static void ShowWindow()
        {
            GetWindow<AlchemyBookUISetup>("AlchemyBook UI Setup");
        }

        void OnGUI()
        {
            GUILayout.Label("AlchemyBook UI Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Auto-Setup AlchemyBookUI GameObject"))
            {
                SetupAlchemyBookUI();
            }

            GUILayout.Space(10);
            GUILayout.Label("This will:", EditorStyles.helpBox);
            GUILayout.Label("• Find AlchemyBookUI GameObject in the scene");
            GUILayout.Label("• Assign AlchemyBookUI.uxml to UIDocument");
            GUILayout.Label("• Assign AlchemyBook.uss style sheet");
            GUILayout.Label("• Configure AlchemyBook component references");
        }

        static void SetupAlchemyBookUI()
        {
            // Find the AlchemyBookUI GameObject
            AlchemyBook alchemyBook = FindFirstObjectByType<AlchemyBook>();
            if (alchemyBook == null)
            {
                Debug.LogError("AlchemyBookUI GameObject with AlchemyBook component not found in scene!");
                return;
            }

            GameObject alchemyBookGO = alchemyBook.gameObject;
            UIDocument uiDocument = alchemyBookGO.GetComponent<UIDocument>();
            
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument component not found on AlchemyBookUI GameObject!");
                return;
            }

            // Load the UXML and USS files
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/AlchemyBookUI.uxml");
            StyleSheet uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/Styles/AlchemyBook.uss");
            VisualTreeAsset entryTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/UXML/AlchemyBookEntry.uxml");

            if (uxml == null)
            {
                Debug.LogError("AlchemyBookUI.uxml not found at Assets/UI/UXML/AlchemyBookUI.uxml");
                return;
            }

            if (uss == null)
            {
                Debug.LogError("AlchemyBook.uss not found at Assets/UI/Styles/AlchemyBook.uss");
                return;
            }

            // Configure UIDocument for Unity 6
            uiDocument.visualTreeAsset = uxml;
            
            // In Unity 6, we need to add style sheets through the root visual element
            if (uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(uss);
            }

            // Configure AlchemyBook component
            alchemyBook.uiDocument = uiDocument;
            alchemyBook.bookStyleSheet = uss;
            if (entryTemplate != null)
            {
                alchemyBook.entryTemplate = entryTemplate;
            }

            // Mark objects as dirty so changes are saved
            EditorUtility.SetDirty(uiDocument);
            EditorUtility.SetDirty(alchemyBook);

            Debug.Log("✅ AlchemyBookUI setup completed successfully!");
            Debug.Log("- UXML assigned: AlchemyBookUI.uxml");
            Debug.Log("- Style sheet assigned: AlchemyBook.uss");
            Debug.Log("- Component references configured");
            
            if (entryTemplate != null)
                Debug.Log("- Entry template assigned: AlchemyBookEntry.uxml");
            else
                Debug.LogWarning("- Entry template not found (optional)");
        }
    }
}