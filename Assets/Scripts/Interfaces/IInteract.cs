using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interfaces
{
    public interface IInteract {
        /// <summary>
        /// Performs interaction logic, typically triggered by the player
        /// </summary>
        /// <param name="interactor">The GameObject initiating the interaction</param>
        void Interact(GameObject interactor);
        
        /// <summary>
        /// The text shown to the player when the interactable is "on focus"
        /// For example: "Open", "Talk", or "Pick Up"
        /// </summary>
        string InteractText { get; }
        
        /// <summary>
        /// Determines if object can currently be interacted with
        /// </summary>
        /// <param name="interactor">The GameObject attempting interaction</param>
        /// <returns>True if interaction is allowed; otherwise, false</returns>
        bool CanInteract(GameObject interactor);

        /// <summary>
        /// Called when the interactor, typically the player, targets the interactable
        /// </summary>
        void OnFocus();
        
        /// <summary>
        /// Called when the interactor, typically the player, no longer targets the interactable
        /// </summary>
        void OnUnfocus();
    }
}