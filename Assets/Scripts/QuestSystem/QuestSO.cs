using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.QuestSystem
{
    [CreateAssetMenu(fileName = "QuestSO", menuName = "Scriptable Objects/QuestSO")]
    [System.Serializable]
    public class QuestSO : ScriptableObject
    {
        public string questName;
        public string description;
        public List<QuestTask> tasks = new List<QuestTask>();
    }
}