using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Visualization
{
    [ExecuteInEditMode]
    public class RoomGizmoVisualizer : MonoBehaviour {
        [SerializeField] private SpaceType spaceType;

        private void OnDrawGizmos() {
            var box = GetComponent<BoxCollider>();
            if (box == null) return;
            Gizmos.color = spaceType switch {
                SpaceType.Room => Color.green,
                SpaceType.Hallway => Color.blue,
                _ => Color.black
            };
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}