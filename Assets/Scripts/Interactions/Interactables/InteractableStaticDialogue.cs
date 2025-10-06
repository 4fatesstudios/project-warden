using FourFatesStudios.ProjectWarden.Interactions.StaticDialogue;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interactions.Interactables
{
    public class InteractableStaticDialogue : Interactable
    {
        [SerializeField] private string dialogueKnotName;
        private DialogueManager dm;

        private void Start()
        {
            dm = GameObject.FindWithTag("DialogueManager").GetComponent<DialogueManager>();
        }


        public override void Interact(GameObject interactor)
        {
            if (dm.dialoguePlaying)
            {
                dm.ContinueOrExitStory();
            }
            if (!dialogueKnotName.Equals(""))
            {
                DialogueEventSystem.instance.dialogueEvents.EnterDialogue(dialogueKnotName);

            }

        }
    }
}