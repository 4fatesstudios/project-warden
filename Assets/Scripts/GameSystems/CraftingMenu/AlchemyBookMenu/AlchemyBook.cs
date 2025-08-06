using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyBookMenu
{
    [System.Serializable]
    
    
    public class AlchemyBook : MonoBehaviour
    {
        public UIDocument uiDocument;
        private VisualElement leftPage;
        private VisualElement rightPage;
        private Button nextButton;
        private Button prevButton;
        VisualTreeAsset pageTemplate; // Assign via Inspector or Resources.Load

        private int currentPageIndex;
        public List<BestiaryEntry> Entries = new List<BestiaryEntry>
        {
            new BestiaryEntry {
                Title = "Black King",
                Hp = 300,
                Weakness = "Ice",
                Nullified = "Fire",
                Description = "A dark knight that guards Yukiko's castle. Heavy armor, slow but powerful.",
                Skills = new List<string> { "Power Slash", "Tarukaja" }
            },
            // Add more entries here
        };

        void OnEnable()
        {
            var root = uiDocument.rootVisualElement;
            leftPage = root.Q<VisualElement>("left-page");
            rightPage = root.Q<VisualElement>("right-page");
            nextButton = root.Q<Button>("next-button");
            prevButton = root.Q<Button>("prev-button");

            nextButton.clicked += () => FlipPage(1);
            prevButton.clicked += () => FlipPage(-1);

            UpdatePages();
        }

        void FlipPage(int direction)
        {
            currentPageIndex = Mathf.Clamp(currentPageIndex + direction * 2, 0, Entries.Count - 2);
            UpdatePages();
        }

        void UpdatePages()
        {
            leftPage.Clear();
            rightPage.Clear();

            AddEntryToPage(leftPage, currentPageIndex);
            if (currentPageIndex + 1 < Entries.Count)
                AddEntryToPage(rightPage, currentPageIndex + 1);
        }
        
        void AddEntryToPage(VisualElement pageRoot, int index)
        {
            var entryData = Entries[index];
            var entryElement = pageTemplate.CloneTree();

            entryElement.Q<Label>("entry-title").text = entryData.Title;
            entryElement.Q<Label>("entry-description").text = entryData.Description;
            entryElement.Q<Label>("entry-stat").text = $"HP: {entryData.Hp}\nWeakness: {entryData.Weakness}\nNull: {entryData.Nullified}";

            var skillContainer = entryElement.Q<VisualElement>("entry-skills");
            skillContainer.Clear();
            foreach (var skill in entryData.Skills)
                //skillContainer.Add(new Label(skill) { classList = { "entry-skill" } });

            pageRoot.Add(entryElement);
        }
    }
}