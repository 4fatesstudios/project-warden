using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Test script to verify grid preservation during clearing
    /// </summary>
    public class GridVisualizationTest : MonoBehaviour
    {
        [ContextMenu("Test Grid Preservation")]
        public void TestGridPreservation()
        {
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("No GridGameManager found!");
                return;
            }

            // Count grid objects before clearing
            Transform[] childrenBefore = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                childrenBefore[i] = transform.GetChild(i);
            }

            Debug.Log($"🧪 Grid objects before clearing: {childrenBefore.Length}");
            
            // Clear the grid
            gridManager.ClearGrid();
            
            // Count grid objects after clearing
            Transform[] childrenAfter = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                childrenAfter[i] = transform.GetChild(i);
            }

            Debug.Log($"🧪 Grid objects after clearing: {childrenAfter.Length}");
            
            if (childrenBefore.Length == childrenAfter.Length)
            {
                Debug.Log("✅ Grid preservation test PASSED - Grid objects preserved!");
            }
            else
            {
                Debug.LogError("❌ Grid preservation test FAILED - Grid objects were destroyed!");
            }
        }
    }
}