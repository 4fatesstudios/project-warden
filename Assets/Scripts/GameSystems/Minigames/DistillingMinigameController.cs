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
}
