using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    public abstract class BaseDataSO : ScriptableObject {
        [SerializeField] private string id;
        
        public string ID { get => id; set => id = value; }
    }
}