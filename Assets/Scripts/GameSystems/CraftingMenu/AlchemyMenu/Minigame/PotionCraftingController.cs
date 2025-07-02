using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu
{
    public class PotionCraftingController : MonoBehaviour
    {
        public UIDocument uiDocument;
        [SerializeField] private RhythmMinigameController rhythmMinigameController;
        [SerializeField] private ItemSlotContainerHolder ingredientInventoryHolder;

        private Button[] ingredientSlots = new Button[3];
        private Ingredient[] selectedIngredients = new Ingredient[3];
        private Button craftButton;
        private Label resultLabel;

        private List<Ingredient> _cachedUsedIngredients;
        private AlchemyRecipe _cachedRecipe;

        private void OnEnable()
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
            if (ingredientInventoryHolder == null || ingredientInventoryHolder.Container == null)
            {
                Debug.LogWarning("Inventory not assigned or initialized.");
                return;
            }

            var slots = ingredientInventoryHolder.Container.Slots;

            // Get both Ingredients and AlchemyComponents from inventory
            var availableItems = slots
                .Select(slot => slot.Item)
                .Where(item => item is Ingredient or item is AlchemyComponent)
                .Distinct()
                .ToList();

            // Exclude already selected items in other slots
            var alreadySelected = selectedIngredients
                .Where((ing, idx) => ing != null && idx != slotIndex)
                .Cast<object>()
                .ToHashSet();

            var availableForSelection = availableItems
                .Where(item => !alreadySelected.Contains(item))
                .ToList();

            if (availableForSelection.Count == 0)
            {
                Debug.LogWarning("No available items to select.");
                return;
            }

            var root = uiDocument.rootVisualElement;
            var popup = new VisualElement();
            popup.style.backgroundColor = new StyleColor(Color.gray);
            popup.style.position = Position.Absolute;
            popup.style.left = 200;
            popup.style.top = 200;
            popup.style.paddingLeft = 10;
            popup.style.paddingRight = 10;
            popup.style.paddingTop = 10;
            popup.style.paddingBottom = 10;
            popup.style.borderTopLeftRadius = 5;
            popup.style.borderTopRightRadius = 5;
            popup.style.borderBottomLeftRadius = 5;
            popup.style.borderBottomRightRadius = 5;

            foreach (var item in availableForSelection)
            {
                string displayName = item is Ingredient ing ? ing.name : ((AlchemyComponent)item).Name;
                var button = new Button(() =>
                {
                    // Store as Ingredient or handle as component as needed
                    if (item is Ingredient ingredient)
                        selectedIngredients[slotIndex] = ingredient;
                    else
                        selectedIngredients[slotIndex] = null; // Or handle component selection logic

                    ingredientSlots[slotIndex].text = displayName;
                    root.Remove(popup);
                })
                { text = displayName };
                popup.Add(button);
            }

            if (selectedIngredients[slotIndex] != null)
            {
                var clearButton = new Button(() =>
                {
                    selectedIngredients[slotIndex] = null;
                    ingredientSlots[slotIndex].text = "+";
                    root.Remove(popup);
                })
                { text = "Clear Slot" };
                popup.Add(clearButton);
            }

            var cancelButton = new Button(() => root.Remove(popup)) { text = "Cancel" };
            popup.Add(cancelButton);

            root.Add(popup);
        }

        private List<PotionEffect> ResolvePotionEffects(List<Ingredient> usedIngredients)
        {
            if (usedIngredients == null || usedIngredients.Count == 0)
                return new List<PotionEffect>();

            IEnumerable<PotionEffect> sharedEffects = usedIngredients[0].PotionEffects;

            foreach (var ingredient in usedIngredients.Skip(1))
            {
                sharedEffects = sharedEffects.Intersect(ingredient.PotionEffects);
            }

            return sharedEffects.ToList();
        }

        private void CreateComponent(List<Ingredient> ingredients)
        {
            var sorted = ingredients.OrderBy(i => i.name).ToList();

            // Create a new AlchemyComponent asset
            var component = ScriptableObject.CreateInstance<AlchemyComponent>();
            component.name = "Component from " + sorted[0].name + " + " + sorted[1].name;

            // Set base ingredients via reflection (since fields are private and not settable directly)
            var base1Field = typeof(AlchemyComponent).GetField("baseIngredient1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var base2Field = typeof(AlchemyComponent).GetField("baseIngredient2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            base1Field?.SetValue(component, sorted[0]);
            base2Field?.SetValue(component, sorted[1]);

            // Add to inventory
            if (ingredientInventoryHolder != null && ingredientInventoryHolder.Container != null)
            {
                ingredientInventoryHolder.AddItem(component, 1);
            }
            resultLabel.text = "You created an Alchemy Component:\n" + component.name;
        }

        private void CreateUniquePotion(AlchemyRecipe recipe)
        {
            var potion = ScriptableObject.CreateInstance<Potion>();
            potion.name = recipe.OutputPotion.ItemName;

            // Set effects and upgraded via reflection (if needed)
            var effectsField = typeof(Potion).GetField("potionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            effectsField?.SetValue(potion, recipe.OutputPotion.PotionEffects.ToList());

            var upgradedProp = typeof(Potion).GetProperty("Upgraded");
            upgradedProp?.SetValue(potion, recipe.OutputPotion.Upgraded);

            // Add to inventory
            if (ingredientInventoryHolder != null && ingredientInventoryHolder.Container != null)
            {
                ingredientInventoryHolder.AddItem(potion, 1);
            }
            resultLabel.text = "Unique Recipe Matched!\nCrafted: " + potion.name;
        }

        private void CreateEffectBasedPotion(List<Ingredient> ingredients)
        {
            var effects = ResolvePotionEffects(ingredients);
            if (effects.Count == 0)
            {
                resultLabel.text = "No matching effects found. Crafting failed.";
                return;
            }

            var potion = ScriptableObject.CreateInstance<Potion>();
            potion.name = "Potion of " + string.Join(", ", effects.Select(e => e.Suffix));

            // Set effects and upgraded via reflection (if needed)
            var effectsField = typeof(Potion).GetField("potionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            effectsField?.SetValue(potion, effects);

            var upgradedProp = typeof(Potion).GetProperty("Upgraded");
            upgradedProp?.SetValue(potion, false);

            // Add to inventory
            if (ingredientInventoryHolder != null && ingredientInventoryHolder.Container != null)
            {
                ingredientInventoryHolder.AddItem(potion, 1);
            }
            resultLabel.text = "Crafted Generic Potion:\n" + potion.name + "\nEffects:\n" +
                               string.Join("\n", effects.Select(e => "- " + e.Suffix));
        }

        private void OnMinigameFinished(bool success)
        {
            rhythmMinigameController.UIDocument.rootVisualElement.style.display = DisplayStyle.None;
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            if (success)
            {
                if (_cachedRecipe != null)
                    CreateUniquePotion(_cachedRecipe);
                else
                    CreateEffectBasedPotion(_cachedUsedIngredients);
            }
            else
            {
                resultLabel.text = "The brew fizzled... try a steadier rhythm!";
            }

            ResetUI();
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

            if (ingredientInventoryHolder == null || ingredientInventoryHolder.Container == null)
            {
                Debug.LogWarning("Inventory holder is missing.");
                return;
            }

            foreach (var ing in used)
                ingredientInventoryHolder.Container.Remove(ing, 1);

            if (!hasSolvent)
            {
                CreateComponent(used);
                ResetUI();
                return;
            }

            int defaultHits = Mathf.CeilToInt((float)used.Average(i => (int)i.ItemRarity) * 1.5f);
            int defaultAttempts = defaultHits + 2;

            var db = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
            if (db == null)
            {
                Debug.LogError("AlchemyRecipeDatabase asset not found in Resources/Databases/AlchemyRecipeDatabase");
                resultLabel.text = "Recipe database missing! Cannot start minigame.";
                ResetUI();
                return;
            }

            var recipe = db.GetRecipeByIngredients(used);

            int hitsNeeded = recipe != null ? recipe.RequiredHits : defaultHits;
            int tries = recipe != null ? recipe.MaxAttempts : defaultAttempts;

            // Switch UI (via controller method)
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;

            rhythmMinigameController.OnMinigameEnd -= OnMinigameFinished;
            rhythmMinigameController.OnMinigameEnd += OnMinigameFinished;
            rhythmMinigameController.Init(hitsNeeded, tries);

            _cachedUsedIngredients = used;
            _cachedRecipe = recipe;
        }


        private void ResetUI()
        {
            for (int i = 0; i < ingredientSlots.Length; i++)
            {
                ingredientSlots[i].text = "+";
                selectedIngredients[i] = null;
            }
        }
    }
}
