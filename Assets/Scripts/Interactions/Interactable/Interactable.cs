using FourFatesStudios.ProjectWarden.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


namespace FourFatesStudios.ProjectWarden.Interactions.Interactable
{
    [RequireComponent(typeof(SphereCollider))]
    public abstract class Interactable : MonoBehaviour, IInteract
    {
        [SerializeField] protected string interactText = "Interact";
        [SerializeField] protected GameObject focusVisual;
        protected Collider InteractCollider;
        protected RectTransform InteractFocusTransform;
        private TextMeshProUGUI _focusText;
        
        private bool _canInteract = true;
        
        public string InteractText => interactText;

        private void Awake() {
            InteractCollider = GetComponent<Collider>();
            InteractFocusTransform = focusVisual.GetComponent<RectTransform>();
            _focusText = focusVisual.GetComponent<TextMeshProUGUI>();
            _focusText.text = interactText;
            focusVisual.SetActive(false);
        }

        public abstract void Interact(GameObject interactor);
        
        public bool CanInteract(GameObject interactor) {
            return _canInteract;
        }

        public void SetCanInteract(bool canInteract) {
            _canInteract = canInteract;
        }
        
        public void OnFocus() {
            Debug.Log($"{name} is focused.");
            focusVisual.SetActive(true);
        }
        
        public void OnUnfocus() {
            Debug.Log($"{name} is unfocused.");
            focusVisual.SetActive(false);
        }
        
    }
}