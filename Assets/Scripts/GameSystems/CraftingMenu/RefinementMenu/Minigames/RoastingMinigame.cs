using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;


namespace FourFatesStudios.ProjectWarden.GameSystems.Minigames
{
    public class RoastingMinigame : IMiniGame
    {
        private RoastingMinigameController controller;

        public void StartGame()
        {
            GameObject obj = GameObject.FindWithTag("RoastingController");
            if (obj == null)
            {
                Debug.LogError("RoastingMinigameController not found in scene.");
                return;
            }

            controller = obj.GetComponent<RoastingMinigameController>();
            controller.ResetGame();
            Debug.Log("RoastingMinigame started via interface.");
        }

        public void UpdateGame()
        {
            // No need to handle per-frame logic here since the controller MonoBehaviour runs it
        }

        public void EndGame()
        {
            Debug.Log("RoastingMinigame ended via interface.");
        }
    }
}