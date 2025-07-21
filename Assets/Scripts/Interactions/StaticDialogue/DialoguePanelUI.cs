using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using JetBrains.Annotations;
using UnityEngine.UI;
using FourFatesStudios.ProjectWarden.Interactions.StaticDialogue;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("NOTE: SpeakerText is a variable stored in the Ink story")]
    [Header("Components")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private DialogueChoiceButton[] choiceButtons;

    [Header("Speaker Profile Image Components")]
    [SerializeField] NameToProfileSO nameToProfileSO;
    [SerializeField] Image speakerImage;

    [SerializeField] Animator animator;
    private void Awake()
    {

        ResetPanel();
    }

    private void OnEnable()
    {
        DialogueEventSystem.instance.dialogueEvents.onDialogueStarted += DialogueStarted;
        DialogueEventSystem.instance.dialogueEvents.onDialogueFinished += DialogueFinished;
        DialogueEventSystem.instance.dialogueEvents.onDisplayDialogue += DisplayDialogue;

    }

    private void OnDisable()
    {
        DialogueEventSystem.instance.dialogueEvents.onDialogueStarted -= DialogueStarted;
        DialogueEventSystem.instance.dialogueEvents.onDialogueFinished -= DialogueFinished;
        DialogueEventSystem.instance.dialogueEvents.onDisplayDialogue -= DisplayDialogue;

    }

    private void DialogueStarted()
    {

        animator.SetTrigger("PopIn");
        animator.ResetTrigger("PopOut");
    }

    private void DialogueFinished()
    {

        animator.SetTrigger("PopOut");
        animator.ResetTrigger("PopIn");
        
        // reset anything for next time
        ResetPanel();
    }

    private void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices, string speakerName)
    {
        dialogueText.text = dialogueLine;
        nameText.text = speakerName;

        // defensive check - if there are more choices coming in than we can support, log an error
        if (dialogueChoices.Count > choiceButtons.Length)
        {
            Debug.LogError("More dialogue choices ("
                + dialogueChoices.Count + ") came through than are supported ("
                + choiceButtons.Length + ").");
        }

        // start with all of the choice buttons hidden
        foreach (DialogueChoiceButton choiceButton in choiceButtons)
        {
            choiceButton.gameObject.SetActive(false);
        }

        if (dialogueChoices.Count > 0)
        {
            //Display the player in the SpeakerProfile image when choosing dialogue options
            speakerImage.sprite = nameToProfileSO.GetSpriteFromName("Aramis");

        }
        else
        {
            //Display the speaker when not choosing dialogue option
            speakerImage.sprite = nameToProfileSO.GetSpriteFromName(speakerName);
        }


        // enable and set info for buttons depending on ink choice information
        int choiceButtonIndex = dialogueChoices.Count - 1;


        for (int inkChoiceIndex = 0; inkChoiceIndex < dialogueChoices.Count; inkChoiceIndex++)
        {
            Choice dialogueChoice = dialogueChoices[inkChoiceIndex];
            DialogueChoiceButton choiceButton = choiceButtons[choiceButtonIndex];

            choiceButton.gameObject.SetActive(true);
            choiceButton.SetChoiceText(dialogueChoice.text);
            choiceButton.SetChoiceIndex(inkChoiceIndex);

            if (inkChoiceIndex == 0)
            {
                choiceButton.SelectButton();
                DialogueEventSystem.instance.dialogueEvents.UpdateChoiceIndex(0);
            }

            choiceButtonIndex--;
        }
    }

    private void ResetPanel()
    {
        dialogueText.text = "";
    }
}