using UnityEngine;

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
        // No per-frame logic needed here
    }

    public void EndGame()
    {
        if (controller != null)
            controller.EndGame();

        Debug.Log("RoastingMinigame ended via interface.");
    }
}
