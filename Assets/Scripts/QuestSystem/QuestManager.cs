using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.QuestSystem
{
    public class QuestManager : MonoBehaviour
    {

        public static QuestManager instance;

        [Header("Debug")]
        [SerializeField] private QuestSO debugQuestToAdd; // Drag your QuestSO here in the inspector
        [SerializeField] private bool showDebugButtons = true;
        [SerializeField] private Button addQuestButton; // Optional: UI button to add quest
        [SerializeField] private Button clearQuestsButton; // Optional: UI button to clear quests

        [Header("Quest Database")]
        [SerializeField] private QuestDatabase questDatabase; // Reference to quest database

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        List<QuestSO> activeQuests = new List<QuestSO>();
        List<QuestSO> completedQuests = new List<QuestSO>();
        HashSet<string> acceptedQuestIds = new HashSet<string>(); // Track accepted quests by ID
        
        // Event to notify when quests change
        public static event Action OnQuestsChanged;

        private void Start()
        {
            // Set up debug UI buttons if assigned
            if (addQuestButton != null)
            {
                addQuestButton.onClick.AddListener(AddDebugQuest);
            }
            
            if (clearQuestsButton != null)
            {
                clearQuestsButton.onClick.AddListener(ClearAllActiveQuests);
            }

            // Subscribe to quest events
            if (QuestEventSystem.instance != null)
            {
                QuestEventSystem.instance.questEvents.onCheckQuestAcceptable += CanAcceptQuest;
            }

            // Subscribe to task completion events
            QuestTask.OnTaskCompleted += OnTaskCompleted;
        }

        private void OnDestroy()
        {
            // Unsubscribe from quest events
            if (QuestEventSystem.instance != null)
            {
                QuestEventSystem.instance.questEvents.onCheckQuestAcceptable -= CanAcceptQuest;
            }

            // Unsubscribe from task events
            QuestTask.OnTaskCompleted -= OnTaskCompleted;
        }

        /// <summary>
        /// Called when any task is completed
        /// </summary>
        private void OnTaskCompleted(QuestTask completedTask)
        {
            Debug.Log($"Task completed: {completedTask.TaskName}");
            
            // Check if any quest is now complete
            CheckQuestCompletion();
            
            // Trigger UI update
            OnQuestsChanged?.Invoke();
        }

        public void AddQuest(QuestSO quest)
        {
            if (quest == null)
            {
                Debug.LogWarning("Attempted to add null quest");
                return;
            }

            string questId = quest.questName; // Using quest name as ID
            
            if (!CanAcceptQuest(questId))
            {
                Debug.LogWarning($"Quest '{questId}' cannot be accepted - already accepted or completed");
                return;
            }

            activeQuests.Add(quest);
            acceptedQuestIds.Add(questId);
            
            // Trigger events
            OnQuestsChanged?.Invoke();
            if (QuestEventSystem.instance != null)
            {
                QuestEventSystem.instance.questEvents.QuestAccepted(quest);
            }
            
            Debug.Log($"Quest accepted: {quest.questName}");
        }

        /// <summary>
        /// Attempts to accept a quest by its ID/name
        /// </summary>
        public bool TryAcceptQuestById(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("Attempted to accept quest with null or empty ID");
                return false;
            }

            if (!CanAcceptQuest(questId))
            {
                Debug.Log($"Quest '{questId}' cannot be accepted - already accepted or completed");
                return false;
            }

            // Try to get quest from database first
            QuestSO questToAdd = null;
            if (questDatabase != null)
            {
                questToAdd = questDatabase.GetQuestById(questId);
            }
            else
            {
                Debug.LogWarning($"No QuestDatabase assigned to QuestManager! Cannot accept quest '{questId}'");
                return false;
            }

            // If not found in database, don't create dummy quest
            if (questToAdd == null)
            {
                Debug.LogWarning($"Quest '{questId}' not found in database! Make sure to add it to the QuestDatabase ScriptableObject.");
                return false;
            }
            
            AddQuest(questToAdd);
            return true;
        }

        /// <summary>
        /// Checks if a quest can be accepted (not already accepted or completed)
        /// </summary>
        public bool CanAcceptQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return false;
                
            return !acceptedQuestIds.Contains(questId) && 
                   !completedQuests.Any(q => q.questName == questId);
        }

        public void CompleteQuest(QuestSO quest)
        {
            if (activeQuests.Contains(quest))
            {
                activeQuests.Remove(quest);
                completedQuests.Add(quest);
                
                // Trigger events
                OnQuestsChanged?.Invoke();
                if (QuestEventSystem.instance != null)
                {
                    QuestEventSystem.instance.questEvents.QuestCompleted(quest);
                }
                
                Debug.Log($"Quest completed: {quest.questName}");
            }
        }

        public List<QuestSO> GetActiveQuests()
        {
            return activeQuests;
        }

        public List<QuestSO> GetCompletedQuests()
        {
            return completedQuests;
        }

        // Debug methods - these will show as buttons in the inspector
        [ContextMenu("Add Debug Quest")]
        public void AddDebugQuest()
        {
            if (debugQuestToAdd != null)
            {
                AddQuest(debugQuestToAdd);
                Debug.Log($"Added quest: {debugQuestToAdd.questName}");
            }
            else
            {
                Debug.LogWarning("No debug quest assigned! Drag a QuestSO to the 'Debug Quest To Add' field in the inspector.");
            }
        }

        [ContextMenu("Clear All Active Quests")]
        public void ClearAllActiveQuests()
        {
            int questCount = activeQuests.Count;
            activeQuests.Clear();
            acceptedQuestIds.Clear(); // Also clear the tracking set
            OnQuestsChanged?.Invoke(); // Notify listeners
            Debug.Log($"Cleared {questCount} active quests");
        }

        [ContextMenu("Log Active Quests")]
        public void LogActiveQuests()
        {
            Debug.Log($"Active Quests ({activeQuests.Count}):");
            foreach (QuestSO quest in activeQuests)
            {
                Debug.Log($"- {quest.questName}: {quest.description}");
            }
        }

        /// <summary>
        /// Checks if any active quests are now complete and moves them to completed list
        /// </summary>
        private void CheckQuestCompletion()
        {
            var questsToComplete = new List<QuestSO>();
            
            foreach (var quest in activeQuests)
            {
                if (IsQuestComplete(quest))
                {
                    questsToComplete.Add(quest);
                }
            }
            
            foreach (var quest in questsToComplete)
            {
                CompleteQuest(quest);
            }
        }

        /// <summary>
        /// Checks if a quest is complete (all tasks finished)
        /// </summary>
        private bool IsQuestComplete(QuestSO quest)
        {
            if (quest.tasks == null || quest.tasks.Count == 0)
                return false;
                
            return quest.tasks.All(task => task.IsCompleted);
        }

        /// <summary>
        /// Complete a specific task by name in any active quest
        /// </summary>
        public bool CompleteTaskByName(string taskName)
        {
            foreach (var quest in activeQuests)
            {
                var task = quest.tasks.FirstOrDefault(t => t.TaskName.Equals(taskName, StringComparison.OrdinalIgnoreCase));
                if (task != null && !task.IsCompleted)
                {
                    task.CompleteTask();
                    Debug.Log($"Completed task '{taskName}' in quest '{quest.questName}'");
                    return true;
                }
            }
            
            Debug.LogWarning($"Task '{taskName}' not found in any active quest");
            return false;
        }

        /// <summary>
        /// Complete a specific task in a specific quest
        /// </summary>
        public bool CompleteTask(string questName, string taskName)
        {
            var quest = activeQuests.FirstOrDefault(q => q.questName.Equals(questName, StringComparison.OrdinalIgnoreCase));
            if (quest == null)
            {
                Debug.LogWarning($"Quest '{questName}' not found in active quests");
                return false;
            }

            var task = quest.tasks.FirstOrDefault(t => t.TaskName.Equals(taskName, StringComparison.OrdinalIgnoreCase));
            if (task == null)
            {
                Debug.LogWarning($"Task '{taskName}' not found in quest '{questName}'");
                return false;
            }

            if (task.IsCompleted)
            {
                Debug.Log($"Task '{taskName}' is already completed");
                return false;
            }

            task.CompleteTask();
            Debug.Log($"Completed task '{taskName}' in quest '{questName}'");
            return true;
        }

        // ===== DEBUG METHODS FOR EASY TASK COMPLETION =====

        [ContextMenu("Complete First Task in First Quest")]
        public void DebugCompleteFirstTask()
        {
            if (activeQuests.Count == 0)
            {
                Debug.LogWarning("No active quests to complete tasks in");
                return;
            }

            var firstQuest = activeQuests[0];
            if (firstQuest.tasks == null || firstQuest.tasks.Count == 0)
            {
                Debug.LogWarning($"Quest '{firstQuest.questName}' has no tasks");
                return;
            }

            var firstIncompleteTask = firstQuest.tasks.FirstOrDefault(t => !t.IsCompleted);
            if (firstIncompleteTask != null)
            {
                firstIncompleteTask.CompleteTask();
                Debug.Log($"DEBUG: Completed task '{firstIncompleteTask.TaskName}' in quest '{firstQuest.questName}'");
            }
            else
            {
                Debug.Log($"All tasks in quest '{firstQuest.questName}' are already completed");
            }
        }

        [ContextMenu("Complete Random Task")]
        public void DebugCompleteRandomTask()
        {
            var allIncompleteTasks = new List<(QuestSO quest, QuestTask task)>();
            
            foreach (var quest in activeQuests)
            {
                if (quest.tasks != null)
                {
                    foreach (var task in quest.tasks.Where(t => !t.IsCompleted))
                    {
                        allIncompleteTasks.Add((quest, task));
                    }
                }
            }

            if (allIncompleteTasks.Count == 0)
            {
                Debug.LogWarning("No incomplete tasks found in any active quest");
                return;
            }

            var randomIndex = UnityEngine.Random.Range(0, allIncompleteTasks.Count);
            var (randomQuest, randomTask) = allIncompleteTasks[randomIndex];
            
            randomTask.CompleteTask();
            Debug.Log($"DEBUG: Randomly completed task '{randomTask.TaskName}' in quest '{randomQuest.questName}'");
        }

        [ContextMenu("Show All Active Tasks")]
        public void DebugShowAllActiveTasks()
        {
            if (activeQuests.Count == 0)
            {
                Debug.Log("No active quests");
                return;
            }

            foreach (var quest in activeQuests)
            {
                Debug.Log($"=== Quest: {quest.questName} ===");
                if (quest.tasks == null || quest.tasks.Count == 0)
                {
                    Debug.Log("  No tasks");
                    continue;
                }

                for (int i = 0; i < quest.tasks.Count; i++)
                {
                    var task = quest.tasks[i];
                    string status = task.IsCompleted ? "✓ COMPLETED" : "○ INCOMPLETE";
                    Debug.Log($"  Task {i + 1}: {task.TaskName} [{status}]");
                }
            }
        }
        
    }
}