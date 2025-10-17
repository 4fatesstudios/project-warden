using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.QuestSystem
{
    /// <summary>
    /// UI component for individual quest tasks with completion functionality
    /// </summary>
    public class QuestTaskUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI taskText;
        [SerializeField] private Button completeButton;
        [SerializeField] private Image checkmarkImage;
        
        [Header("Visual Settings")]
        [SerializeField] private Color completedColor = Color.green;
        [SerializeField] private Color incompleteColor = Color.white;
        
        private QuestTask associatedTask;
        private QuestSO parentQuest;
        
        public void Initialize(QuestTask task, QuestSO quest)
        {
            associatedTask = task;
            parentQuest = quest;
            
            if (taskText != null)
            {
                taskText.text = task.TaskName;
            }
            
            if (completeButton != null)
            {
                completeButton.onClick.AddListener(CompleteTask);
            }
            
            UpdateVisuals();
        }
        
        public void CompleteTask()
        {
            if (associatedTask != null && !associatedTask.IsCompleted)
            {
                associatedTask.CompleteTask();
                UpdateVisuals();
                Debug.Log($"UI: Completed task '{associatedTask.TaskName}' in quest '{parentQuest?.questName}'");
            }
        }
        
        private void UpdateVisuals()
        {
            if (associatedTask == null) return;
            
            bool isCompleted = associatedTask.IsCompleted;
            
            // Update text color
            if (taskText != null)
            {
                taskText.color = isCompleted ? completedColor : incompleteColor;
                taskText.text = $"{(isCompleted ? "✓" : "○")} {associatedTask.TaskName}";
            }
            
            // Update button state
            if (completeButton != null)
            {
                completeButton.interactable = !isCompleted;
                completeButton.gameObject.SetActive(!isCompleted);
            }
            
            // Update checkmark
            if (checkmarkImage != null)
            {
                checkmarkImage.gameObject.SetActive(isCompleted);
            }
        }
        
        private void OnDestroy()
        {
            if (completeButton != null)
            {
                completeButton.onClick.RemoveListener(CompleteTask);
            }
        }
        
        /// <summary>
        /// Debug method to toggle task completion
        /// </summary>
        [ContextMenu("Toggle Task Completion")]
        public void DebugToggleCompletion()
        {
            if (associatedTask != null)
            {
                if (associatedTask.IsCompleted)
                {
                    associatedTask.ResetTask();
                }
                else
                {
                    associatedTask.CompleteTask();
                }
                UpdateVisuals();
            }
        }
    }
}