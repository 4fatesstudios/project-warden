using UnityEngine;
using System;

namespace FourFatesStudios.ProjectWarden.QuestSystem
{
    [System.Serializable]
    public class QuestTask
    {
        QuestTask() { }
        QuestTask(string name)
        {
            taskName = name;
            isCompleted = false;
        }

        [SerializeField] string taskName;
        [SerializeField] bool isCompleted;

        public string TaskName => taskName;
        public bool IsCompleted => isCompleted;

        /// <summary>
        /// Event triggered when this task is completed
        /// </summary>
        public static event Action<QuestTask> OnTaskCompleted;

        public void CompleteTask()
        {
            if (!isCompleted)
            {
                isCompleted = true;
                Debug.Log($"Task completed: {taskName}");
                OnTaskCompleted?.Invoke(this);
            }
        }

        /// <summary>
        /// Reset task to incomplete state (for debugging)
        /// </summary>
        public void ResetTask()
        {
            isCompleted = false;
            Debug.Log($"Task reset: {taskName}");
        }
    }
}