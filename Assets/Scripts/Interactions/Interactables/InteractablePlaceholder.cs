using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interactions.Interactables
{
    public class InteractablePlaceholder : Interactable
    {
        public override void Interact(GameObject interactor) {
            Debug.LogWarning("Placeholder interactable used! Replace with real one!");
        }
    }
}