using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Interactions.StaticDialogue
{
    [CreateAssetMenu(fileName = "NameToProfileSO", menuName = "Scriptable Objects/NameToProfileSO")]
    public class NameToProfileSO : ScriptableObject
    {
        [SerializeField] List<string> names;
        [SerializeField] List<Sprite> sprites;


        public Sprite GetSpriteFromName(string name)
        {
            int i = names.IndexOf(name);
            Sprite spr = sprites[i];
            return spr;
        }
    }
}