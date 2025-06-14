using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interactions.StaticDialogue
{
    [CreateAssetMenu(fileName = "ScriptedDialogue", menuName = "ScriptedDialogue")]
    public class ScriptedDialogueSO : ScriptableObject
    {

        //TODO: Possibly add a name field for the speaker
        [Tooltip("Lines of dialogue to be displayed in order.")]
        [TextArea(3, 10)]
        public string[] lines;
    }
}