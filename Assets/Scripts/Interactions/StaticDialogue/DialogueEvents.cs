using UnityEngine;
using System;
using Ink.Runtime;
using System.Collections.Generic;


namespace FourFatesStudios.ProjectWarden.Interactions.StaticDialogue
{
    public class DialogueEvents
    {
        public event Action<string> onEnterDialogue;
        public void EnterDialogue(string knotName)
        {
            if (onEnterDialogue != null)
            {
                onEnterDialogue(knotName);
            }
        }
        public event Action onDialogueStarted;
        public void DialogueStarted()
        {
            if (onDialogueStarted != null)
            {
                onDialogueStarted();
            }
        }
        public event Action onDialogueFinished;
        public void DialogueFinished()
        {
            if (onDialogueFinished != null)
            {
                onDialogueFinished();
            }
        } 

        public event Action<string, List<Choice>, string> onDisplayDialogue;
        public void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices, string speakerName)
        {
            if (onDisplayDialogue != null)
            {
                onDisplayDialogue(dialogueLine, dialogueChoices, speakerName);
            }
        }

        public event Action<int> onUpdateChoiceIndex;
        public void UpdateChoiceIndex(int choiceIndex)
        {
            if (onUpdateChoiceIndex != null)
            {
                onUpdateChoiceIndex(choiceIndex);
            }
        }
    }
}