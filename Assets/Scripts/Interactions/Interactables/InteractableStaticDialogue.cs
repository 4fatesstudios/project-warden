using FourFatesStudios.ProjectWarden.Interactions.StaticDialogue;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interactions.Interactables
{
    public class InteractableStaticDialogue : Interactable
    {
        [SerializeField] private string dialogueKnotName;
        private DialogueManager dm;

        private void Awake()
        {
            dm = GameObject.FindWithTag("DialogueManager").GetComponent<DialogueManager>();
        }


        public override void Interact(GameObject interactor)
        {
            if (dm.dialoguePlaying)
            {
                dm.ContinueOrExitStory();
            }
            Debug.Log($"{name} interacted with by {interactor.name}");
            if (!dialogueKnotName.Equals(""))
            {
                Debug.Log("Made it this far");
                DialogueEventSystem.instance.dialogueEvents.EnterDialogue(dialogueKnotName);

            }

        }
    }
}