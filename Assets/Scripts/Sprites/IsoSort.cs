using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Sprites
{
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class IsoSort : MonoBehaviour
    {
        SpriteRenderer sr;

        void Awake() => sr = GetComponent<SpriteRenderer>();

        void LateUpdate()
        {
            float depth = transform.position.x + transform.position.z;
            sr.sortingOrder = Mathf.RoundToInt(-depth * 100);
        }
    }

}