using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Specialized button component for ingredient selection with built-in hover tooltip support
    /// </summary>
    public class IngredientButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image iconImage;
        
        [Header("Visual Settings")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private bool useGlow = true;
        [SerializeField] private Color glowColor = Color.white;
        
        private Ingredient associatedIngredient;
        private bool isSelected = false;
        private Vector3 originalScale;
        private Color originalColor;
        private Outline glowOutline;
        
        // Events
        public System.Action<Ingredient> OnIngredientClicked;
        public System.Action<Ingredient, Vector2> OnIngredientHovered;
        public System.Action OnIngredientHoverEnded;
        
        private void Awake()
        {
            AutoFindComponents();
            SetupButton();
        }
        
        private void Start()
        {
            originalScale = transform.localScale;
            if (backgroundImage != null)
            {
                originalColor = backgroundImage.color;
            }
        }
        
        private void AutoFindComponents()
        {
            if (button == null)
                button = GetComponent<Button>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (nameText == null)
                nameText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (iconImage == null)
            {
                // Look for an Image component that's not the background
                Image[] images = GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    if (img != backgroundImage && img.name.ToLower().Contains("icon"))
                    {
                        iconImage = img;
                        break;
                    }
                }
            }
        }
        
        private void SetupButton()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            
            // Setup glow effect
            if (useGlow)
            {
                glowOutline = gameObject.GetComponent<Outline>();
                if (glowOutline == null)
                {
                    glowOutline = gameObject.AddComponent<Outline>();
                }
                glowOutline.effectColor = glowColor;
                glowOutline.effectDistance = Vector2.zero;
                glowOutline.enabled = false;
            }
        }
        
        public void SetupIngredient(Ingredient ingredient, GridGameManager gridManager = null)
        {
            associatedIngredient = ingredient;
            
            if (ingredient == null) return;
            
            // Update visual appearance
            UpdateVisuals();
            
            // Update name
            name = $"IngredientBtn_{ingredient.ItemName}";
            
            // Add drag and drop functionality if GridGameManager is provided
            if (gridManager != null)
            {
                DraggableIngredient draggable = GetComponent<DraggableIngredient>();
                if (draggable == null)
                {
                    draggable = gameObject.AddComponent<DraggableIngredient>();
                }
                draggable.Initialize(ingredient, gridManager);
            }
        }
        
        private void UpdateVisuals()
        {
            if (associatedIngredient == null) return;
            
            Color aspectColor = GetAspectColor(associatedIngredient.IngredientAspect);
            
            // Update name text
            if (nameText != null)
            {
                nameText.text = associatedIngredient.ItemName;
                nameText.color = aspectColor;
            }
            
            // Update icon
            if (iconImage != null)
            {
                if (associatedIngredient.ItemIcon != null)
                {
                    iconImage.sprite = associatedIngredient.ItemIcon;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = aspectColor;
                }
            }
            
            // Update button colors using the new system
            UpdateButtonColors();
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateButtonColors();
        }
        
        private void UpdateButtonColors()
        {
            if (button == null || associatedIngredient == null) return;
            
            Color aspectColor = GetAspectColor(associatedIngredient.IngredientAspect);
            ColorBlock colors = button.colors;
            
            if (isSelected)
            {
                // Selected state: lighter version of aspect color
                Color selectedColor = Color.Lerp(aspectColor, Color.white, 0.4f);
                selectedColor.a = 0.8f;
                
                colors.normalColor = selectedColor;
                colors.highlightedColor = Color.Lerp(selectedColor, Color.white, 0.3f);
                colors.pressedColor = Color.Lerp(selectedColor, Color.black, 0.2f);
                colors.selectedColor = selectedColor;
            }
            else
            {
                // Normal state: aspect color with proper hover/click states
                Color normalColor = aspectColor;
                normalColor.a = 0.6f;
                
                // Hover state: darker shade
                Color hoverColor = Color.Lerp(aspectColor, Color.black, 0.3f);
                hoverColor.a = 0.8f;
                
                // Click state: lighter shade
                Color clickColor = Color.Lerp(aspectColor, Color.white, 0.3f);
                clickColor.a = 0.9f;
                
                colors.normalColor = normalColor;
                colors.highlightedColor = hoverColor;
                colors.pressedColor = clickColor;
                colors.selectedColor = normalColor;
            }
            
            button.colors = colors;
            
            // Update background image if available
            if (backgroundImage != null)
            {
                backgroundImage.color = colors.normalColor;
                originalColor = colors.normalColor;
            }
            
            // Update glow effect
            if (glowOutline != null)
            {
                glowOutline.enabled = isSelected;
                if (isSelected)
                {
                    glowOutline.effectDistance = Vector2.one * 2f;
                    glowOutline.effectColor = aspectColor;
                }
                else
                {
                    glowOutline.effectDistance = Vector2.zero;
                }
            }
        }
        
        private void OnButtonClicked()
        {
            if (associatedIngredient != null)
            {
                OnIngredientClicked?.Invoke(associatedIngredient);
                
                // Add click animation
                StartClickAnimation();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Hover animation
            StartHoverAnimation(true);
            
            // Show tooltip
            if (associatedIngredient != null)
            {
                Vector2 screenPos = eventData.position;
                OnIngredientHovered?.Invoke(associatedIngredient, screenPos);
            }
            
            // Enable glow effect
            if (glowOutline != null && !isSelected)
            {
                glowOutline.enabled = true;
                glowOutline.effectDistance = Vector2.one;
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            // Hover animation
            StartHoverAnimation(false);
            
            // Hide tooltip
            OnIngredientHoverEnded?.Invoke();
            
            // Disable glow effect
            if (glowOutline != null && !isSelected)
            {
                glowOutline.enabled = false;
            }
        }
        
        private void StartHoverAnimation(bool hovering)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateScale(hovering ? originalScale * hoverScale : originalScale));
        }
        
        private void StartClickAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(ClickAnimationSequence());
        }
        
        private System.Collections.IEnumerator AnimateScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / animationDuration;
                
                // Use smooth animation curve
                progress = Mathf.SmoothStep(0f, 1f, progress);
                
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }
            
            transform.localScale = targetScale;
        }
        
        private System.Collections.IEnumerator ClickAnimationSequence()
        {
            // Quick scale down
            yield return StartCoroutine(AnimateScale(originalScale * 0.9f));
            
            // Scale back up with slight overshoot
            yield return StartCoroutine(AnimateScale(originalScale * 1.05f));
            
            // Return to normal
            yield return StartCoroutine(AnimateScale(originalScale));
        }
        
        private Color GetAspectColor(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Scorch: return new Color(1f, 0.3f, 0.3f, 1f);
                case Aspect.Frigid: return new Color(0.3f, 0.8f, 1f, 1f);
                case Aspect.Arc: return new Color(1f, 1f, 0.3f, 1f);
                case Aspect.Caustic: return new Color(0.8f, 0.5f, 0.2f, 1f);
                case Aspect.Corporeal: return new Color(0.7f, 0.7f, 0.7f, 1f);
                case Aspect.Divine: return new Color(1f, 1f, 1f, 1f);
                default: return new Color(0.6f, 0.6f, 0.6f, 1f);
            }
        }
        
        // Public properties
        public Ingredient AssociatedIngredient => associatedIngredient;
        public bool IsSelected => isSelected;
        public Button Button => button;
        
        // Utility methods
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
        
        public void SetGlowEnabled(bool enabled)
        {
            useGlow = enabled;
            if (glowOutline != null)
            {
                glowOutline.enabled = enabled && (isSelected || IsHovered());
            }
        }
        
        private bool IsHovered()
        {
            return EventSystem.current.currentSelectedGameObject == gameObject;
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}