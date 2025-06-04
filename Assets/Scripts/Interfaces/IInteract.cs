using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interfaces
{
    public interface IInteract {
        void Interact();
        string InteractText { get; }
    }
}