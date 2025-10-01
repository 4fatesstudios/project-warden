using UnityEngine;


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

        public void CompleteTask()
        {
            isCompleted = true;
        }
    }
}