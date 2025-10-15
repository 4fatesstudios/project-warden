using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.QuestSystem
{
    [CreateAssetMenu(fileName = "QuestDatabase", menuName = "Scriptable Objects/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        [Header("Quest Registry")]
        [SerializeField] private List<QuestSO> availableQuests = new List<QuestSO>();

        /// <summary>
        /// Gets a quest by its ID/name
        /// </summary>
        public QuestSO GetQuestById(string questId)
        {
            return availableQuests.FirstOrDefault(quest => quest.questName == questId);
        }

        /// <summary>
        /// Checks if a quest exists in the database
        /// </summary>
        public bool HasQuest(string questId)
        {
            return availableQuests.Any(quest => quest.questName == questId);
        }

        /// <summary>
        /// Gets all available quest IDs
        /// </summary>
        public List<string> GetAllQuestIds()
        {
            return availableQuests.Select(quest => quest.questName).ToList();
        }

        /// <summary>
        /// Adds a quest to the database (for runtime use)
        /// </summary>
        public void RegisterQuest(QuestSO quest)
        {
            if (quest != null && !availableQuests.Contains(quest))
            {
                availableQuests.Add(quest);
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Editor method to validate quest IDs are unique
        /// </summary>
        [ContextMenu("Validate Quest IDs")]
        private void ValidateQuestIds()
        {
            var duplicates = availableQuests
                .GroupBy(quest => quest.questName)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            if (duplicates.Any())
            {
                Debug.LogWarning($"Found duplicate quest IDs in database: {string.Join(", ", duplicates)}");
            }
            else
            {
                Debug.Log("All quest IDs are unique.");
            }
        }
        #endif
    }
}