using System;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.QuestSystem
{
    public class QuestEvents
    {
        /// <summary>
        /// Event triggered when a quest is accepted
        /// </summary>
        public event Action<QuestSO> onQuestAccepted;
        public void QuestAccepted(QuestSO quest)
        {
            if (onQuestAccepted != null)
            {
                onQuestAccepted(quest);
            }
        }

        /// <summary>
        /// Event triggered when a quest is completed
        /// </summary>
        public event Action<QuestSO> onQuestCompleted;
        public void QuestCompleted(QuestSO quest)
        {
            if (onQuestCompleted != null)
            {
                onQuestCompleted(quest);
            }
        }

        /// <summary>
        /// Event triggered when a quest acceptance is attempted
        /// </summary>
        public event Action<string, bool> onQuestAcceptanceAttempt;
        public void QuestAcceptanceAttempt(string questId, bool wasAccepted)
        {
            if (onQuestAcceptanceAttempt != null)
            {
                onQuestAcceptanceAttempt(questId, wasAccepted);
            }
        }

        /// <summary>
        /// Event triggered when quest status needs to be checked
        /// </summary>
        public event Func<string, bool> onCheckQuestAcceptable;
        public bool IsQuestAcceptable(string questId)
        {
            if (onCheckQuestAcceptable != null)
            {
                return onCheckQuestAcceptable(questId);
            }
            return true; // Default to acceptable if no listeners
        }
    }
}