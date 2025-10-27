using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.UI;

namespace FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu
{
    public class GrindingMinigameController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Grinding Settings")]
        [SerializeField] private float grindingDuration = 8f;
        [SerializeField] private float requiredGrinds = 20f;
        [SerializeField] private float perfectRhythmWindow = 0.2f;
        [SerializeField] private float rhythmBeatInterval = 1f;
        
        [Header("Animation Settings")]
        [SerializeField] private float pestleShakeIntensity = 0.2f;
        [SerializeField] private float particleIntensity = 1f;
        [SerializeField] private float mortarVibrationAmount = 0.05f;
        
        private VisualElement mortar;
        private VisualElement pestle;
        private VisualElement ingredient;
        private VisualElement grindParticles;
        private VisualElement rhythmIndicator;
        private ProgressBar grindingProgress;
        private ProgressBar rhythmMeter;
        private Button grindButton;
        private Button backButton;  // Add back button
        private Label instructionsLabel;
        private Label statusLabel;
        private Label grindCountLabel;
        
        private Ingredient currentIngredient;
        private float grindingTime = 0f;
        private bool isGrinding = false;
        private int grindCount = 0;
        private float lastBeatTime = 0f;
        private float nextBeatTime = 0f;
        private bool isInRhythmWindow = false;
        private int perfectGrinds = 0;
        private int totalGrinds = 0;
        private GrindingState grindingState = GrindingState.Ready;
        
        public event Action<bool, Ingredient> OnGrindingComplete;
        public event Action OnBackPressed;

        private enum GrindingState
        {
            Ready,
            Grinding,
            Complete,
            Failed
        }

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        public void SetTargetIngredient(Ingredient ingredient)
        {
            currentIngredient = ingredient;
            
            if (uiDocument?.rootVisualElement != null)
            {
                SetupUI();
                
                Debug.Log($"🔨 Grinding minigame set up for ingredient: {ingredient.ItemName}");
                
                // Show visual feedback that ingredient is loaded
                if (statusLabel != null)
                {
                    statusLabel.text = $"Ready to grind {ingredient.ItemName}";
                    statusLabel.style.color = new StyleColor(Color.white);
                }
                
                if (instructionsLabel != null)
                {
                    instructionsLabel.text = $"Click the grind button in rhythm to grind {ingredient.ItemName}. Match the beat for best results!";
                }
                
                // Reset state
                grindingState = GrindingState.Ready;
                grindCount = 0;
                perfectGrinds = 0;
                totalGrinds = 0;
                UpdateGrindCountDisplay();
            }
        }
        
        public void InitializeGrinding(Ingredient ingredient)
        {
            currentIngredient = ingredient;
            SetupUI();
            StartGrinding();
        }

        private void SetupUI()
        {
            var root = uiDocument.rootVisualElement;
            
            mortar = root.Q<VisualElement>("Mortar");
            pestle = root.Q<VisualElement>("Pestle");
            ingredient = root.Q<VisualElement>("IngredientPieces");
            grindParticles = root.Q<VisualElement>("GrindParticles");
            rhythmIndicator = root.Q<VisualElement>("RhythmIndicator");
            grindingProgress = root.Q<ProgressBar>("GrindingProgress");
            rhythmMeter = root.Q<ProgressBar>("RhythmMeter");
            grindButton = root.Q<Button>("GrindButton");
            backButton = root.Q<Button>("BackButton");  // Get back button
            instructionsLabel = root.Q<Label>("InstructionsLabel");
            statusLabel = root.Q<Label>("StatusLabel");
            grindCountLabel = root.Q<Label>("GrindCountLabel");
            
            grindButton?.RegisterCallback<ClickEvent>(_ => PerformGrind());
            backButton?.RegisterCallback<ClickEvent>(_ => NavigateBack());  // Register back button
            
            // Set ingredient appearance
            if (ingredient != null && currentIngredient?.ItemIcon != null)
            {
                ingredient.style.backgroundImage = new StyleBackground(currentIngredient.ItemIcon);
            }
            
            // Initialize UI state
            grindingProgress.value = 0f;
            rhythmMeter.value = 0f;
            instructionsLabel.text = $"Grinding {currentIngredient?.ItemName}. Click in rhythm for better results!";
            statusLabel.text = "Ready to grind";
            grindCountLabel.text = $"0/{(int)requiredGrinds}";
            
            // Hide particles initially
            if (grindParticles != null)
                grindParticles.style.opacity = 0f;
        }

        private void StartGrinding()
        {
            isGrinding = true;
            grindingTime = 0f;
            grindCount = 0;
            perfectGrinds = 0;
            totalGrinds = 0;
            grindingState = GrindingState.Ready;
            nextBeatTime = rhythmBeatInterval;
            
            StartCoroutine(GrindingProcess());
            StartCoroutine(AnimateRhythmIndicator());
            StartCoroutine(UpdateRhythmMeter());
        }

