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
        public RelationshipStatus relationshipStatus { get; private set; } = RelationshipStatus.Neutral;
        public int relationshipPoints { get; private set; } = 0;


        /// <summary>
        /// Changes the relationship points of the character given a positive or negative value.
        /// This will also update the relationship status based on the new points.
        /// </summary>
        /// <param name="points"></param>
        public void ChangeRelationshipPoints(int points)
        {
            relationshipPoints += points;
            UpdateRelationshipStatus();
        }

        /// <summary>
        /// Updates the relationship status based on the current relationship points.
        /// </summary>
        void UpdateRelationshipStatus()
        {
            if (relationshipPoints < (int)RelationshipThresholds.Distrustful)
            {
                relationshipStatus = RelationshipStatus.Distrustful;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Neutral)
            {
                relationshipStatus = RelationshipStatus.Neutral;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Aquainted)
            {
                relationshipStatus = RelationshipStatus.Aquainted;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Friendly)
            {
                relationshipStatus = RelationshipStatus.Friendly;
            }
            else if (relationshipPoints < (int)RelationshipThresholds.Allied)
            {
                relationshipStatus = RelationshipStatus.Allied;
            }
            else if (relationshipPoints >= (int)RelationshipThresholds.Romanced)
            {
                relationshipStatus = RelationshipStatus.Romanced;
            }
        }



    }
}