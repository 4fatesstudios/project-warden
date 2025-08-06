using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Class;
using FourFatesStudios.ProjectWarden.Stats;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class ClassComponent : MonoBehaviour
    {

        [SerializeField] private int level;
        [SerializeField] private StatsComponent statsComponent;
        [SerializeField] private Class _class;
        private List<IntPair<Skill>> availableClassSkills;
        private List<IntPair<StatModifierList>> appliedClassModifiers;

        public List<IntPair<Skill>> GetAvailableClassSkills() {
            return availableClassSkills;
        }
    }
}