using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Sprites
{
    [ExecuteAlways]
    public class PlanarBillboard : MonoBehaviour {
        private Transform cam;
        private Vector3 initialUp;

        private void Start() {
            cam = UnityEngine.Camera.main.transform;
            initialUp = transform.up;
        }

        private void LateUpdate() {
            if (!cam) return;
            
            Vector3 camForward = cam.forward;
            camForward.y = 0;
            camForward.Normalize();
            
            transform.rotation = Quaternion.LookRotation(camForward, initialUp);
        }
    }
}