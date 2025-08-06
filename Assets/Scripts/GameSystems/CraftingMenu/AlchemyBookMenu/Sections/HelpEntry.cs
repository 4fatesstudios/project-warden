using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems.CraftingMenu.AlchemyBookMenu.Sections
{
    [System.Serializable]
    public class HelpEntry : BaseEntry
    {
        [Header("Help Properties")]
        public HelpTopicType topicType;
        public string content; // Lorem ipsum or actual content

        public override EntryType GetEntryType() => EntryType.Help;

        public override VisualElement CreateEntryVisual()
        {
            var container = CreateBaseVisual();
            
            // Help entries are always visible
            isSeen = true;
            
            var contentContainer = new VisualElement();
            contentContainer.AddToClassList("help-content");
            
            var contentLabel = new Label(content);
            contentLabel.AddToClassList("help-text");
            contentContainer.Add(contentLabel);
            
            container.Add(contentContainer);
            
            return container;
        }
    }

    public enum HelpTopicType
    {
        Refinements,
        PotionCrafting,
        Harvesting,
        CombatSystem,
        Template
    }
}