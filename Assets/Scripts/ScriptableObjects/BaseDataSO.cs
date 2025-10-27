using System;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    public abstract class BaseDataSO : ScriptableObject {
        [SerializeField, HideInInspector] private string id;
        
        public string ID { get => id; set => id = value; }
        
#if UNITY_EDITOR
        // Called in editor when the object is loaded or a value changes
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this); // Mark as dirty so it gets saved
            }
        }
#endif
    }
}