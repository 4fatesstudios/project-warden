using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Roasts;

namespace FourFatesStudios.ProjectWarden.GameSystems.Minigames
{
    public class RoastingMinigameController : MonoBehaviour
    {
        [Header("UI Toolkit")]
        public UIDocument uiDocument;

        [Header("Roast Data")]
        public Roast roastData;

        private Label stateLabel;
        private Label instructionLabel;
        private Button collectButton;

        private RoastVisualState currentVisualState;
        private float totalTimer;
        private float cookedTimer;
        private bool isInCookedState;
        private bool gameEnded;
        private bool isCorrupted;

        void Start()
        {
            var root = uiDocument.rootVisualElement;
            stateLabel = root.Q<Label>("stateLabel");
            instructionLabel = root.Q<Label>("instructionLabel");
            collectButton = root.Q<Button>("collectButton");

            collectButton.clicked += OnCollectClicked;

            if (roastData == null)
            {
                Debug.LogError("Roast data not assigned!");
                gameEnded = true;
                return;
            }

            ResetGame();
        }

        void Update()
        {
            if (gameEnded) return;

            totalTimer += Time.deltaTime;
            UpdateVisualState();

            if (totalTimer >= roastData.TotalCookTime)
            {
                instructionLabel.text = "Overcooked! Roasting failed.";
                gameEnded = true;
                return;
            }

            if (currentVisualState == RoastVisualState.Cooked)
            {
                if (!isInCookedState)
                {
                    isInCookedState = true;
                    instructionLabel.text = "Cooking perfectly!";
                }

                cookedTimer += Time.deltaTime;
                float remaining = roastData.RequiredCookedTime - cookedTimer;
                instructionLabel.text = $"Hold for {remaining:F1}s more";
            }
            else
            {
                if (isInCookedState)
                {
                    isInCookedState = false;
                    cookedTimer = 0f;
                    instructionLabel.text = "Wrong state! Timer reset.";
                }
                else
                {
                    instructionLabel.text = currentVisualState == RoastVisualState.Uncooked
                        ? "Not cooked enough!"
                        : "It's burning!";
                }
            }
        }

        void UpdateVisualState()
        {
            float progress = totalTimer / roastData.TotalCookTime;

            if (progress < 0.33f)
                currentVisualState = RoastVisualState.Uncooked;
            else if (progress < 0.66f)
                currentVisualState = RoastVisualState.Cooked;
            else
                currentVisualState = RoastVisualState.Burnt;

            // Corrupted recipes: invert logic
            if (isCorrupted)
            {
                currentVisualState = currentVisualState switch
                {
                    RoastVisualState.Uncooked => RoastVisualState.Burnt,
                    RoastVisualState.Burnt => RoastVisualState.Uncooked,
                    _ => RoastVisualState.Cooked,
                };
            }

            stateLabel.text = currentVisualState.ToString();
        }

        void OnCollectClicked()
        {
            if (gameEnded) return;

            bool success = cookedTimer >= roastData.RequiredCookedTime;

            if (success)
                instructionLabel.text = $"Success! Produced: {roastData.OutputIngredient?.name ?? "Unknown"}";
            else
                instructionLabel.text = "Failed! Undercooked or incorrect state.";

            gameEnded = true;
        }

        public void ResetGame()
        {
            totalTimer = 0f;
            cookedTimer = 0f;
            isInCookedState = false;
            gameEnded = false;
            instructionLabel.text = "Begin roasting!";
            UpdateVisualState();
        }

        public void SetCorrupted(bool value)
        {
            isCorrupted = value;
        }
    }
}