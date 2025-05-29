using UnityEngine;

public class DistillingMinigame : IMiniGame
{
    private DistillingMinigameController controller;

    public void StartGame()
    {
        GameObject obj = GameObject.FindWithTag("DistillingController");
        if (obj == null)
        {
            Debug.LogError("DistillingMinigameController not found in scene.");
            return;
        }

        controller = obj.GetComponent<DistillingMinigameController>();
        controller.ResetGame();
        Debug.Log("DistillingMinigame started via interface.");
    }

    public void UpdateGame()
    {
        // Per-frame logic handled by the controller
    }

    public void EndGame()
    {
        Debug.Log("DistillingMinigame ended via interface.");
    }
}
