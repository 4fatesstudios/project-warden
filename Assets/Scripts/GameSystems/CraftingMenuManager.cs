using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    public class CraftingMenuManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private Button ingredientsButton;
        private Button refinementsButton;
        private Button alchemyButton;
        private Button backButton;

        [Header("Navigation Targets")]
        public string ingredientsScene = "IngredientsBookScene";
        public string refinementsScene = "RefinementMenuScene";
        public string alchemyScene = "AlchemyScene";
        public string backScene = "MainMenu";

        void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            ingredientsButton = root.Q<Button>("ingredientsButton");
            refinementsButton = root.Q<Button>("refinementsButton");
            alchemyButton = root.Q<Button>("alchemyButton");
            backButton = root.Q<Button>("backButton");

            ingredientsButton?.RegisterCallback<ClickEvent>(_ => LoadScene(ingredientsScene));
            refinementsButton?.RegisterCallback<ClickEvent>(_ => LoadScene(refinementsScene));
            alchemyButton?.RegisterCallback<ClickEvent>(_ => LoadScene(alchemyScene));
            backButton?.RegisterCallback<ClickEvent>(_ => LoadScene(backScene));
        }

        private void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                Debug.LogWarning("Scene name not assigned.");
        }
    }
}