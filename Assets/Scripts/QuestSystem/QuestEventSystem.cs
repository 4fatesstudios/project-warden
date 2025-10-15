using UnityEngine;
using System;

namespace FourFatesStudios.ProjectWarden.QuestSystem
{
    /// <summary>
    /// Quest Event System - Manages quest-related events for the dialogue-quest integration
    /// 
    /// Setup Instructions:
    /// 1. Add this component to a GameObject in your scene (e.g., create an empty GameObject called "QuestEventSystem")
    /// 2. Make sure you have a QuestManager in your scene with a QuestDatabase assigned
    /// 3. In your Ink dialogue files, use the following functions:
    ///    - {can_accept_quest("quest_id")} - Check if a quest can be accepted (for conditional choices)
    ///    - ~ accept_quest("quest_id") - Accept a quest (call this in dialogue response)
    /// 
    /// Example Ink dialogue:
    /// * {can_accept_quest("find_artifact")} [Accept the quest]
    ///     ~ accept_quest("find_artifact")
    ///     Great! You've accepted the quest.
    /// </summary>
    public class QuestEventSystem : MonoBehaviour
    {
        public static QuestEventSystem instance { get; private set; }

        public QuestEvents questEvents;

        private void OnEnable()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one Quest Event System in the scene");
            }
            instance = this;

            questEvents = new QuestEvents();
        }
    }
}