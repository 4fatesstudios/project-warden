<<<<<<< Updated upstream
// DistillingMinigameController.cs
using UnityEngine;
using UnityEngine.UIElements;

public class DistillingMinigameController : MonoBehaviour
{
    [Header("UI Toolkit")]
    public UIDocument uiDocument;

    private Label stateLabel;
    private Label instructionLabel;
    private Button collectButton;

    public enum DistillState { None, Shiny, Sparkling, Both }
    public DistillState correctState;

    private DistillState currentState;
    private float stateTimer;
    private float stateDuration = 3f;

    private bool isCorrupted;
    private bool gameEnded;

    void Start()
    {
        var root = uiDocument.rootVisualElement;
        stateLabel = root.Q<Label>("stateLabel");
        instructionLabel = root.Q<Label>("instructionLabel");
        collectButton = root.Q<Button>("collectButton");

        collectButton.clicked += OnCollectClicked;

        ResetGame();
    }

    void Update()
    {
        if (gameEnded) return;

        stateTimer += Time.deltaTime;
        if (stateTimer >= stateDuration)
        {
            ChangeState();
            stateTimer = 0f;
        }
    }

    void ChangeState()
    {
        currentState = (DistillState)Random.Range(0, 4);
        stateLabel.text = currentState.ToString();
    }

    void OnCollectClicked()
    {
        bool correct = (!isCorrupted && currentState == correctState) ||
                       (isCorrupted && currentState != correctState);

        instructionLabel.text = correct ? "Correct!" : "Wrong State!";
        gameEnded = true;
    }

    public void ResetGame()
    {
        gameEnded = false;
        stateTimer = 0f;
        ChangeState();
        instructionLabel.text = "Wait and collect at correct state!";
    }

    public void SetCorrupted(bool value)
    {
        isCorrupted = value;
    }
=======
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Distills;  // Import your Distill namespace

namespace FourFatesStudios.ProjectWarden.GameSystems.Minigames
{
    public class DistillingMinigameController : MonoBehaviour
    {
        [Header("UI Toolkit")]
        public UIDocument uiDocument;

        [Header("Distill Data")]
        public Distill distillData;  // Assign this in inspector with a Distill ScriptableObject

        private Label stateLabel;
        private Label instructionLabel;
        private Button collectButton;

        private DistillVisualState currentVisualState;
        private float stateTimer;
        private float totalTimer;
        private float requiredHoldTimer;
        private bool isInCorrectState;
        private bool isCorrupted;
        private bool gameEnded;

        // The target correct visual state from the progressionStages
        private DistillVisualState correctVisualState;

        void Start()
        {
            var root = uiDocument.rootVisualElement;
            stateLabel = root.Q<Label>("stateLabel");
            instructionLabel = root.Q<Label>("instructionLabel");
            collectButton = root.Q<Button>("collectButton");

            collectButton.clicked += OnCollectClicked;

            if (distillData == null)
            {
                Debug.LogError("DistillingMinigameController: Distill data not assigned!");
                gameEnded = true;
                return;
            }

            // Pick the "correct" state as the last stage in the progression for example,
            // Or customize logic to pick which stage is correct
            correctVisualState = distillData.ProgressionStages.Count > 0
                ? distillData.ProgressionStages[^1]
                : DistillVisualState.None;

            ResetGame();
        }

        void Update()
        {
            if (gameEnded) return;

            totalTimer += Time.deltaTime;
            if (totalTimer >= distillData.TotalDuration)
            {
                instructionLabel.text = "Time's up! Distillation failed.";
                gameEnded = true;
                return;
            }

            stateTimer += Time.deltaTime;

            if (stateTimer >= 3f) // Or you could add a property in Distill for state change duration
            {
                ChangeState();
                stateTimer = 0f;
            }

            // Determine if current state is "correct" based on corruption and correctVisualState
            bool shouldBeCorrect = (!isCorrupted && currentVisualState == correctVisualState) ||
                                   (isCorrupted && currentVisualState != correctVisualState);

            if (shouldBeCorrect)
            {
                if (!isInCorrectState)
                {
                    isInCorrectState = true;
                    requiredHoldTimer = 0f;
                    instructionLabel.text = "Hold on the correct state...";
                }
                else
                {
                    requiredHoldTimer += Time.deltaTime;
                    float remaining = distillData.RequiredDuration - requiredHoldTimer;
                    instructionLabel.text = $"Hold for {remaining:F1}s more";
                }
            }
            else
            {
                if (isInCorrectState)
                {
                    isInCorrectState = false;
                    requiredHoldTimer = 0f;
                    instructionLabel.text = "Wrong state! Timer reset.";
                }
                else
                {
                    instructionLabel.text = "Wait and collect at correct state!";
                }
            }
        }

        void ChangeState()
        {
            // Pick a random progression stage from the Distill SO progression stages
            var stages = distillData.ProgressionStages;
            if (stages == null || stages.Count == 0)
            {
                Debug.LogWarning("Distill data has no progression stages!");
                return;
            }

            int randomIndex = Random.Range(0, stages.Count);
            currentVisualState = stages[randomIndex];
            stateLabel.text = currentVisualState.ToString();
        }

        void OnCollectClicked()
        {
            if (gameEnded) return;

            bool canCollect = isInCorrectState && requiredHoldTimer >= distillData.RequiredDuration;

            if (canCollect)
            {
                instructionLabel.text = $"Success! Produced: {distillData.OutputIngredient?.name ?? "Unknown"}";
            }
            else
            {
                instructionLabel.text = "Too early or wrong state!";
            }
            gameEnded = true;
        }

        public void ResetGame()
        {
            gameEnded = false;
            stateTimer = 0f;
            totalTimer = 0f;
            requiredHoldTimer = 0f;
            isInCorrectState = false;
            ChangeState();
            instructionLabel.text = "Wait and collect at correct state!";
        }

        public void SetCorrupted(bool value)
        {
            isCorrupted = value;
        }
    }
>>>>>>> Stashed changes
}
