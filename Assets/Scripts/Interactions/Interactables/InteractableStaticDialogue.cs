using FourFatesStudios.ProjectWarden.Interactions.StaticDialogue;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interactions.Interactables
{
    public class InteractableStaticDialogue : Interactable
    {
        public ScriptedDialogueSO dialogueSO;


        public override void Interact(GameObject interactor)
        {
            Debug.Log($"{name} interacted with by {interactor.name}");
            interactor.transform.parent.GetComponentInChildren<DialogueDisplay>().StartDialogue(dialogueSO.lines);

        }
    }
}