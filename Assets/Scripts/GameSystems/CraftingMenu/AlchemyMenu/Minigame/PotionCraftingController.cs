//using FourFatesStudios.ProjectWarden.GameSystems.Inventory;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GameSystems.Crafting;
using FourFatesStudios.ProjectWarden.Inventory;
using FourFatesStudios.ProjectWarden.RuntimeData;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    public class PotionCraftingController : MonoBehaviour
    {
        public UIDocument uiDocument;
        public ItemSlotContainer<Ingredient> ingredientInventory; // Link to the inventory system

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
            var slots = ingredientInventory.Slots;
            var uniqueIngredients = slots.Select(slot => slot.Item).Distinct().ToList();

            if (uniqueIngredients.Count == 0)
            {
                Debug.LogWarning("No ingredients in inventory.");
                return;
            }

            // Simulate selection (replace with UI popup)
            Ingredient selected = uniqueIngredients[Random.Range(0, uniqueIngredients.Count)];
            selectedIngredients[slotIndex] = selected;
            ingredientSlots[slotIndex].text = selected.name;
        }

        private List<PotionEffect> ResolvePotionEffects(List<Ingredient> usedIngredients)
        {
            if (usedIngredients == null || usedIngredients.Count == 0)
                return new List<PotionEffect>();

            // Start with the effects of the first ingredient
            IEnumerable<PotionEffect> sharedEffects = usedIngredients[0].PotionEffects;

            // Intersect with the effects of the remaining ingredients
            foreach (var ingredient in usedIngredients.Skip(1))
            {
                sharedEffects = sharedEffects.Intersect(ingredient.PotionEffects);
            }

            return sharedEffects.ToList();
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
            {
                int leftover = ingredientInventory.Remove(ing, 1);
                if (leftover > 0)
                    Debug.LogWarning($"Couldn't fully remove {ing.name} from inventory.");
            }


            if (hasSolvent)
            {
                // Create Potion
                var effects = ResolvePotionEffects(used);
                var newPotion = new CraftedPotion
                {
                    Name = "Potion of " + string.Join(", ", effects.Select(e => e.name)),
                    Effects = effects,
                    Upgraded = false
                };
                PlayerCraftingInventory.CraftedPotions.Add(newPotion);

                resultLabel.text = $"You crafted: {newPotion.Name}\nEffects:\n" +
                    string.Join("\n", newPotion.Effects.Select(e => $"- {e.name}"));
            }
            else
            {
                // Create Component
                var sorted = used.OrderBy(i => i.name).ToList();
                var newComponent = new CraftedAlchemyComponent
                {
                    Name = $"Component from {sorted[0].name} + {sorted[1].name}",
                    Base1 = sorted[0],
                    Base2 = sorted[1]
                };
                PlayerCraftingInventory.CraftedComponents.Add(newComponent);

                resultLabel.text = $"You created an Alchemy Component:\n{newComponent.Name}";
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
