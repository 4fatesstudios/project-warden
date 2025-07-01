using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GameSystems.Crafting;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.Minigame;
using FourFatesStudios.ProjectWarden.Inventory;
using FourFatesStudios.ProjectWarden.RuntimeData;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.Minigame
{
    public class PotionCraftingController : MonoBehaviour
    {
        public UIDocument uiDocument;
        public ItemSlotContainer<Ingredient> ingredientInventory; // Link to the inventory system

        private Button[] ingredientSlots = new Button[3];
        private Ingredient[] selectedIngredients = new Ingredient[3];
        private Button craftButton;
        private Label resultLabel;
        [SerializeField] private RhythmMinigameController rhythmMinigameController;  // drag in Inspector


        private List<Ingredient> _cachedUsedIngredients;
        private AlchemyRecipe _cachedRecipe;

        private void OnMinigameFinished(bool success)
        {
            // Hide rhythm UI and show crafting UI
            rhythmMinigameController.uiDocument.rootVisualElement.style.display = DisplayStyle.None;
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            if (success)
            {
                if (_cachedRecipe != null)
                    CreateUniquePotion(_cachedRecipe);
                else
                    CreateEffectBasedPotion(_cachedUsedIngredients);

                CraftingSaveSystem.Save();
            }
            else
            {
                resultLabel.text = "The brew fizzledÅc try a steadier rhythm!";
            }

            ResetUI();
        }

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

        private void CreateComponent(List<Ingredient> ingredients)
        {
            var sorted = ingredients.OrderBy(i => i.name).ToList();
            var component = new CraftedAlchemyComponent
            {
                Name = $"Component from {sorted[0].name} + {sorted[1].name}",
                Base1 = sorted[0],
                Base2 = sorted[1]
            };

            PlayerCraftingInventory.CraftedComponents.Add(component);
            resultLabel.text = $"You created an Alchemy Component:\n{component.Name}";
        }

        private void CreateUniquePotion(AlchemyRecipe recipe)
        {
            var potion = new CraftedPotion
            {
                Name = recipe.OutputPotion.ItemName,
                Effects = recipe.OutputPotion.PotionEffects.ToList(),
                Upgraded = recipe.OutputPotion.Upgraded
            };

            PlayerCraftingInventory.CraftedPotions.Add(potion);
            resultLabel.text = $"Unique Recipe Matched!\nCrafted: {potion.Name}";
        }

        private void CreateEffectBasedPotion(List<Ingredient> ingredients)
        {
            var effects = ResolvePotionEffects(ingredients);
            if (effects.Count == 0)
            {
                resultLabel.text = "No matching effects found. Crafting failed.";
                return;
            }

            var potion = new CraftedPotion
            {
                Name = "Potion of " + string.Join(", ", effects.Select(e => e.name)),
                Effects = effects,
                Upgraded = false
            };

            PlayerCraftingInventory.CraftedPotions.Add(potion);
            resultLabel.text = $"Crafted Generic Potion:\n{potion.Name}\nEffects:\n" +
                               string.Join("\n", potion.Effects.Select(e => $"- {e.name}"));
        }

        private void ResetUI()
        {
            for (int i = 0; i < ingredientSlots.Length; i++)
            {
                ingredientSlots[i].text = "+";
                selectedIngredients[i] = null;
            }
        }

        private void TryCraftPotion()
        {
            var used = selectedIngredients.Where(i => i != null).ToList();
            if (used.Count < 2)
            {
                resultLabel.text = "Select at least two ingredients.";
                return;
            }

            bool hasSolvent = used.Any(i => i.IngredientArchetype == IngredientArchetype.Solvent);

            // Remove ingredients from inventory
            foreach (var ing in used)
                ingredientInventory.Remove(ing, 1);

            if (!hasSolvent)
            {
                CreateComponent(used);      // solvent missing Å® component
                ResetUI();
                return;
            }

            // --- at this point we know a potion CAN be brewed, so prep rhythm game ---

            // Determine hit/try numbers
            int defaultHits = Mathf.CeilToInt((float)used.Average(i => (int)i.ItemRarity) * 1.5f);
            int defaultAttempts = defaultHits + 2;
            var recipe = AlchemyRecipeDatabase.Instance.GetRecipeByIngredients(used);

            int hitsNeeded = recipe?.RequiredHits ?? defaultHits;
            int tries = recipe?.MaxAttempts ?? defaultAttempts;

            // Swap UI panels
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;  // hide crafting UI
            rhythmMinigameController.uiDocument.rootVisualElement.style.display = DisplayStyle.Flex; // show rhythm UI

            // Initialise minigame
            rhythmMinigameController.Init(hitsNeeded, tries);
            // Unsubscribe the old handler (if needed)
            rhythmMinigameController.OnMinigameEnd -= OnMinigameFinished;
            rhythmMinigameController.OnMinigameEnd += OnMinigameFinished;

            // Cache data needed in callback
            _cachedUsedIngredients = used;
            _cachedRecipe = recipe;
        }
    }
}
