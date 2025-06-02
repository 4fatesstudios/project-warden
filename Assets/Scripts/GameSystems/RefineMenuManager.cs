using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class RefinementMenuManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button grindingButton;
    private Button distillingButton;
    private Button roastingButton;
    private Button backButton;

    [Header("Minigame Scene Names or Activation Flags")]
    public string grindingSceneName = "GrindingScene";
    public string distillingSceneName = "DistillingScene";
    public string roastingSceneName = "RoastingScene";
    public string previousSceneName = "MainMenu"; // or whatever scene you're coming from

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        grindingButton = root.Q<Button>("grindingButton");
        distillingButton = root.Q<Button>("distillingButton");
        roastingButton = root.Q<Button>("roastingButton");
        backButton = root.Q<Button>("backButton");

        grindingButton?.RegisterCallback<ClickEvent>(evt => LoadScene(grindingSceneName));
        distillingButton?.RegisterCallback<ClickEvent>(evt => LoadScene(distillingSceneName));
        roastingButton?.RegisterCallback<ClickEvent>(evt => LoadScene(roastingSceneName));
        backButton?.RegisterCallback<ClickEvent>(evt => LoadScene(previousSceneName));
    }

    private void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty or null.");
        }
    }
}
