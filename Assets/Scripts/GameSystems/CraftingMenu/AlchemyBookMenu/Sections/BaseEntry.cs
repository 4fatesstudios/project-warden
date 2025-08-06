using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems.CraftingMenu.AlchemyBookMenu.Sections
{
    [System.Serializable]
    public abstract class BaseEntry
    {
        [Header("Base Entry Properties")]
        public string title;
        public string description;
        public Sprite visualElement;
        public bool isSeen;
        public bool isBookmarked;
        public int pageNumber; // Auto-generated

        public abstract EntryType GetEntryType();
        public abstract VisualElement CreateEntryVisual();
        
        protected VisualElement CreateBaseVisual()
        {
            var container = new VisualElement();
            container.AddToClassList("entry-container");
            
            if (!isSeen)
            {
                container.AddToClassList("entry-unseen");
                var questionMark = new Label("???");
                questionMark.AddToClassList("entry-unknown");
                container.Add(questionMark);
                return container;
            }
            
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("entry-title");
            container.Add(titleLabel);
            
            if (visualElement)
            {
                var image = new VisualElement();
                image.AddToClassList("entry-image");
                image.style.backgroundImage = new StyleBackground(visualElement);
                container.Add(image);
            }
            
            var descLabel = new Label(description);
            descLabel.AddToClassList("entry-description");
            container.Add(descLabel);
            
            if (isBookmarked)
            {
                var bookmark = new VisualElement();
                bookmark.AddToClassList("entry-bookmark");
                container.Add(bookmark);
            }
            
            return container;
        }
    }

    public enum EntryType
    {
        Bestiary,
        Recipe,
        Ingredient,
        Help,
        Bookmarked
    }
}
