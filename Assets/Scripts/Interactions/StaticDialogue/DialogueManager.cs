using UnityEngine;
using Ink.Runtime;
using UnityEngine.InputSystem;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.RelationshipSystem;
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
        }

        private void Start()
        {
            // Check if RelationshipManager exists
            if (RelationshipManager.Instance == null)
            {
                Debug.LogError("[DialogueManager] RelationshipManager.Instance is null! Make sure there's a GameObject with RelationshipManager script in the scene.");
            }
            
            // Bind the relationship_points function to Ink
            story.BindExternalFunction("relationship_points", (string characterName, int points) => {
                Debug.Log($"[DialogueManager] relationship_points called: {characterName}, {points}");
                if (RelationshipManager.Instance != null)
                {
                    RelationshipManager.Instance.ChangeRelationshipPoints(characterName, points);
                    Debug.Log($"Changed relationship with {characterName} by {points} points");
                }
                else
                {
                    Debug.LogError("[DialogueManager] RelationshipManager.Instance is null when trying to change relationship points!");
                }
            });
            
            // Bind function to get current relationship status
            story.BindExternalFunction("get_relationship_status", (string characterName) => {
                if (RelationshipManager.Instance != null)
                {
                    return RelationshipManager.Instance.GetRelationshipStatus(characterName).ToString();
                }
                Debug.LogError("[DialogueManager] RelationshipManager.Instance is null when getting relationship status!");
                return "Neutral";
            });
            
            // Bind function to get current relationship points
            story.BindExternalFunction("get_relationship_points", (string characterName) => {
                if (RelationshipManager.Instance != null)
                {
                    return RelationshipManager.Instance.GetRelationshipPoints(characterName);
                }
                Debug.LogError("[DialogueManager] RelationshipManager.Instance is null when getting relationship points!");
                return 0;
            });
            
            // Bind function to update relationship status (for story events)
            story.BindExternalFunction("update_relationship_status", (string characterName) => {
                if (RelationshipManager.Instance != null)
                {
                    RelationshipManager.Instance.UpdateRelationshipStatus(characterName);
                    Debug.Log($"Updated relationship status for {characterName}");
                }
                else
                {
                    Debug.LogError("[DialogueManager] RelationshipManager.Instance is null when updating relationship status!");
                }
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
                Debug.Log($"[DialogueManager] Making choice {currentChoiceIndex}: {story.currentChoices[currentChoiceIndex].text}");
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