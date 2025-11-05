using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Visualization
{
    [ExecuteInEditMode]
    public class RoomGizmoVisualizer : MonoBehaviour
    {
        [SerializeField] private SpaceType spaceType;

        private void OnDrawGizmos()
        {
            // Set color based on SpaceType
            Gizmos.color = spaceType switch
            {
                SpaceType.Room => Color.green,
                SpaceType.Hallway => Color.blue,
                _ => Color.black
            };

            // Draw all meshes in this object and children
            var meshFilters = GetComponentsInChildren<MeshFilter>();
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;

                // Set Gizmo matrix to mesh transform
                Gizmos.matrix = mf.transform.localToWorldMatrix;

                // Draw the mesh as wireframe
                Gizmos.DrawWireMesh(mf.sharedMesh);
            }
        }
    }
}