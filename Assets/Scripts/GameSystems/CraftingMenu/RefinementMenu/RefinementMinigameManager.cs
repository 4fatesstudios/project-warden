using System;
using FourFatesStudios.ProjectWarden;
using FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Inventory;
using UnityEngine;

namespace GameSystems.CraftingMenu.RefinementMenu
{
    public class RefinementMinigameManager : MonoBehaviour
    {
        [Header("Minigame Controllers")]
        [SerializeField] private RoastingMinigameController roastingController;
        [SerializeField] private DistillationMinigameController distillationController;
        [SerializeField] private GrindingMinigameController grindingController;
        
        [Header("Inventory")]
        [SerializeField] private ItemSlotContainerHolder inventoryHolder;
        
        [Header("UI References")]
        [SerializeField] private GameObject refinementUI;
        
        private RefinementType currentRefinementType;
        private Ingredient currentIngredient;
        
        public event Action<bool, Ingredient, RefinementType> OnRefinementComplete;
        
        public enum RefinementType
        {
            Grinding,
            Distillation,
            Roasting
        }

        private void Awake()
        {
            // Subscribe to minigame completion events
            if (roastingController != null)
                roastingController.OnRoastingComplete += OnRoastingComplete;
            
            if (distillationController != null)
            {
                distillationController.OnDistillationComplete += OnDistillationComplete;
                distillationController.OnBackPressed += OnBackToRefinementMenu;
            }
            
            if (grindingController != null)
            {
                grindingController.OnGrindingComplete += OnGrindingComplete;
                grindingController.OnBackPressed += OnBackToRefinementMenu;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (roastingController != null)
                roastingController.OnRoastingComplete -= OnRoastingComplete;
            
            if (distillationController != null)
            {
                distillationController.OnDistillationComplete -= OnDistillationComplete;
                distillationController.OnBackPressed -= OnBackToRefinementMenu;
            }
            
            if (grindingController != null)
            {
                grindingController.OnGrindingComplete -= OnGrindingComplete;
                grindingController.OnBackPressed -= OnBackToRefinementMenu;
            }
        }

        public bool CanRefineIngredient(Ingredient ingredient, RefinementType refinementType)
        {
            if (ingredient == null) return false;
            
            return refinementType switch
            {
                RefinementType.Grinding => ingredient.CanGrind && ingredient.GrindingResult != null,
                RefinementType.Distillation => ingredient.CanDistill && ingredient.DistillingResult != null,
                RefinementType.Roasting => ingredient.CanRoast && ingredient.RoastingResult != null,
                _ => false
            };
        }

        public void StartRefinement(Ingredient ingredient, RefinementType refinementType)
        {
            if (!CanRefineIngredient(ingredient, refinementType))
            {
                Debug.LogWarning($"Cannot refine {ingredient.name} using {refinementType}");
                return;
            }
            
            // Check if player has the ingredient in inventory
            if (inventoryHolder?.Container != null)
            {
                int availableCount = inventoryHolder.Container.GetItemCount(ingredient);
                if (availableCount <= 0)
                {
                    Debug.LogWarning($"No {ingredient.name} available in inventory");
                    return;
                }
                
                // Consume one ingredient
                inventoryHolder.Container.Remove(ingredient, 1);
            }
            
            currentIngredient = ingredient;
            currentRefinementType = refinementType;
            
            // Hide all minigames first
            HideAllMinigames();
            
            // Show the appropriate minigame
            switch (refinementType)
            {
                case RefinementType.Grinding:
                    if (grindingController != null)
                    {
                        grindingController.Show();
                        grindingController.InitializeGrinding(ingredient);
                    }
                    break;
                    
                case RefinementType.Distillation:
                    if (distillationController != null)
                    {
                        distillationController.Show();
                        distillationController.InitializeDistillation(ingredient);
                    }
                    break;
                    
                case RefinementType.Roasting:
                    if (roastingController != null)
                    {
                        roastingController.Show();
                        roastingController.InitializeRoasting(ingredient);
                    }
                    break;
            }
            
            // Hide main refinement UI
            if (refinementUI != null)
                refinementUI.SetActive(false);
        }

        private void HideAllMinigames()
        {
            roastingController?.Hide();
            distillationController?.Hide();
            grindingController?.Hide();
        }

        private void OnRoastingComplete(bool success, Ingredient result)
        {
            HandleRefinementComplete(success, result, RefinementType.Roasting);
        }

        private void OnDistillationComplete(bool success, Ingredient result)
        {
            HandleRefinementComplete(success, result, RefinementType.Distillation);
        }

        private void OnGrindingComplete(bool success, Ingredient result)
        {
            HandleRefinementComplete(success, result, RefinementType.Grinding);
        }

        private void HandleRefinementComplete(bool success, Ingredient result, RefinementType type)
        {
            // Hide the minigame
            HideAllMinigames();
            
            // Show main refinement UI
            if (refinementUI != null)
                refinementUI.SetActive(true);
            
            // Add result to inventory if successful
            if (success && result != null && inventoryHolder?.Container != null)
            {
                inventoryHolder.Container.Add(result, 1);
                Debug.Log($"Successfully refined {currentIngredient.name} into {result.name}!");
            }
            else if (!success)
            {
                // Optionally return some materials or provide partial result
                Debug.Log($"Refinement of {currentIngredient.name} failed!");
            }
            
            // Notify listeners
            OnRefinementComplete?.Invoke(success, result, type);
            
            // Clear current state
            currentIngredient = null;
            currentRefinementType = RefinementType.Grinding;
        }

        public void CancelRefinement()
        {
            // Return ingredient to inventory
            if (currentIngredient != null && inventoryHolder?.Container != null)
            {
                inventoryHolder.Container.Add(currentIngredient, 1);
            }
            
            // Hide all minigames
            HideAllMinigames();
            
            // Show main refinement UI
            if (refinementUI != null)
                refinementUI.SetActive(true);
            
            // Clear state
            currentIngredient = null;
            currentRefinementType = RefinementType.Grinding;
        }

        public RefinementType[] GetAvailableRefinements(Ingredient ingredient)
        {
            if (ingredient == null) return new RefinementType[0];
            
            var availableTypes = new System.Collections.Generic.List<RefinementType>();
            
            if (ingredient.CanGrind && ingredient.GrindingResult != null)
                availableTypes.Add(RefinementType.Grinding);
            
            if (ingredient.CanDistill && ingredient.DistillingResult != null)
                availableTypes.Add(RefinementType.Distillation);
            
            if (ingredient.CanRoast && ingredient.RoastingResult != null)
                availableTypes.Add(RefinementType.Roasting);
            
            return availableTypes.ToArray();
        }

        public string GetRefinementDescription(RefinementType type)
        {
            return type switch
            {
                RefinementType.Grinding => "Use mortar and pestle to grind ingredients into powder form. Click in rhythm for better results.",
                RefinementType.Distillation => "Use distillation column to extract pure essences. Maintain optimal pressure for efficiency.",
                RefinementType.Roasting => "Use frying pan to roast ingredients. Watch temperature and stop at the perfect moment.",
                _ => "Unknown refinement process"
            };
        }

        public Sprite GetRefinementIcon(RefinementType type)
        {
            // Return appropriate icon for each refinement type
            // You would assign these in the inspector or load from resources
            return type switch
            {
                RefinementType.Grinding => Resources.Load<Sprite>("Icons/mortar_pestle"),
                RefinementType.Distillation => Resources.Load<Sprite>("Icons/distillation_column"),
                RefinementType.Roasting => Resources.Load<Sprite>("Icons/frying_pan"),
                _ => null
            };
        }

        public float GetRefinementDifficulty(Ingredient ingredient, RefinementType type)
        {
            // Calculate difficulty based on ingredient properties
            float baseDifficulty = (int)ingredient.ItemRarity * 0.2f;
            
            float typeDifficulty = type switch
            {
                RefinementType.Grinding => 0.3f,     // Easiest - rhythm based
                RefinementType.Roasting => 0.5f,     // Medium - timing based
                RefinementType.Distillation => 0.7f, // Hardest - precision based
                _ => 0.5f
            };
            
            return Mathf.Clamp01(baseDifficulty + typeDifficulty);
        }

        // Editor utilities
#if UNITY_EDITOR
        [ContextMenu("Test Grinding")]
        private void TestGrinding()
        {
            var testIngredient = Resources.Load<Ingredient>("Ingredients/TestIngredient");
            if (testIngredient != null && testIngredient.CanGrind)
            {
                StartRefinement(testIngredient, RefinementType.Grinding);
            }
        }

        [ContextMenu("Test Distillation")]
        private void TestDistillation()
        {
            var testIngredient = Resources.Load<Ingredient>("Ingredients/TestIngredient");
            if (testIngredient != null && testIngredient.CanDistill)
            {
                StartRefinement(testIngredient, RefinementType.Distillation);
            }
        }

        [ContextMenu("Test Roasting")]
        private void TestRoasting()
        {
            var testIngredient = Resources.Load<Ingredient>("Ingredients/TestIngredient");
            if (testIngredient != null && testIngredient.CanRoast)
            {
                StartRefinement(testIngredient, RefinementType.Roasting);
            }
        }
#endif

        private void OnBackToRefinementMenu()
        {
            // Hide all minigame UIs
            grindingController?.Hide();
            distillationController?.Hide();
            roastingController?.Hide();
            
            // Show the main refinement UI
            if (refinementUI != null)
            {
                refinementUI.SetActive(true);
            }
            
            Debug.Log("🔙 Returned to refinement menu");
        }
    }
}