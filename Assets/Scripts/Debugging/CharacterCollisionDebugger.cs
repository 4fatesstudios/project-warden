using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Debugging
{
    [ExecuteAlways]
    [RequireComponent(typeof(CharacterController))]
    public class CharacterCollisionDebugger : MonoBehaviour
    {
        private CharacterController characterController;

        private void OnEnable()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnDrawGizmos()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (characterController == null)
                return;

            Gizmos.color = Color.red;

            Vector3 center = characterController.transform.position + characterController.center;
            float radius = characterController.radius;
            float height = Mathf.Max(characterController.height, radius * 2f);

            // Draw capsule approximation
            Vector3 top = center + Vector3.up * (height / 2f - radius);
            Vector3 bottom = center - Vector3.up * (height / 2f - radius);

            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
            Gizmos.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
            Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
            Gizmos.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
        }
    }
}
