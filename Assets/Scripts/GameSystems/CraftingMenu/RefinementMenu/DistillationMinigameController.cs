using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu
{
    public class DistillationMinigameController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Distillation Settings")]
        [SerializeField] private float distillationDuration = 15f;
        [SerializeField] private float perfectPressureRange = 0.1f;
        [SerializeField] private float targetPressure = 0.7f;
        
        [Header("Animation Settings")]
        [SerializeField] private float bubbleSpeed = 1f;
        [SerializeField] private float steamRiseSpeed = 2f;
        [SerializeField] private float pressureFluctuation = 0.05f;
        
        private VisualElement distillationColumn;
        private VisualElement boilingFlask;
        private VisualElement condenser;
        private VisualElement collectionFlask;
        private VisualElement liquidLevel;
        private VisualElement bubbles;
        private VisualElement steamPipe;
        private VisualElement distillate;
        private ProgressBar pressureGauge;
        private ProgressBar progressBar;
        private Button adjustPressureUp;
        private Button adjustPressureDown;
        private Label instructionsLabel;
        private Label statusLabel;
        
        private Ingredient currentIngredient;
        private float distillationTime = 0f;
        private bool isDistilling = false;
        private float currentPressure = 0.5f;
        private float distillateAmount = 0f;
        private DistillationState distillationState = DistillationState.Heating;
        
        public event Action<bool, Ingredient> OnDistillationComplete;
        
        private enum DistillationState
        {
            Heating,
            Boiling,
            Distilling,
            Complete,
            Failed
        }

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        public void InitializeDistillation(Ingredient ingredient)
        {
            currentIngredient = ingredient;
            SetupUI();
            StartDistillation();
        }

        private void SetupUI()
        {
            var root = uiDocument.rootVisualElement;
            
            distillationColumn = root.Q<VisualElement>("DistillationColumn");
            boilingFlask = root.Q<VisualElement>("BoilingFlask");
            condenser = root.Q<VisualElement>("Condenser");
            collectionFlask = root.Q<VisualElement>("CollectionFlask");
            liquidLevel = root.Q<VisualElement>("LiquidLevel");
            bubbles = root.Q<VisualElement>("Bubbles");
            steamPipe = root.Q<VisualElement>("SteamPipe");
            distillate = root.Q<VisualElement>("Distillate");
            pressureGauge = root.Q<ProgressBar>("PressureGauge");
            progressBar = root.Q<ProgressBar>("ProgressBar");
            adjustPressureUp = root.Q<Button>("AdjustPressureUp");
            adjustPressureDown = root.Q<Button>("AdjustPressureDown");
            instructionsLabel = root.Q<Label>("InstructionsLabel");
            statusLabel = root.Q<Label>("StatusLabel");
            
            adjustPressureUp?.RegisterCallback<ClickEvent>(_ => AdjustPressure(0.05f));
            adjustPressureDown?.RegisterCallback<ClickEvent>(_ => AdjustPressure(-0.05f));
            
            // Initialize UI state
            pressureGauge.value = currentPressure * 100f;
            progressBar.value = 0f;
            instructionsLabel.text = $"Distilling {currentIngredient?.ItemName}. Maintain optimal pressure for best results!";
            statusLabel.text = "Heating...";
            
            // Set initial liquid level
            if (liquidLevel != null)
                liquidLevel.style.height = Length.Percent(80);
            
            // Hide distillate initially
            if (distillate != null)
                distillate.style.height = Length.Percent(0);
        }

        private void StartDistillation()
        {
            isDistilling = true;
            distillationTime = 0f;
            currentPressure = 0.5f;
            distillateAmount = 0f;
            distillationState = DistillationState.Heating;
            
            StartCoroutine(DistillationProcess());
            StartCoroutine(AnimateBubbles());
            StartCoroutine(AnimateSteam());
            StartCoroutine(AnimatePressureFluctuation());
        }

        private void AdjustPressure(float adjustment)
        {
            if (!isDistilling) return;
            
            currentPressure = Mathf.Clamp01(currentPressure + adjustment);
            
            if (pressureGauge != null)
            {
                pressureGauge.value = currentPressure * 100f;
                
                // Change gauge color based on how close to target
                float distance = Mathf.Abs(currentPressure - targetPressure);
                Color gaugeColor;
                
                if (distance <= perfectPressureRange)
                    gaugeColor = Color.green;
                else if (distance <= perfectPressureRange * 2)
                    gaugeColor = Color.yellow;
                else
                    gaugeColor = Color.red;
                
                pressureGauge.Q<VisualElement>("unity-progress-bar").style.backgroundColor = new StyleColor(gaugeColor);
            }
        }

        private IEnumerator DistillationProcess()
        {
            while (isDistilling && distillationTime < distillationDuration)
            {
                distillationTime += Time.deltaTime;
                
                // Update progress bar
                if (progressBar != null)
                    progressBar.value = (distillationTime / distillationDuration) * 100f;
                
                // Update distillation state based on time
                UpdateDistillationState();
                
                // Calculate distillate production based on pressure accuracy
                if (distillationState == DistillationState.Distilling)
                {
                    float pressureAccuracy = 1f - (Mathf.Abs(currentPressure - targetPressure) / 0.5f);
                    pressureAccuracy = Mathf.Clamp01(pressureAccuracy);
                    
                    distillateAmount += Time.deltaTime * pressureAccuracy * 0.1f;
                    
                    // Update distillate visual
                    if (distillate != null)
                    {
                        float distillateHeight = Mathf.Clamp01(distillateAmount) * 60f;
                        distillate.style.height = Length.Percent(distillateHeight);
                    }
                }
                
                // Update liquid level (decreases as distillation progresses)
                if (liquidLevel != null && distillationState != DistillationState.Heating)
                {
                    float remainingLiquid = 80f * (1f - distillationTime / distillationDuration);
                    liquidLevel.style.height = Length.Percent(Mathf.Max(20f, remainingLiquid));
                }
                
                yield return null;
            }
            
            if (isDistilling)
            {
                CompleteDistillation();
            }
        }

        private void UpdateDistillationState()
        {
            var previousState = distillationState;
            
            float progress = distillationTime / distillationDuration;
            
            if (progress < 0.2f)
                distillationState = DistillationState.Heating;
            else if (progress < 0.3f)
                distillationState = DistillationState.Boiling;
            else if (progress < 0.9f)
                distillationState = DistillationState.Distilling;
            else
                distillationState = DistillationState.Complete;
            
            // Update status label
            if (statusLabel != null)
            {
                statusLabel.text = distillationState switch
                {
                    DistillationState.Heating => "Heating liquid...",
                    DistillationState.Boiling => "Liquid boiling!",
                    DistillationState.Distilling => "Distilling in progress",
                    DistillationState.Complete => "Distillation complete!",
                    DistillationState.Failed => "Process failed!",
                    _ => "Unknown state"
                };
                
                var color = distillationState switch
                {
                    DistillationState.Heating => Color.blue,
                    DistillationState.Boiling => Color.yellow,
                    DistillationState.Distilling => Color.green,
                    DistillationState.Complete => Color.cyan,
                    DistillationState.Failed => Color.red,
                    _ => Color.white
                };
                
                statusLabel.style.color = new StyleColor(color);
            }
            
            // Play sound effects when state changes
            if (previousState != distillationState && audioSource != null)
            {
                PlayStateChangeSound(distillationState);
            }
        }

        private IEnumerator AnimateBubbles()
        {
            if (bubbles == null) yield break;
            
            while (isDistilling)
            {
                // Show bubbles only when boiling or distilling
                bool shouldShowBubbles = distillationState == DistillationState.Boiling || 
                                       distillationState == DistillationState.Distilling;
                
                if (shouldShowBubbles)
                {
                    // Animate bubble intensity based on pressure
                    float intensity = currentPressure * 2f;
                    bubbles.style.opacity = intensity;
                    
                    // Bubble movement animation
                    float offset = Mathf.Sin(Time.time * bubbleSpeed * intensity) * 10f;
                    bubbles.style.translate = new StyleTranslate(new Translate(0, offset));
                    
                    // Scale animation
                    float scale = 1f + Mathf.Sin(Time.time * bubbleSpeed * 2f) * 0.1f;
                    bubbles.style.scale = new StyleScale(new Scale(Vector3.one * scale));
                }
                else
                {
                    bubbles.style.opacity = 0f;
                }
                
                yield return null;
            }
        }

        private IEnumerator AnimateSteam()
        {
            if (steamPipe == null) yield break;
            
            while (isDistilling)
            {
                // Show steam during distilling phase
                bool shouldShowSteam = distillationState == DistillationState.Distilling;
                
                if (shouldShowSteam)
                {
                    // Steam flow animation
                    float flow = Mathf.Sin(Time.time * steamRiseSpeed) * 0.5f + 0.5f;
                    steamPipe.style.opacity = flow * 0.8f;
                    
                    // Steam particle effect simulation
                    float rise = Time.time * steamRiseSpeed * 50f;
                    steamPipe.style.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Top, rise));
                }
                else
                {
                    steamPipe.style.opacity = 0f;
                }
                
                yield return null;
            }
        }

        private IEnumerator AnimatePressureFluctuation()
        {
            while (isDistilling)
            {
                // Add realistic pressure fluctuations
                float fluctuation = Mathf.Sin(Time.time * 3f) * pressureFluctuation;
                float displayPressure = Mathf.Clamp01(currentPressure + fluctuation);
                
                if (pressureGauge != null)
                {
                    pressureGauge.value = displayPressure * 100f;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void CompleteDistillation()
        {
            isDistilling = false;
            StopAllCoroutines();
            
            // Determine success based on distillate amount
            bool success = distillateAmount >= 0.6f; // Need at least 60% efficiency
            Ingredient result = success ? currentIngredient.DistillingResult : null;
            
            distillationState = success ? DistillationState.Complete : DistillationState.Failed;
            UpdateDistillationState();
            
            OnDistillationComplete?.Invoke(success, result);
        }

        private void PlayStateChangeSound(DistillationState state)
        {
            // Placeholder for sound effects
            switch (state)
            {
                case DistillationState.Boiling:
                    // Play bubbling sound
                    break;
                case DistillationState.Distilling:
                    // Play distillation/steam sound
                    break;
                case DistillationState.Complete:
                    // Play success sound
                    break;
                case DistillationState.Failed:
                    // Play failure sound
                    break;
            }
        }

        public void Show()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }

        public void Hide()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
            
            StopAllCoroutines();
        }
    }
}