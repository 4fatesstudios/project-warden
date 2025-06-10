using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Stats
{
    [Serializable]
    public class StatModifierList {
        [SerializeField, Tooltip("List of stat modifiers for this instance.")]
        private List<StatModifier> statModifiers = new();

        [SerializeField, HideInInspector]
        private string _sourceModifierID; // should be set to the source objects GUID

        public List<StatModifier> StatModifiers => statModifiers;
        public string SourceModifierID => _sourceModifierID;

#if UNITY_EDITOR
        public void ValidateID() {
            if (string.IsNullOrEmpty(_sourceModifierID))
                Debug.LogWarning("Stat Modifiers List Source ID cannot be null or empty, assign in source object");
        }
        
        public void SetSourceID(string guid) {
            _sourceModifierID = guid;
        }
#endif
    }
}