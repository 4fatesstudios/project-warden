using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu
{
    public class RoastingMinigameController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Roasting Settings")]
        [SerializeField] private float roastingDuration = 10f;
        [SerializeField] private float perfectRoastWindow = 2f;
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
        private Label instructionsLabel;
        private Label statusLabel;
        
        private Ingredient currentIngredient;
        private float roastingTime = 0f;
        private bool isRoasting = false;
        private float currentTemperature = 0f;
        private RoastingState roastingState = RoastingState.Raw;
        
        public event Action<bool, Ingredient> OnRoastingComplete;
        
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

        public void InitializeRoasting(Ingredient ingredient)
        {
            currentIngredient = ingredient;
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
            instructionsLabel = root.Q<Label>("InstructionsLabel");
            statusLabel = root.Q<Label>("StatusLabel");
            
            stopButton?.RegisterCallback<ClickEvent>(_ => StopRoasting());
            
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
            
            // Hide steam initially
            if (steamEffect != null)
                steamEffect.style.opacity = 0f;
        }

        private void StartRoasting()
        {
            isRoasting = true;
            roastingTime = 0f;
            currentTemperature = 0f;
            roastingState = RoastingState.Raw;
            
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
            
            bool success = roastingState == RoastingState.Perfect;
            Ingredient result = success ? currentIngredient.RoastingResult : null;
            
            OnRoastingComplete?.Invoke(success, result);
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