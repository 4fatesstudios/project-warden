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
        }

        private void OnDestroy()
        {
            // Unsubscribe from quest events
            if (QuestEventSystem.instance != null)
            {
                QuestEventSystem.instance.questEvents.onCheckQuestAcceptable -= CanAcceptQuest;
            }
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

            // If not found in database, create a dummy quest SO
            if (questToAdd == null)
            {
                Debug.LogWarning($"Quest '{questId}' not found in database, creating dummy quest");
                questToAdd = ScriptableObject.CreateInstance<QuestSO>();
                questToAdd.questName = questId;
                questToAdd.description = $"Quest: {questId}";
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
        
    }
}