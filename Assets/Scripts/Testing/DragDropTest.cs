using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.Testing
{
    public class DragDropTest : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument uiDocument;
    
        void Start()
        {
            Debug.Log("Drag and Drop Test: Checking UI setup...");
        
            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                var ingredientPalette = root?.Q<ScrollView>("IngredientPalette");
                var gridContainer = root?.Q<VisualElement>("GridContainer");
            
                Debug.Log($"UI Elements found - Palette: {ingredientPalette != null}, Grid: {gridContainer != null}");
            
                if (ingredientPalette != null)
                {
                    TestDragSetup(ingredientPalette);
                }
            }
            else
            {
                Debug.LogWarning("UIDocument not assigned to DragDropTest!");
            }
        }
    
        void TestDragSetup(VisualElement palette)
        {
            // Create a test draggable element
            var testElement = new VisualElement();
            testElement.style.width = 50;
            testElement.style.height = 50;
            testElement.style.backgroundColor = Color.red;
            testElement.name = "TestDragElement";
        
            // Add basic drag events
            testElement.RegisterCallback<PointerDownEvent>(evt => 
            {
                Debug.Log("Test drag started!");
            });
        
            testElement.RegisterCallback<PointerMoveEvent>(evt => 
            {
                if (testElement.HasPointerCapture(evt.pointerId))
                {
                    Debug.Log($"Dragging to position: {evt.position}");
                }
            });
        
            testElement.RegisterCallback<PointerUpEvent>(evt => 
            {
                Debug.Log("Test drag ended!");
                testElement.ReleasePointer(evt.pointerId);
            });
        
            palette.Add(testElement);
            Debug.Log("Test draggable element added to palette");
        }
    }
}