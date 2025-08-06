using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects.Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu
{
    public class PotionCraftingController : MonoBehaviour
    {
        public UIDocument uiDocument;
        [SerializeField] private GridMinigameController gridMinigameController;
        [SerializeField] private ItemSlotContainerHolder ingredientInventoryHolder;

        private Button[] ingredientSlots = new Button[3];
        private Ingredient[] selectedIngredients = new Ingredient[3];
        private Button craftButton;
        private Button autoCraftButton;
        private Button bulkCraftButton;
        private Label resultLabel;
        private Label autoCraftStatusLabel;

        private List<Ingredient> cachedUsedIngredients;
        private AlchemyRecipe cachedRecipe;

        private void OnEnable()
        {
            // Try to auto-assign UIDocument if not set
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
            
            if (uiDocument == null)
            {
                Debug.LogWarning("UIDocument not found on PotionCraftingController - UI functionality disabled until assigned.");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("UIDocument root is null! Check if UXML file is assigned.");
                return;
            }

            // Find ingredient slots with null checks
            ingredientSlots[0] = root.Q<Button>("ingredientSlot1");
            ingredientSlots[1] = root.Q<Button>("ingredientSlot2");
            ingredientSlots[2] = root.Q<Button>("ingredientSlot3");
            
            // Find other UI elements with null checks
            craftButton = root.Q<Button>("craftButton");
            autoCraftButton = root.Q<Button>("autoCraftButton");
            bulkCraftButton = root.Q<Button>("bulkCraftButton");
            resultLabel = root.Q<Label>("resultLabel");
            autoCraftStatusLabel = root.Q<Label>("autoCraftStatusLabel");

            // Connect ingredient slot events
            for (int i = 0; i < ingredientSlots.Length; i++)
            {
                if (ingredientSlots[i] != null)
                {
                    int index = i;
                    ingredientSlots[i].clicked += () => OpenIngredientSelector(index);
                }
                else
                {
                    Debug.LogWarning($"Ingredient slot {i + 1} not found in UI!");
                }
            }

            // Connect button events with null checks
            if (craftButton != null)
                craftButton.clicked += TryCraftPotion;
            else
                Debug.LogWarning("Craft button not found in UI!");
                
            if (autoCraftButton != null)
                autoCraftButton.clicked += TryAutoCraft;
            else
                Debug.LogWarning("Auto-craft button not found in UI!");
                
            if (bulkCraftButton != null)
                bulkCraftButton.clicked += OpenBulkCrafting;
            else
                Debug.LogWarning("Bulk craft button not found in UI!");

            // Initialize UI state
            if (resultLabel != null)
                resultLabel.text = "Select ingredients and craft potions!";
                
            if (autoCraftStatusLabel != null)
                autoCraftStatusLabel.text = "Select ingredients to check auto-craft availability";
            
            UpdateAutoCraftUI();
        }

        private void OpenIngredientSelector(int slotIndex)
        {
            if (ingredientInventoryHolder == null || ingredientInventoryHolder.Container == null)
            {
                Debug.LogWarning("Inventory not assigned or initialized.");
                if (resultLabel != null)
                    resultLabel.text = "No inventory found! Assign ItemSlotContainerHolder in inspector.";
                return;
            }

            // Get all available ingredients from our placeholder inventory
            var availableItems = ingredientInventoryHolder.Container.GetAllItems()
                .Where(item => item is Ingredient)
                .Cast<Ingredient>()
                .ToList();

            // Exclude already selected items in other slots
            var alreadySelected = selectedIngredients
                .Where((ing, idx) => ing != null && idx != slotIndex)
                .ToHashSet();

            var availableForSelection = availableItems
                .Where(item => !alreadySelected.Contains(item))
                .ToList();

            if (availableForSelection.Count == 0)
            {
                Debug.LogWarning("No available items to select.");
                if (resultLabel != null)
                    resultLabel.text = "No ingredients available! Add ingredients to inventory.";
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
            popup.style.width = 250;
            popup.style.maxHeight = 300;
            popup.style.overflow = Overflow.Hidden;

            // Add a ScrollView for the ingredient buttons
            var scrollView = new ScrollView();
            scrollView.style.height = 220;
            scrollView.style.width = 230;

            foreach (var ingredient in availableForSelection)
            {
                string displayName = ingredient.ItemName;
                var button = new Button(() =>
                {
                    selectedIngredients[slotIndex] = ingredient;
                    
                    if (ingredientSlots[slotIndex] != null)
                        ingredientSlots[slotIndex].text = displayName;
                    
                    root.Remove(popup);
                    UpdateAutoCraftUI(); // Update auto-craft status when ingredients change
                })
                { text = displayName };
                scrollView.Add(button);
            }

            popup.Add(scrollView);

            if (selectedIngredients[slotIndex] != null)
            {
                var clearButton = new Button(() =>
                {
                    selectedIngredients[slotIndex] = null;
                    ingredientSlots[slotIndex].text = "+";
                    root.Remove(popup);
                    UpdateAutoCraftUI(); // Update auto-craft status when ingredients change
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

            // Defensive: skip ingredients with null or empty PotionEffects
            var validIngredients = usedIngredients
                .Where(i => i != null && i.PotionEffects != null && i.PotionEffects.Count > 0)
                .ToList();

            if (validIngredients.Count == 0)
                return new List<PotionEffect>();

            IEnumerable<PotionEffect> sharedEffects = validIngredients[0].PotionEffects;

            foreach (var ingredient in validIngredients.Skip(1))
            {
                if (ingredient.PotionEffects == null)
                    continue;
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

        private void CreateUniquePotion(AlchemyRecipe recipe, List<Ingredient> unpacked, CraftingRank rank = CraftingRank.A)
        {
            var potion = ScriptableObject.CreateInstance<Potion>();
            potion.name = recipe.OutputPotion.ItemName;

            var effectsField = typeof(Potion).GetField("potionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var originalEffects = recipe.OutputPotion.PotionEffects.ToList();
            
            // Apply rank potency modifier
            var modifiedEffects = ApplyRankModifier(originalEffects, rank);
            effectsField?.SetValue(potion, modifiedEffects);

            var upgradedProp = typeof(Potion).GetProperty("Upgraded");
            upgradedProp?.SetValue(potion, recipe.OutputPotion.Upgraded);

            if (ingredientInventoryHolder != null && ingredientInventoryHolder.Container != null)
            {
                ingredientInventoryHolder.AddItem(potion, 1);
            }
            
            string rankText = rank.GetRankDisplayName();
            float potencyPercent = rank.GetPotencyMultiplier() * 100f;
            resultLabel.text = $"Unique Recipe Matched! (Rank: {rankText})\nCrafted: {potion.name}\nPotency: {potencyPercent:F0}%";

            // Record the crafting result for skill progression
            if (AlchemySkillSystem.Instance != null)
            {
                AlchemySkillSystem.Instance.RecordCraftingResult(recipe, unpacked, rank);
            }
        }

        private void CreateEffectBasedPotion(List<Ingredient> ingredients, CraftingRank rank = CraftingRank.A)
        {
            var unpacked = UnpackIngredients(ingredients);
            var effects = ResolvePotionEffects(unpacked);
            if (effects.Count == 0)
            {
                resultLabel.text = "No matching effects found. Crafting failed.";
                return;
            }

            var potion = ScriptableObject.CreateInstance<Potion>();
            potion.name = "Potion of " + string.Join(", ", effects.Select(e => e.Suffix));

            // Apply rank potency modifier
            var modifiedEffects = ApplyRankModifier(effects, rank);
            var effectsField = typeof(Potion).GetField("potionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            effectsField?.SetValue(potion, modifiedEffects);

            var upgradedProp = typeof(Potion).GetProperty("Upgraded");
            upgradedProp?.SetValue(potion, false);

            // Add to inventory
            if (ingredientInventoryHolder != null && ingredientInventoryHolder.Container != null)
            {
                ingredientInventoryHolder.AddItem(potion, 1);
            }
            
            string rankText = rank.GetRankDisplayName();
            float potencyPercent = rank.GetPotencyMultiplier() * 100f;
            resultLabel.text = $"Crafted Generic Potion (Rank: {rankText}):\n{potion.name}\nPotency: {potencyPercent:F0}%\nEffects:\n" +
                               string.Join("\n", modifiedEffects.Select(e => "- " + e.Suffix));

            // Record the crafting result for skill progression
            if (AlchemySkillSystem.Instance != null)
            {
                AlchemySkillSystem.Instance.RecordCraftingResult(null, unpacked, rank);
            }
        }

        private List<PotionEffect> ApplyRankModifier(List<PotionEffect> originalEffects, CraftingRank rank)
        {
            float multiplier = rank.GetPotencyMultiplier();
            
            // Create modified copies of the effects (this would depend on your PotionEffect implementation)
            // For now, return the original effects - you'd modify the strength/duration based on multiplier
            return originalEffects.ToList();
        }

        private void OnMinigameFinished(bool success, CraftingRank rank)
        {
            gridMinigameController.UIDocument.rootVisualElement.style.display = DisplayStyle.None;
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

            if (success)
            {
                if (cachedRecipe != null)
                    CreateUniquePotion(cachedRecipe, UnpackIngredients(cachedUsedIngredients), rank);
                else
                    CreateEffectBasedPotion(cachedUsedIngredients, rank);
            }
            else
            {
                resultLabel.text = "The crafting process failed... try a better ingredient arrangement!";
            }

            ResetUI();
            UpdateAutoCraftUI(); // Update auto-craft availability after crafting
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

            // Switch to grid minigame
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;

            gridMinigameController.OnMinigameEnd -= OnMinigameFinished;
            gridMinigameController.OnMinigameEnd += OnMinigameFinished;
            gridMinigameController.Init(used);

            cachedUsedIngredients = used;
            
            // Check if there's a unique recipe for this combination
            var unpacked = UnpackIngredients(used);
            var db = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
            cachedRecipe = db?.GetRecipeByIngredients(unpacked);
        }

        private void TryAutoCraft()
        {
            var used = selectedIngredients.Where(i => i != null).ToList();
            if (used.Count < 2)
            {
                resultLabel.text = "Select at least two ingredients.";
                return;
            }

            if (ingredientInventoryHolder == null || ingredientInventoryHolder.Container == null)
            {
                Debug.LogWarning("Inventory holder is missing.");
                return;
            }

            // Check if auto-crafting is available
            var unpacked = UnpackIngredients(used);
            var db = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
            var recipe = db?.GetRecipeByIngredients(unpacked);

            if (AlchemySkillSystem.Instance == null || !AlchemySkillSystem.Instance.CanAutoCraft(recipe, unpacked))
            {
                resultLabel.text = "Auto-crafting not available for this recipe. Achieve S-rank first!";
                return;
            }

            // Consume ingredients
            foreach (var ing in used)
                ingredientInventoryHolder.Container.Remove(ing, 1);

            // Get auto-craft rank and create potion
            var autoCraftRank = AlchemySkillSystem.Instance.GetAutoCraftRank(recipe, unpacked);
            
            if (recipe != null)
                CreateUniquePotion(recipe, unpacked, autoCraftRank);
            else
                CreateEffectBasedPotion(used, autoCraftRank);

            ResetUI();
            UpdateAutoCraftUI();
        }

        private void UpdateAutoCraftUI()
        {
            if (autoCraftButton == null || autoCraftStatusLabel == null) return;

            var used = selectedIngredients.Where(i => i != null).ToList();
            
            if (used.Count < 2)
            {
                autoCraftButton.SetEnabled(false);
                autoCraftStatusLabel.text = "Select ingredients to check auto-craft availability";
                return;
            }

            var unpacked = UnpackIngredients(used);
            var db = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
            var recipe = db?.GetRecipeByIngredients(unpacked);

            if (AlchemySkillSystem.Instance != null && AlchemySkillSystem.Instance.CanAutoCraft(recipe, unpacked))
            {
                autoCraftButton.SetEnabled(true);
                var autoCraftRank = AlchemySkillSystem.Instance.GetAutoCraftRank(recipe, unpacked);
                var potency = autoCraftRank.GetPotencyMultiplier() * 100f;
                
                string recipeKey = recipe != null ? $"recipe_{recipe.name}" : $"generic_{string.Join("-", unpacked.Select(i => i.name).OrderBy(n => n))}";
                int sRankCount = AlchemySkillSystem.Instance.GetSRankCount(recipeKey);
                
                autoCraftStatusLabel.text = $"Auto-craft available (Rank: {autoCraftRank.GetRankDisplayName()})\nPotency: {potency:F0}%\nS-Ranks achieved: {sRankCount}";
            }
            else
            {
                autoCraftButton.SetEnabled(false);
                autoCraftStatusLabel.text = "Auto-craft locked. Achieve S-rank in manual crafting to unlock.";
            }

            // Update bulk craft button
            if (bulkCraftButton != null)
            {
                var hasUnlockedRecipes = AlchemySkillSystem.Instance?.GetUnlockedAutoCraftRecipes().Count > 0;
                bulkCraftButton.SetEnabled(hasUnlockedRecipes);
            }
        }

        private void OpenBulkCrafting()
        {
            // This would typically load a new scene or open a popup
            // For now, just show a message
            resultLabel.text = "Bulk crafting feature would open here. This requires S-rank achievements on recipes.";
        }

        private List<Ingredient> UnpackIngredients(List<Ingredient> ingredients)
        {
            var result = new List<Ingredient>();
            foreach (var ing in ingredients)
            {
                if (ing is AlchemyComponent comp)
                {
                    // Recursively unpack base ingredients
                    if (comp.BaseIngredient1 != null)
                        result.AddRange(UnpackIngredients(new List<Ingredient> { comp.BaseIngredient1 }));
                    if (comp.BaseIngredient2 != null)
                        result.AddRange(UnpackIngredients(new List<Ingredient> { comp.BaseIngredient2 }));
                }
                else if (ing != null)
                {
                    result.Add(ing);
                }
            }
            return result;
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
