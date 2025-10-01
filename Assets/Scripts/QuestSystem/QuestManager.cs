using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
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
        }

        public void AddQuest(QuestSO quest)
        {
            if (!activeQuests.Contains(quest) && !completedQuests.Contains(quest))
            {
                activeQuests.Add(quest);
                OnQuestsChanged?.Invoke(); // Notify listeners
            }
        }

        public void CompleteQuest(QuestSO quest)
        {
            if (activeQuests.Contains(quest))
            {
                activeQuests.Remove(quest);
                completedQuests.Add(quest);
                OnQuestsChanged?.Invoke(); // Notify listeners
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