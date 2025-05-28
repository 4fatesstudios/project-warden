using UnityEngine;
using UnityEngine.UIElements; // This one enables Label, Slider, etc.


public class GrindingMinigameController : MonoBehaviour
{
    [Header("UI Toolkit Elements")]
    public UIDocument uiDocument;

    private ProgressBar grindSpeedBar;
    private ProgressBar poundRateBar;
    private Label resultLabel;


    [Header("Config")]
    public float requiredDuration = 5f;
    public float grindDecayRate = 3f;

    [Header("Grind Settings")]
    public Transform grindCenter; // Assign the center of the bowl in the Inspector

    [Header("Thresholds")]
    public float minGrindSpeed = 2f;
    public float maxGrindSpeed = 8f;
    public float minPoundRate = 1f;
    public float maxPoundRate = 5f;

    private float grindSpeed;
    private float grindRotationSum;
    private Vector2 lastMouseDirection;

    private float poundFrequency;
    private float poundCount;
    private float poundTimer;

    private float totalTime;
    private bool gameEnded;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        grindSpeedBar = root.Q<ProgressBar>("grindSpeedBar");
        poundRateBar = root.Q<ProgressBar>("poundRateBar");
        resultLabel = root.Q<Label>("resultLabel");

        ResetGame(); // Ensure UI is initialized at start
    }

    void Update()
    {
        if (gameEnded) return;

        totalTime += Time.deltaTime;

        HandleGrindingInput();
        HandlePoundingInput();
        UpdateUI();
        EvaluateGameState();
    }

    private void HandleGrindingInput()
    {
        if (Input.GetMouseButton(0) && grindCenter != null)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 centerScreen = Camera.main.WorldToScreenPoint(grindCenter.position);
            Vector2 dir = (mousePos - centerScreen).normalized;

            if (lastMouseDirection != Vector2.zero)
            {
                float angle = Vector2.SignedAngle(lastMouseDirection, dir);
                grindRotationSum += angle;
            }

            lastMouseDirection = dir;
        }
        else
        {
            grindRotationSum = Mathf.Lerp(grindRotationSum, 0f, Time.deltaTime * grindDecayRate);
            lastMouseDirection = Vector2.zero;
        }

        grindSpeed = Mathf.Abs(grindRotationSum) / 10f;
    }

    private void HandlePoundingInput()
    {
        poundTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            poundCount++;
        }

        if (poundTimer >= 1f)
        {
            poundFrequency = poundCount / poundTimer;
            poundTimer = 0f;
            poundCount = 0f;
        }
    }

    private void UpdateUI()
    {
        if (grindSpeedBar != null)
        {
            float grindPercent = Mathf.Clamp01(grindSpeed / maxGrindSpeed) * 100f;
            grindSpeedBar.value = grindPercent;

            UpdateProgressBarClass(grindSpeedBar, grindSpeed, minGrindSpeed, maxGrindSpeed);
        }

        if (poundRateBar != null)
        {
            float poundPercent = Mathf.Clamp01(poundFrequency / maxPoundRate) * 100f;
            poundRateBar.value = poundPercent;

            UpdateProgressBarClass(poundRateBar, poundFrequency, minPoundRate, maxPoundRate);
        }

    }

    private void UpdateProgressBarClass(ProgressBar bar, float value, float min, float max)
    {
        bar.RemoveFromClassList("progress-bar-good");
        bar.RemoveFromClassList("progress-bar-warning");
        bar.RemoveFromClassList("progress-bar-danger");

        if (value > max || value < min)
        {
            bar.AddToClassList("progress-bar-danger");
        }
        else if (value < min + 1f || value > max - 1f)
        {
            bar.AddToClassList("progress-bar-warning");
        }
        else
        {
            bar.AddToClassList("progress-bar-good");
        }
    }

    private void EvaluateGameState()
    {
        if (grindSpeed > maxGrindSpeed || poundFrequency > maxPoundRate)
        {
            EndGame("Too aggressive! Bowl broke.");
        }
        else if (grindSpeed < minGrindSpeed || poundFrequency < minPoundRate)
        {
            if (totalTime >= requiredDuration)
                EndGame("Too slow! Could not grind to dust.");
        }
        else
        {
            if (totalTime >= requiredDuration)
                EndGame("Perfect! Ground to dust.");
        }
    }

    public void ResetGame()
    {
        grindSpeed = 0f;
        grindRotationSum = 0f;
        lastMouseDirection = Vector2.zero;

        poundFrequency = 0f;
        poundCount = 0f;
        poundTimer = 0f;

        totalTime = 0f;
        gameEnded = false;

        if (resultLabel != null)
            resultLabel.text = "Grind (mouse) and Pound (space)!";
        if (grindSpeedBar != null)
            grindSpeedBar.value = 0f;
        if (poundRateBar != null)
            poundRateBar.value = 0f;

    }


    private void EndGame(string message)
    {
        gameEnded = true;
        if (resultLabel != null)
            resultLabel.text = message; // or "Grind (mouse) and Pound (space)!" in Reset

        Debug.Log("Grinding End: " + message);
    }
}
