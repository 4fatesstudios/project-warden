using System;
using FourFatesStudios.ProjectWarden.Effects;
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
        
        [SerializeField, Tooltip("The type of the skill")]
        private SkillType skillType;
        
        [SerializeField, Tooltip("The description of the skill")]
        private string skillDescription;
        
        [SerializeField, HideInInspector] private string _skillID;
        
        [Header("Skill Effects")]
        [SerializeField] private EffectBundle effects;
        
        [Header("Common Attributes")]
        [SerializeField, Tooltip("Target Team")] private Team targetTeam;
        [SerializeField, Tooltip("Targeting")] private Targeting targeting;
        [SerializeField, Tooltip("Numo Cost")] private int numoCost;
        [SerializeField, Tooltip("Unique Cost")] private int uniqueCost;
        
        
        public string SkillName { get => skillName; set => skillName = value; }
        public SkillType SkillType {get => skillType; set => skillType = value;}
        public string SkillDescription { get => skillDescription; set => skillDescription = value; }
        public string SkillID => _skillID;
        public EffectBundle Effects { get => this.effects; set => this.effects = value; }
        public Team TargetTeam { get => targetTeam; set => targetTeam = value; }
        public Targeting Targeting { get => targeting; set => targeting = value; }
        public int NumoCost {get => numoCost; set => numoCost = value;}
        public int UniqueCost {get => uniqueCost; set => uniqueCost = value;}

#if UNITY_EDITOR
        [ContextMenu("Regenerate Skill ID")]
        public void RegenerateSkillID() => _skillID = Guid.NewGuid().ToString();
        
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(skillName)) skillName = "Unnamed Skill";
            if (string.IsNullOrEmpty(skillDescription)) skillDescription = "Empty Description";
            if (string.IsNullOrEmpty(_skillID))
                _skillID = Guid.NewGuid().ToString();
            
            // for always having source of debuff, buff instances
            // if (buffStatInstances is { Length: > 0 })
            //     foreach (var instance in buffStatInstances)
            //         instance.StatModifierList.SetSourceID(_skillID);
            // if (debuffStatInstances is { Length: > 0 })
            //     foreach (var instance in debuffStatInstances)
            //         instance.StatModifierList.SetSourceID(_skillID);
            
            EditorUtility.SetDirty(this);
        }
#endif
    }
}