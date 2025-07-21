using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Stats;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Class
{
    [CreateAssetMenu(fileName = "Class", menuName = "Class")]
    public class Class : ScriptableObject
    {
        [SerializeField, Tooltip("The name of the class")]
        private string className;
        
        [SerializeField, Tooltip("Skills and required level specific to each class")]
        private List<IntPair<Skill>> classSkills;
        
        [SerializeField, Tooltip("Modifiers and required level specific to each class")]
        private List<IntPair<StatModifierList>> classModifiers;
    }

    [System.Serializable]
    public class IntPair<T>{
        public T key; //skill
        public int value; //required level
    }
}