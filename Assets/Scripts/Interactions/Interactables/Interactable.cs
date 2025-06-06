using FourFatesStudios.ProjectWarden.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


namespace FourFatesStudios.ProjectWarden.Interactions.Interactables
{
    [RequireComponent(typeof(SphereCollider))]
    public abstract class Interactable : MonoBehaviour, IInteract
    {
        [SerializeField, Tooltip("Text that will be displayed when interactable is focused on.")] 
        protected string interactText = "Interact";
        
        [SerializeField, Tooltip("Focus visual GameObject.")] 
        protected GameObject focusVisual;
        
        protected Collider InteractCollider;
        protected RectTransform InteractFocusTransform;
        private TextMeshPro _focusText;
        
        private bool _canInteract = true;
        
        public string InteractText => interactText;

        private void Awake() {
            InteractCollider = GetComponent<Collider>();
            InteractFocusTransform = focusVisual.GetComponent<RectTransform>();
            _focusText = focusVisual.GetComponent<TextMeshPro>();
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