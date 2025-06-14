using System.Collections;
using UnityEngine;
using TMPro;
namespace FourFatesStudios.ProjectWarden.Interactions.StaticDialogue
{

    //<summary>
    // DialogueDisplay is a component that manages the display of dialogue text in the game.
    // It handles the typing effect of the text and manages the flow of dialogue.
    //</summary>
    public class DialogueDisplay : MonoBehaviour
    {
        public TextMeshProUGUI textComponent;
        private string[] lines;

        public float textSpeed;
        public GameObject dialogueBox;
        private int index;
        public bool isTalking = false;


        private void Awake()
        {
            dialogueBox.SetActive(false);
        }


        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (textComponent.text == lines[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    textComponent.text = lines[index];
                }
            }
        }

        public void StartDialogue(string[] givenLines)
        {

            if (!isTalking)
            {
                textComponent.text = string.Empty;
                dialogueBox.SetActive(true);
                lines = givenLines;
                index = 0;
                isTalking = true;
                StartCoroutine(TypeLine());

            }

        }

        IEnumerator TypeLine()
        {
            foreach (char c in lines[index].ToCharArray())
            {
                textComponent.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
        }

        void NextLine()
        {
            if (index < lines.Length - 1)
            {
                index++;
                textComponent.text = string.Empty;
                StartCoroutine(TypeLine());
            }
            else
            {
                isTalking = false;
                dialogueBox.SetActive(false);
            }
        }
    }
}