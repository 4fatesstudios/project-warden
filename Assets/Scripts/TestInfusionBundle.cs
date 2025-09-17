using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects;

namespace FourFatesStudios.ProjectWarden
{
    public class TestInfusionBundle : MonoBehaviour
    {
        void Start()
        {
            var bundle = new InfusionBundle();
            Debug.Log("InfusionBundle test successful");
        }
    }
}