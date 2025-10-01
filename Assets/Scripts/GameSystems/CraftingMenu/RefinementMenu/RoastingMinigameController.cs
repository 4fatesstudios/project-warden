using System;
using System.Collections;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu
{
    public class RoastingMinigameController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Roasting Settings")]
        [SerializeField] private float roastingDuration = 10f;
        //[SerializeField] private float perfectRoastWindow = 2f;
        [SerializeField] private float burnThreshold = 15f;
        
        [Header("Animation Settings")]
        [SerializeField] private float fireFlickerSpeed = 2f;
        [SerializeField] private float panShakeIntensity = 0.1f;
        [SerializeField] private float steamOpacityMax = 0.8f;
        
        private VisualElement fryingPan;
        private VisualElement fireFlames;
        private VisualElement ingredient;
        private VisualElement steamEffect;
        private ProgressBar roastingProgress;
        private ProgressBar temperatureGauge;
        private Button stopButton;
        private Button startButton;
        private Button backButton;  // Add back button
        private Label instructionsLabel;
        private Label statusLabel;
        
        private Ingredient currentIngredient;
        private float roastingTime;
        private bool isRoasting;
        private float currentTemperature;
        private RoastingState roastingState = RoastingState.Raw;
        
        public event Action<bool, Ingredient> OnRoastingComplete;
        public event Action OnBackPressed;  // Add back event
        
        private enum RoastingState
        {
            Raw,
            Cooking,
            Perfect,
            Overcooked,
            Burned
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
                
                Debug.Log($"🔥 Roasting minigame set up for ingredient: {ingredient.ItemName}");
                
                // Show visual feedback that ingredient is loaded
                if (statusLabel != null)
                {
                    statusLabel.text = $"Ready to roast {ingredient.ItemName}";
                    statusLabel.style.color = new StyleColor(Color.white);
                }
                
                if (instructionsLabel != null)
                {
                    instructionsLabel.text = $"Click Start to begin roasting {ingredient.ItemName}. Watch the temperature and stop at the perfect moment!";
                }
            }
        }

        public void InitializeRoasting(Ingredient ingredientTemp)
        {
            currentIngredient = ingredientTemp;
            SetupUI();
            StartRoasting();
        }

        private void SetupUI()
        {
            var root = uiDocument.rootVisualElement;
            
            fryingPan = root.Q<VisualElement>("FryingPan");
            fireFlames = root.Q<VisualElement>("FireFlames");
            ingredient = root.Q<VisualElement>("IngredientSprite");
            steamEffect = root.Q<VisualElement>("SteamEffect");
            roastingProgress = root.Q<ProgressBar>("RoastingProgress");
            temperatureGauge = root.Q<ProgressBar>("TemperatureGauge");
            stopButton = root.Q<Button>("StopButton");
            startButton = root.Q<Button>("StartButton");
            backButton = root.Q<Button>("BackButton");
            instructionsLabel = root.Q<Label>("InstructionsLabel");
            statusLabel = root.Q<Label>("StatusLabel");
            
            stopButton?.RegisterCallback<ClickEvent>(_ => StopRoasting());
            startButton?.RegisterCallback<ClickEvent>(_ => StartRoasting());
            backButton?.RegisterCallback<ClickEvent>(_ => NavigateBack());
            
            // Set ingredient sprite
            if (ingredient != null && currentIngredient?.ItemIcon != null)
            {
                ingredient.style.backgroundImage = new StyleBackground(currentIngredient.ItemIcon);
            }
            
            // Initialize UI state
            roastingProgress.value = 0f;
            temperatureGauge.value = 0f;
            instructionsLabel.text = $"Roasting {currentIngredient?.ItemName}. Watch the temperature and stop at the perfect moment!";
            statusLabel.text = "Raw";
            
            // Show/hide buttons based on state
            if (startButton != null)
            {
                startButton.style.display = isRoasting ? DisplayStyle.None : DisplayStyle.Flex;
                startButton.text = $"Start Roasting {currentIngredient?.ItemName}";
            }
            
            if (stopButton != null)
            {
                stopButton.style.display = isRoasting ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            // Hide steam initially
            if (steamEffect != null)
                steamEffect.style.opacity = 0f;
        }

        private void StartRoasting()
        {
            if (currentIngredient == null)
            {
                Debug.LogWarning("Cannot start roasting: No ingredient selected");
                return;
            }
            
            if (isRoasting)
            {
                Debug.LogWarning("Roasting already in progress");
                return;
            }
            
            isRoasting = true;
            roastingTime = 0f;
            currentTemperature = 0f;
            roastingState = RoastingState.Raw;
            
            // Update button visibility
            if (startButton != null)
                startButton.style.display = DisplayStyle.None;
            if (stopButton != null)
                stopButton.style.display = DisplayStyle.Flex;
            
            // Update instructions
            if (instructionsLabel != null)
                instructionsLabel.text = $"Roasting {currentIngredient.ItemName} in progress! Watch the temperature and stop at the perfect moment!";
            
            Debug.Log($"🔥 Started roasting {currentIngredient.ItemName}");
            
            StartCoroutine(RoastingProcess());
            StartCoroutine(AnimateFireFlames());
            StartCoroutine(AnimatePanShaking());
            StartCoroutine(AnimateSteam());
        }

        private void StopRoasting()
        {
            if (!isRoasting) return;
            
            isRoasting = false;
            StopAllCoroutines();
            
            // Update button visibility
            if (startButton != null)
            {
                startButton.style.display = DisplayStyle.Flex;
                startButton.text = "Start New Roast";
            }
            if (stopButton != null)
                stopButton.style.display = DisplayStyle.None;
            
            bool success = roastingState == RoastingState.Perfect;
            Ingredient result = success ? currentIngredient.RoastingResult : null;
            
            // Update status based on result
            if (statusLabel != null)
            {
                if (success)
                {
                    statusLabel.text = $"SUCCESS! {currentIngredient.ItemName} roasted perfectly!";
                    statusLabel.style.color = new StyleColor(Color.green);
                }
                else
                {
                    statusLabel.text = $"Failed to roast {currentIngredient.ItemName} properly";
                    statusLabel.style.color = new StyleColor(Color.red);
                }
            }
            
            if (instructionsLabel != null)
            {
                instructionsLabel.text = success ? 
                    $"Perfect! You successfully roasted {currentIngredient.ItemName}!" :
                    $"Better luck next time! Try roasting {currentIngredient.ItemName} again.";
            }
            
            Debug.Log($"🔥 Roasting completed. Success: {success}, Result: {result?.ItemName ?? "None"}");
            
            OnRoastingComplete?.Invoke(success, result);
        }

        /// <summary>
        /// Navigate back to refinement menu
        /// </summary>
        private void NavigateBack()
        {
            // Stop roasting if in progress
            if (isRoasting)
            {
                isRoasting = false;
                StopAllCoroutines();
            }
            
            // Try to use CraftingNavigationController navigation
            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            if (navigationController != null)
            {
                navigationController.ShowRefinement();
            }
            else
            {
                // Fallback to event
                OnBackPressed?.Invoke();
            }
        }

        private IEnumerator RoastingProcess()
        {
            while (isRoasting && roastingTime < burnThreshold)
            {
                roastingTime += Time.deltaTime;
                
                // Update temperature (non-linear heating)
                currentTemperature = Mathf.Pow(roastingTime / roastingDuration, 1.5f);
                
                // Update progress bars
                if (roastingProgress != null)
                    roastingProgress.value = roastingTime / roastingDuration * 100f;
                
                if (temperatureGauge != null)
                {
                    temperatureGauge.value = Mathf.Clamp01(currentTemperature) * 100f;
                    
                    // Change color based on temperature
                    var color = GetTemperatureColor(currentTemperature);
                    temperatureGauge.Q<VisualElement>("unity-progress-bar").style.backgroundColor = new StyleColor(color);
                }
                
                // Update roasting state
                UpdateRoastingState();
                
                // Update ingredient appearance
                UpdateIngredientAppearance();
                
                // Auto-stop if burned
                if (roastingState == RoastingState.Burned)
                {
                    StopRoasting();
                    break;
                }
                
                yield return null;
            }
            
            if (isRoasting)
            {
                StopRoasting();
            }
        }

        private void UpdateRoastingState()
        {
            var previousState = roastingState;
            
            if (currentTemperature < 0.6f)
                roastingState = RoastingState.Raw;
            else if (currentTemperature < 0.8f)
                roastingState = RoastingState.Cooking;
            else if (currentTemperature < 1.0f)
                roastingState = RoastingState.Perfect;
            else if (currentTemperature < 1.3f)
                roastingState = RoastingState.Overcooked;
            else
                roastingState = RoastingState.Burned;
            
            // Update status label
            if (statusLabel != null)
            {
                statusLabel.text = roastingState switch
                {
                    RoastingState.Raw => "Raw",
                    RoastingState.Cooking => "Cooking...",
                    RoastingState.Perfect => "PERFECT! Stop now!",
                    RoastingState.Overcooked => "Getting overcooked!",
                    RoastingState.Burned => "BURNED!",
                    _ => "Unknown"
                };
                
                var color = roastingState switch
                {
                    RoastingState.Raw => Color.white,
                    RoastingState.Cooking => Color.yellow,
                    RoastingState.Perfect => Color.green,
                    RoastingState.Overcooked => new Color(1f, 0.5f, 0f),
                    RoastingState.Burned => Color.red,
                    _ => Color.white
                };
                
                statusLabel.style.color = new StyleColor(color);
            }
            
            // Play sound effects when state changes
            if (previousState != roastingState && audioSource != null)
            {
                PlayStateChangeSound(roastingState);
            }
        }

        private void UpdateIngredientAppearance()
        {
            if (ingredient == null) return;
            
            // Change ingredient color/tint based on roasting state
            var tint = roastingState switch
            {
                RoastingState.Raw => Color.white,
                RoastingState.Cooking => new Color(1f, 0.9f, 0.8f),
                RoastingState.Perfect => new Color(0.8f, 0.6f, 0.4f),
                RoastingState.Overcooked => new Color(0.6f, 0.4f, 0.2f),
                RoastingState.Burned => new Color(0.3f, 0.2f, 0.1f),
                _ => Color.white
            };
            
            ingredient.style.unityBackgroundImageTintColor = new StyleColor(tint);
        }

        private Color GetTemperatureColor(float temperature)
        {
            if (temperature < 0.5f)
                return Color.Lerp(Color.blue, Color.green, temperature * 2f);
            else if (temperature < 1f)
                return Color.Lerp(Color.green, Color.yellow, (temperature - 0.5f) * 2f);
            else if (temperature < 1.3f)
                return Color.Lerp(Color.yellow, Color.red, (temperature - 1f) / 0.3f);
            else
                return Color.red;
        }

        private IEnumerator AnimateFireFlames()
        {
            if (fireFlames == null) yield break;
            
            while (isRoasting)
            {
                // Flicker animation
                float flicker = Mathf.Sin(Time.time * fireFlickerSpeed) * 0.2f + 0.8f;
                fireFlames.style.opacity = flicker;
                
                // Scale animation
                float scale = 1f + Mathf.Sin(Time.time * fireFlickerSpeed * 0.7f) * 0.1f;
                fireFlames.style.scale = new StyleScale(new Scale(Vector3.one * scale));
                
                // Color animation (orange to red)
                float hue = Mathf.PingPong(Time.time * 0.5f, 0.1f);
                Color fireColor = Color.HSVToRGB(hue, 1f, 1f);
                fireFlames.style.unityBackgroundImageTintColor = new StyleColor(fireColor);
                
                yield return null;
            }
        }

        private IEnumerator AnimatePanShaking()
        {
            if (fryingPan == null) yield break;
            
            Vector3 originalPosition = Vector3.zero;
            
            while (isRoasting)
            {
                // Gentle shaking based on temperature
                float shakeAmount = currentTemperature * panShakeIntensity;
                Vector3 shake = new Vector3(
                    UnityEngine.Random.Range(-shakeAmount, shakeAmount),
                    UnityEngine.Random.Range(-shakeAmount, shakeAmount),
                    0f
                );
                
                fryingPan.style.translate = new StyleTranslate(new Translate(shake.x, shake.y));
                
                yield return new WaitForSeconds(0.1f);
            }
            
            // Reset position
            fryingPan.style.translate = new StyleTranslate(new Translate(0, 0));
        }

        private IEnumerator AnimateSteam()
        {
            if (steamEffect == null) yield break;
            
            while (isRoasting)
            {
                // Steam becomes visible as temperature increases
                float steamAlpha = Mathf.Clamp01((currentTemperature - 0.3f) / 0.4f) * steamOpacityMax;
                steamEffect.style.opacity = steamAlpha;
                
                // Steam rising animation
                float rise = Mathf.Sin(Time.time * 2f) * 5f;
                steamEffect.style.translate = new StyleTranslate(new Translate(0, rise));
                
                yield return null;
            }
        }

        private void PlayStateChangeSound(RoastingState state)
        {
            // Placeholder for sound effects
            // You would load and play appropriate audio clips here
            switch (state)
            {
                case RoastingState.Cooking:
                    // Play sizzling sound
                    break;
                case RoastingState.Perfect:
                    // Play success chime
                    break;
                case RoastingState.Burned:
                    // Play burning/smoke sound
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