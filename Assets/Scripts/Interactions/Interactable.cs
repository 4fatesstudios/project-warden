using FourFatesStudios.ProjectWarden.Interfaces;
using UnityEngine;
using UnityEngine.Events;


namespace FourFatesStudios.ProjectWarden.Interactions
{
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour, IInteract
    {
        [SerializeField] protected string interactText = "Interact";
        
        private bool _canInteract = true;
        
        public string InteractText => interactText;

        public abstract void Interact(GameObject interactor);
        
        public bool CanInteract(GameObject interactor) {
            return _canInteract;
        }

        public void SetCanInteract(bool canInteract) {
            _canInteract = canInteract;
        }
        
        public void OnFocus() {
            Debug.Log($"{name} is focused.");
        }
        
        public void OnUnfocus() {
            Debug.Log($"{name} is unfocused.");
        }
        
    }
}