using UnityEngine;
using Ink.Runtime;
using UnityEngine.InputSystem;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.QuestSystem;
using System.Collections;
using System.Threading;

namespace FourFatesStudios.ProjectWarden.Interactions.StaticDialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("Ink Story")]
        [SerializeField] private TextAsset inkJson;
        private int currentChoiceIndex = -1;
        public bool dialoguePlaying = false;

        private PlayerMovement pm;

        private Story story;

        private void Awake()
        {
            story = new Story(inkJson.text);
            pm = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
            
            // Bind external functions for quest system
            BindQuestExternalFunctions();
        }

        /// <summary>
        /// Binds quest-related external functions to the Ink story
        /// </summary>
        private void BindQuestExternalFunctions()
        {
            // Function to check if a quest can be accepted
            story.BindExternalFunction("can_accept_quest", (string questId) =>
            {
                if (QuestManager.instance != null)
                {
                    bool canAccept = QuestManager.instance.CanAcceptQuest(questId);
                    Debug.Log($"Checking if quest '{questId}' can be accepted: {canAccept}");
                    return canAccept;
                }
                Debug.LogWarning("QuestManager instance not found when checking quest acceptance");
                return false;
            });

            // Function to accept a quest
            story.BindExternalFunction("accept_quest", (string questId) =>
            {
                if (QuestManager.instance != null)
                {
                    bool accepted = QuestManager.instance.TryAcceptQuestById(questId);
                    Debug.Log($"Attempting to accept quest '{questId}': {accepted}");
                    
                    // Trigger quest event
                    if (QuestEventSystem.instance != null)
                    {
                        QuestEventSystem.instance.questEvents.QuestAcceptanceAttempt(questId, accepted);
                    }
                    
                    return accepted;
                }
                Debug.LogWarning("QuestManager instance not found when accepting quest");
                return false;
            });
        }

        private void OnEnable()
        {
            DialogueEventSystem.instance.dialogueEvents.onEnterDialogue += EnterDialogue;
            DialogueEventSystem.instance.dialogueEvents.onUpdateChoiceIndex += UpdateChoiceIndex;
        }

        private void OnDisable()
        {
            DialogueEventSystem.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
            DialogueEventSystem.instance.dialogueEvents.onUpdateChoiceIndex -= UpdateChoiceIndex;

        }

        private void UpdateChoiceIndex(int choiceIndex)
        {
            this.currentChoiceIndex = choiceIndex;
        }

        public void EnterDialogue(string knotName)
        {
            pm.enabled = false;
            DialogueEventSystem.instance.dialogueEvents.DialogueStarted();
            if (dialoguePlaying)
            {
                return;
            }
            dialoguePlaying = true;

            if (!knotName.Equals(""))
            {
                story.ChoosePathString(knotName);
            }
            else
            {
                Debug.LogWarning("Knot name was empty string when entering dialogue");
            }

            ContinueOrExitStory();


        }

        public void ContinueOrExitStory()
        {
            if (story.currentChoices.Count > 0 && currentChoiceIndex != -1)
            {
                story.ChooseChoiceIndex(currentChoiceIndex);
                currentChoiceIndex = -1;
            }

            if (story.canContinue)
            {


                string dialogueLine = story.Continue();

                while (IsLineBlank(dialogueLine) && story.canContinue)
                {
                    dialogueLine = story.Continue();
                }

                if (IsLineBlank(dialogueLine) && !story.canContinue)
                {
                    ExitDialogue();
                }
                else
                {
                    DialogueEventSystem.instance.dialogueEvents.DisplayDialogue(dialogueLine, story.currentChoices, (string)story.variablesState["speakerName"]);
                }

            }
            else if (story.currentChoices.Count == 0)
            {

                StartCoroutine(ExitDialogue());
            }

        }

        private IEnumerator ExitDialogue()
        {
            yield return null;
            dialoguePlaying = false;
            pm.enabled = true;
            DialogueEventSystem.instance.dialogueEvents.DialogueFinished();
            story.ResetState();
        }

        private bool IsLineBlank(string dialogueLine)
        {
            return dialogueLine.Trim().Equals("") || dialogueLine.Trim().Equals("\n");
        }
    }
}