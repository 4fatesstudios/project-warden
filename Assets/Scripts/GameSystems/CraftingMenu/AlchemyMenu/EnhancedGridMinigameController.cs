// This file has been merged into GridMinigameController.cs
// All enhanced features are now part of the main GridMinigameController
// Please use GridMinigameController instead

using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu
{
    [System.Obsolete("This class has been merged into GridMinigameController. Use GridMinigameController instead.")]
    public class EnhancedGridMinigameController : MonoBehaviour
    {
        private void Awake()
        {
            Debug.LogWarning("EnhancedGridMinigameController is obsolete. Use GridMinigameController instead.");
            gameObject.SetActive(false);
        }
    }
}