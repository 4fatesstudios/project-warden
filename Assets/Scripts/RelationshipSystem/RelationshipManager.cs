using System.Collections.Generic;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.RelationshipSystem
{
    public class RelationshipManager : MonoBehaviour
    {
        public static RelationshipManager Instance { get; private set; }
        
        private Dictionary<string, RelationshipTracker> characterRelationships = new Dictionary<string, RelationshipTracker>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Registers a character's relationship tracker
        /// </summary>
        public void RegisterCharacter(string characterName, RelationshipTracker tracker)
        {
            if (string.IsNullOrEmpty(characterName) || tracker == null)
            {
                Debug.LogWarning("Invalid character name or tracker provided for registration.");
                return;
            }
            characterRelationships[characterName] = tracker;
        }
        
        /// <summary>
        /// Updates relationship status for a specific character
        /// </summary>
        public void UpdateRelationshipStatus(string characterName)
        {
            if (characterRelationships.TryGetValue(characterName, out RelationshipTracker tracker))
            {
                tracker.UpdateRelationshipStatus();
            }
            else
            {
                Debug.LogWarning($"Character '{characterName}' not found in relationship system!");
            }
        }
        
        /// <summary>
        /// Changes relationship points for a specific character
        /// </summary>
        public void ChangeRelationshipPoints(string characterName, int points)
        {
            if (characterRelationships.TryGetValue(characterName, out RelationshipTracker tracker))
            {
                tracker.ChangeRelationshipPoints(points);
            }
            else
            {
                Debug.LogWarning($"Character '{characterName}' not found in relationship system!");
            }
        }
        
        /// <summary>
        /// Gets the current relationship status with a character
        /// </summary>
        public RelationshipStatus GetRelationshipStatus(string characterName)
        {
            if (characterRelationships.TryGetValue(characterName, out RelationshipTracker tracker))
            {
                return tracker.relationshipStatus;
            }
            
            Debug.LogWarning($"Character '{characterName}' not found in relationship system!");
            return RelationshipStatus.Neutral;
        }
        
        /// <summary>
        /// Gets the current relationship points with a character
        /// </summary>
        public int GetRelationshipPoints(string characterName)
        {
            if (characterRelationships.TryGetValue(characterName, out RelationshipTracker tracker))
            {
                return tracker.relationshipPoints;
            }
            
            Debug.LogWarning($"Character '{characterName}' not found in relationship system!");
            return 0;
        }
    }
}
