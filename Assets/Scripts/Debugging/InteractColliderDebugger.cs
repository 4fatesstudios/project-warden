using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Debugging
{
    [ExecuteAlways]
    public class InteractColliderDebugger : MonoBehaviour
    {
        private void OnDrawGizmos() {
            SphereCollider sphere = GetComponent<SphereCollider>();
            if (sphere == null) return;
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}