        private void PerformGrind()
        {
            if (!isGrinding || grindingState == GrindingState.Complete) return;
            
            totalGrinds++;
            grindingState = GrindingState.Grinding;
            
            // Check if grind was performed in rhythm
            bool inRhythm = isInRhythmWindow;
            if (inRhythm)
            {
                perfectGrinds++;
                grindCount += 2; // Perfect grinds count double
                ShowPerfectGrindEffect();
            }
            else
            {
                grindCount += 1;
            }
            
            // Update UI
            UpdateGrindingUI();
            
            // Start animations
            StartCoroutine(AnimatePestleGrind(inRhythm));
            StartCoroutine(AnimateParticleEffect(inRhythm));
            StartCoroutine(AnimateMortarVibration());
            
            // Play sound
            PlayGrindSound(inRhythm);
            
            // Check completion
            if (grindCount >= requiredGrinds)
            {
                CompleteGrinding();
            }
        }

        /// <summary>
        /// Navigate back to refinement menu
        /// </summary>
        private void NavigateBack()
        {
            // Stop grinding if in progress
            if (isGrinding)
            {
                isGrinding = false;
                grindingState = GrindingState.Complete;
                StopAllCoroutines();
            }
            
            OnBackPressed?.Invoke();
            Hide();
        }

        private void UpdateGrindCountDisplay()
        {
            if (grindCountLabel != null)
            {
                grindCountLabel.text = $"{grindCount}/{(int)requiredGrinds}";
            }
        }

        private void UpdateGrindingUI()
        {
            // Update progress bar
            if (grindingProgress != null)
            {
                float progress = Mathf.Clamp01(grindCount / requiredGrinds) * 100f;
                grindingProgress.value = progress;
            }
            
            // Update grind count
            if (grindCountLabel != null)
            {
                grindCountLabel.text = $"{grindCount}/{(int)requiredGrinds}";
            }
            
            // Update status
            if (statusLabel != null)
            {
                float accuracy = totalGrinds > 0 ? (float)perfectGrinds / totalGrinds : 0f;
                statusLabel.text = $"Grinding... Accuracy: {accuracy:P0}";
                
                Color statusColor = accuracy switch
                {
                    >= 0.8f => Color.green,
                    >= 0.6f => Color.yellow,
                    >= 0.4f => new Color(1f, 0.5f, 0f),
                    _ => Color.red
                };
                
                statusLabel.style.color = new StyleColor(statusColor);
            }
        }

        private IEnumerator GrindingProcess()
        {
            while (isGrinding && grindingTime < grindingDuration)
            {
                grindingTime += Time.deltaTime;
                
                // Update rhythm timing
                float timeSinceLastBeat = grindingTime - lastBeatTime;
                
                if (timeSinceLastBeat >= rhythmBeatInterval)
                {
                    lastBeatTime = grindingTime;
                    nextBeatTime = grindingTime + rhythmBeatInterval;
                }
                
                // Check if we're in rhythm window
                float timeToNextBeat = nextBeatTime - grindingTime;
                isInRhythmWindow = timeToNextBeat <= perfectRhythmWindow;
                
                yield return null;
            }
            
            if (isGrinding && grindCount < requiredGrinds)
            {
                // Time ran out
                CompleteGrinding();
            }
        }

        private IEnumerator AnimateRhythmIndicator()
        {
            if (rhythmIndicator == null) yield break;
            
            while (isGrinding)
            {
                // Pulse rhythm indicator
                float timeToNextBeat = nextBeatTime - grindingTime;
                float beatProgress = 1f - (timeToNextBeat / rhythmBeatInterval);
                
                // Scale animation
                float scale = 1f + Mathf.Sin(beatProgress * Mathf.PI) * 0.3f;
                rhythmIndicator.style.scale = new StyleScale(new Scale(Vector3.one * scale));
                
                // Color animation
                Color indicatorColor = isInRhythmWindow ? Color.green : Color.white;
                rhythmIndicator.style.backgroundColor = new StyleColor(indicatorColor);
                
                // Opacity pulse
                float opacity = 0.5f + Mathf.Sin(beatProgress * Mathf.PI * 2f) * 0.3f;
                rhythmIndicator.style.opacity = opacity;
                
                yield return null;
            }
        }

        private IEnumerator UpdateRhythmMeter()
        {
            if (rhythmMeter == null) yield break;
            
            while (isGrinding)
            {
                float timeToNextBeat = nextBeatTime - grindingTime;
                float meterValue = (1f - (timeToNextBeat / rhythmBeatInterval)) * 100f;
                
                rhythmMeter.value = meterValue;
                
                // Change meter color based on rhythm window
                Color meterColor = isInRhythmWindow ? Color.green : Color.cyan;
                rhythmMeter.Q<VisualElement>("unity-progress-bar").style.backgroundColor = new StyleColor(meterColor);
                
                yield return null;
            }
        }

