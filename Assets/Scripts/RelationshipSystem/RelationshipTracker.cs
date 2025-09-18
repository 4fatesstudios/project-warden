using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.RelationshipSystem
{
    public enum RelationshipStatus
    {
        Distrustful = -1,
        Neutral = 0,
        Aquainted = 1,
        Friendly = 2,
        Allied = 3,
        Romanced= 4
    }
    public enum RelationshipThresholds
    {
        Distrustful = -10,
        Neutral = 0,
        Aquainted = 10,
        Friendly = 20,
        Allied = 40,
        Romanced = 100
    }

    public class RelationshipTracker : MonoBehaviour
    {
        [SerializeField] private string characterName; // Set this in the inspector
        
        public RelationshipStatus relationshipStatus { get; private set; } = RelationshipStatus.Neutral;
        public int relationshipPoints { get; private set; } = 0;
        
        private void Awake()
        {
            // Register this character with the relationship manager
            if (!string.IsNullOrEmpty(characterName))
            {
                RelationshipManager.Instance?.RegisterCharacter(characterName, this);
            }
        }


        /// <summary>
        /// Changes the relationship points of the character given a positive or negative value.
        /// Points are capped at the next threshold until UpdateRelationshipStatus() is called externally.
        /// </summary>
        /// <param name="points"></param>
        public void ChangeRelationshipPoints(int points)
        {
            Debug.Log($"ChangeRelationshipPoints called for {characterName}: adding {points} points (current: {relationshipPoints})");

            int newPoints = relationshipPoints + points;

            /// Enforces the invariant that points must be within minus 10 of Distrustful and plus 20 of Romanced.
            if (relationshipStatus == RelationshipStatus.Distrustful)
            {
                //points cannot be greater than (int)RelationshipThreshold.Neutral
                if (newPoints > (int)RelationshipThresholds.Neutral)
                {
                    newPoints = (int)RelationshipThresholds.Neutral;
                }
            }
            else if (relationshipStatus == RelationshipStatus.Neutral)
            {
                //points cannot be greater than (int)RelationshipThreshold.Aquainted
                if (newPoints > (int)RelationshipThresholds.Aquainted)
                {
                    newPoints = (int)RelationshipThresholds.Aquainted;
                }
            }
            else if (relationshipStatus == RelationshipStatus.Aquainted)
            {
                //points cannot be greater than (int)RelationshipThreshold.Friendly
                if (newPoints > (int)RelationshipThresholds.Friendly)
                {
                    newPoints = (int)RelationshipThresholds.Friendly;
                }
            }
            else if (relationshipStatus == RelationshipStatus.Friendly)
            {
                //points cannot be greater than (int)RelationshipThreshold.Allied
                if (newPoints > (int)RelationshipThresholds.Allied)
                {
                    newPoints = (int)RelationshipThresholds.Allied;
                }
            }
            else if (relationshipStatus == RelationshipStatus.Allied)
            {
                //points cannot be greater than (int)RelationshipThreshold.Romanced
                if (newPoints > (int)RelationshipThresholds.Romanced)
                {
                    newPoints = (int)RelationshipThresholds.Romanced;
                }
            }

            relationshipPoints = Mathf.Clamp(newPoints, (int)RelationshipThresholds.Distrustful - 10, (int)RelationshipThresholds.Romanced + 20);
            Debug.Log($"Relationship points for {characterName} updated to: {relationshipPoints}");
        }


        /// <summary>
        /// Updates the relationship status based on the current relationship points.
        /// </summary>
        public void UpdateRelationshipStatus()
        {
            if (relationshipPoints < (int)RelationshipThresholds.Distrustful)
            {
                relationshipStatus = RelationshipStatus.Distrustful;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Neutral)
            {
                relationshipStatus = RelationshipStatus.Distrustful;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Aquainted)
            {
                relationshipStatus = RelationshipStatus.Neutral;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Friendly)
            {
                relationshipStatus = RelationshipStatus.Aquainted;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Allied)
            {
                relationshipStatus = RelationshipStatus.Friendly;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Romanced)
            {
                relationshipStatus = RelationshipStatus.Allied;
            }
            else
            {
                relationshipStatus = RelationshipStatus.Romanced;
            }
        }

        // Temporary test method - remove this later
        [ContextMenu("Test Add Points")]
        public void TestAddPoints()
        {
            ChangeRelationshipPoints(5);
        }



    }
}