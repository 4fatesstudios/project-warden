//using FourFatesStudios.ProjectWarden.GameSystems.Inventory;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    public class PotionCraftingController : MonoBehaviour
    {
        public UIDocument uiDocument;
        //public Inventory playerInventory; // Link to your inventory system

        private Button[] ingredientSlots = new Button[3];
        private Ingredient[] selectedIngredients = new Ingredient[3];
        private Button craftButton;
        private Label resultLabel;

        void OnEnable()
        {
            var root = uiDocument.rootVisualElement;
            ingredientSlots[0] = root.Q<Button>("ingredientSlot1");
            ingredientSlots[1] = root.Q<Button>("ingredientSlot2");
            ingredientSlots[2] = root.Q<Button>("ingredientSlot3");
            craftButton = root.Q<Button>("craftButton");
            resultLabel = root.Q<Label>("resultLabel");

            for (int i = 0; i < ingredientSlots.Length; i++)
            {
                int index = i;
                ingredientSlots[i].clicked += () => OpenIngredientSelector(index);
            }

            craftButton.clicked += TryCraftPotion;
        }

        private void OpenIngredientSelector(int slotIndex)
        {
            // Replace this with your actual inventory UI logic
            var ingredients = playerInventory.GetAllIngredients(); // Assume it returns List<Ingredient>

            // TODO: Create popup/select UI. For now, simulate selection
            Ingredient selected = ingredients[Random.Range(0, ingredients.Count)];
            selectedIngredients[slotIndex] = selected;
            ingredientSlots[slotIndex].text = selected.name;
        }

        private void TryCraftPotion()
        {
            var used = selectedIngredients.Where(x => x != null).ToList();

            if (used.Count == 0)
            {
                resultLabel.text = "No ingredients selected.";
                return;
            }

            bool hasSolvent = used.Any(x => x.IngredientArchetype == IngredientArchetype.Solvent);

            // Remove from inventory
            foreach (var ing in used)
                //playerInventory.RemoveItem(ing);

            if (hasSolvent)
            {
                // Create a Potion
                resultLabel.text = "You crafted a Potion!";
                // TODO: Create and store Potion ScriptableObject
            }
            else
            {
                // Create a Component
                resultLabel.text = "You created a Component.";
                // TODO: Create and store Component ScriptableObject
            }

            // Clear state
            for (int i = 0; i < ingredientSlots.Length; i++)
            {
                ingredientSlots[i].text = "+";
                selectedIngredients[i] = null;
            }
        }
    }
}