        private IEnumerator AnimatePestleGrind(bool perfectGrind)
        {
            if (pestle == null) yield break;
            
            Vector3 originalPosition = Vector3.zero;
            float animDuration = perfectGrind ? 0.3f : 0.2f;
            float shakeAmount = perfectGrind ? pestleShakeIntensity * 1.5f : pestleShakeIntensity;
            
            float elapsed = 0f;
            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animDuration;
                
                // Downward motion then return
                float yOffset = Mathf.Sin(progress * Mathf.PI) * -20f;
                
                // Add shake effect
                Vector3 shake = new Vector3(
                    UnityEngine.Random.Range(-shakeAmount, shakeAmount),
                    yOffset,
                    0f
                );
                
                pestle.style.translate = new StyleTranslate(new Translate(shake.x, shake.y));
                
                // Rotation for more dynamic feel
                float rotation = Mathf.Sin(progress * Mathf.PI * 4f) * 5f;
                pestle.style.rotate = new StyleRotate(new Rotate(rotation));
                
                yield return null;
            }
            
            // Reset position and rotation
            pestle.style.translate = new StyleTranslate(new Translate(0, 0));
            pestle.style.rotate = new StyleRotate(new Rotate(0));
        }

        private IEnumerator AnimateParticleEffect(bool perfectGrind)
        {
            if (grindParticles == null) yield break;
            
            float intensity = perfectGrind ? particleIntensity * 2f : particleIntensity;
            float duration = perfectGrind ? 0.8f : 0.5f;
            
            // Show particles
            grindParticles.style.opacity = intensity;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Particle spread animation
                float spread = elapsed / duration * 50f;
                grindParticles.style.scale = new StyleScale(new Scale(Vector3.one * (1f + spread * 0.01f)));
                
                // Fade out particles
                float fadeOut = 1f - (elapsed / duration);
                grindParticles.style.opacity = intensity * fadeOut;
                
                yield return null;
            }
            
            // Reset particles
            grindParticles.style.opacity = 0f;
            grindParticles.style.scale = new StyleScale(new Scale(Vector3.one));
        }

        private IEnumerator AnimateMortarVibration()
        {
            if (mortar == null) yield break;
            
            float duration = 0.4f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                Vector3 vibration = new Vector3(
                    UnityEngine.Random.Range(-mortarVibrationAmount, mortarVibrationAmount),
                    UnityEngine.Random.Range(-mortarVibrationAmount, mortarVibrationAmount),
                    0f
                );
                
                mortar.style.translate = new StyleTranslate(new Translate(vibration.x, vibration.y));
                
                yield return new WaitForSeconds(0.05f);
            }
            
            // Reset position
            mortar.style.translate = new StyleTranslate(new Translate(0, 0));
        }

        private void ShowPerfectGrindEffect()
        {
            StartCoroutine(PerfectGrindFlash());
        }

        private IEnumerator PerfectGrindFlash()
        {
            // Flash the screen or UI to indicate perfect grind
            var root = uiDocument.rootVisualElement;
            var flashOverlay = new VisualElement();
            flashOverlay.style.position = Position.Absolute;
            flashOverlay.style.left = 0;
            flashOverlay.style.top = 0;
            flashOverlay.style.right = 0;
            flashOverlay.style.bottom = 0;
            flashOverlay.style.backgroundColor = new StyleColor(new Color(1f, 1f, 0f, 0.3f));
            
            root.Add(flashOverlay);
            
            yield return new WaitForSeconds(0.1f);
            
            root.Remove(flashOverlay);
        }

        private void CompleteGrinding()
        {
            isGrinding = false;
            StopAllCoroutines();
            
            // Calculate success based on grind completion and accuracy
            float completion = grindCount / requiredGrinds;
            float accuracy = totalGrinds > 0 ? (float)perfectGrinds / totalGrinds : 0f;
            
            bool success = completion >= 0.8f && accuracy >= 0.5f;
            Ingredient result = success ? currentIngredient.GrindingResult : null;
            
            grindingState = success ? GrindingState.Complete : GrindingState.Failed;
            
            if (statusLabel != null)
            {
                statusLabel.text = success ? "Grinding complete!" : "Grinding failed - try again!";
                statusLabel.style.color = new StyleColor(success ? Color.green : Color.red);
            }
            
            OnGrindingComplete?.Invoke(success, result);
        }

        private void PlayGrindSound(bool perfectGrind)
        {
            if (audioSource == null) return;
            
            // Play appropriate grinding sound
            // You would load different audio clips for normal vs perfect grinds
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