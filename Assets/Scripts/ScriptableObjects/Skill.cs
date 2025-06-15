using System;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEditor;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Skill")]
    public class Skill : ScriptableObject {
        [SerializeField, Tooltip("The name of the skill")]
        private string skillName;
        
        [SerializeField, Tooltip("The description of the skill")]
        private string skillDescription;
        
        [SerializeField, HideInInspector] private string _skillID;

        [Header("Damage")] 
        [SerializeField, Tooltip("Damage Instances")] private DamageInstance[] damageInstances;
        
        [Header("Healing")]
        [SerializeField, Tooltip("Damage Instances")] private HealInstance[] healingInstances;
        
        [Header("Buffing")]
        [SerializeField] private BuffStatInstance[] buffStatInstances;
        [SerializeField] private BuffHealInstance[] buffHealInstances;
        
        [Header("Debuffing")]
        [SerializeField] private DebuffStatInstance[] debuffStatInstances;
        [SerializeField] private DebuffDOTInstance[] debuffDOTInstances;
        
        [Header("Common Attributes")]
        [SerializeField, Tooltip("Target Team")] private Team targetTeam;
        [SerializeField, Tooltip("Targeting")] private Targeting targeting;
        
#if UNITY_EDITOR
        [ContextMenu("Regenerate Skill ID")]
        public void RegenerateSkillID() => _skillID = Guid.NewGuid().ToString();
        
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(skillName)) skillName = "Unnamed Skill";
            if (string.IsNullOrEmpty(skillDescription)) skillDescription = "Empty Description";
            if (string.IsNullOrEmpty(_skillID))
                _skillID = Guid.NewGuid().ToString();
            
            if (buffStatInstances is { Length: > 0 })
                foreach (var instance in buffStatInstances)
                    instance.StatModifierList.SetSourceID(_skillID);
            if (debuffStatInstances is { Length: > 0 })
                foreach (var instance in debuffStatInstances)
                    instance.StatModifierList.SetSourceID(_skillID);
            
            EditorUtility.SetDirty(this);
        }
#endif
    }

    public class Damage {
        
    }
}