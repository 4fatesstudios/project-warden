using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;


namespace FourFatesStudios.ProjectWarden.GameSystems.Minigames
{
    public class GrindingMinigame : IMiniGame
    {
        private GrindingMinigameController controller;

        public void StartGame()
        {
            GameObject obj = GameObject.FindWithTag("GrindingController");
            if (obj == null)
            {
                Debug.LogError("GrindingMinigameController not found in scene.");
                return;
            }

            controller = obj.GetComponent<GrindingMinigameController>();
            controller.ResetGame();
            Debug.Log("GrindingMinigame started via interface.");
        }

        public void UpdateGame()
        {
            // No need to handle per-frame logic here since the controller MonoBehaviour runs it
        }

        public void EndGame()
        {
            Debug.Log("GrindingMinigame ended via interface.");
        }
    }
}