using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    public class AlchemyMenuManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private Button potionCraftingButton;
        private Button potionEnhancingButton;
        private Button backButton;

        [Header("Scene Names")]
        public string potionCraftingSceneName = "PotionCraftingScene";
        public string potionEnhancingSceneName = "PotionEnhancingScene";
        public string previousSceneName = "MainMenu";

        [Header("Placeholder for Unlock Logic")]
        public bool hasEnhancingSkill = false; // TODO: Replace with actual skill system check

        void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            potionCraftingButton = root.Q<Button>("potionCraftingButton");
            potionEnhancingButton = root.Q<Button>("potionEnhancingButton");
            backButton = root.Q<Button>("backButton");

            potionCraftingButton?.RegisterCallback<ClickEvent>(evt => LoadScene(potionCraftingSceneName));
            backButton?.RegisterCallback<ClickEvent>(evt => LoadScene(previousSceneName));

            if (hasEnhancingSkill)
            {
                potionEnhancingButton?.RegisterCallback<ClickEvent>(evt => LoadScene(potionEnhancingSceneName));
            }
            else
            {
                potionEnhancingButton?.SetEnabled(false);
                potionEnhancingButton.tooltip = "Unlock the required skill to enhance potions.";
            }
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
}
