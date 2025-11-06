using UnityEngine;
using System.Collections.Generic;

namespace FourFatesStudios.ProjectWarden.ProceduralGeneration
{
    using UnityEngine;

    public static class RhombusOverlapChecker
    {
        /// <summary>
        /// Returns true if two convex quads (rhombus shapes) overlap using SAT
        /// </summary>
        public static bool DoOverlap(Vector2[] a, Vector2[] b)
        {
            // Both must have exactly 4 points
            if (a.Length != 4 || b.Length != 4)
            {
                Debug.LogError("RhombusOverlapChecker: Polygons must have 4 vertices.");
                return false;
            }

            // Check both polygons' edges as separating axes
            return !HasSeparatingAxis(a, b) && !HasSeparatingAxis(b, a);
        }

        private static bool HasSeparatingAxis(Vector2[] polyA, Vector2[] polyB)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 p1 = polyA[i];
                Vector2 p2 = polyA[(i + 1) % 4];

                // Edge normal (perpendicular axis)
                Vector2 axis = new Vector2(-(p2.y - p1.y), p2.x - p1.x).normalized;

                Project(polyA, axis, out float minA, out float maxA);
                Project(polyB, axis, out float minB, out float maxB);

                // If no overlap on this axis → separating axis found
                if (maxA < minB || maxB < minA)
                    return true;
            }

            return false;
        }

        private static void Project(Vector2[] poly, Vector2 axis, out float min, out float max)
        {
            float dot = Vector2.Dot(poly[0], axis);
            min = max = dot;

            for (int i = 1; i < 4; i++)
            {
                dot = Vector2.Dot(poly[i], axis);
                if (dot < min) min = dot;
                if (dot > max) max = dot;
            }
        }
    }

}