using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Enums
{
    public enum CardinalDirection
    {
        None,
        North,
        South,
        East,
        West,
    }
    
    public static class CardinalDirectionMask
    {
        public static int GetMask(CardinalDirection dir)
        {
            return dir switch
            {
                CardinalDirection.North => 0b0001, // 1
                CardinalDirection.South => 0b0001, // 1 (same as North)
                CardinalDirection.East  => 0b0010, // 2
                CardinalDirection.West  => 0b0010, // 2 (same as East)
                _ => 0b0000
            };
        }

        public static bool EqualOpposites(CardinalDirection a, CardinalDirection b) {
            return a != b && GetMask(a) == GetMask(b);
        }
    }
}