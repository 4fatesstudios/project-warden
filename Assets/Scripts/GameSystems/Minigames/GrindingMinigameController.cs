using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Grinds;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GameSystems.Minigames
{
    public class GrindingMinigameController : MonoBehaviour
    {
        [Header("UI Toolkit Elements")]
        public UIDocument uiDocument;

        private ProgressBar grindSpeedBar;
        private ProgressBar poundRateBar;
        private Label resultLabel;

        [Header("Grind Recipe")]
        public Grind grindRecipe;

        [Header("Config")]
        public float requiredDuration = 5f;
        public float grindDecayRate = 3f;

        [Header("Grind Settings")]
        public Transform grindCenter;

        private float grindSpeed;
        private float grindRotationSum;
        private Vector2 lastMouseDirection;

        private float poundFrequency;
        private float poundCount;
        private float poundTimer;

        private float totalTime;
        private bool gameEnded;

        private float minGrindSpeed;
        private float maxGrindSpeed;
        private float minPoundRate;
        private float maxPoundRate;

        void Start()
        {
            var root = uiDocument.rootVisualElement;

            grindSpeedBar = root.Q<ProgressBar>("grindSpeedBar");
            poundRateBar = root.Q<ProgressBar>("poundRateBar");
            resultLabel = root.Q<Label>("resultLabel");

            if (grindRecipe == null)
            {
                Debug.LogError("Grind recipe not assigned!");
                return;
            }

            minGrindSpeed = grindRecipe.MinGrindSpeed;
            maxGrindSpeed = grindRecipe.MaxGrindSpeed;
            minPoundRate = grindRecipe.MinPoundSpeed;
            maxPoundRate = grindRecipe.MaxPoundSpeed;

            ResetGame();
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
                Vector2 centerScreen = UnityEngine.Camera.main.WorldToScreenPoint(grindCenter.position);
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
            float grindNormalized = Mathf.Clamp01(grindSpeed / maxGrindSpeed);
            float poundNormalized = Mathf.Clamp01(poundFrequency / maxPoundRate);

            UpdateProgressBar(grindSpeedBar, grindNormalized);
            UpdateProgressBar(poundRateBar, poundNormalized);
        }

        private void UpdateProgressBar(ProgressBar bar, float normalizedValue)
        {
            if (bar == null) return;

            bar.RemoveFromClassList("progress-bar-good");
            bar.RemoveFromClassList("progress-bar-warning");
            bar.RemoveFromClassList("progress-bar-danger");

            if (normalizedValue >= 0.7f)
                bar.AddToClassList("progress-bar-good");
            else if (normalizedValue >= 0.4f)
                bar.AddToClassList("progress-bar-warning");
            else
                bar.AddToClassList("progress-bar-danger");

            bar.value = normalizedValue;
        }

        private void EvaluateGameState()
        {
            if (grindSpeed > maxGrindSpeed || poundFrequency > maxPoundRate)
            {
                EndGame("Too aggressive! Bowl broke.", null);
            }
            else if (grindSpeed < minGrindSpeed || poundFrequency < minPoundRate)
            {
                if (totalTime >= requiredDuration)
                    EndGame("Too slow! Could not grind to dust.", null);
            }
            else
            {
                if (totalTime >= requiredDuration)
                    EndGame("Perfect! Ground to dust.", grindRecipe.OutputIngredient);
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

        private void EndGame(string message, Ingredient output)
        {
            gameEnded = true;

            if (resultLabel != null)
                resultLabel.text = message;

            if (output != null)
            {
                Debug.Log($"Grinding successful! Output ingredient: {output.ItemID}");
                // Optionally pass to inventory or result system
            }
            else
            {
                Debug.Log("Grinding failed. No output.");
            }
        }

    }
}
