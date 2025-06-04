using FourFatesStudios.ProjectWarden.Interfaces;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class InteractableComponent : MonoBehaviour, IInteract
    {
        [SerializeField] protected string interactText = "Interact";
        private bool _canInteract = true;
        
        public string InteractText => interactText;
        
        public void Interact(GameObject interactor) {
            throw new System.NotImplementedException();
        }
        
        public bool CanInteract(GameObject interactor) {
            return _canInteract;
        }
        
        public void OnFocus() {
            throw new System.NotImplementedException();
        }
        
        public void OnUnfocus() {
            throw new System.NotImplementedException();
        }
        
    }
}