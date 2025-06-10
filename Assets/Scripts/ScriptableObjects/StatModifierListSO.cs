using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    [CreateAssetMenu(fileName = "StatModifierList", menuName = "Stats/Stat Modifier List")]
    public class StatModifierListSO : ScriptableObject{
        [SerializeField, Tooltip("List of stat modifiers for this instance.")] 
        private List<StatModifier> statModifiers;

        [SerializeField, HideInInspector] private string _statModifierListID;
        
        public List<StatModifier> StatModifiers => statModifiers;
        public string StatModifierListID => _statModifierListID;
        
#if UNITY_EDITOR
        [ContextMenu("Regenerate StatModifierList ID")]
        public void RegenerateStatModifierListID() => _statModifierListID = Guid.NewGuid().ToString();

        private void OnValidate() {
            if (!string.IsNullOrEmpty(_statModifierListID)) return;
            _statModifierListID = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}