using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems.CraftingMenu.AlchemyBookMenu.Sections
{
    [System.Serializable]
    public class HelpEntryUI : BaseEntry
    {
        [Header("Help Properties")]
        public HelpCategory category;
        public string[] steps;
        public string[] tips;
        public string[] relatedTopics;
        public Sprite[] illustrationImages;
        public bool isBasicTutorial;

        public override EntryType GetEntryType() => EntryType.Help;

        public override VisualElement CreateEntryVisual()
        {
            var container = CreateBaseVisual();
            if (!isSeen) return container;

            // Category section
            var categoryContainer = new VisualElement();
            categoryContainer.AddToClassList("help-category");
            
            var categoryLabel = new Label($"Category: {category}");
            categoryLabel.AddToClassList("category-label");
            categoryContainer.Add(categoryLabel);
            container.Add(categoryContainer);

            // Tutorial indicator
            if (isBasicTutorial)
            {
                var tutorialContainer = new VisualElement();
                tutorialContainer.AddToClassList("help-tutorial");
                
                var tutorialLabel = new Label("📚 Basic Tutorial");
                tutorialLabel.AddToClassList("tutorial-label");
                tutorialContainer.Add(tutorialLabel);
                container.Add(tutorialContainer);
            }

            // Steps section
            if (steps != null && steps.Length > 0)
            {
                var stepsContainer = new VisualElement();
                stepsContainer.AddToClassList("help-steps");
                
                var stepsTitle = new Label("Steps:");
                stepsTitle.AddToClassList("steps-title");
                stepsContainer.Add(stepsTitle);
                
                for (int i = 0; i < steps.Length; i++)
                {
                    var stepLabel = new Label($"{i + 1}. {steps[i]}");
                    stepLabel.AddToClassList("step-label");
                    stepsContainer.Add(stepLabel);
                }
                
                container.Add(stepsContainer);
            }

            // Tips section
            if (tips != null && tips.Length > 0)
            {
                var tipsContainer = new VisualElement();
                tipsContainer.AddToClassList("help-tips");
                
                var tipsTitle = new Label("💡 Tips:");
                tipsTitle.AddToClassList("tips-title");
                tipsContainer.Add(tipsTitle);
                
                foreach (var tip in tips)
                {
                    var tipLabel = new Label($"• {tip}");
                    tipLabel.AddToClassList("tip-label");
                    tipsContainer.Add(tipLabel);
                }
                
                container.Add(tipsContainer);
            }

            // Illustration images
            if (illustrationImages != null && illustrationImages.Length > 0)
            {
                var imagesContainer = new VisualElement();
                imagesContainer.AddToClassList("help-images");
                
                foreach (var illustration in illustrationImages)
                {
                    if (illustration != null)
                    {
                        var imageElement = new VisualElement();
                        imageElement.AddToClassList("help-illustration");
                        imageElement.style.backgroundImage = new StyleBackground(illustration);
                        imagesContainer.Add(imageElement);
                    }
                }
                
                container.Add(imagesContainer);
            }

            // Related topics section
            if (relatedTopics != null && relatedTopics.Length > 0)
            {
                var relatedContainer = new VisualElement();
                relatedContainer.AddToClassList("help-related");
                
                var relatedTitle = new Label("Related Topics:");
                relatedTitle.AddToClassList("related-title");
                relatedContainer.Add(relatedTitle);
                
                foreach (var topic in relatedTopics)
                {
                    var topicLabel = new Label($"→ {topic}");
                    topicLabel.AddToClassList("related-topic");
                    relatedContainer.Add(topicLabel);
                }
                
                container.Add(relatedContainer);
            }

            return container;
        }
    }
}