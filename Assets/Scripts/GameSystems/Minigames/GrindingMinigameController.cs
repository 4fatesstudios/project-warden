using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrindingMinigameController : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider grindSpeedSlider;
    public Slider poundRateSlider;
    public TMP_Text resultText;

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
        if (grindSpeedSlider != null)
            grindSpeedSlider.value = Mathf.Clamp01(grindSpeed / maxGrindSpeed);
        if (poundRateSlider != null)
            poundRateSlider.value = Mathf.Clamp01(poundFrequency / maxPoundRate);
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

        if (resultText != null)
            resultText.text = "Grind (mouse) and Pound (space)!";
        if (grindSpeedSlider != null)
            grindSpeedSlider.value = 0f;
        if (poundRateSlider != null)
            poundRateSlider.value = 0f;
    }

    private void EndGame(string message)
    {
        gameEnded = true;
        if (resultText != null)
            resultText.text = message;

        Debug.Log("Grinding End: " + message);
    }
}
