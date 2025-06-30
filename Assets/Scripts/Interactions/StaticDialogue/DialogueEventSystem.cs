using FourFatesStudios.ProjectWarden.Interactions.StaticDialogue;
using UnityEngine;

public class DialogueEventSystem : MonoBehaviour
{
    public static DialogueEventSystem instance { get; private set; }

    public DialogueEvents dialogueEvents;

    private void OnEnable()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Dialogue Event System in the scene");
        }
        instance = this;

        dialogueEvents = new DialogueEvents();
    }
}
