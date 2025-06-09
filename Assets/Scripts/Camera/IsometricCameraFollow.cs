using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Camera
{
    public class IsometricCameraFollow : MonoBehaviour
    {
        public Transform target; // player reference
        public Vector3 offset = new ((float)0.05, 0, 0); // isometric camera angle
        public float followSpeed = 5f;

        void LateUpdate() {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }
    }
}